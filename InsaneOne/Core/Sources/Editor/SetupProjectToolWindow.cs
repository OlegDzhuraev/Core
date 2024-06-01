using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InsaneOne.Core.Development
{
    public class SetupProjectToolWindow : EditorWindow
    {
        const string OriginPlacePrefName = "Create3DObject.PlaceAtWorldOrigin";
        
        static AddRequest installRequest;
        
        readonly Color checklistBadColor = Color.yellow;
        
        /// <summary> Resources folder name. Should be NOT Resources, any other naming. </summary>
        string contentFolder = "Game";

        int dimension, foldersStyle;
        
        Vector2 scroll;
        
        string selectedNamespace = "Game";
        string companyName = "InsaneOne";

        GUIStyle richText, partitionHeader, blockStyle, bigBlockStyle;

        bool separateUiInClassicStyle;
        
        [MenuItem("Tools/Setup Project Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupProjectToolWindow>(false, "Setup Project Tool", true);
            window.Init();
        }

        void Init()
        {
            minSize = new Vector2(532, 532);
            maxSize = new Vector2(798, 798);
            richText = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true};
            blockStyle = new GUIStyle(EditorStyles.helpBox);
            bigBlockStyle = EditorHelpers.GetBigBlockStyle();
            
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
            GUILayout.BeginVertical(bigBlockStyle);
            
            DrawPartitionHeader("Project folders");
            
            dimension = EditorGUILayout.Popup("Project dimensions", dimension, new[] {"3D", "2D"});
            foldersStyle = EditorGUILayout.Popup("Folders style", foldersStyle, new[] {"Feature-oriented", "Classic"});

            if (foldersStyle == 1)
            {
                contentFolder = EditorGUILayout.TextField("Content folder name", contentFolder);
                separateUiInClassicStyle = EditorGUILayout.Toggle("Separate UI folder", separateUiInClassicStyle);
            }

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
            var coreData = CoreData.Load();
            
            var prevGUIEnabled = GUI.enabled;
            GUI.enabled = installRequest == null;
            
            GUILayout.BeginVertical(bigBlockStyle);
            DrawPartitionHeader("Packages and Assets");
            
            DrawAddModule("Frequently used modules - add/update", coreData.GitPackages);
            DrawAddModule("Frequently used packages - add/update", coreData.Packages);
            DrawAddModule("Frequently used assets - open in browser", coreData.AssetLinks, true);
            GUILayout.EndVertical();
            
            GUI.enabled = prevGUIEnabled;
        }

        void DrawAddModule(string blockTitle, List<LinkHolder> links, bool openUrl = false)
        { 
            GUILayout.Space(5);
            
            GUILayout.BeginVertical(blockStyle);
            DrawHeader(blockTitle, false);
            GUILayout.BeginHorizontal();
            
            for (var q = 0; q < links.Count; q++)
            {
                if (q % 4 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                
                var assetOrPackage = links[q];
                
                if (GUILayout.Button(assetOrPackage.Name))
                {
                    if (openUrl)
                        OpenUrl(assetOrPackage.Link);
                    else
                        AddPackage(assetOrPackage.Link);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        void OpenUrl(string url) => Process.Start(url);
        
        void DrawChecklist()
        {
            GUILayout.BeginVertical(bigBlockStyle);
            DrawPartitionHeader("Setup Checklist");
            
            DrawHeader("Graphics settings", false);
            
            // todo unify things like this to remove a lot of duplicated code
            DrawFixColorSpace();
            DrawFixSpritePacker();
            
            DrawHeader("Other settings");
            
            DrawFixNamespace();
            DrawFixCompanyName();
            DrawFixCreateAtOrigin();
            
            GUILayout.EndVertical();
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
            var isFine = EditorSettings.spritePackerMode is SpritePackerMode.SpriteAtlasV2;
            var buttonText = isFine ? "Disable" : "Enable";
            var previousGuiColor = GUI.color;
            var neededResult = isFine ? SpritePackerMode.Disabled : SpritePackerMode.SpriteAtlasV2;
            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            
            GUILayout.BeginVertical(blockStyle);

            GUILayout.Label(!isFine
                ? "Sprite packer V2 is disabled. Preffered is Enabled"
                : "Sprite packer V2 is enabled. It is preffered. But you can disable.");

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
            var isFine = EditorPrefs.GetBool(OriginPlacePrefName);
            var previousGuiColor = GUI.color;
            
            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            
            GUILayout.BeginVertical(blockStyle);
            GUILayout.Label(!isFine
                    ? $"Create objects at origin is <b>disabled</b>. It is recommended to <b>enable</b>."
                    : $"Create objects at origin is <b>enabled</b>.",
                richText);
            
            if (!isFine && GUILayout.Button("Fix"))
                EditorPrefs.SetBool(OriginPlacePrefName, true);
            
            GUILayout.EndVertical();
            
            GUI.color = previousGuiColor;
        }
        
        
        void DrawPartitionHeader(string text)
        {
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
            var contentPath = $"Assets/{contentFolder}";

            var foldersToCreate = new List<FolderData>
            {
                new ("Assets", "Resources"),
                new ("Assets/Resources", "Data"),
                new ("Assets", contentFolder),
                new (contentPath, "Sounds"),
                new (contentPath, "Materials"),
                new (contentPath, "Animations"),
                new (contentPath, "Scenes"),
                new (contentPath, "Prefabs"),
                new ($"{contentPath}/Prefabs", "Effects"),
                new ($"{contentPath}/Prefabs", "Environment"),
            
                new (contentPath, "Sources"),
                new ($"{contentPath}/Sources", "Editor"),
                new ($"{contentPath}/Sources", "Storing"),
            };

            if (separateUiInClassicStyle)
            {
                var uiPath = "Assets/UI";
                foldersToCreate.AddRange(new List<FolderData>()
                {
                    new($"Assets", "UI"),
                    new(uiPath, "Sources"),
                    new(uiPath, "Prefabs"),
                    new(uiPath, "Sprites"),
                    new(uiPath, "Fonts"),
                    new(uiPath, "Animations"),
                });
            }
            else
            {
                foldersToCreate.AddRange(new List<FolderData>()
                {
                    new (contentPath, "UI"),
                    new ($"{contentPath}/UI", "Fonts"),
                    new ($"{contentPath}/Sources", "UI"),
                    new ($"{contentPath}/Animations", "UI"),
                    new ($"{contentPath}/Prefabs", "UITemplates"),
                });
            }

            foreach (var folderData in foldersToCreate)
                CreateFolderIfNotExist(folderData.Path, folderData.FolderName);

            if (is3D)
            {
                CreateFolderIfNotExist(contentPath, "Models");
                CreateFolderIfNotExist(contentPath, "Textures");
            }
            else
            {
                CreateFolderIfNotExist(contentPath, "Sprites");
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