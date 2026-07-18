using InsaneOne.Core.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Development
{
	public sealed class CoreInitEditorWindow : EditorWindow
	{
		const string StylesPath = "InsaneOne/ToolsStyles";
		const string PackageName = "InsaneOne.Core";
		public const string DismissedSessionKey = "InsaneOne.Core.Setup.Dismissed";

		VisualElement content;
		VisualElement initialSetupContainer;
		Label infoLabel;
		Button setupButton;

		[MenuItem("Tools/InsaneOne/Initial setup...", priority = 200)]
		public static void ShowWindow()
		{
			var wnd = CreateInstance<CoreInitEditorWindow>();
			wnd.titleContent = new GUIContent($"Setup");
			wnd.minSize = new Vector2(360, 96);
			wnd.maxSize = new Vector2(360, 96);
			wnd.ShowModal();
		}

		void CreateGUI()
		{
			var root = rootVisualElement;

			var style = Resources.Load(StylesPath) as StyleSheet;
			root.styleSheets.Add(style);

			content = new VisualElement();
			content.AddToClassList("core-init-root");
			root.Add(content);

			var titleLabel = new Label($"{PackageName} Setup");
			titleLabel.AddToClassList("core-init-title");
			content.Add(titleLabel);

			initialSetupContainer = new VisualElement();
			content.Add(initialSetupContainer);

			infoLabel = new Label($"Looks like {PackageName} was not setup before\n(No {nameof(CoreData)} asset found).");
			initialSetupContainer.Add(infoLabel);

			setupButton = new Button(Init) { text = $"Setup {PackageName}" };
			setupButton.AddToClassList("core-init-setup-button");

			var skipButton = new Button(Dismiss) { text = "Dismiss in current session" };

			initialSetupContainer.Add(setupButton);
			initialSetupContainer.Add(skipButton);

			TryShowSetupComplete();
		}

		void Dismiss()
		{
			SessionState.SetBool(DismissedSessionKey, true);
			Close();
		}

		void Init()
		{
			var newData = CreateInstance<CoreData>();

			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/InsaneOne"))
				AssetDatabase.CreateFolder("Assets/Resources", "InsaneOne");

			AssetDatabase.Refresh();
			AssetDatabase.CreateAsset(newData, "Assets/Resources/InsaneOne/CoreData.asset");

			CoreUnityLogger.I.Log($"new {nameof(CoreData)} was <b><color=#55ff33>created</color></b>.");

			TryShowSetupComplete();
		}

		void TryShowSetupComplete()
		{
			if (!CoreData.TryLoad(out _))
				return;

			content.Remove(initialSetupContainer);

			var setupLabel = new Label("Setup is finished!");
			content.Add(setupLabel);

			var closeButton = new Button(Close) { text = "Close window" };
			content.Add(closeButton);
		}
	}
}