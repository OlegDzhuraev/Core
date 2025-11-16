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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneOne.Core.UI
{
	[DisallowMultipleComponent]
	public sealed class ProgressBar : Element<ProgressBarViewModel>
	{
		[SerializeField] Image fillBar;
		[SerializeField] TMP_Text titleText;
		[SerializeField] TMP_Text numberText;

		public override void OnViewModelChanged(ProgressBarViewModel viewModel)
		{
			viewModel.Changed += OnChanged;
		}

		void OnDestroy()
		{
			if (ViewModel != null)
				ViewModel.Changed -= OnChanged;
		}

		void OnChanged(float value)
		{
			if (fillBar)
				fillBar.fillAmount = value;

			if (numberText)
				numberText.text = ViewModel.MakeText();
		}
	}

	public class ProgressBarViewModel
	{
		public event Action<float> Changed;

		public string Title { get; private set; }

		public float Progress { get; private set;  }
		public float Value { get; private set; }
		public float MaxValue { get; private set; }

		public ProgressBarViewModel(float value, float maxValue, string title = "")
		{
			Change(value, maxValue);
			Title = title;
		}

		public float Change(float value, float maxValue)
		{
			Value = value;
			MaxValue = maxValue;

			Progress = Value / MaxValue;

			Changed?.Invoke(Progress);
			return Progress;
		}

		public string MakeText() => $"{Value}/{MaxValue}";
	}
}