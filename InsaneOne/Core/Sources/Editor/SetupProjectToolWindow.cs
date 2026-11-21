using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using InsaneOne.Core.Editor;
using InsaneOne.Core.Utility;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
using LogLevel = InsaneOne.Core.Utility.LogLevel;

namespace InsaneOne.Core.Development
{
    public sealed class SetupProjectToolWindow : EditorWindow
    {
        const string LayoutPath = "InsaneOne/SetupProjectToolWindow";
        const string OriginPlacePrefName = "Create3DObject.PlaceAtWorldOrigin";

        static readonly string[] foldersDisplayOptions = {"Classic", "Classic ECS", "Feature-oriented"};
        static readonly string[] dimensionDisplayOptions = {"3D", "2D"};

        static Request activePackageRequest;

        static readonly Dictionary<string, string> manifestDependencies = new Dictionary<string, string>();
        static readonly Regex manifestDependencyRegex = new Regex("\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"");
        static DateTime manifestWriteTime;

        [SerializeField] string contentFolder = "Game";
        [SerializeField] int dimension;
        [SerializeField] FoldersGenerationStyle foldersStyle;
        [SerializeField] bool separateUiInClassicStyle;
        [SerializeField] string selectedNamespace = "Game";
        [SerializeField] string companyName = "InsaneOne";

        readonly List<PackageRow> packageRows = new List<PackageRow>();

        PopupField<string> dimensionField;
        PopupField<string> foldersStyleField;
        TextField contentFolderField;
        Toggle separateUiToggle;
        Label foldersWarning;
        Button generateButton;
        VisualElement graphicsItems;
        VisualElement otherItems;

        [MenuItem("Tools/InsaneOne/Setup Project Tool...")]
        public static void ShowWindow() => GetWindow<SetupProjectToolWindow>("Setup Project Tool");

        [MenuItem("Tools/InsaneOne/Update")]
        public static void UpdatePlugin() => AddPackage($"https://github.com/{SetupProjectPaths.RepoName}/Core.git");

        public void CreateGUI()
        {
            minSize = new Vector2(532, 532);
            maxSize = new Vector2(798, 798);

            packageRows.Clear();

            rootVisualElement.Clear();
            Resources.Load<VisualTreeAsset>(LayoutPath).CloneTree(rootVisualElement);

            rootVisualElement.Q(className: "setup-project")
                .EnableInClassList("light-theme", !EditorGUIUtility.isProSkin);

            rootVisualElement.Q<ScrollView>("scroll").horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            BuildFoldersBlock();
            BuildPackagesBlock();
            BuildChecklistBlock();

            RefreshPackageRows();
        }

        void BuildFoldersBlock()
        {
            var fields = rootVisualElement.Q<VisualElement>("folders-fields");

            dimensionField = new PopupField<string>("Project dimensions", new List<string>(dimensionDisplayOptions), dimension);
            dimensionField.AddToClassList("tool-control");
            dimensionField.RegisterValueChangedCallback(e => dimension = Array.IndexOf(dimensionDisplayOptions, e.newValue));

            foldersStyleField = new PopupField<string>("Folders style", new List<string>(foldersDisplayOptions), (int)foldersStyle);
            foldersStyleField.AddToClassList("tool-control");
            foldersStyleField.RegisterValueChangedCallback(e =>
            {
                foldersStyle = (FoldersGenerationStyle)Array.IndexOf(foldersDisplayOptions, e.newValue);
                UpdateFolderFields();
            });

            contentFolderField = new TextField("Content folder name") { value = contentFolder };
            contentFolderField.AddToClassList("tool-control");
            contentFolderField.RegisterValueChangedCallback(e =>
            {
                contentFolder = e.newValue;
                UpdateFolderWarning();
            });

            separateUiToggle = new Toggle("Separate UI folder") { value = separateUiInClassicStyle };
            separateUiToggle.RegisterValueChangedCallback(e => separateUiInClassicStyle = e.newValue);

            fields.Add(dimensionField);
            fields.Add(foldersStyleField);
            fields.Add(contentFolderField);
            fields.Add(separateUiToggle);

            foldersWarning = rootVisualElement.Q<Label>("folders-warning");

            generateButton = rootVisualElement.Q<Button>("generate-folders");
            generateButton.clicked += GenerateProjectFolders;

            UpdateFolderFields();
            UpdateFolderWarning();
        }

        void UpdateFolderFields()
        {
            var isClassic = foldersStyle is FoldersGenerationStyle.Classic or FoldersGenerationStyle.ClassicECS;
            contentFolderField.style.display = isClassic ? DisplayStyle.Flex : DisplayStyle.None;
            separateUiToggle.style.display = isClassic ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void UpdateFolderWarning()
        {
            var isInvalid = contentFolder is "" or "Resources";

            foldersWarning.EnableInClassList("hidden", !isInvalid);
            generateButton.SetEnabled(!isInvalid);
        }

        void GenerateProjectFolders()
        {
            if (foldersStyle == FoldersGenerationStyle.FeatureOriented)
                GenerateProjectFoldersFeatures(dimension == 0);
            else if (foldersStyle is FoldersGenerationStyle.Classic or FoldersGenerationStyle.ClassicECS)
                GenerateProjectFoldersClassic(dimension == 0, foldersStyle is FoldersGenerationStyle.ClassicECS);
        }

        void BuildPackagesBlock()
        {
            BuildPackageRows(rootVisualElement.Q<VisualElement>("modules-rows"), SetupProjectPaths.GitPackages, false);
            BuildPackageRows(rootVisualElement.Q<VisualElement>("registry-rows"), SetupProjectPaths.Packages, false);
            BuildPackageRows(rootVisualElement.Q<VisualElement>("assets-rows"), SetupProjectPaths.AssetLinks, true);
        }

        void BuildPackageRows(VisualElement container, List<LinkHolder> links, bool openUrl)
        {
            foreach (var link in links)
                container.Add(CreatePackageRow(link, openUrl));
        }

        VisualElement CreatePackageRow(LinkHolder holder, bool openUrl)
        {
            var row = new VisualElement();
            row.AddToClassList("row");

            var title = new Label(holder.Name);
            title.AddToClassList("row-title");
            row.Add(title);

            var actions = new VisualElement();
            actions.AddToClassList("row-actions");
            row.Add(actions);

            if (openUrl)
            {
                var open = new Button(() => OpenUrl(holder.Link)) { text = "Open" };
                open.AddToClassList("row-button");
                open.AddToClassList("is-accent");
                actions.Add(open);

                return row;
            }

            var add = new Button(() => { AddPackage(holder.Link); RefreshPackageRows(); }) { text = "Add" };
            add.AddToClassList("row-button");

            var remove = new Button(() => { RemovePackage(holder.Link); RefreshPackageRows(); }) { text = "Remove" };
            remove.AddToClassList("row-button");

            actions.Add(add);
            actions.Add(remove);
            packageRows.Add(new PackageRow(holder.Link, add, remove));

            return row;
        }

        void RefreshPackageRows()
        {
            var isIdle = activePackageRequest == null;

            foreach (var row in packageRows)
            {
                var isInstalled = IsPackageInstalled(row.Link);
                row.Add.SetEnabled(isIdle && !isInstalled);
                row.Remove.SetEnabled(isIdle && isInstalled);
            }
        }

        void BuildChecklistBlock()
        {
            graphicsItems = rootVisualElement.Q<VisualElement>("graphics-items");
            otherItems = rootVisualElement.Q<VisualElement>("other-items");
            RebuildChecklist();
        }

        void RebuildChecklist()
        {
            graphicsItems.Clear();
            otherItems.Clear();

            var colorSpaceFine = PlayerSettings.colorSpace == ColorSpace.Linear;
            AddChecklistItem(graphicsItems, colorSpaceFine,
                "Color space set to <b>Linear</b>. It is preferred. But you can reset to <b>Gamma</b>.",
                "Color space set to <b>Gamma</b>. For PC preferred is <b>Linear</b>.",
                colorSpaceFine ? "Set to Gamma" : "Set to Linear",
                () => PlayerSettings.colorSpace = colorSpaceFine ? ColorSpace.Gamma : ColorSpace.Linear);

            var spritePackerFine = EditorSettings.spritePackerMode is SpritePackerMode.SpriteAtlasV2;
            AddChecklistItem(graphicsItems, spritePackerFine,
                "Sprite packer V2 is enabled. It is preferred. But you can disable.",
                "Sprite packer V2 is disabled. Preferred is Enabled",
                spritePackerFine ? "Disable" : "Enable",
                () => EditorSettings.spritePackerMode = spritePackerFine ? SpritePackerMode.Disabled : SpritePackerMode.SpriteAtlasV2);

            var namespaceFine = EditorSettings.projectGenerationRootNamespace != string.Empty;
            AddChecklistItem(otherItems, namespaceFine,
                $"Project root namespace is set. Current namespace is <b>{EditorSettings.projectGenerationRootNamespace}</b>.",
                "Project root namespace isn't set. Fix?",
                "Set",
                () => EditorSettings.projectGenerationRootNamespace = selectedNamespace,
                () =>
                {
                    var field = new TextField("Namespace") { value = selectedNamespace };
                    field.AddToClassList("tool-control");
                    field.RegisterValueChangedCallback(e => selectedNamespace = e.newValue);
                    return field;
                });

            var companyFine = PlayerSettings.companyName != "DefaultCompany" && PlayerSettings.companyName != string.Empty;
            AddChecklistItem(otherItems, companyFine,
                $"Company name is set. Current company name is <b>{PlayerSettings.companyName}</b>",
                $"Developer Company is not set. Set to value below ({companyName})?",
                "Set",
                () => PlayerSettings.companyName = companyName,
                () =>
                {
                    var field = new TextField { value = companyName };
                    field.AddToClassList("tool-control");
                    field.RegisterValueChangedCallback(e => companyName = e.newValue);
                    return field;
                });

            var originFine = EditorPrefs.GetBool(OriginPlacePrefName);
            AddChecklistItem(otherItems, originFine,
                "Create objects at origin is <b>enabled</b>.",
                "Create objects at origin is <b>disabled</b>. It is recommended to <b>enable</b>.",
                originFine ? string.Empty : "Fix",
                () => EditorPrefs.SetBool(OriginPlacePrefName, true));

            var correctPlayMode = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            var playModeFine = EditorSettings.enterPlayModeOptionsEnabled && EditorSettings.enterPlayModeOptions == correctPlayMode;
            AddChecklistItem(otherItems, playModeFine,
                "Enter Play Mode options: Reload domain settings are correct.",
                "Enter Play Mode options: Reload domain and scene is <b>enabled</b>. It slows down reload time after every recompile. Recommended is to <b>disable</b> these settings.",
                playModeFine ? string.Empty : "Fix",
                () =>
                {
                    EditorSettings.enterPlayModeOptionsEnabled = true;
                    EditorSettings.enterPlayModeOptions = correctPlayMode;
                });

            var namingFine = EditorSettings.gameObjectNamingScheme == EditorSettings.NamingScheme.Underscore;
            AddChecklistItem(otherItems, namingFine,
                "Namings scheme is correct.",
                "Namings scheme is not set to <b>underscore</b> with 2 digits (Prefab_01). It is recommended to use <b>underscore</b> and several digits for better consistent naming.",
                namingFine ? string.Empty : "Fix",
                () =>
                {
                    EditorSettings.gameObjectNamingScheme = EditorSettings.NamingScheme.Underscore;
                    EditorSettings.gameObjectNamingDigits = 2;
                    EditorSettings.assetNamingUsesSpace = false;
                });
        }

        void AddChecklistItem(VisualElement container, bool isFine, string fineText, string wrongText,
            string buttonText, Action fix, Func<VisualElement> extraContent = null)
        {
            var item = new VisualElement();
            item.AddToClassList("checklist-item");
            item.EnableInClassList("is-fine", isFine);
            item.EnableInClassList("is-wrong", !isFine);

            var text = new Label(isFine ? fineText : wrongText) { enableRichText = true };
            text.AddToClassList("checklist-text");
            item.Add(text);

            if (extraContent != null)
                item.Add(extraContent());

            if (!string.IsNullOrEmpty(buttonText))
            {
                var button = new Button(() => { fix?.Invoke(); RebuildChecklist(); }) { text = buttonText };
                button.AddToClassList("primary-button");
                item.Add(button);
            }

            container.Add(item);
        }

        static void OpenUrl(string url) => Application.OpenURL(url);

        static bool IsPackageInstalled(string nameOrUrl) => FindInstalledPackageName(nameOrUrl) != null;

        // Registry packages are stored as "name": "version", git packages as "name": "url",
        // so a package can be matched either by its name or by its link.
        static string FindInstalledPackageName(string nameOrUrl)
        {
            foreach (var dependency in GetManifestDependencies())
            {
                if (string.Equals(dependency.Key, nameOrUrl, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(dependency.Value, nameOrUrl, StringComparison.OrdinalIgnoreCase))
                    return dependency.Key;
            }

            return null;
        }

        // Reading Packages/manifest.json directly is cheaper and simpler than an async Client.List request,
        // and it gives the installed state synchronously while building the rows.
        static Dictionary<string, string> GetManifestDependencies()
        {
            var manifestPath = Path.Combine("Packages", "manifest.json");

            if (!File.Exists(manifestPath))
                return manifestDependencies;

            var writeTime = File.GetLastWriteTime(manifestPath);

            if (writeTime == manifestWriteTime && manifestDependencies.Count > 0)
                return manifestDependencies;

            manifestWriteTime = writeTime;
            manifestDependencies.Clear();

            var content = File.ReadAllText(manifestPath);
            var dependenciesStart = content.IndexOf("\"dependencies\"", StringComparison.Ordinal);

            if (dependenciesStart < 0)
                return manifestDependencies;

            var blockStart = content.IndexOf('{', dependenciesStart);
            var blockEnd = content.IndexOf('}', blockStart);

            if (blockStart < 0 || blockEnd < 0)
                return manifestDependencies;

            var block = content.Substring(blockStart, blockEnd - blockStart);

            foreach (Match match in manifestDependencyRegex.Matches(block))
                manifestDependencies[match.Groups[1].Value] = match.Groups[2].Value;

            return manifestDependencies;
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
                new (sharedFolderPath, "Rendering"),
                new ($"{sharedFolderPath}/Rendering", "Shaders"),
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

        void GenerateProjectFoldersClassic(bool is3D, bool isEcs)
        {
            var contentPath = $"Assets/{contentFolder}";

            var foldersToCreate = new List<FolderData>
            {
                new ("Assets", contentFolder),
                new (contentPath, "Sounds"),
                new (contentPath, "Data"),
                new (contentPath, "Materials"),
                new (contentPath, "Animations"),
                new (contentPath, "Scenes"),
                new (contentPath, "Prefabs"),
                new ($"{contentPath}/Prefabs", "VFX"),
                new ($"{contentPath}/Prefabs", "Environment"),

                new (contentPath, "Sources"),
                new ($"{contentPath}/Sources", "Editor"),
                new ($"{contentPath}/Sources", "Data"),

                new (contentPath, "Rendering"),
                new ($"{contentPath}/Rendering", "Shaders"),
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
                foldersToCreate.AddRange(new List<FolderData>
                {
                    new ("Assets", "UI"),
                    new (uiPath, "Sources"),
                    new (uiPath, "Prefabs"),
                    new (uiPath, "Sprites"),
                    new (uiPath, "Fonts"),
                    new (uiPath, "Animations"),
                });
            }
            else
            {
                foldersToCreate.AddRange(new List<FolderData>
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
            if (activePackageRequest != null)
                return;

            activePackageRequest = Client.Add(nameOrUrl);
            EditorApplication.update += PackageRequestProgress;

            CoreUnityLogger.I.Log("Started installing " + nameOrUrl);
        }

        static void RemovePackage(string nameOrUrl)
        {
            if (activePackageRequest != null)
                return;

            var packageName = FindInstalledPackageName(nameOrUrl) ?? nameOrUrl;

            activePackageRequest = Client.Remove(packageName);
            EditorApplication.update += PackageRequestProgress;

            CoreUnityLogger.I.Log("Started removing " + packageName);
        }

        static void PackageRequestProgress()
        {
            if (!activePackageRequest.IsCompleted)
                return;

            if (activePackageRequest.Status == StatusCode.Success)
            {
                if (activePackageRequest is AddRequest addRequest)
                    CoreUnityLogger.I.Log($"Installed: {addRequest.Result.packageId}");
                else
                    CoreUnityLogger.I.Log("Package removed.");
            }
            else if (activePackageRequest.Status >= StatusCode.Failure)
            {
                CoreUnityLogger.I.Log(activePackageRequest.Error.message, LogLevel.Error);
            }

            EditorApplication.update -= PackageRequestProgress;
            activePackageRequest = null;

            // Force the manifest cache to be re-read, then refresh the open window if any.
            manifestWriteTime = default;

            if (HasOpenInstances<SetupProjectToolWindow>())
                GetWindow<SetupProjectToolWindow>().RefreshPackageRows();
        }

        readonly struct PackageRow
        {
            public readonly string Link;
            public readonly Button Add;
            public readonly Button Remove;

            public PackageRow(string link, Button add, Button remove)
            {
                Link = link;
                Add = add;
                Remove = remove;
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
