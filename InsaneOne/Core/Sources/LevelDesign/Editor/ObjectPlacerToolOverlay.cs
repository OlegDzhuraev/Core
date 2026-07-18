using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Scene view toolbar overlay hosting the Object Placer activation toggle, same as Unity's own Scene view overlays. </summary>
	[Overlay(typeof(SceneView), Id, "Object Placer", defaultDisplay: true)]
	public class ObjectPlacerToolOverlay : ToolbarOverlay
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay";

		ObjectPlacerToolOverlay() : base(ObjectPlacerActiveToggle.Id) { }
	}

	[EditorToolbarElement(Id, typeof(SceneView))]
	class ObjectPlacerActiveToggle : EditorToolbarToggle
	{
		public const string Id = "InsaneOne.LevelDesign.ObjectPlacerOverlay.ActiveToggle";

		public ObjectPlacerActiveToggle()
		{
			text = "Object Placer";
			tooltip = "Toggles the Object Placer tool. While active, clicking a scene collider places the selected palette prefab.";

			SetValueWithoutNotify(ObjectPlacerToolState.IsActive);
			this.RegisterValueChangedCallback(OnToggleChanged);

			ObjectPlacerToolState.ActiveChanged += OnStateActiveChanged;
			RegisterCallback<DetachFromPanelEvent>(_ => ObjectPlacerToolState.ActiveChanged -= OnStateActiveChanged);
		}

		void OnToggleChanged(ChangeEvent<bool> ev) => ObjectPlacerToolState.SetActive(ev.newValue);
		void OnStateActiveChanged(bool value) => SetValueWithoutNotify(value);
	}
}
