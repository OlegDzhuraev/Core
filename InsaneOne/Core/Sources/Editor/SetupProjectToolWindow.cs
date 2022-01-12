using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace InsaneOne.Core.Development
{
    public class SetupProjectToolWindow : EditorWindow
    {
        const string repoName = "OlegDzhuraev";
        const string originPlacePrefName = "Create3DObject.PlaceAtWorldOrigin";
        
        static AddRequest installRequest;
        
        readonly Color checklistBadColor = Color.yellow;
        
        /// <summary> Resources folder name. Should be NOT Resources, any other naming. </summary>
        string contentFolder = "Resource";

        int dimension, foldersStyle;

        readonly Dictionary<string, string> gitPackages = new()
        {
            { "Perseids Pooling", $"https://github.com/{repoName}/PerseidsPooling.git" },
        };
        
        readonly Dictionary<string, string> unityPackages = new ()
        {
            { "Cinemachine", "com.unity.cinemachine" },
            { "Post Effects", "com.unity.postprocessing" },
            { "Recorder", "com.unity.recorder" },
        };

        Vector2 scroll;
        
        string selectedNamespace = "Game";
        string companyName = "InsaneOne";

        GUIStyle richText, partitionHeader, blockStyle;
        
        [MenuItem("Tools/Setup Project Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupProjectToolWindow>(false, "Setup Project Tool", true);
            window.Init();
        }

        void Init()
        {
            minSize = new Vector2(512, 512);
            maxSize = new Vector2(768, 768);
            richText = new GUIStyle(EditorStyles.label) { richText = true };
            blockStyle = new GUIStyle(EditorStyles.helpBox);
            
            partitionHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16, 
                richText = true, 
                alignment = TextAnchor.UpperCenter
            };
        }

        void OnGUI()
        {
            scroll = GUILayout.BeginScrollView(scroll);

            DrawGenerateFolders();
            DrawModulesInstall();
            DrawChecklist();
            
            GUILayout.EndScrollView();
        }

        void DrawGenerateFolders()
        {
            GUILayout.BeginVertical(blockStyle);
            
            DrawHeader("Project folders", false);
            
            dimension = EditorGUILayout.Popup("Project dimensions", dimension, new[] {"3D", "2D"});
            foldersStyle = EditorGUILayout.Popup("Folders style", foldersStyle, new[] {"Feature-oriented", "Classic"});
            
            if (foldersStyle == 1)
                contentFolder = EditorGUILayout.TextField("Content folder name", contentFolder);

            if (contentFolder == String.Empty || contentFolder == "Resources")
            {
                GUILayout.Label("Wrong naming for content folder. Do not recommended to use \"Resources\" name.");
                GUI.enabled = false;
            }

            if (GUILayout.Button("Generate project folders"))
            {
                if (foldersStyle == 0)
                    GenerateProjectFoldersFeatures(dimension == 0);
                else
                    GenereteProjectFolders(dimension == 0);
            }    
            
            GUILayout.EndVertical();
        }

        void DrawModulesInstall()
        {
            var prevGUIEnabled = GUI.enabled;
            GUI.enabled = installRequest == null;
            
            GUILayout.BeginVertical(blockStyle);
            
            DrawHeader("Frequently used modules - add/update", false);
            
            GUILayout.BeginHorizontal();
            
            foreach (var package in gitPackages)
                if (GUILayout.Button(package.Key))
                    AddPackage(package.Value);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical(blockStyle);
            DrawHeader("Frequently used assets - add/update", false);
            
            GUILayout.BeginHorizontal();
            
            foreach (var package in unityPackages)
                if (GUILayout.Button(package.Key))
                    AddPackage(package.Value);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            /* idk now how to load and install assets - no such api in documentation
            "DOTween", "Odin Inspector", "ME Coroutines", "Rainbow Hierarchy"
            */
            
            GUI.enabled = prevGUIEnabled;
        }
        
        void DrawChecklist()
        {
            DrawPartitionHeader("Setup Checklist");
            
            DrawHeader("Graphics settings", false);
            
            // todo unify things like this to remove a lot of duplicated code
            DrawFixColorSpace();
            DrawFixSpritePacker();
            
            DrawHeader("Other settings");
            
            DrawFixNamespace();
            DrawFixCompanyName();
            DrawFixCreateAtOrigin();
        }

        void DrawFixColorSpace()
        {
            var isFine = PlayerSettings.colorSpace == ColorSpace.Linear;

            var buttonText = isFine ? "Set to Gamma" : "Set to Linear";
            var neededResult = isFine ? ColorSpace.Gamma : ColorSpace.Linear;
            var previousGuiColor = GUI.color;
            
            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            
            GUILayout.BeginVertical(blockStyle);

            GUILayout.Label(isFine
                ? "Color space set to <b>Linear</b>. It is preffered. But you can reset to <b>Gamma</b>."
                : "Color space set to <b>Gamma</b>. For PC preferred is <b>Linear</b>.",
                richText);

            if (GUILayout.Button(buttonText))
                PlayerSettings.colorSpace = neededResult;
            
            GUILayout.EndVertical();

            GUI.color = previousGuiColor;
        }
        
        void DrawFixSpritePacker()
        {
            var isFine = EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOnAtlas;
            var buttonText = isFine ? "Disable" : "Enable";
            var previousGuiColor = GUI.color;
            var neededResult = isFine ? SpritePackerMode.Disabled : SpritePackerMode.AlwaysOnAtlas;
            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            
            GUILayout.BeginVertical(blockStyle);

            GUILayout.Label(!isFine
                ? "Sprite packer is disabled. Preffered is Enabled"
                : "Sprite packer is enabled. It is preffered. But you can disable.");

            if (GUILayout.Button(buttonText))
                EditorSettings.spritePackerMode = neededResult;
            
            GUILayout.EndVertical();
            
            GUI.color = previousGuiColor;
        }

        void DrawFixNamespace()
        {
            var actualNamespace = EditorSettings.projectGenerationRootNamespace;
            var isFine = actualNamespace != String.Empty;
            var previousGuiColor = GUI.color;
            
            GUI.color = isFine ? previousGuiColor : checklistBadColor;

            GUILayout.BeginVertical(blockStyle);

            GUILayout.Label(isFine
                ? $"Project root namespace is set. Current namespace is <b>{actualNamespace}</b>."
                : "Project root namespace isn't set. Fix?",
                richText);

            selectedNamespace = EditorGUILayout.TextField("Namespace", selectedNamespace);

            if (GUILayout.Button("Set"))
                EditorSettings.projectGenerationRootNamespace = selectedNamespace;
            
            GUILayout.EndVertical();
            
            GUI.color = previousGuiColor;
        }

        void DrawFixCompanyName()
        {
            var isFine = PlayerSettings.companyName != "DefaultCompany" && PlayerSettings.companyName != String.Empty;
            var previousGuiColor = GUI.color;
            
            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            
            GUILayout.BeginVertical(blockStyle);

            GUILayout.Label(!isFine
                ? $"Developer Company is not set. Set to value below ({companyName})?"
                : $"Company name is set. Current company name is <b>{PlayerSettings.companyName}</b>",
                richText);

            companyName = GUILayout.TextField(companyName);
                
            if (GUILayout.Button("Set"))
                PlayerSettings.companyName = companyName;
            
            GUILayout.EndVertical();
            
            GUI.color = previousGuiColor;
        }

        void DrawFixCreateAtOrigin()
        {  
            var isFine = EditorPrefs.GetBool(originPlacePrefName);
            var previousGuiColor = GUI.color;
            
            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            
            GUILayout.BeginVertical(blockStyle);
            GUILayout.Label(!isFine
                    ? $"Create objects at origin is <b>disabled</b>. It is recommended to <b>enable</b>."
                    : $"Create objects at origin is <b>enabled</b>.",
                richText);
            
            if (!isFine && GUILayout.Button("Fix"))
                EditorPrefs.SetBool(originPlacePrefName, true);
            
            GUILayout.EndVertical();
            
            GUI.color = previousGuiColor;
        }
        
        
        void DrawPartitionHeader(string text)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label(text, partitionHeader);
            EditorGUILayout.Space(5);
        }
        
        void DrawHeader(string text, bool addUpSpace = true)
        {
            if (addUpSpace)
                EditorGUILayout.Space(15);
            
            GUILayout.Label(text, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }
        
        void GenerateProjectFoldersFeatures(bool is3D)
        {
            var featuresFolderName = "Features";
            var featuresFolderPath = $"Assets/{featuresFolderName}";
            var sharedFolderPath = $"{featuresFolderPath}/Shared";
            
            var foldersToCreate = new List<FolderData>
            {
                new ("Assets", featuresFolderName),
                new (featuresFolderPath, "ExampleFeature"),
                new ($"{featuresFolderPath}/ExampleFeature", "Prefabs"),
                new ($"{featuresFolderPath}/ExampleFeature", "Sources"),
                new (featuresFolderPath, "Shared"),
                new (sharedFolderPath, "Prefabs"),
                new (sharedFolderPath, "Sounds"),
                new (sharedFolderPath, "Materials"),
                new (sharedFolderPath, "Animations"),
                new (sharedFolderPath, "UI"),
                new ($"{sharedFolderPath}/UI", "Fonts"),
                new ($"{sharedFolderPath}/UI", "Sprites"),
                new ($"{sharedFolderPath}/UI", "Animations"),
                new ($"{sharedFolderPath}/UI", "Templates"),
                new (sharedFolderPath, "Scenes"),
                new (sharedFolderPath, "Sources"),
                new (sharedFolderPath, "Resources"),
                new ($"{sharedFolderPath}/Resources", "Data")
            };
            
            foreach (var folderData in foldersToCreate)
                CreateFolderIfNotExist(folderData.Path, folderData.FolderName);

            if (is3D)
            {
                CreateFolderIfNotExist(sharedFolderPath, "Models");
                CreateFolderIfNotExist(sharedFolderPath, "Textures");
            }
            else
            {
                CreateFolderIfNotExist(sharedFolderPath, "Sprites");
            }
        }
        
        void GenereteProjectFolders(bool is3D)
        {
            var resourceFolderPath = $"Assets/{contentFolder}";
            
            var foldersToCreate = new List<FolderData>
            {
                new ("Assets", "Resources"),
                new ("Assets/Resources", "Data"),
                new ("Assets", contentFolder),
                new (resourceFolderPath, "Sounds"),
                new (resourceFolderPath, "Materials"),
                new (resourceFolderPath, "Animations"),
                new ($"{resourceFolderPath}/Animations", "UI"),
                new (resourceFolderPath, "UI"),
                new ($"{resourceFolderPath}/UI", "Fonts"),
                new (resourceFolderPath, "Scenes"),
                new (resourceFolderPath, "Prefabs"),
                new ($"{resourceFolderPath}/Prefabs", "Effects"),
                new ($"{resourceFolderPath}/Prefabs", "Environment"),
                new ($"{resourceFolderPath}/Prefabs", "UITemplates"),
                new (resourceFolderPath, "Sources"),
                new ($"{resourceFolderPath}/Sources", "UI"),
                new ($"{resourceFolderPath}/Sources", "Editor"),
                new ($"{resourceFolderPath}/Sources", "Storing"),
            };
            
            foreach (var folderData in foldersToCreate)
                CreateFolderIfNotExist(folderData.Path, folderData.FolderName);

            if (is3D)
            {
                CreateFolderIfNotExist(resourceFolderPath, "Models");
                CreateFolderIfNotExist(resourceFolderPath, "Textures");
            }
            else
            {
                CreateFolderIfNotExist(resourceFolderPath, "Sprites");
            }
        }

        void CreateFolderIfNotExist(string path, string folderName)
        {
            if (!AssetDatabase.IsValidFolder(path + "/" + folderName))
                AssetDatabase.CreateFolder(path, folderName);
        }

        void AddPackage(string nameOrUrl)
        {
            if (installRequest != null)
                return;
            
            installRequest = Client.Add(nameOrUrl);
            EditorApplication.update += InstallProgress;
    
            Debug.Log("Started installing " + nameOrUrl);
        }

        static void InstallProgress()
        {
            if (installRequest.IsCompleted)
            {
                if (installRequest.Status == StatusCode.Success)
                    Debug.Log($"Installed: {installRequest.Result.packageId}");
                else if (installRequest.Status >= StatusCode.Failure)
                    Debug.Log(installRequest.Error.message);

                EditorApplication.update -= InstallProgress;
                installRequest = null;
            }
        }

        readonly struct FolderData
        {
            public readonly string Path;
            public readonly string FolderName;

            public FolderData(string path, string folderName)
            {
                Path = path;
                FolderName = folderName;
            }
        }
    }
}