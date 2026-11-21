using UnityEngine;

namespace InsaneOne.Core.Effects
{
	public sealed class SheetAnimatorRandomize : MonoBehaviour
#if PERSEIDS_POOLING
		, PerseidsPooling.IResettable
#endif
	{
		[SerializeField] SheetAnimator animator;
		[SerializeField] bool randomizeAtStart;

		void Start()
		{
			if (randomizeAtStart)
				PlayRandom();
		}

		public void PlayRandom()
		{
			if (!animator || animator.Animations == null || animator.Animations.Length == 0)
			{
				Debug.LogWarning($"[{nameof(SheetAnimatorRandomize)}] No animator or no animations set on [{name}] - nothing to play.", this);
				return;
			}

			var random = animator.Animations.Random();
			animator.PlayAnimation(random.Name);
		}
		
#if PERSEIDS_POOLING
		public void ResetPooled() => PlayRandom();
#endif
	}
}