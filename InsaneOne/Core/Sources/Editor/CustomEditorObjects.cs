using UnityEngine;
using UnityEditor;

namespace InsaneOne.Core.Development
{
	/// <summary> Adds new spawns to the Create menu. </summary>
	public static class CustomEditorObjects
	{
		const string pathToUI = "InsaneOne/UI/";

		static Canvas sceneCanvas;

		[MenuItem("GameObject/UI/Named Dropdown", priority = -199)]
		static void SpawnNamedDropdown()
		{
			var namedDropdown = (GameObject)Resources.Load(pathToUI + "NamedDropdown", typeof(GameObject));
			SpawnUIElement(namedDropdown);
		}

		[MenuItem("GameObject/UI/Named Slider", priority = -199)]
		static void SpawnNamedSlider()
		{
			var namedSlider = (GameObject)Resources.Load(pathToUI + "NamedSlider", typeof(GameObject));
			SpawnUIElement(namedSlider);
		}
		
		[MenuItem("GameObject/UI/Floating Panel", priority = -199)]
		static void SpawnFloatingPanel()
		{
			var floatingPanel = (GameObject)Resources.Load(pathToUI + "FloatingPanel", typeof(GameObject));
			SpawnUIElement(floatingPanel);
		}
		
		[MenuItem("GameObject/UI/Tab Control", priority = -199)]
		static void SpawnTabControl()
		{
			var tabControl = (GameObject)Resources.Load(pathToUI + "TabControl", typeof(GameObject));
			SpawnUIElement(tabControl);
		}

		static void SpawnUIElement(GameObject uiObject)
		{
			SetupCanvas();
			
			var spawnedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(uiObject);
			PrefabUtility.UnpackPrefabInstance(spawnedPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			
			spawnedPrefab.transform.SetParent(sceneCanvas.transform);
		}

		static void SetupCanvas()
		{
			if (sceneCanvas)
				return;
			
			sceneCanvas = GameObject.FindObjectOfType<Canvas>();

			if (!sceneCanvas)
				sceneCanvas = new GameObject().AddComponent<Canvas>();
		}
	}
}