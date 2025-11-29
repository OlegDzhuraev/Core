using System;
using System.Collections.Generic;
using System.Diagnostics;
using InsaneOne.Core.Utility;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using LogLevel = InsaneOne.Core.Utility.LogLevel;

namespace InsaneOne.Core.Development
{
    public sealed class SetupProjectToolWindow : EditorWindow
    {
        const string OriginPlacePrefName = "Create3DObject.PlaceAtWorldOrigin";

        static readonly string[] foldersDisplayOptions = {"Classic", "Classic ECS", "Feature-oriented"};
        static readonly string[] dimensionDisplayOptions = {"3D", "2D"};

        static AddRequest installRequest;
        
        readonly Color checklistBadColor = Color.yellow;
        
        /// <summary> Resources folder name. Should be NOT Resources, any other naming. </summary>
        string contentFolder = "Game";

        int dimension;
        FoldersGenerationStyle foldersStyle;
        
        Vector2 scroll;
        
        string selectedNamespace = "Game";
        string companyName = "InsaneOne";

        GUIStyle richText, partitionHeader, blockStyle, bigBlockStyle;

        bool separateUiInClassicStyle;
        
        [MenuItem("Tools/InsaneOne/Setup Project Tool...")]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupProjectToolWindow>(false, "Setup Project Tool", true);
            window.Init();
        }

        [MenuItem("Tools/InsaneOne/Update")]
        public static void UpdatePlugin()
        {
            AddPackage($"https://github.com/{CoreData.RepoName}/Core.git");
        }

