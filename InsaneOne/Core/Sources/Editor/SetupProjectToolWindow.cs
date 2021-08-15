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
        
        readonly Dictionary<string, string> gitPackages = new Dictionary<string, string>
        {
            { "Signals", $"https://github.com/{repoName}/Signals.git" },
            { "Tags", $"https://github.com/{repoName}/Tags.git" },
            { "Perseids Pooling", $"https://github.com/{repoName}/PerseidsPooling.git" },
        };
        
        readonly Dictionary<string, string> unityPackages = new Dictionary<string, string>
        {
            { "Cinemachine", "com.unity.cinemachine" },
            { "Post Effects", "com.unity.postprocessing" },
        };

        Vector2 scroll;
        
        string selectedNamespace = "Game";
        string companyName = "InsaneOne";
        
        [MenuItem("Tools/Setup Project Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupProjectToolWindow>(false, "Setup Project Tool", true);
            window.minSize = new Vector2(512, 512);
            window.maxSize = new Vector2(768, 512);
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
                GUILayout.Label("Wrong naming for content folder. Do not use Resources name etc.");
                GUI.enabled = false;
            }

            if (GUILayout.Button("Generate project folders"))
            {
                if (foldersStyle == 0)
                    GenerateProjectFoldersFeatures(dimension == 0);
                else
                    GenereteProjectFolders(dimension == 0);
            }

            DrawHeader("Frequently used modules - add/update");
            
            var prevGUIEnabled = GUI.enabled;
            GUI.enabled = installRequest == null;
            
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
            
            DrawHeader("Graphics settings");

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
                GUILayout.Label("Color space set to GAMMA. For PC Preferred is LINEAR.");

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
            if (EditorSettings.projectGenerationRootNamespace == String.Empty)
            {
                GUILayout.Label("Project root namespace isn't set. Fix?");

                selectedNamespace = EditorGUILayout.TextField("Namespace", selectedNamespace);

                if (GUILayout.Button("Set"))
                    EditorSettings.projectGenerationRootNamespace = selectedNamespace;
            }
            else
            {
                GUILayout.Label("Project root namespace is set. It is preffered.");
            }
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

        void DrawHeader(string text)
        {
            EditorGUILayout.Space(15);
            GUILayout.Label(text, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }
        
        void GenerateProjectFoldersFeatures(bool is3D)
        {
            var featuresFolderName = "Features";
            var featuresFolderPath = $"Assets/{featuresFolderName}/";
            var sharedFolderPath = $"{featuresFolderPath}/Shared";
            
            List<FolderData> foldersToCreate = new List<FolderData>
            {
                new FolderData("Assets", featuresFolderName),
                new FolderData(featuresFolderPath, "Shared"),
                new FolderData(sharedFolderPath, "Prefabs"),
                new FolderData(sharedFolderPath, "Sounds"),
                new FolderData(sharedFolderPath, "Materials"),
                new FolderData(sharedFolderPath, "Animations"),
                new FolderData(sharedFolderPath, "UI"),
                new FolderData($"{sharedFolderPath}/UI", "Fonts"),
                new FolderData($"{sharedFolderPath}/UI", "Sprites"),
                new FolderData($"{sharedFolderPath}/UI", "Animations"),
                new FolderData($"{sharedFolderPath}/UI", "Templates"),
                new FolderData(sharedFolderPath, "Scenes"),
                new FolderData(sharedFolderPath, "Sources"),
                new FolderData(sharedFolderPath, "Resources"),
                new FolderData($"{sharedFolderPath}/Resources", "Data")
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
            var resourceFolderPath = "Assets/" + contentFolder;
            
            List<FolderData> foldersToCreate = new List<FolderData>
            {
                new FolderData("Assets", "Resources"),
                new FolderData("Assets/Resources", "Data"),
                new FolderData("Assets", contentFolder),
                new FolderData(resourceFolderPath, "Sounds"),
                new FolderData(resourceFolderPath, "Materials"),
                new FolderData(resourceFolderPath, "Animations"),
                new FolderData($"{resourceFolderPath}/Animations", "UI"),
                new FolderData(resourceFolderPath, "UI"),
                new FolderData($"{resourceFolderPath}/UI", "Fonts"),
                new FolderData(resourceFolderPath, "Scenes"),
                new FolderData(resourceFolderPath, "Prefabs"),
                new FolderData($"{resourceFolderPath}/Prefabs", "Effects"),
                new FolderData($"{resourceFolderPath}/Prefabs", "Environment"),
                new FolderData($"{resourceFolderPath}/Prefabs", "UITemplates"),
                new FolderData(resourceFolderPath, "Sources"),
                new FolderData($"{resourceFolderPath}/Sources", "UI"),
                new FolderData($"{resourceFolderPath}/Sources", "Editor"),
                new FolderData($"{resourceFolderPath}/Sources", "Storing"),
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
        
        class FolderData
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