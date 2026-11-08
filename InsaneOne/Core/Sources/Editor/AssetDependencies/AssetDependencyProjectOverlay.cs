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

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Editor
{
    [InitializeOnLoad]
    static class AssetDependencyProjectOverlay
    {
        const string MenuPath = "Tools/InsaneOne/Asset Dependencies/Show Counts in Project";
        const string Preference = "InsaneOne.AssetDependencies.ShowCounts";
        const float ColumnWidth = 46f;
        const float ArrowWidth = 12f;

        sealed class BadgeContent
        {
            public GUIContent Outgoing;
            public GUIContent Incoming;
        }

        static readonly Dictionary<string, BadgeContent> Contents = new();
        static readonly GUIContent OutgoingArrow = new("→", "Inspect outgoing dependencies.");
        static readonly GUIContent IncomingArrow = new("←", "Inspect incoming references.");
        static readonly GUIContent PendingOutgoing = new("…", "Index is updating. Click to inspect outgoing dependencies.");
        static readonly GUIContent PendingIncoming = new("…", "Index is updating. Click to inspect incoming references.");
        static readonly GUIContent FailedOutgoing = new("!", "Indexing failed. Click for details.");
        static readonly GUIContent FailedIncoming = new("!", "Indexing failed. Click for details.");
        static readonly HashSet<string> Selected = new();
        static GUIStyle _style;
        static GUIStyle _selectedStyle;
        static bool _enabled;
        static readonly PropertyInfo CurrentView = typeof(EditorWindow).Assembly.GetType("UnityEditor.GUIView")
            ?.GetProperty("current", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        static readonly PropertyInfo ActualView = typeof(EditorWindow).Assembly.GetType("UnityEditor.HostView")
            ?.GetProperty("actualView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        static readonly PropertyInfo VisibleRect = typeof(GUI).Assembly.GetType("UnityEngine.GUIClip")
            ?.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        static readonly Dictionary<EditorWindow, Dictionary<float, (string Guid, Rect Outgoing, Rect Incoming)>> HitAreas
            = new();

        static AssetDependencyProjectOverlay()
        {
            _enabled = EditorPrefs.GetBool(Preference, true);
            EditorApplication.projectWindowItemOnGUI += Draw;
            AssetDependencyIndex.Changed += Contents.Clear;
            AssetDependencyIndex.Changed += ClearHitAreas;
            Selection.selectionChanged += UpdateSelection;
            UpdateSelection();
        }

        static void ClearHitAreas()
        {
            foreach (var areas in HitAreas.Values)
                areas.Clear();
        }

        static void TrackHitArea(string guid, Rect outgoing, Rect incoming)
        {
            if (VisibleRect?.GetValue(null) is Rect visible && !visible.Contains(incoming.center))
                return;
            var view = CurrentView?.GetValue(null);
            if (view == null || !(ActualView?.GetValue(view) is EditorWindow window))
                return;
            if (!HitAreas.TryGetValue(window, out var areas))
            {
                areas = new Dictionary<float, (string Guid, Rect Outgoing, Rect Incoming)>();
                HitAreas.Add(window, areas);
                window.rootVisualElement.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (!_enabled || evt.button != 0)
                        return;
                    var screenPoint = (Vector2)evt.position + window.position.position;
                    foreach (var area in areas)
                    {
                        var incomingClicked = area.Value.Incoming.Contains(screenPoint);
                        if (!incomingClicked && !area.Value.Outgoing.Contains(screenPoint))
                            continue;
                        var selectedGuid = area.Value.Guid;
                        evt.StopImmediatePropagation();
                        EditorApplication.delayCall += () => AssetDependenciesWindow.ShowAsset(selectedGuid, incomingClicked);
                        return;
                    }
                    areas.Clear();
                }, TrickleDown.TrickleDown);
                window.rootVisualElement.RegisterCallback<WheelEvent>(_ => areas.Clear(), TrickleDown.TrickleDown);
                window.rootVisualElement.RegisterCallback<KeyDownEvent>(_ => areas.Clear(), TrickleDown.TrickleDown);
                window.rootVisualElement.RegisterCallback<GeometryChangedEvent>(_ => areas.Clear());
                window.rootVisualElement.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    areas.Clear();
                    EditorApplication.delayCall += () =>
                    {
                        if (window == null)
                            HitAreas.Remove(window);
                    };
                });
            }
            var outgoingScreen = GUIUtility.GUIToScreenRect(outgoing);
            var incomingScreen = GUIUtility.GUIToScreenRect(incoming);
            areas[incomingScreen.y] = (guid, outgoingScreen, incomingScreen);
        }

        static void UpdateSelection()
        {
            Selected.Clear();
            Selected.UnionWith(Selection.assetGUIDs);
        }

        [MenuItem(MenuPath)]
        static void Toggle()
        {
            _enabled = !_enabled;
            EditorPrefs.SetBool(Preference, _enabled);
            EditorApplication.RepaintProjectWindow();
        }

        [MenuItem(MenuPath, true)]
        static bool Validate()
        {
            Menu.SetChecked(MenuPath, _enabled);
            return true;
        }

        static void Draw(string guid, Rect rect)
        {
            if (!_enabled || rect.height > 22f || rect.width < 140f
                || !AssetDependencyIndex.Graph.TryGetCounts(guid, out var dependencies, out var users))
                return;
            _style ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0)
            };
            _selectedStyle ??= new GUIStyle(_style) { normal = { textColor = Color.white } };
            if (!Contents.TryGetValue(guid, out var content))
            {
                content = new BadgeContent
                {
                    Outgoing = new GUIContent(dependencies.ToString(), $"Outgoing dependencies: {dependencies}\nClick to inspect assets this file uses."),
                    Incoming = new GUIContent(users.ToString(), $"Incoming references: {users}\nClick to inspect assets referencing this file.")
                };
                Contents.Add(guid, content);
            }
            var outgoingContent = content.Outgoing;
            var incomingContent = content.Incoming;
            if (AssetDependencyIndex.Error != null)
            {
                outgoingContent = FailedOutgoing;
                incomingContent = FailedIncoming;
            }
            else if (AssetDependencyIndex.IsUpdating)
            {
                outgoingContent = PendingOutgoing;
                incomingContent = PendingIncoming;
            }
            var incoming = new Rect(rect.xMax - ColumnWidth - 4f, rect.y, ColumnWidth, rect.height);
            var outgoing = new Rect(incoming.x - ColumnWidth, rect.y, ColumnWidth, rect.height);
            var badge = new Rect(outgoing.x, rect.y, ColumnWidth * 2f, rect.height);
            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && badge.Contains(evt.mousePosition))
            {
                evt.Use();
                var incomingClicked = incoming.Contains(evt.mousePosition);
                EditorApplication.delayCall += () => AssetDependenciesWindow.ShowAsset(guid, incomingClicked);
            }
            if (Event.current.type != EventType.Repaint)
                return;
            TrackHitArea(guid, outgoing, incoming);
            EditorGUIUtility.AddCursorRect(outgoing, MouseCursor.Link);
            EditorGUIUtility.AddCursorRect(incoming, MouseCursor.Link);
            var selected = Selected.Contains(guid);
            EditorGUI.DrawRect(badge, selected
                ? (EditorGUIUtility.isProSkin ? new Color(0.17f, 0.36f, 0.53f) : new Color(0.24f, 0.49f, 0.9f))
                : (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f)));
            var style = selected ? _selectedStyle : _style;
            EditorGUI.DrawRect(new Rect(incoming.x, rect.y + 3f, 1f, rect.height - 6f),
                new Color(0.5f, 0.5f, 0.5f, 0.35f));
            DrawColumn(outgoing, OutgoingArrow, outgoingContent, style);
            DrawColumn(incoming, IncomingArrow, incomingContent, style);
        }

        static void DrawColumn(Rect rect, GUIContent arrow, GUIContent count, GUIStyle style)
        {
            var original = GUI.contentColor;
            if (count.text == "0")
            {
                var faded = original;
                faded.a *= 0.4f;
                GUI.contentColor = faded;
            }
            try
            {
                GUI.Label(new Rect(rect.x + 1f, rect.y, ArrowWidth, rect.height), arrow, style);
                GUI.Label(new Rect(rect.x + ArrowWidth + 1f, rect.y, rect.width - ArrowWidth - 2f, rect.height), count, style);
            }
            finally
            {
                GUI.contentColor = original;
            }
        }
    }
}
