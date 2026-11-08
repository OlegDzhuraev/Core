/*
 * Copyright 2026 Oleg Dzhuraev <godlikeaurora@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Editor
{
    public sealed class AssetDependenciesWindow : EditorWindow
    {
        const string LayoutPath = "InsaneOne/AssetDependenciesWindow";
        [SerializeField] string _guid;
        [SerializeField] bool _incoming;
        [SerializeField] bool _recursive;
        [SerializeField] bool _locked;
        readonly List<string> _visible = new();
        ObjectField _asset;
        Label _status;
        Label _summary;
        Label _empty;
        Label _scope;
        Button _outgoing;
        Button _incomingButton;
        ListView _list;
        ToolbarSearchField _search;

        [MenuItem("Tools/InsaneOne/Asset Dependencies/Show window...")]
        public static void Open() => ShowWindow(null, null);

        public static void ShowAsset(string guid) => ShowWindow(guid, null);

        public static void ShowAsset(string guid, bool incoming) => ShowWindow(guid, incoming);

        static void ShowWindow(string guid, bool? incoming)
        {
            var window = GetWindow<AssetDependenciesWindow>("Asset Dependencies", false);
            if (guid != null)
                window._guid = guid;
            if (incoming.HasValue)
            {
                window._incoming = incoming.Value;
                window._search?.SetValueWithoutNotify("");
            }
            window.Show();
            if (!window.docked)
            {
                var main = EditorGUIUtility.GetMainWindowPosition();
                var current = window.position;
                if (current.xMin < main.xMin || current.yMin < main.yMin
                    || current.xMax > main.xMax || current.yMax > main.yMax)
                {
                    var size = new Vector2(Mathf.Min(current.width, main.width - 32f),
                        Mathf.Min(current.height, main.height - 64f));
                    window.position = new Rect(main.center - size * 0.5f, size);
                }
            }
            window.Refresh();
            window.Focus();
        }

        void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            AssetDependencyIndex.Changed += Refresh;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            AssetDependencyIndex.Changed -= Refresh;
        }

        public void CreateGUI()
        {
            minSize = new Vector2(480f, 420f);
            rootVisualElement.Clear();
            Resources.Load<VisualTreeAsset>(LayoutPath).CloneTree(rootVisualElement);
            rootVisualElement.Q(className: "asset-dependencies").EnableInClassList("light-theme", !EditorGUIUtility.isProSkin);
            _asset = rootVisualElement.Q<ObjectField>("asset");
            _asset.objectType = typeof(UnityEngine.Object);
            _asset.allowSceneObjects = false;
            _asset.RegisterValueChangedCallback(e =>
            {
                _guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(e.newValue));
                Refresh();
            });
            _status = rootVisualElement.Q<Label>("status");
            _summary = rootVisualElement.Q<Label>("summary");
            _empty = rootVisualElement.Q<Label>("empty");
            _scope = rootVisualElement.Q<Label>("scope");
            _search = rootVisualElement.Q<ToolbarSearchField>("search");
            _search.RegisterValueChangedCallback(_ => Refresh());
            _outgoing = rootVisualElement.Q<Button>("outgoing");
            _incomingButton = rootVisualElement.Q<Button>("incoming");
            _outgoing.clicked += () => { _incoming = false; Refresh(); };
            _incomingButton.clicked += () => { _incoming = true; Refresh(); };
            var recursive = rootVisualElement.Q<Toggle>("recursive");
            recursive.SetValueWithoutNotify(_recursive);
            recursive.RegisterValueChangedCallback(e => { _recursive = e.newValue; Refresh(); });
            var locked = rootVisualElement.Q<Toggle>("locked");
            locked.SetValueWithoutNotify(_locked);
            locked.RegisterValueChangedCallback(e => { _locked = e.newValue; if (!_locked) OnSelectionChanged(); });
            rootVisualElement.Q<Button>("rebuild").clicked += AssetDependencyIndex.Rebuild;
            AddPathMenu(_asset, () => AssetDatabase.GUIDToAssetPath(_guid ?? ""));
            _list = rootVisualElement.Q<ListView>("results");
            _list.itemsSource = _visible;
            _list.fixedItemHeight = 34f;
            _list.selectionType = SelectionType.Single;
            _list.makeItem = () =>
            {
                var row = new VisualElement();
                row.AddToClassList("dependency-row");
                var icon = new Image { name = "icon", pickingMode = PickingMode.Ignore };
                icon.AddToClassList("asset-icon");
                row.Add(icon);
                var label = new Label { name = "assetName", pickingMode = PickingMode.Ignore };
                label.AddToClassList("asset-name");
                row.Add(label);
                var type = new Label { name = "assetType", pickingMode = PickingMode.Ignore };
                type.AddToClassList("asset-type");
                row.Add(type);
                var inspect = new Button(() => ShowAsset((string)row.userData)) { text = "↗", tooltip = "Inspect connections" };
                inspect.AddToClassList("inspect-button");
                row.Add(inspect);
                AddPathMenu(row, () => AssetDatabase.GUIDToAssetPath((string)row.userData));
                return row;
            };
            _list.bindItem = (element, index) =>
            {
                var guid = _visible[index];
                element.userData = guid;
                var path = AssetDependencyIndex.Graph.TryGet(guid, out var entry) ? entry.Path : AssetDatabase.GUIDToAssetPath(guid);
                element.tooltip = path;
                element.Q<Label>("assetName").text = Path.GetFileName(path);
                element.Q<Image>("icon").image = AssetDatabase.GetCachedIcon(path);
                element.Q<Label>("assetType").text = AssetDatabase.GetMainAssetTypeAtPath(path)?.Name ?? "Asset";
            };
            _list.selectionChanged += items =>
            {
                foreach (string guid in items)
                    EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            };
            _list.itemsChosen += items =>
            {
                foreach (string guid in items)
                    AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            };
            if (string.IsNullOrEmpty(_guid))
                OnSelectionChanged();
            Refresh();
        }

        void OnSelectionChanged()
        {
            if (_locked)
                return;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDependencyGraph.IsAssetPath(path))
                return;
            _guid = AssetDatabase.AssetPathToGUID(path);
            Refresh();
        }

        static void AddPathMenu(VisualElement element, Func<string> getPath)
        {
            element.AddManipulator(new ContextualMenuManipulator(evt => PopulatePathMenu(evt.menu, getPath())));
        }

        static void PopulatePathMenu(DropdownMenu menu, string path)
        {
            if (!string.IsNullOrEmpty(path))
                menu.AppendAction("Copy Path", _ => EditorGUIUtility.systemCopyBuffer = path);
        }

        void Refresh()
        {
            if (_list == null)
                return;
            _status.text = AssetDependencyIndex.Error ?? (AssetDependencyIndex.IsUpdating
                ? $"Indexing… {AssetDependencyIndex.Processed:N0} / {AssetDependencyIndex.Total:N0} paths"
                : $"●  Index up to date  ·  {AssetDependencyIndex.Graph.Count:N0} assets");
            _status.EnableInClassList("has-error", AssetDependencyIndex.Error != null);
            AssetDependencyIndex.Graph.TryGetCounts(_guid ?? "", out var dependencies, out var users);
            _outgoing.text = $"Outgoing  →  {dependencies:N0}";
            _incomingButton.text = $"Incoming  ←  {users:N0}";
            _outgoing.EnableInClassList("is-active", !_incoming);
            _incomingButton.EnableInClassList("is-active", _incoming);
            _outgoing.SetEnabled(_incoming);
            _incomingButton.SetEnabled(!_incoming);
            _scope.text = _recursive ? "INCLUDING INDIRECT" : "DIRECT LINKS";
            _asset.SetValueWithoutNotify(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(_guid ?? "")));
            _asset.tooltip = AssetDatabase.GUIDToAssetPath(_guid ?? "");
            _visible.Clear();
            var emptyMessage = "Select an asset in Project\nor drag it into the field above.";
            if (string.IsNullOrEmpty(_guid) || !AssetDependencyIndex.Graph.TryGet(_guid, out _))
                _summary.text = "Connections";
            else
            {
                var related = AssetDependencyIndex.Graph.Find(_guid, _incoming, _recursive);
                foreach (var guid in related)
                    if (AssetDependencyIndex.Graph.TryGet(guid, out var entry)
                        && entry.Path.IndexOf(_search.value ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        _visible.Add(guid);
                _summary.text = $"{(_incoming ? "Referenced by" : "Uses")}  ·  {_visible.Count:N0}"
                    + (_visible.Count != related.Count ? $" of {related.Count:N0}" : "");
                emptyMessage = related.Count == 0
                    ? (_incoming ? "No incoming references\nNo indexed assets reference this file." : "No outgoing dependencies\nThis asset has no indexed dependencies.")
                    : "No matching assets\nTry a different name or clear the search.";
            }
            _empty.text = emptyMessage;
            _empty.EnableInClassList("hidden", _visible.Count != 0);
            _list.ClearSelection();
            _list.Rebuild();
        }
    }
}
