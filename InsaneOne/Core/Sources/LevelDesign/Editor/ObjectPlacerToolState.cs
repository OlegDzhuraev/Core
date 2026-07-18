using System;
using UnityEditor;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Shared activation state for the Object Placer tool, kept in sync between the tool window and the Scene view toolbar toggle.
	/// Backed by SessionState, so it survives domain reloads within the same Editor session. </summary>
	public static class ObjectPlacerToolState
	{
		const string ActiveKey = "InsaneOne.ObjectPlacer.Active";

		public static event Action<bool> ActiveChanged;

		public static bool IsActive
		{
			get => SessionState.GetBool(ActiveKey, false);
			private set => SessionState.SetBool(ActiveKey, value);
		}

		public static void SetActive(bool value)
		{
			if (IsActive == value)
				return;

			IsActive = value;
			ActiveChanged?.Invoke(value);
		}
	}
}
