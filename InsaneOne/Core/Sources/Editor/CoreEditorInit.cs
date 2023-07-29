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
				if (!AssetDatabase.IsValidFolder("Assets/InsaneOne"))
					AssetDatabase.CreateFolder("Assets", "InsaneOne");
				
				if (!AssetDatabase.IsValidFolder("Assets/InsaneOne/Resources"))
					AssetDatabase.CreateFolder("Assets/InsaneOne", "Resources");
				
				if (!AssetDatabase.IsValidFolder("Assets/InsaneOne/Resources/InsaneOne"))
					AssetDatabase.CreateFolder("Assets/InsaneOne/Resources", "InsaneOne");
				
				AssetDatabase.CreateAsset(newData, "Assets/InsaneOne/Resources/InsaneOne/CoreData.asset");
				
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