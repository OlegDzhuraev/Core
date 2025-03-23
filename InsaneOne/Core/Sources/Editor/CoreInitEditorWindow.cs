using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Development
{
	public class CoreInitEditorWindow : EditorWindow
	{
		Label infoLabel;
		Button setupButton;

		public static void ShowWindow()
		{
			var wnd = GetWindow<CoreInitEditorWindow>();
			wnd.titleContent = new GUIContent("InsaneOne.Core Setup");
			wnd.minSize = new Vector2(340, 64);
			wnd.maxSize = new Vector2(340, 128);
		}

		void CreateGUI()
		{
			var root = rootVisualElement;

			infoLabel = new Label("Looks like InsaneOne.Core was not setup before\n(No CoreData asset found).");
			root.Add(infoLabel);

			setupButton = new Button
			{
				name = "setup_button",
				text = "Setup InsaneOne.Core",
				style =
				{
					backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.2f)),
				},
			};

			root.Add(setupButton);

			TryShowSetupComplete();
			SetupButtonHandler();
		}

		void SetupButtonHandler()
		{
			var buttons = rootVisualElement.Query<Button>();
			buttons.ForEach(RegisterHandler);
		}

		void RegisterHandler(Button button)
		{
			button.RegisterCallback<ClickEvent>(OnButtonClick);
		}

		void OnButtonClick(ClickEvent evt)
		{
			if (evt.currentTarget is not Button btn)
				return;

			if (btn.name.Contains("setup_button"))
			    Init();
			else if (btn.name.Contains("close_button"))
				Close();
		}

		void Init()
		{
			var newData = ScriptableObject.CreateInstance<CoreData>();

			if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				AssetDatabase.CreateFolder("Assets", "Resources");

			if (!AssetDatabase.IsValidFolder("Assets/Resources/InsaneOne"))
				AssetDatabase.CreateFolder("Assets/Resources", "InsaneOne");

			AssetDatabase.Refresh();
			AssetDatabase.CreateAsset(newData, "Assets/Resources/InsaneOne/CoreData.asset");

			CoreData.Log("new CoreData was <b><color=#55ff33>created</color></b>.");

			TryShowSetupComplete();
		}

		void TryShowSetupComplete()
		{
			if (!CoreData.TryLoad(out _))
				return;

			var root = rootVisualElement;

			setupButton.SetEnabled(false);
			infoLabel.SetEnabled(false);

			var setupLabel = new Label("Setup is finished!");
			root.Add(setupLabel);

			var closeButton = new Button();
			closeButton.name = "close_button";
			closeButton.text = "Close window";
			root.Add(closeButton);

			SetupButtonHandler();
		}
	}
}