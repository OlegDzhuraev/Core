using UnityEngine;

namespace InsaneOne.Core
{
	public sealed class DelayedDestruction : MonoBehaviour
	{
		public float SecondsToDestruction => secondsToDestruction;

		[SerializeField] [Range(0f, 1000f)] float secondsToDestruction = 3f;
		[SerializeField] bool detachChildren;

		void Update()
		{

			secondsToDestruction -= Time.deltaTime;

			if (secondsToDestruction > 0)
				return;
			
			if (detachChildren)
				for (int i = 0; i < transform.childCount; i++)
					transform.GetChild(i).SetParent(null);

			Destroy(gameObject);
		}

		public void SetDestructionTime(float newTime) => secondsToDestruction = newTime;

		/// <summary> Will destroy an object after time passed. Can be used with Perseids Pooling. </summary>
		public static void ApplyTo(GameObject target, float delay = 3f, bool usePoolingIfPossible = false)
		{
#if PERSEIDS_POOLING
			if (usePoolingIfPossible)
			{
				target.GetOrAddComponent<PerseidsPooling.Utils.DelayedPoolDestruction>().SetDestructionTime(delay);
				return;
			}
#endif
			target.GetOrAddComponent<DelayedDestruction>().SetDestructionTime(delay);
		}
	}

	public static class DelayedDestructionExtension
	{
		/// <summary> Will destroy a object after time passed. Can be used with Perseids Pooling. </summary>
		public static void DelayedDestroy(this GameObject gameObject, float delay = 3f, bool usePoolingIfPossible = false)
		{
			DelayedDestruction.ApplyTo(gameObject, delay, usePoolingIfPossible);
		}
	}
}