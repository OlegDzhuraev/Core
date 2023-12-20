#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Development
{
	public class CoreEditorInit
	{
		[InitializeOnLoadMethod]
		static void OnBeforeSceneLoadRuntimeMethod()
		{
			if (!Resources.Load<CoreData>("InsaneOne/CoreData"))
			{
				var newData = ScriptableObject.CreateInstance<CoreData>();
				if (!AssetDatabase.IsValidFolder("Assets/Resources"))
					AssetDatabase.CreateFolder("Assets", "Resources");

				if (!AssetDatabase.IsValidFolder("Assets/Resources/InsaneOne"))
					AssetDatabase.CreateFolder("Assets/Resources", "InsaneOne"); 
				
				AssetDatabase.CreateAsset(newData, "Assets/Resources/InsaneOne/CoreData.asset");
				
				Log("<b>InsaneOne.Core:</b> new CoreData was <b><color=\"#55ff33\">created</color></b>.");
			}
			
			Log("<b>InsaneOne.Core:</b> initialized.");
		}

		static void Log(string text)
		{
			var coreData = Resources.Load<CoreData>("InsaneOne/CoreData");

			if (coreData && coreData.SuppressLogs)
				return;
			
			Debug.Log(text);
		}
	}
}
#endif