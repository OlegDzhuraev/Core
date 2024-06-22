#if UNITY_EDITOR
using UnityEditor.Callbacks;

namespace InsaneOne.Core.Development
{
	public class CoreEditorInit
	{
		[DidReloadScripts]
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