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

using System.Collections.Generic;
using InsaneOne.Core.Goals;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> Shows a temporary on-screen popup with a goal's title and progress bar whenever it gets some progress.
	/// Title is displayed through the child ProgressBar's own titleText, not duplicated here - wire the ProgressBar's
	/// fillBar/titleText/numberText in its own inspector. Call Bind with a GoalsService instance (usually resolved via
	/// ServiceLocator) to start listening to it. If several goals get progress close together, popups for them are
	/// queued and shown one after another. </summary>
	public sealed class GoalNotificationPopup : Element<GoalProgress>
	{
		[SerializeField] ProgressBar progressBar;
		[Tooltip("How long the popup stays visible after progress changes, in seconds.")]
		[SerializeField] float showDuration = 3f;

		readonly Queue<GoalProgress> queue = new ();

		GoalsService subscribedService;
		float hideTimeLeft;

		protected override void OnAwake() => Hide();

		void Update()
		{
			if (hideTimeLeft <= 0f)
				return;

			hideTimeLeft -= Time.deltaTime;

			if (hideTimeLeft > 0f)
				return;

			if (queue.Count > 0)
				ShowNext();
			else
				Hide();
		}

		void OnDestroy() => Unbind();

		/// <summary> Starts listening to the given service's progress and showing a popup on every change. Replaces any previous binding. </summary>
		public void Bind(GoalsService service)
		{
			Unbind();

			subscribedService = service;
			subscribedService.ProgressChanged += Enqueue;
		}

		/// <summary> Stops listening to the currently bound service, if any. </summary>
		public void Unbind()
		{
			if (subscribedService != null)
				subscribedService.ProgressChanged -= Enqueue;

			subscribedService = null;
		}

		public override void OnViewModelChanged(GoalProgress progress)
		{
			if (progressBar)
				progressBar.SetViewModel(new ProgressBarViewModel(progress.Current, progress.Required, progress.Title));
		}

		/// <summary> Recommended to use it only for test purposes. </summary>
		public void SetupInternal(ProgressBar progressBar, float showDuration = 0.2f)
		{
			this.progressBar = progressBar;
			this.showDuration = showDuration;
		}

		void Enqueue(GoalProgress progress)
		{
			queue.Enqueue(progress);

			if (hideTimeLeft <= 0f) // nothing is being shown right now
				ShowNext();
		}

		void ShowNext()
		{
			if (queue.Count == 0)
				return;

			SetViewModel(queue.Dequeue());
			Show();
			hideTimeLeft = showDuration;
		}
	}
}
