using System;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	[DisallowMultipleComponent]
	public sealed class Toggle : Element<ToggleViewModel>
	{
		[SerializeField] UnityEngine.UI.Toggle toggle;

		void OnUnityToggleChanced(bool isOn) => ViewModel.SetValue(isOn);

		public override void OnViewModelChanged(ToggleViewModel viewModel)
		{
			toggle.onValueChanged.RemoveAllListeners();
			toggle.isOn = viewModel.IsOn;

			toggle.onValueChanged.AddListener(OnUnityToggleChanced);
		}
	}

	public class ToggleViewModel
	{
		public event Action<bool> WasChanged;
		public bool IsOn { get; private set; }

		public ToggleViewModel() { }
		public ToggleViewModel(bool isOn) => IsOn = isOn;

		public void SetValue(bool value)
		{
			if (IsOn == value)
				return;

			IsOn = value;
			WasChanged?.Invoke(value);
		}
	}
}