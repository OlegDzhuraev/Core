#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

namespace InsaneOne.Core.Development
{
	public static class CoreEditorInit
	{
		[DidReloadScripts]
		static void Initialize()
		{
			if (!CoreData.TryLoad(out _) && !SessionState.GetBool(CoreInitEditorWindow.DismissedSessionKey, false))
				EditorApplication.delayCall += CoreInitEditorWindow.ShowWindow;
		}
	}
}
#endif