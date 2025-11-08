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

using TMPro;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> This class allows to show panel with info near cursor. </summary>
	public sealed class Hint : Element<HintViewModel>
	{
		[SerializeField] Transform canvasTransform;
		[SerializeField] TMP_Text nameText;

		RectTransform rectTransform;

		void Awake() => rectTransform = GetComponent<RectTransform>();
		void Start() => Hide();

		public override void OnViewModelChanged(HintViewModel viewModel)
		{
			var finalPos = Vector2.zero;

			if (viewModel.ShowOnCursor)
				finalPos = (Vector2) Input.mousePosition + viewModel.CursorOffset;
			else
				finalPos = viewModel.Position;

			rectTransform.anchoredPosition = finalPos * canvasTransform.localScale.x;
			nameText.text = viewModel.Text;
		}

		void Update()
		{
			if (viewModel.ShowOnCursor)
				rectTransform.anchoredPosition = Input.mousePosition;
		}
	}

	public class HintViewModel
	{
		public readonly string Text;
		public readonly bool ShowOnCursor;
		public readonly Vector2 Position;
		public readonly Vector2 CursorOffset;

		public HintViewModel(string text, bool showOnCursor, Vector2 position, Vector2 cursorOffset)
		{
			Text = text;
			ShowOnCursor = showOnCursor;
			Position = position;
			CursorOffset = cursorOffset;
		}
	}
}