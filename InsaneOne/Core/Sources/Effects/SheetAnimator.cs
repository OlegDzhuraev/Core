using System;
using System.Linq;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> You can use this for flipbook-animations for Ui sprites and 2D games. </summary>
	public abstract class SheetAnimator : MonoBehaviour
#if PERSEIDS_POOLING
		, PerseidsPooling.IResettable
#endif
	{
		[SerializeField] string startAnimation;
		[SerializeField] bool loopStartAnimation = true;
		[SerializeField] SpriteAnimationData[] animations = Array.Empty<SpriteAnimationData>();

		public SpriteAnimationData[] Animations => animations;
		
		protected Sprite defaultSprite;
		
		SpriteAnimationData activeAnimation;
		int frame;
		bool loop;
		
		float frameTimeLength, frameProgress;

		void Start() => Init();

		protected virtual void Init()
		{
			PlayStartAnimation();
		}

		void Update()
		{
			if (activeAnimation == null)
				return;

			SetSprite(activeAnimation.Frames[frame]);
			
			if (frameProgress < frameTimeLength)
			{
				frameProgress += Time.deltaTime;
				return;
			}
			
			frameProgress = 0;
			frame++;
			
			var endAnimation = false;
			if (frame >= activeAnimation.Frames.Length)
			{
				frame = 0;
				endAnimation = !loop;
			}

			if (endAnimation)
				activeAnimation = null;
		}

		public void PlayAnimation(string animationName, bool loopAnimation = false, bool hardSet = false)
		{
			if (activeAnimation != null && activeAnimation.Name == animationName && !hardSet)
				return;
			
			for (var i = 0; i < animations.Length; i++)
			{
				var animationData = animations[i];

				if (animationData.Name == animationName)
				{
					activeAnimation = animationData;
					frame = 0;
					frameProgress = 0;
					loop = loopAnimation;
					frameTimeLength = 1f / activeAnimation.FramesPerSpeed;
					break;
				}
			}
		}

		public void PlayStartAnimation()
		{
			if (!string.IsNullOrEmpty(startAnimation))
				PlayAnimation(startAnimation, loopStartAnimation, true);
		}

		public void SetAnimation(SpriteAnimationData data)
		{
			for (var i = 0; i < animations.Length; i++)
			{
				if (animations[i].Name == data.Name)
				{
					animations[i] = data;
					break;
				}
			}

			animations = animations.Append(data).ToArray();
		}

		public void Stop()
		{
			SetSprite(defaultSprite);
			loop = false;
		}

		protected abstract void SetSprite(Sprite sprite);
		
#if PERSEIDS_POOLING
		public void ResetPooled() => Init();
#endif
	}
}