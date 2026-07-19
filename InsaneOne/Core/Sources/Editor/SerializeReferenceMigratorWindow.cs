using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using InsaneOne.Core.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneOne.Core.Development
{
	/// <summary> Finds ScriptableObject assets with [SerializeReference] fields pointing to a type that Unity can no
	/// longer find (renamed class/namespace, or moved to a different asmdef), and lets you point each one at its new
	/// type without losing the already-serialized field data.
	/// <para> There is no public Unity API to change the type of an existing missing managed reference in place -
	/// detection uses SerializationUtility.GetManagedReferencesWithMissingTypes (read-only, confirmed available on
	/// Unity 6000.0), but applying a fix patches the asset's YAML text directly: Unity stores SerializeReference data
	/// as `- rid: &lt;id&gt;` / `type: {class: X, ns: Y, asm: Z}` / `data: ...` entries, and keeps the `data` block on
	/// disk regardless of whether the type currently resolves - so remapping just the `type: {...}` line for a given
	/// rid, without touching `data`, is enough to migrate the value forward. </para>
	/// <para> This only works for assets serialized as text (Edit > Project Settings > Editor > Asset Serialization
	/// set to Force Text or Mixed) - Force Binary projects aren't supported by this tool. </para></summary>
	public sealed class SerializeReferenceMigratorWindow : EditorWindow
	{
		[MenuItem("Tools/InsaneOne/SerializeReference Migrator...")]
		public static void ShowWindow()
		{
			var wnd = GetWindow<SerializeReferenceMigratorWindow>(false, "SerializeReference Migrator", true);
			wnd.minSize = new Vector2(200, 200);
		}

		readonly List<MissingTypeEntry> entries = new ();

		ScrollView listContainer;
		Label statusLabel;

		void CreateGUI()
		{
			var root = rootVisualElement;
			root.style.paddingLeft = 6;
			root.style.paddingRight = 6;
			root.style.paddingTop = 6;
			root.style.paddingBottom = 6;

			root.Add(new Label("SerializeReference rename migrator") { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 14 } });

			root.Add(new Label("Finds ScriptableObject assets with [SerializeReference] fields whose stored type can't be found " +
				"anymore (renamed namespace/assembly/class), and lets you point them at the new type.<br>Requires assets to be " +
				"serialized as text (Edit > Project Settings > Editor > Asset Serialization, not Force Binary).")
			{
				style = { whiteSpace = WhiteSpace.Normal, marginBottom = 6 }
			});

			root.Add(new Button(Scan) { text = "Scan Project" });

			statusLabel = new Label();
			root.Add(statusLabel);

			listContainer = new ScrollView { style = { flexGrow = 1, marginTop = 6, marginBottom = 6 } };
			root.Add(listContainer);

			root.Add(new Button(Apply) { text = "Apply" });

			Scan();
		}

		void Scan()
		{
			entries.Clear();
			listContainer.Clear();

			var guids = AssetDatabase.FindAssets("t:ScriptableObject");

			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

				if (!asset || !SerializationUtility.HasManagedReferencesWithMissingTypes(asset))
					continue;

				foreach (var missing in SerializationUtility.GetManagedReferencesWithMissingTypes(asset))
				{
					entries.Add(new MissingTypeEntry
					{
						Asset = asset,
						AssetPath = path,
						ReferenceId = missing.referenceId,
						OldAssemblyName = missing.assemblyName,
						OldNamespace = missing.namespaceName,
						OldClassName = missing.className,
						NewAssemblyName = missing.assemblyName,
						NewNamespace = missing.namespaceName,
						NewClassName = missing.className,
					});
				}
			}

			foreach (var entry in entries)
				listContainer.Add(BuildEntryRow(entry));

			statusLabel.text = entries.Count == 0
				? "No broken SerializeReference types found."
				: $"Found {entries.Count} broken reference(s).";
		}

		static VisualElement BuildEntryRow(MissingTypeEntry entry)
		{
			var box = new Box { style = { marginBottom = 6, paddingLeft = 4, paddingRight = 4, paddingTop = 4, paddingBottom = 4 } };

			var header = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
			header.Add(new Label(entry.AssetPath) { style = { unityFontStyleAndWeight = FontStyle.Bold } });
			header.Add(new Button(() => EditorGUIUtility.PingObject(entry.Asset)) { text = "Ping" });
			box.Add(header);

			box.Add(new Label($"Broken type: {entry.OldNamespace}.{entry.OldClassName}, {entry.OldAssemblyName}")
			{
				style = { color = new Color(1f, 0.6f, 0.2f), marginBottom = 4 }
			});

			var assemblyField = new TextField("Assembly") { value = entry.NewAssemblyName };
			assemblyField.RegisterValueChangedCallback(evt => entry.NewAssemblyName = evt.newValue);
			box.Add(assemblyField);

			var namespaceField = new TextField("Namespace") { value = entry.NewNamespace };
			namespaceField.RegisterValueChangedCallback(evt => entry.NewNamespace = evt.newValue);
			box.Add(namespaceField);

			var classField = new TextField("Class name") { value = entry.NewClassName };
			classField.RegisterValueChangedCallback(evt => entry.NewClassName = evt.newValue);
			box.Add(classField);

			return box;
		}

		void Apply()
		{
			if (EditorSettings.serializationMode is SerializationMode.ForceBinary)
			{
				EditorUtility.DisplayDialog("SerializeReference Migrator",
					"Project is set to Force Binary asset serialization. This tool patches the asset's YAML text " +
					"directly, so it can't work in this mode. Switch to Force Text or Mixed in " +
					"Project Settings > Editor > Asset Serialization first.", "Close");
				return;
			}

			// Group by asset so a file with several broken references is only read/written once.
			var entriesByAsset = new Dictionary<string, List<MissingTypeEntry>>();

			foreach (var entry in entries)
			{
				if (!entriesByAsset.TryGetValue(entry.AssetPath, out var list))
					entriesByAsset[entry.AssetPath] = list = new List<MissingTypeEntry>();

				list.Add(entry);
			}

			var appliedCount = 0;
			var failedCount = 0;

			foreach (var (assetPath, assetEntries) in entriesByAsset)
			{
				string text;

				try
				{
					text = File.ReadAllText(assetPath);
				}
				catch (Exception ex)
				{
					CoreUnityLogger.I.Log($"[SerializeReference Migrator] Could not read [ {assetPath} ]: {ex.Message}", LogLevel.Error);
					failedCount += assetEntries.Count;
					continue;
				}

				var changedAny = false;

				foreach (var entry in assetEntries)
				{
					var patched = PatchManagedReferenceType(text, entry.ReferenceId, entry.NewClassName, entry.NewNamespace, entry.NewAssemblyName);

					if (patched == text)
					{
						CoreUnityLogger.I.Log($"[SerializeReference Migrator] Could not find managed reference id [ {entry.ReferenceId} ] in [ {assetPath} ] – skipped.", LogLevel.Warning);
						failedCount++;
						continue;
					}

					text = patched;
					changedAny = true;
					appliedCount++;
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
					CoreUnityLogger.I.Log($"[SerializeReference Migrator] Could not write [ {assetPath} ]: {ex.Message}", LogLevel.Error);
				}
			}

			AssetDatabase.Refresh();

			statusLabel.text = $"Applied: {appliedCount}, failed: {failedCount}.";

			Scan(); // re-scan, so fixed entries disappear and anything still broken stays listed
		}

		/// <summary> Replaces the `type: {...}` entry for the given managed reference id inside a ScriptableObject
		/// asset's YAML text, leaving its `data` block (the actual serialized field values) untouched. Returns the
		/// text unchanged if <paramref name="referenceId"/> couldn't be found in it. Exposed as public static so the
		/// core patching logic can be unit tested without needing a real asset/AssetDatabase. </summary>
		public static string PatchManagedReferenceType(string yamlText, long referenceId, string newClassName, string newNamespace, string newAssemblyName)
		{
			var pattern = $@"(rid:\s*{referenceId}\s*\r?\n\s*type:\s*\{{)[^}}]*(\}})";
			var replacement = $"class: {newClassName}, ns: {newNamespace}, asm: {newAssemblyName}";

			return Regex.Replace(yamlText, pattern, m => m.Groups[1].Value + replacement + m.Groups[2].Value);
		}

		sealed class MissingTypeEntry
		{
			public Object Asset;
			public string AssetPath;
			public long ReferenceId;

			public string OldAssemblyName;
			public string OldNamespace;
			public string OldClassName;

			public string NewAssemblyName;
			public string NewNamespace;
			public string NewClassName;
		}
	}
}
