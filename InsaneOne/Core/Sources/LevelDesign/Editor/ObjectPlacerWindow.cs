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

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Editor tool to place prefabs from an ObjectPalette onto scene colliders by clicking (or drag-scattering) in the Scene view. </summary>
	public class ObjectPlacerWindow : EditorWindow
	{
		const string StylesPath = "InsaneOne/ToolsStyles";
		const string GroupStyleName = "group-box";
		const float RaycastMaxDistance = 1000f;
		const string PlaceUndoGroupName = "Place Prefabs";
		const string EraseUndoGroupName = "Erase Prefabs";

		const float NormalPreviewLength = 1f;

		static readonly Color PreviewSphereReadyColor = new (0.3f, 0.75f, 1f, 0.9f);
		static readonly Color PreviewSphereEraseReadyColor = new (1f, 0.6f, 0.1f, 0.9f);
		static readonly Color PreviewSphereNoEntryColor = new (1f, 0.3f, 0.3f, 0.9f);
		static readonly Color PositionScatterDiscColor = new (0.3f, 0.75f, 1f, 0.15f);

		ObjectPlacerGeneralSection generalSection;
		ObjectPlacerPaletteSection paletteSection;
		ObjectPlacerBrushSection brushSection;

		bool isDragScattering;
		Vector3 lastPlacementPoint;
		int activeUndoGroup = -1;

		[MenuItem("Tools/InsaneOne/Level Design/Object Placer...")]
		static void Init() => Open();

		/// <summary> Opens the Object Placer window, or focuses it if it's already open. </summary>
		public static void Open()
		{
			var window = (ObjectPlacerWindow)GetWindow(typeof(ObjectPlacerWindow), false, "Object Placer", true);
			window.titleContent = new GUIContent("Object Placer", EditorGUIUtility.IconContent("GameObject Icon").image);
			window.Show();
		}

		void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
		void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

		void CreateGUI()
		{
			var style = Resources.Load(StylesPath) as StyleSheet;
			var root = rootVisualElement;
			root.styleSheets.Add(style);
			root.AddToClassList("core-init-root");

			var infoBox = new VisualElement();
			infoBox.AddToClassList(GroupStyleName);
			infoBox.Add(new Label("Places a prefab from the palette onto scene colliders on click, while the tool is active.")
			{
				style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter), whiteSpace = WhiteSpace.Normal },
			});

			generalSection = new ObjectPlacerGeneralSection();
			paletteSection = new ObjectPlacerPaletteSection();
			brushSection = new ObjectPlacerBrushSection();

			var scrollView = new ScrollView(ScrollViewMode.Vertical);
			scrollView.Add(infoBox);
			scrollView.Add(generalSection);
			scrollView.Add(paletteSection);
			scrollView.Add(brushSection);

			root.Add(scrollView);
		}

		void OnSceneGUI(SceneView sceneView)
		{
			if (generalSection == null || !ObjectPlacerToolState.IsActive)
				return;

			var e = Event.current;
			var controlId = GUIUtility.GetControlID(FocusType.Passive);

			if (e.type == EventType.Layout)
				HandleUtility.AddDefaultControl(controlId);

			var mode = ObjectPlacerToolState.Mode;
			var isEraseMode = mode == ObjectPlacerMode.Erase;
			var brush = brushSection.SelectedBrush;

			var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
			var hasHit = Physics.Raycast(ray, out var hit, RaycastMaxDistance, generalSection.LayerMask);

			var (eraseTarget, eraseRegistry) = hasHit && isEraseMode ? FindPlacedRoot(hit.collider.gameObject) : (null, null);
			var isSlopeValid = hasHit && !isEraseMode && Vector3.Angle(hit.normal, Vector3.up) <= brush.MaxSlopeAngle;
			var canPlaceHere = hasHit && !isEraseMode && isSlopeValid && paletteSection.CanPlace;
			var canActHere = isEraseMode ? eraseTarget != null : canPlaceHere;

			if (hasHit)
			{
				Handles.color = canActHere ? (isEraseMode ? PreviewSphereEraseReadyColor : PreviewSphereReadyColor) : PreviewSphereNoEntryColor;
				var size = HandleUtility.GetHandleSize(hit.point) * 0.15f;
				Handles.SphereHandleCap(0, hit.point, Quaternion.identity, size, EventType.Repaint);

				if (!isEraseMode && brush.AlignToNormal)
				{
					Handles.color = Color.cyan;
					Handles.DrawLine(hit.point, hit.point + hit.normal * NormalPreviewLength);
				}

				if (!isEraseMode && brush.RandomizePosition)
				{
					Handles.color = PositionScatterDiscColor;
					Handles.DrawSolidDisc(hit.point, hit.normal, brush.MaxPositionOffset);
				}
			}

			var eventType = e.GetTypeForControl(controlId);

			if (eventType == EventType.MouseDown && e.button == 0 && !e.alt && !e.control && !e.command)
			{
				GUIUtility.hotControl = controlId;
				isDragScattering = mode == ObjectPlacerMode.DragScatter || isEraseMode;

				BeginUndoGroup(isEraseMode);

				if (canActHere)
				{
					PerformAction(isEraseMode, hit, eraseTarget, eraseRegistry);
					lastPlacementPoint = hit.point;
				}

				e.Use();
			}
			else if (eventType == EventType.MouseDrag && e.button == 0 && GUIUtility.hotControl == controlId)
			{
				if (isDragScattering && canActHere && Vector3.Distance(hit.point, lastPlacementPoint) >= generalSection.DragScatterSpacing)
				{
					PerformAction(isEraseMode, hit, eraseTarget, eraseRegistry);
					lastPlacementPoint = hit.point;
				}

				e.Use();
			}
			else if (eventType == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == controlId)
			{
				GUIUtility.hotControl = 0;
				isDragScattering = false;
				EndUndoGroup();
				e.Use();
			}

			if (e.type == EventType.MouseMove)
				sceneView.Repaint();
		}

		void PerformAction(bool isEraseMode, RaycastHit hit, GameObject eraseTarget, ObjectPlacerRegistry eraseRegistry)
		{
			if (isEraseMode)
				EraseTarget(eraseTarget, eraseRegistry);
			else
				PlaceEntry(hit);
		}

		static (GameObject root, ObjectPlacerRegistry registry) FindPlacedRoot(GameObject hitObject)
		{
			var registries = Object.FindObjectsOfType<ObjectPlacerRegistry>();
			var current = hitObject.transform;

			while (current)
			{
				var candidate = current.gameObject;

				foreach (var registry in registries)
					if (registry.PlacedObjects.Contains(candidate))
						return (candidate, registry);

				current = current.parent;
			}

			return (null, null);
		}

		static void EraseTarget(GameObject target, ObjectPlacerRegistry registry)
		{
			if (!target)
				return;

			if (registry)
			{
				Undo.RecordObject(registry, "Erase Prefab");
				registry.PlacedObjects.Remove(target);
			}

			Undo.DestroyObjectImmediate(target);
		}

		static ObjectPlacerRegistry GetOrCreateRegistry()
		{
			var registry = Object.FindObjectOfType<ObjectPlacerRegistry>();
			if (registry)
				return registry;

			var go = new GameObject("Object Placer Registry");
			registry = go.AddComponent<ObjectPlacerRegistry>();
			Undo.RegisterCreatedObjectUndo(go, "Create Object Placer Registry");

			return registry;
		}

		void BeginUndoGroup(bool isEraseMode)
		{
			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName(isEraseMode ? EraseUndoGroupName : PlaceUndoGroupName);
			activeUndoGroup = Undo.GetCurrentGroup();
		}

		void EndUndoGroup()
		{
			if (activeUndoGroup < 0)
				return;

			Undo.CollapseUndoOperations(activeUndoGroup);
			activeUndoGroup = -1;
		}

		void PlaceEntry(RaycastHit hit)
		{
			var entry = paletteSection.GetEntryToPlace();
			if (entry == null || !entry.Prefab)
				return;

			var brush = brushSection.SelectedBrush;

			var instance = (GameObject)PrefabUtility.InstantiatePrefab(entry.Prefab);
			Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");

			var registry = GetOrCreateRegistry();
			Undo.RecordObject(registry, "Place Prefab");
			registry.PlacedObjects.Add(instance);

			if (generalSection.ParentTransform)
				instance.transform.SetParent(generalSection.ParentTransform, false);

			var position = hit.point;
			if (brush.RandomizePosition)
				position += GetRandomTangentOffset(hit.normal, brush.MaxPositionOffset);
			if (brush.SnapToGrid)
				position = SnapToGrid(position, brush.GridSize);

			var rotation = brush.AlignToNormal ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity;
			if (brush.RandomizeRotation)
				rotation *= Quaternion.Euler(0f, Random.Range(-brush.MaxRotationAngle, brush.MaxRotationAngle), 0f);

			instance.transform.SetPositionAndRotation(position, rotation);

			if (brush.RandomizeScale)
			{
				var range = brush.ScaleRange;
				var multiplier = Random.Range(range.x, range.y);
				instance.transform.localScale = entry.Prefab.transform.localScale * multiplier;
			}
		}

		static Vector3 SnapToGrid(Vector3 position, float gridSize)
		{
			if (gridSize <= 0f)
				return position;

			return new Vector3(
				Mathf.Round(position.x / gridSize) * gridSize,
				Mathf.Round(position.y / gridSize) * gridSize,
				Mathf.Round(position.z / gridSize) * gridSize);
		}

		static Vector3 GetRandomTangentOffset(Vector3 normal, float maxDistance)
		{
			if (maxDistance <= 0f)
				return Vector3.zero;

			var tangent = Vector3.Cross(normal, Vector3.up);
			if (tangent.sqrMagnitude < 0.001f)
				tangent = Vector3.Cross(normal, Vector3.right);
			tangent.Normalize();

			var bitangent = Vector3.Cross(normal, tangent);
			var circle = Random.insideUnitCircle * maxDistance;

			return tangent * circle.x + bitangent * circle.y;
		}
	}
}
