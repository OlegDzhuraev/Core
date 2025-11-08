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
using UnityEngine;

namespace InsaneOne.Core.UI
{
	public interface IElement
	{
		public void Show();
		public void Hide();
	}

	[DisallowMultipleComponent]
	public abstract class Element<TViewModel> : MonoBehaviour, IElement
	{
		public event Action WasShown, WasHidden;

		[SerializeField] protected GameObject selfObject;
		protected TViewModel viewModel;

		public void SetViewModel(TViewModel viewModel)
		{
			this.viewModel = viewModel;
			OnViewModelChanged(viewModel);
		}

		public virtual void OnViewModelChanged(TViewModel viewModel) { }

		public virtual void Show()
		{
			if (selfObject.activeSelf)
				return;

			selfObject.SetActive(true);
			WasShown?.Invoke();
		}

		public virtual void Hide()
		{
			if (!selfObject.activeSelf)
				return;

			selfObject.SetActive(false);
			WasHidden?.Invoke();
		}
	}
}