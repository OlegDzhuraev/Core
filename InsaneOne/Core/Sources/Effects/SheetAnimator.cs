using System;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> You can use this for flipbook-animations for Ui sprites and 2D games. If you don't need a Unity
	/// component, use <see cref="SheetAnimatorCore"/> directly instead - it has the same animation logic/API. </summary>
	public abstract class SheetAnimator : MonoBehaviour
#if PERSEIDS_POOLING
		, PerseidsPooling.IResettable
#endif
	{
		[SerializeField] string startAnimation;
		[SerializeField] bool loopStartAnimation = true;
		[SerializeField] SpriteAnimationData[] animations = Array.Empty<SpriteAnimationData>();

		public SpriteAnimationData[] Animations => core.Animations;

		protected Sprite defaultSprite
		{
			get => core.DefaultSprite;
			set => core.DefaultSprite = value;
		}

		Core core;

		protected virtual void Awake() => core = new Core(this, animations, startAnimation, loopStartAnimation);

		void Start() => Init();

		protected virtual void Init() => core.Init();

		void Update() => core.Tick(Time.deltaTime);

		public void PlayAnimation(string animationName, bool loopAnimation = false, bool hardSet = false) => core.PlayAnimation(animationName, loopAnimation, hardSet);

		public void PlayStartAnimation() => core.PlayStartAnimation();

		public void SetAnimation(SpriteAnimationData data) => core.SetAnimation(data);

		public void Stop() => core.Stop();

		protected abstract void SetSprite(Sprite sprite);

#if PERSEIDS_POOLING
		public void ResetPooled() => Init();
#endif

		/// <summary> Bridges the MonoBehaviour to the plain <see cref="SheetAnimatorCore"/> logic, forwarding sprite changes to the owner. </summary>
		sealed class Core : SheetAnimatorCore
		{
			readonly SheetAnimator owner;

			public Core(SheetAnimator owner, SpriteAnimationData[] animations, string startAnimation, bool loopStartAnimation)
				: base(animations, startAnimation, loopStartAnimation) => this.owner = owner;

			public Sprite DefaultSprite { get => defaultSprite; set => defaultSprite = value; }

			protected override void SetSprite(Sprite sprite) => owner.SetSprite(sprite);
		}
	}
}
