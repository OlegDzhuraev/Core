#if UNITY_EDITOR
using UnityEditor.Callbacks;

namespace InsaneOne.Core.Development
{
	public class CoreEditorInit
	{
		[DidReloadScripts]
		static void Initialize()
		{
			if (!CoreData.TryLoad(out _))
				CoreInitEditorWindow.ShowWindow();
		}
	}
}
#endif