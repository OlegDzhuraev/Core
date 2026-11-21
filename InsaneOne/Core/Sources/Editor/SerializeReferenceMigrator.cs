using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace InsaneOne.Core.Development
{
	/// <summary> Headless core of the SerializeReference Migrator - the EditorWindow and the AI bridge are both thin
	/// wrappers around this class. Finds ScriptableObject assets with [SerializeReference] fields pointing to a type
	/// that Unity can no longer find (renamed class/namespace, or moved to a different asmdef), and lets you re-point
	/// them at the new type without losing the already-serialized field data.
	/// <para> There is no public Unity API to change the type of an existing missing managed reference in place -
	/// detection uses SerializationUtility.GetManagedReferencesWithMissingTypes (read-only), but applying a fix patches
	/// the asset's YAML text directly: Unity stores SerializeReference data as `- rid: <id>` /
	/// `type: {class: X, ns: Y, asm: Z}` / `data: ...` entries, and keeps the `data` block on disk regardless of whether
	/// the type currently resolves - so remapping just the `type: {...}` line for a given rid, without touching `data`,
	/// is enough to migrate the value forward. </para>
	/// <para> This only works for assets serialized as text (Force Text or Mixed asset serialization) - Force Binary
	/// projects aren't supported. </para></summary>
	public static class SerializeReferenceMigrator
	{
		/// <summary> Scans all ScriptableObject assets in the project and returns every broken [SerializeReference] found. </summary>
		public static List<MissingType> ScanProject()
		{
			var found = new List<MissingType>();

			foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

				if (!asset || !SerializationUtility.HasManagedReferencesWithMissingTypes(asset))
					continue;

				foreach (var missing in SerializationUtility.GetManagedReferencesWithMissingTypes(asset))
				{
					found.Add(new MissingType
					{
						Asset = asset,
						AssetPath = path,
						ReferenceId = missing.referenceId,
						AssemblyName = missing.assemblyName,
						NamespaceName = missing.namespaceName,
						ClassName = missing.className,
					});
				}
			}

			return found;
		}

		/// <summary> Re-points the listed broken references at their new types by patching the assets' YAML text
		/// (grouped per asset, so a file with several broken references is only read/written once). Doesn't touch
		/// serialized field data. Refreshes the AssetDatabase when at least one fix was applied. </summary>
		public static ApplyResult ApplyFixes(IReadOnlyList<FixEntry> fixes)
		{
			var result = new ApplyResult();

			if (EditorSettings.serializationMode is SerializationMode.ForceBinary)
			{
				result.Errors.Add("Project is set to Force Binary asset serialization. This tool patches the asset's YAML text " +
					"directly, so it can't work in this mode. Switch to Force Text or Mixed in Project Settings > Editor > Asset Serialization first.");

				result.FailedCount += fixes.Count;
				return result;
			}

			var fixesByAsset = new Dictionary<string, List<FixEntry>>();

			foreach (var fix in fixes)
			{
				if (!fixesByAsset.TryGetValue(fix.AssetPath, out var list))
					fixesByAsset[fix.AssetPath] = list = new List<FixEntry>();

				list.Add(fix);
			}

			foreach (var (assetPath, assetFixes) in fixesByAsset)
			{
				string text;

				try
				{
					text = File.ReadAllText(assetPath);
				}
				catch (Exception ex)
				{
					result.Errors.Add($"Could not read [ {assetPath} ]: {ex.Message}");
					result.FailedCount += assetFixes.Count;
					continue;
				}

				var changedAny = false;

				foreach (var fix in assetFixes)
				{
					var patched = PatchManagedReferenceType(text, fix.ReferenceId, fix.NewClassName, fix.NewNamespace, fix.NewAssemblyName);

					if (patched == text)
					{
						result.Errors.Add($"Could not find managed reference id [ {fix.ReferenceId} ] in [ {assetPath} ] - skipped.");
						result.FailedCount++;
						continue;
					}

					text = patched;
					changedAny = true;
					result.AppliedCount++;
				}

				if (!changedAny)
					continue;

				try
				{
					File.WriteAllText(assetPath, text);
					AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
				}
				catch (Exception ex)
				{
					result.Errors.Add($"Could not write [ {assetPath} ]: {ex.Message}");
				}
			}

			AssetDatabase.Refresh();

			return result;
		}

		/// <summary> Replaces the `type: {...}` entry for the given managed reference id inside a ScriptableObject
		/// asset's YAML text, leaving its `data` block (the actual serialized field values) untouched. Returns the
		/// text unchanged if <paramref name="referenceId"/> couldn't be found in it. </summary>
		public static string PatchManagedReferenceType(string yamlText, long referenceId, string newClassName, string newNamespace, string newAssemblyName)
		{
			var pattern = $@"(rid:\s*{referenceId}\s*\r?\n\s*type:\s*\{{)[^}}]*(\}})";
			var replacement = $"class: {newClassName}, ns: {newNamespace}, asm: {newAssemblyName}";

			return Regex.Replace(yamlText, pattern, m => m.Groups[1].Value + replacement + m.Groups[2].Value);
		}

		/// <summary> One broken [SerializeReference] found by <see cref="ScanProject"/>. </summary>
		public sealed class MissingType
		{
			public Object Asset;
			public string AssetPath;
			public long ReferenceId;

			public string AssemblyName;
			public string NamespaceName;
			public string ClassName;
		}

		/// <summary> A single requested type remap for <see cref="ApplyFixes"/>. </summary>
		public sealed class FixEntry
		{
			public string AssetPath;
			public long ReferenceId;

			public string NewAssemblyName;
			public string NewNamespace;
			public string NewClassName;
		}

		/// <summary> Outcome of <see cref="ApplyFixes"/>. </summary>
		public sealed class ApplyResult
		{
			public int AppliedCount;
			public int FailedCount;
			public readonly List<string> Errors = new ();
		}
	}
}