        void Init()
        {
            minSize = new Vector2(532, 532);
            maxSize = new Vector2(798, 798);
            richText = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
            blockStyle = new GUIStyle(EditorStyles.helpBox);
            bigBlockStyle = EditorHelpers.GetBigBlockStyle();
            
            partitionHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16, richText = true, alignment = TextAnchor.UpperCenter,
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
            
            dimension = EditorGUILayout.Popup("Project dimensions", dimension, dimensionDisplayOptions);
            foldersStyle = (FoldersGenerationStyle)EditorGUILayout.Popup("Folders style", (int)foldersStyle, foldersDisplayOptions);

            if (foldersStyle is FoldersGenerationStyle.Classic or FoldersGenerationStyle.ClassicECS)
            {
                contentFolder = EditorGUILayout.TextField("Content folder name", contentFolder);
                separateUiInClassicStyle = EditorGUILayout.Toggle("Separate UI folder", separateUiInClassicStyle);
            }

            if (contentFolder is "" or "Resources")
            {
                GUILayout.Label("Wrong naming for content folder. Do not recommended to use \"Resources\" name.");
                GUI.enabled = false;
            }

            if (GUILayout.Button("Generate project folders"))
            {
                if (foldersStyle == FoldersGenerationStyle.FeatureOriented)
                    GenerateProjectFoldersFeatures(dimension == 0);
                else if (foldersStyle is FoldersGenerationStyle.Classic or FoldersGenerationStyle.ClassicECS)
                    GenereteProjectFolders(dimension == 0, foldersStyle is FoldersGenerationStyle.ClassicECS);
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
            DrawFixColorSpace();
            DrawFixSpritePacker();
            
            DrawHeader("Other settings");
            DrawFixPhysicsTimeStep();
            DrawFixNamespace();
            DrawFixCompanyName();
            DrawFixCreateAtOrigin();
            DrawFixReloadDomain();
            DrawFixNamings();

            GUILayout.EndVertical();
        }

        void DrawChecklistToggle(bool isFine, string fineLabel, string wrongLabel, string fineButtonText, string wrongButtonText, Action fixAction, Action innerDraw = null)
        {
            var buttonText = isFine ? fineButtonText : wrongButtonText;
            var previousGuiColor = GUI.color;

            GUI.color = isFine ? previousGuiColor : checklistBadColor;
            GUILayout.BeginVertical(blockStyle);
            GUILayout.Label(isFine ? fineLabel : wrongLabel, richText);

            innerDraw?.Invoke();

            if (!string.IsNullOrEmpty(buttonText) && GUILayout.Button(buttonText))
                fixAction.Invoke();

            GUILayout.EndVertical();
            GUI.color = previousGuiColor;
        }

        void DrawFixColorSpace()
        {
            var isFine = PlayerSettings.colorSpace == ColorSpace.Linear;

            DrawChecklistToggle(isFine,
                "Color space set to <b>Linear</b>. It is preferred. But you can reset to <b>Gamma</b>.",
                "Color space set to <b>Gamma</b>. For PC preferred is <b>Linear</b>.",
                "Set to Gamma",
                "Set to Linear",
                () => { PlayerSettings.colorSpace = isFine ? ColorSpace.Gamma : ColorSpace.Linear; });
        }
        
        void DrawFixSpritePacker()
        {
            var isFine = EditorSettings.spritePackerMode is SpritePackerMode.SpriteAtlasV2;
            var neededResult = isFine ? SpritePackerMode.Disabled : SpritePackerMode.SpriteAtlasV2;

            DrawChecklistToggle(isFine,
                "Sprite packer V2 is enabled. It is preferred. But you can disable.",
                "Sprite packer V2 is disabled. Preferred is Enabled",
                "Disable",
                "Enable",
                () => { EditorSettings.spritePackerMode = neededResult; });
        }

        void DrawFixPhysicsTimeStep()
        {
            /*
            var timeManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0];
            var serializedObject = new SerializedObject(timeManager);
            var property = serializedObject.FindProperty("Fixed Timestep"); // TODO: InsaneOne.Core not correct to access like this

            DrawChecklistToggle(Mathf.Approximately(property.floatValue, 0.0167f),
                "Physics fixed time step now being called 60 times per second",
                "Physics fixed time step set to be called 50 times a second. It can cause laggy physics view, if no interpolation used. Setting to 60 frames can fix this.",
                "",
                "Set to 60 frames",
                () =>
                {
                    property.floatValue = 0.0167f;
                    serializedObject.ApplyModifiedProperties();
                });
                */
        }

        void DrawFixNamespace()
        {
            var actualNamespace = EditorSettings.projectGenerationRootNamespace;

            DrawChecklistToggle(actualNamespace != string.Empty,
                $"Project root namespace is set. Current namespace is <b>{actualNamespace}</b>.",
                "Project root namespace isn't set. Fix?",
                "Set",
                "Set",
                () => { EditorSettings.projectGenerationRootNamespace = selectedNamespace; },
                () => { selectedNamespace = EditorGUILayout.TextField("Namespace", selectedNamespace); });
        }

        void DrawFixCompanyName()
        {
            var isFine = PlayerSettings.companyName != "DefaultCompany" && PlayerSettings.companyName != String.Empty;

            DrawChecklistToggle(isFine,
                $"Company name is set. Current company name is <b>{PlayerSettings.companyName}</b>",
                $"Developer Company is not set. Set to value below ({companyName})?",
                "Set",
                "Set",
                () => { PlayerSettings.companyName = companyName; },
                () => { companyName = GUILayout.TextField(companyName); });
        }

        void DrawFixCreateAtOrigin()
        {
            DrawChecklistToggle(EditorPrefs.GetBool(OriginPlacePrefName),
                "Create objects at origin is <b>enabled</b>.",
                "Create objects at origin is <b>disabled</b>. It is recommended to <b>enable</b>.",
                "",
                "Fix",
                () => { EditorPrefs.SetBool(OriginPlacePrefName, true); });
        }

        void DrawFixReloadDomain()
        {
            var correctValue = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            var isFine = EditorSettings.enterPlayModeOptionsEnabled && EditorSettings.enterPlayModeOptions == correctValue;

            DrawChecklistToggle(isFine,
                "Enter Play Mode options: Reload domain settings are correct.",
                "Enter Play Mode options: Reload domain and scene is <b>enabled</b>. It slows down reload time after every recompile. Recommended is to <b>disable</b> these settings.",
                "",
                "Fix",
                () =>
                {
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = correctValue;
                });
        }

        void DrawFixNamings()
        {
            var isFine = EditorSettings.gameObjectNamingScheme == EditorSettings.NamingScheme.Underscore;

            DrawChecklistToggle(isFine,
                "Namings scheme is correct.",
                "Namings scheme is not set to <b>underscore</b> with 2 digits (Prefab_01). It is recommended to use <b>underscore</b> and several digits for better consistent naming.",
                "",
                "Fix",
                () =>
                {
                    EditorSettings.gameObjectNamingScheme = EditorSettings.NamingScheme.Underscore;
                    EditorSettings.gameObjectNamingDigits = 2;
                    EditorSettings.assetNamingUsesSpace = false;
                });
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
        
        void GenereteProjectFolders(bool is3D, bool isEcs)
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
                new ($"{contentPath}/Sources", "Data"),
            };

            if (isEcs)
            {
                foldersToCreate.Add(new FolderData($"{contentPath}/Sources", "Systems"));
                foldersToCreate.Add(new FolderData($"{contentPath}/Sources", "Components"));
                foldersToCreate.Add(new FolderData($"{contentPath}/Sources", "Services"));
            }

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

        static void AddPackage(string nameOrUrl)
        {
            if (installRequest != null)
                return;
            
            installRequest = Client.Add(nameOrUrl);
            EditorApplication.update += InstallProgress;
    
            CoreUnityLogger.I.Log("Started installing " + nameOrUrl);
        }

        static void InstallProgress()
        {
            if (!installRequest.IsCompleted)
                return;

            if (installRequest.Status == StatusCode.Success)
                CoreUnityLogger.I.Log($"Installed: {installRequest.Result.packageId}");
            else if (installRequest.Status >= StatusCode.Failure)
                CoreUnityLogger.I.Log(installRequest.Error.message, LogLevel.Error);

            EditorApplication.update -= InstallProgress;
            installRequest = null;
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