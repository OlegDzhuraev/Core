using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace InsaneOne.Core.Development
{
    public static class TemplatesGenerator
    {
        const string assetsPath = "Assets/Create/InsaneOne/Templates/";
        const string coreNamespace = "InsaneOne.Core";
        
        [MenuItem (assetsPath + "SceneInitialization", false, -599)]
        static void CreateSceneInitializationTemplate() => CreateAnyFromTemplate("SceneInitialization");
        
        [MenuItem (assetsPath + "Storage", false, -499)]
        static void CreateStorageTemplate() => CreateAnyFromTemplate("Storage");

        [MenuItem (assetsPath + "Health Behaviour", false, -399)]
        static void CreateHealthBehaviourTemplate() => CreateAnyFromTemplate("HealthBehaviour");
        
        [MenuItem (assetsPath + "Damageable Behaviour", false, -399)]
        static void CreateDamageableTemplate() => CreateAnyFromTemplate("Damageable");
        
        [MenuItem (assetsPath + "Projectile Behaviour", false, -399)]
        static void CreateProjectileBehaviourTemplate() => CreateAnyFromTemplate("Projectile");
        
        [MenuItem (assetsPath + "Player Move Input Behaviour", false, -399)]
        static void CreateMoveInputTemplate() => CreateAnyFromTemplate("MoveInput");
        
        [MenuItem (assetsPath + "Clickable Icon", false, -199)]
        static void CreateClickableIconTemplate() => CreateAnyFromTemplate("ClickableIcon");
        
        [MenuItem (assetsPath + "State Machine", false, -99)]
        static void CreateStateMachineTemplate() => CreateAnyFromTemplate("StateMachine");

        static void CreateAnyFromTemplate(string templateName)
        {
            var fileName = $"{GetSelectionPath()}/{templateName}.cs";
            CreateAndRenameAsset(fileName, GetIcon(), name => CreateTemplateInternal(GetTemplateContent(templateName), name));
        }

        static string CreateFromTemplate(string template, string fileName) 
        {
            if (string.IsNullOrEmpty(fileName)) 
                return "Invalid filename";

            template = $"using {coreNamespace};\n" + template;
            
            var namespaceName = EditorSettings.projectGenerationRootNamespace.Trim();
            
            if (string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace)) 
                namespaceName = "InsaneOne";
            
            template = template.Replace("#NAMESPACE#", namespaceName);
            template = template.Replace("#SCRIPTNAME#", SanitizeClassName(Path.GetFileNameWithoutExtension(fileName)));
            
            try 
            {
                File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(fileName), template);
            } 
            catch (Exception ex) 
            {
                return ex.Message;
            }
            
            AssetDatabase.Refresh();
            
            return null;
        }
        
        static string SanitizeClassName(string className) 
        {
            var sb = new StringBuilder();
            var needUp = true;
            
            foreach (var c in className) 
            {
                if (char.IsLetterOrDigit(c)) 
                {
                    sb.Append (needUp ? char.ToUpperInvariant(c) : c);
                    needUp = false;
                } 
                else 
                {
                    needUp = true;
                }
            }
            
            return sb.ToString();
        }
        
        static string GetSelectionPath() 
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            
            if (!string.IsNullOrEmpty(path) && AssetDatabase.Contains(Selection.activeObject)) 
            {
                if (!AssetDatabase.IsValidFolder(path))
                    path = Path.GetDirectoryName(path);
            } 
            else 
            {
                path = "Assets";
            }
            
            return path;
        }
        
        static string CreateTemplateInternal(string template, string fileName) 
        {
            var result = CreateFromTemplate(template, fileName);
            if (result != null) 
                EditorUtility.DisplayDialog("InsaneOne", result, "Close");
            
            return result;
        }

        static Texture2D GetIcon() => EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;

        static void CreateAndRenameAsset(string fileName, Texture2D icon, Action<string> onSuccess) 
        {
            var action = ScriptableObject.CreateInstance<CustomEndNameAction>();
            action.Callback = onSuccess;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, fileName, icon, null);
        }
        
        static string GetTemplateContent(string templateName)
        {
            var templates = new List<TextAsset>();
            EditorHelpers.LoadAssetsToList(templates, "l: InsaneTemplate");

            var template = templates.Find(t => t && t.name.Contains(templateName));

            return template ? template.text : "No template found";
        }

        sealed class CustomEndNameAction : EndNameEditAction 
        {
            [NonSerialized] public Action<string> Callback;

            public override void Action(int instanceId, string pathName, string resourceFile) 
            {
                Callback?.Invoke(pathName);
            }
        }
    }
}