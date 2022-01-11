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
        
        /// <summary> Resources folder name. Should be NOT Resources, any other naming. </summary>
        string contentFolder = "Resource";

        static AddRequest installRequest;
        
        int dimension;
        int foldersStyle;
        
        readonly Dictionary<string, string> gitPackages = new()
        {
            //{ "Signals", $"https://github.com/{repoName}/Signals.git" }, // not finished
            //{ "Tags", $"https://github.com/{repoName}/Tags.git" },
            { "Perseids Pooling", $"https://github.com/{repoName}/PerseidsPooling.git" },
        };
        
        readonly Dictionary<string, string> unityPackages = new ()
        {
            { "Cinemachine", "com.unity.cinemachine" },
            { "Post Effects", "com.unity.postprocessing" },
            //{ "DOTween", "https://github.com/Demigiant/dotween.git" }, // can't be added - no package manifest included
        };

        Vector2 scroll;
        
        string selectedNamespace = "Game";
        string companyName = "InsaneOne";

        GUIStyle richText, partitionHeader;
        
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
            
            DrawHeader("Project folders");
            
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
            
            var prevGUIEnabled = GUI.enabled;
            GUI.enabled = installRequest == null;
            
            DrawHeader("Frequently used modules - add/update");
            
            GUILayout.BeginHorizontal();
            
            foreach (var package in gitPackages)
                if (GUILayout.Button(package.Key))
                    AddPackage(package.Value);

            GUILayout.EndHorizontal();

            DrawHeader("Frequently used assets - add/update");
            
            GUILayout.BeginHorizontal();
            
            foreach (var package in unityPackages)
                if (GUILayout.Button(package.Key))
                    AddPackage(package.Value);

            GUILayout.EndHorizontal();
            
            /* idk now how to load and install assets - no such api in documentation
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Odin Inspector"))
                Debug.Log("Not implemented");
             
            if (GUILayout.Button("ME Coroutines"))
                Debug.Log("Not implemented");
            
            if (GUILayout.Button("Rainbow Hierarchy"))
                Debug.Log("Not implemented");
            
            GUILayout.EndHorizontal();
            */
            GUI.enabled = prevGUIEnabled;
            
            DrawPartitionHeader("Setup Checklist");
            DrawHeader("Graphics settings", false);

            DrawFixColorSpace();
            DrawFixSpritePacker();

            DrawHeader("Other settings");
            
            DrawFixNamespace();
            
            // todo change scene view - create at origin value using Editor Prefs

            DrawFixCompanyName();
            
            GUILayout.EndScrollView();
        }

        void DrawFixColorSpace()
        {
            if (PlayerSettings.colorSpace == ColorSpace.Gamma)
            {
                GUILayout.Label("Color space set to <b>Gamma</b>. For PC preferred is <b>Linear</b>.", richText);

                if (GUILayout.Button("Set to Linear"))
                    PlayerSettings.colorSpace = ColorSpace.Linear;
            }
            else
            {
                GUILayout.Label("Color space set to Linear. It is preffered. But you can reset to Gamma.");
                
                
                if (GUILayout.Button("Set to Gamma"))
                    PlayerSettings.colorSpace = ColorSpace.Gamma;
            }
        }

        void DrawFixSpritePacker()
        {
            if (EditorSettings.spritePackerMode == SpritePackerMode.Disabled)
            {
                GUILayout.Label("Sprite packer is disabled. Preffered is Enabled");

                if (GUILayout.Button("Enable"))
                    EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            }
            else
            {
                GUILayout.Label("Sprite packer is enabled. It is preffered. But you can disable.");
                
                if (GUILayout.Button("Disable"))
                    EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
            }
        }

        void DrawFixNamespace()
        {
            var actualNamespace = EditorSettings.projectGenerationRootNamespace;
            if (actualNamespace == String.Empty)
                GUILayout.Label("Project root namespace isn't set. Fix?");
            else
                GUILayout.Label($"Project root namespace is already set to {actualNamespace}.");
            
            selectedNamespace = EditorGUILayout.TextField("Namespace", selectedNamespace);

            if (GUILayout.Button("Set"))
                EditorSettings.projectGenerationRootNamespace = selectedNamespace;
        }

        void DrawFixCompanyName()
        {
            if (PlayerSettings.companyName == "DefaultCompany")
            {
                GUILayout.Label($"Developer Company is not set. Set to value below ({companyName})?");
                companyName = GUILayout.TextField(companyName);
                
                if (GUILayout.Button("Ok"))
                    PlayerSettings.companyName = companyName;
            }
            else
            {
                GUILayout.Label("Company name is set to " + PlayerSettings.companyName);
            }
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
                    Debug.Log("Installed: " + installRequest.Result.packageId);
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