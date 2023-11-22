using System;
using InsaneOne.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	/// <summary> Can be used to make simple popup window with some apply/cancel buttons. </summary>
	[RequireComponent(typeof(Panel))]
	public sealed class PopupWindow : MonoBehaviour, IPauseAffector
	{
		public Panel SelfPanel => panel;
		
		[SerializeField] Panel panel;
		[SerializeField] TMP_Text titleTextLabel;
		[SerializeField] TMP_Text textLabel;
		[SerializeField] Button applyButton;
		[SerializeField] Button cancelButton;
		[SerializeField] bool pauseOnShow = true;

		Action callback;

		void Start()
		{
			panel.WasShown += OnShown;
			panel.WasHidden += OnHidden;
			
			applyButton.onClick.AddListener(OnApplyClick);
			cancelButton.onClick.AddListener(OnCancelClick);
		}

		// TODO InsaneOne: OnEnable/OnDisable?
		void OnDestroy()
		{
			if (panel)
			{
				panel.WasShown -= OnShown;
				panel.WasHidden -= OnHidden;
			}
			
			if (applyButton)
				applyButton.onClick.RemoveListener(OnApplyClick);
			
			if (cancelButton)
				cancelButton.onClick.RemoveListener(OnCancelClick);
		}
		
		void OnApplyClick()
		{
			panel.Hide();
			callback?.Invoke();
		}

		void OnCancelClick() => panel.Hide();

		public void Show(string titleText, string text, bool showCancelButton = false, Action callbackAction = null)
		{
			titleTextLabel.text = titleText;
			textLabel.text = text;
			
			cancelButton.gameObject.SetActive(showCancelButton);
			
			callback = callbackAction;
		}
		
		void OnShown()
		{
			if (pauseOnShow)
				PauseUtility.Pause(this);
		}
		
		void OnHidden()
		{
			if (pauseOnShow)
				PauseUtility.Unpause(this);
		}
	}
}