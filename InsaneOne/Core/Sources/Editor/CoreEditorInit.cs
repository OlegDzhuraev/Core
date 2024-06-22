#if UNITY_EDITOR
using UnityEditor;

namespace InsaneOne.Core.Development
{
	public class CoreEditorInit
	{
		[InitializeOnLoadMethod]
		static void OnBeforeSceneLoadRuntimeMethod()
		{
			if (!CoreData.TryLoad(out _))
				CoreInitEditorWindow.ShowWindow();
			else
				CoreData.Log("Initialized.");
		}
	}
}
#endif