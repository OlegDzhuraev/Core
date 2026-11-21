using System.Collections.Generic;
using InsaneOne.Core.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneOne.Core.Development
{
	/// <summary> Editor window UI for the SerializeReference Migrator. Layout lives in
	/// Resources/InsaneOne/SerializeReferenceMigratorWindow.uxml, styles in the matching .uss (referenced from the
	/// uxml itself); only the dynamic entry rows are built in code, styled with the same uss classes. The scan/apply
	/// logic itself lives in the headless static core (<see cref="SerializeReferenceMigrator"/>), which is also driven
	/// programmatically by <see cref="SerializeReferenceMigratorAiBridge"/> - so AI agents can use the same tool
	/// without the window. </summary>
	public sealed class SerializeReferenceMigratorWindow : EditorWindow
	{
		const string LayoutPath = "InsaneOne/SerializeReferenceMigratorWindow";

		// dynamic entry rows are built in code, but styled with these classes from the window uss
		const string EntryClass = "srm-entry";
		const string EntryHeaderClass = "srm-entry-header";
		const string EntryPathClass = "srm-entry-path";
		const string PingButtonClass = "srm-ping-button";
		const string BrokenTypeClass = "srm-broken-type";
		const string EmptyStateClass = "srm-empty-state";
		const string StatusCleanClass = "srm-status-clean";
		const string StatusBrokenClass = "srm-status-broken";

		[MenuItem("Tools/InsaneOne/SerializeReference Migrator...")]
		public static void ShowWindow()
		{
			var wnd = GetWindow<SerializeReferenceMigratorWindow>(false, "SerializeReference Migrator", true);
			wnd.minSize = new Vector2(200, 200);
		}

		readonly List<MissingTypeEntry> entries = new ();

		ScrollView listContainer;
		Label statusLabel;
		Button applyButton;
		Button applyToAllButton;

		TextField bulkAssemblyField;
		TextField bulkNamespaceField;

		void CreateGUI()
		{
			var layout = Resources.Load<VisualTreeAsset>(LayoutPath);

			if (layout == null)
			{
				CoreUnityLogger.I.Log($"[{nameof(SerializeReferenceMigratorWindow)}] Could not load the window layout from Resources at [ {LayoutPath} ]!", LogLevel.Error);
				return;
			}

			var root = rootVisualElement;
			layout.CloneTree(root); // the uss comes along through the <Style> src reference inside the uxml

			root.Q<Button>("scan-button").clicked += Scan;
			applyButton = root.Q<Button>("apply-button");
			applyButton.clicked += Apply;
			applyToAllButton = root.Q<Button>("apply-to-all-button");
			applyToAllButton.clicked += ApplyBulkToAll;

			statusLabel = root.Q<Label>("status-label");
			listContainer = root.Q<ScrollView>("list-container");
			bulkAssemblyField = root.Q<TextField>("bulk-assembly");
			bulkNamespaceField = root.Q<TextField>("bulk-namespace");

			Scan();
		}

		void Scan()
		{
			entries.Clear();

			foreach (var missing in SerializeReferenceMigrator.ScanProject())
			{
				entries.Add(new MissingTypeEntry
				{
					Asset = missing.Asset,
					AssetPath = missing.AssetPath,
					ReferenceId = missing.ReferenceId,
					Holder = missing.Holder,
					OldAssemblyName = missing.AssemblyName,
					OldNamespace = missing.NamespaceName,
					OldClassName = missing.ClassName,
					NewAssemblyName = missing.AssemblyName,
					NewNamespace = missing.NamespaceName,
					NewClassName = missing.ClassName,
				});
			}

			RebuildList();

			statusLabel.text = entries.Count == 0
				? "No broken SerializeReference types found."
				: $"Found {entries.Count} broken reference(s).";
			statusLabel.EnableInClassList(StatusCleanClass, entries.Count == 0);
			statusLabel.EnableInClassList(StatusBrokenClass, entries.Count > 0);

			UpdateControlsState();
		}

		void RebuildList()
		{
			listContainer.Clear();

			if (entries.Count == 0)
			{
				var emptyLabel = new Label("Nothing to migrate — no broken SerializeReference types in the project.");
				emptyLabel.AddToClassList(EmptyStateClass);
				listContainer.Add(emptyLabel);
				return;
			}

			foreach (var entry in entries)
				listContainer.Add(BuildEntryRow(entry));
		}

		/// <summary> The bulk remap and Apply controls are only meaningful when there is something to fix. </summary>
		void UpdateControlsState()
		{
			var hasEntries = entries.Count > 0;

			applyButton.SetEnabled(hasEntries);
			applyToAllButton.SetEnabled(hasEntries);
			bulkAssemblyField.SetEnabled(hasEntries);
			bulkNamespaceField.SetEnabled(hasEntries);
		}

		/// <summary> Sets the assembly/namespace from the bulk fields on every found entry (an empty bulk field keeps each
		/// entry's own value untouched), then rebuilds the list so per-entry fields show the new values. </summary>
		void ApplyBulkToAll()
		{
			if (entries.Count == 0)
				return;

			var assembly = bulkAssemblyField.value;
			var namespaceName = bulkNamespaceField.value;

			foreach (var entry in entries)
			{
				if (!string.IsNullOrWhiteSpace(assembly))
					entry.NewAssemblyName = assembly;

				if (!string.IsNullOrWhiteSpace(namespaceName))
					entry.NewNamespace = namespaceName;
			}

			RebuildList();
		}

		static VisualElement BuildEntryRow(MissingTypeEntry entry)
		{
			var box = new Box();
			box.AddToClassList(EntryClass);

			var header = new VisualElement();
			header.AddToClassList(EntryHeaderClass);

			var pathLabel = new Label(entry.AssetPath);
			pathLabel.AddToClassList(EntryPathClass);
			header.Add(pathLabel);

			var pingButton = new Button(() => EditorGUIUtility.PingObject(entry.Asset)) { text = "Ping" };
			pingButton.AddToClassList(PingButtonClass);
			header.Add(pingButton);

			box.Add(header);

			var holderSuffix = string.IsNullOrEmpty(entry.Holder) ? string.Empty : $", on {entry.Holder}";
			var brokenTypeLabel = new Label($"Broken type: {entry.OldNamespace}.{entry.OldClassName}, {entry.OldAssemblyName} (rid {entry.ReferenceId}{holderSuffix})");
			brokenTypeLabel.AddToClassList(BrokenTypeClass);
			box.Add(brokenTypeLabel);

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

			var fixes = new List<SerializeReferenceMigrator.FixEntry>(entries.Count);

			foreach (var entry in entries)
			{
				fixes.Add(new SerializeReferenceMigrator.FixEntry
				{
					AssetPath = entry.AssetPath,
					ReferenceId = entry.ReferenceId,
					NewAssemblyName = entry.NewAssemblyName,
					NewNamespace = entry.NewNamespace,
					NewClassName = entry.NewClassName,
				});
			}

			var result = SerializeReferenceMigrator.ApplyFixes(fixes);

			foreach (var error in result.Errors)
				CoreUnityLogger.I.Log($"[SerializeReference Migrator] {error}", LogLevel.Error);

			statusLabel.text = $"Applied: {result.AppliedCount}, failed: {result.FailedCount}.";

			Scan(); // re-scan, so fixed entries disappear and anything still broken stays listed
		}

		sealed class MissingTypeEntry
		{
			public Object Asset;
			public string AssetPath;
			public long ReferenceId;
			public string Holder;

			public string OldAssemblyName;
			public string OldNamespace;
			public string OldClassName;

			public string NewAssemblyName;
			public string NewNamespace;
			public string NewClassName;
		}
	}
}
