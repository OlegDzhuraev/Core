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
			var random = animator.Animations.Random();
			animator.PlayAnimation(random.Name);
		}
		
#if PERSEIDS_POOLING
		public void ResetPooled() => PlayRandom();
#endif
	}
}