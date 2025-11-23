using UnityEngine;
using UnityEditor;

namespace InsaneOne.Core.Development
{
	/// <summary> Adds new spawns to the Create menu. </summary>
	public static class CustomEditorObjects
	{
		const string PathToUI = "InsaneOne/UI/";
		const string MenuPath = "GameObject/UI/";

		static Canvas sceneCanvas;

		[MenuItem(MenuPath + "Floating Panel", priority = -199)]
		static void SpawnFloatingPanel() => SpawnUIElement("FloatingPanel");

		[MenuItem(MenuPath + "Tab Control", priority = -199)]
		static void SpawnTabControl() => SpawnUIElement("TabControl");
		
		[MenuItem(MenuPath + "Popup Window", priority = -199)]
		static void SpawnPopupControl() => SpawnUIElement("PopupWindowTpl");
		
		[MenuItem(MenuPath + "Fader", priority = -199)]
		static void SpawnFaderControl() => SpawnUIElement("FaderTpl");

		static void SpawnUIElement(string elemName)
		{
			var prefab = (GameObject)Resources.Load(PathToUI + elemName, typeof(GameObject));
			
			SetupCanvas();
			
			var spawnedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			PrefabUtility.UnpackPrefabInstance(spawnedPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			
			spawnedPrefab.transform.SetParent(sceneCanvas.transform);
		}

		static void SetupCanvas()
		{
			if (sceneCanvas)
				return;
			
			sceneCanvas = Object.FindFirstObjectByType<Canvas>();

			if (!sceneCanvas)
				sceneCanvas = new GameObject("Canvas").AddComponent<Canvas>();
		}
	}
}