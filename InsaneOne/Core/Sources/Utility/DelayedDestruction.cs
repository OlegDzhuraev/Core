using UnityEngine;

namespace InsaneOne.Core
{
	public class DelayedDestruction : MonoBehaviour
	{
		[SerializeField] [Range(0f, 1000f)] float secondsToDestruction = 3f;
		[SerializeField] bool detachChilds;

		void Update()
		{
			secondsToDestruction -= Time.deltaTime;

			if (secondsToDestruction > 0)
				return;
			
			if (detachChilds)
				for (int i = 0; i < transform.childCount; i++)
					transform.GetChild(i).SetParent(null);

			Destroy(gameObject);
		}

		public void SetDestructionTime(float newTime) => secondsToDestruction = newTime;

		public static void ApplyTo(GameObject target, float delay = 3f)
		{
			target.GetOrAddComponent<DelayedDestruction>().SetDestructionTime(delay);
		}
	}

	public static class DelayedDestructionExtension
	{
		public static void DelayedDestroy(this GameObject gameObject, float delay = 3f) =>
			DelayedDestruction.ApplyTo(gameObject, delay);
	}
}