/*
 * Copyright 2025 Oleg Dzhuraev <godlikeaurora@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using InsaneOne.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	/// <summary> Can be used to make simple popup window with some apply/cancel buttons. </summary>
	[RequireComponent(typeof(Panel))]
	public sealed class PopupWindow : Element<PopupViewModel>, IPauseSource
	{
		public Panel SelfPanel => panel;
		
		[SerializeField] Panel panel;
		[SerializeField] TMP_Text titleTextLabel;
		[SerializeField] TMP_Text textLabel;
		[SerializeField] Button applyButton;
		[SerializeField] Button cancelButton;
		[SerializeField] bool pauseOnShow = true;

		void Start()
		{
			panel.WasShown += OnShown;
			panel.WasHidden += OnHidden;
			
			applyButton.onClick.AddListener(OnApplyClick);
			cancelButton.onClick.AddListener(OnCancelClick);
		}

		public override void OnViewModelChanged(PopupViewModel viewModel)
		{
			titleTextLabel.text = viewModel.Title;
			textLabel.text = viewModel.Text;
			cancelButton.gameObject.SetActive(viewModel.ShowCancelButton);
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
			viewModel.ApplyAction?.Invoke();
			Hide();
		}

		void OnCancelClick()
		{
			viewModel.CancelAction?.Invoke();
			Hide();
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

	public class PopupViewModel
	{
		public readonly string Title;
		public readonly string Text;
		public readonly bool ShowCancelButton;
		public readonly Action ApplyAction;
		public readonly Action CancelAction;

		public PopupViewModel(string title, string text, bool showCancelButton, Action apply, Action cancel)
		{
			Title = title;
			Text = text;
			ShowCancelButton = showCancelButton;
			ApplyAction = apply;
			CancelAction = cancel;
		}
	}
}