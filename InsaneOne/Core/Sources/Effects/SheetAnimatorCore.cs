using System;
using System.Linq;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> Same flipbook-animation logic as <see cref="SheetAnimator"/>, but as a plain, non-MonoBehaviour class.
	/// Use this when you don't need (or can't have) a Unity component - just call <see cref="Tick"/> yourself every
	/// frame (e.g. with Time.deltaTime), since a plain class has no Unity Update callback of its own. </summary>
	public abstract class SheetAnimatorCore
	{
		public SpriteAnimationData[] Animations { get; set; }

		public string StartAnimation { get; set; }
		public bool LoopStartAnimation { get; set; } = true;

		protected Sprite defaultSprite;

		SpriteAnimationData activeAnimation;
		int frame;
		bool loop;

		float frameTimeLength, frameProgress;

		protected SheetAnimatorCore(SpriteAnimationData[] animations = null, string startAnimation = null, bool loopStartAnimation = true)
		{
			Animations = animations ?? Array.Empty<SpriteAnimationData>();
			StartAnimation = startAnimation;
			LoopStartAnimation = loopStartAnimation;
		}

		public virtual void Init() => PlayStartAnimation();

		/// <summary> Advances the currently playing animation - call this every frame with the elapsed time. </summary>
		public void Tick(float deltaTime)
		{
			if (activeAnimation == null)
				return;

			SetSprite(activeAnimation.Frames[frame]);

			if (frameProgress < frameTimeLength)
			{
				frameProgress += deltaTime;
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

			for (var i = 0; i < Animations.Length; i++)
			{
				var animationData = Animations[i];

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
			if (!string.IsNullOrEmpty(StartAnimation))
				PlayAnimation(StartAnimation, LoopStartAnimation, true);
		}

		public void SetAnimation(SpriteAnimationData data)
		{
			for (var i = 0; i < Animations.Length; i++)
			{
				if (Animations[i].Name == data.Name)
				{
					Animations[i] = data;
					break;
				}
			}

			Animations = Animations.Append(data).ToArray();
		}

		public void Stop()
		{
			SetSprite(defaultSprite);
			loop = false;
		}

		protected abstract void SetSprite(Sprite sprite);
	}
}
