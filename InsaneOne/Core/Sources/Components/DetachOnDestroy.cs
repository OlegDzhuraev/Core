using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> Detaches selected transforms from root transform on destroy of this object. </summary>
	public class DetachOnDestroy : MonoBehaviour
	{
		[SerializeField] Transform[] toDetach = new Transform[0];
		[SerializeField] bool addDestroyTimer;
		[SerializeField] float childDestroyTime = 3f;
		[SerializeField] bool useUnityBuiltinOnDestroy;
		[SerializeField] bool allowOtherParent = true;

		/// <summary> Can be used for some sort of ECS, where destroy should be called manually. </summary>
		public virtual void OnManualDestroy()
		{
			foreach (var tr in toDetach)
			{
				if (!allowOtherParent && tr.parent != transform)
					continue;

				tr.SetParent(null);

				if (addDestroyTimer)
					tr.gameObject.DelayedDestroy(childDestroyTime);
			}
		}

		void OnDestroy()
		{
			if (!useUnityBuiltinOnDestroy)
				return;

			OnManualDestroy();
		}
	}
}