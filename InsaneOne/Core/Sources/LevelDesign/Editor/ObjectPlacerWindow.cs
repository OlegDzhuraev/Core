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

		static readonly Color PreviewSphereReadyColor = new (0.3f, 0.75f, 1f, 0.9f);
		static readonly Color PreviewSphereNoEntryColor = new (1f, 0.3f, 0.3f, 0.9f);

		ObjectPlacerGeneralSection generalSection;
		ObjectPlacerPaletteSection paletteSection;
		ObjectPlacerPlacementSettingsSection placementSettingsSection;

		bool isDragScattering;
		Vector3 lastPlacementPoint;
		int activeUndoGroup = -1;

		[MenuItem("Tools/InsaneOne/Level Design/Object Placer...")]
		static void Init()
		{
			var window = (ObjectPlacerWindow)GetWindow(typeof(ObjectPlacerWindow), false, "Object Placer", true);
			window.Show();
		}

		void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
		void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

		void CreateGUI()
		{
			var style = Resources.Load(StylesPath) as StyleSheet;
			var root = rootVisualElement;
			root.styleSheets.Add(style);

			var infoBox = new VisualElement();
			infoBox.AddToClassList(GroupStyleName);
			infoBox.Add(new Label("Places a prefab from the palette onto scene colliders on click, while the tool is active.")
			{
				style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter), whiteSpace = WhiteSpace.Normal },
			});

			generalSection = new ObjectPlacerGeneralSection();
			paletteSection = new ObjectPlacerPaletteSection();
			placementSettingsSection = new ObjectPlacerPlacementSettingsSection();

			root.Add(infoBox);
			root.Add(generalSection);
			root.Add(paletteSection);
			root.Add(placementSettingsSection);
		}

		void OnSceneGUI(SceneView sceneView)
		{
			if (generalSection == null || !ObjectPlacerToolState.IsActive)
				return;

			var e = Event.current;
			var controlId = GUIUtility.GetControlID(FocusType.Passive);

			if (e.type == EventType.Layout)
				HandleUtility.AddDefaultControl(controlId);

			var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
			var hasHit = Physics.Raycast(ray, out var hit, RaycastMaxDistance, generalSection.LayerMask);
			var isSlopeValid = hasHit && Vector3.Angle(hit.normal, Vector3.up) <= placementSettingsSection.MaxSlopeAngle;
			var canPlaceHere = hasHit && isSlopeValid && paletteSection.CanPlace;

			if (hasHit)
			{
				Handles.color = canPlaceHere ? PreviewSphereReadyColor : PreviewSphereNoEntryColor;
				var size = HandleUtility.GetHandleSize(hit.point) * 0.15f;
				Handles.SphereHandleCap(0, hit.point, Quaternion.identity, size, EventType.Repaint);
			}

			var eventType = e.GetTypeForControl(controlId);

			if (eventType == EventType.MouseDown && e.button == 0 && !e.alt && !e.control && !e.command)
			{
				GUIUtility.hotControl = controlId;
				isDragScattering = generalSection.DragScatter;

				BeginUndoGroup();

				if (canPlaceHere)
				{
					PlaceEntry(hit);
					lastPlacementPoint = hit.point;
				}

				e.Use();
			}
			else if (eventType == EventType.MouseDrag && e.button == 0 && GUIUtility.hotControl == controlId)
			{
				if (isDragScattering && canPlaceHere && Vector3.Distance(hit.point, lastPlacementPoint) >= generalSection.DragScatterSpacing)
				{
					PlaceEntry(hit);
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

		void BeginUndoGroup()
		{
			Undo.IncrementCurrentGroup();
			Undo.SetCurrentGroupName(PlaceUndoGroupName);
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

			var instance = (GameObject)PrefabUtility.InstantiatePrefab(entry.Prefab);
			Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");

			var position = hit.point;
			if (placementSettingsSection.RandomizePosition)
				position += GetRandomTangentOffset(hit.normal, placementSettingsSection.MaxPositionOffset);

			var rotation = placementSettingsSection.AlignToNormal ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity;
			if (placementSettingsSection.RandomizeRotation)
				rotation *= Quaternion.Euler(0f, Random.Range(-placementSettingsSection.MaxRotationAngle, placementSettingsSection.MaxRotationAngle), 0f);

			instance.transform.SetPositionAndRotation(position, rotation);

			if (placementSettingsSection.RandomizeScale)
			{
				var range = placementSettingsSection.ScaleRange;
				var multiplier = Random.Range(range.x, range.y);
				instance.transform.localScale = entry.Prefab.transform.localScale * multiplier;
			}
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
