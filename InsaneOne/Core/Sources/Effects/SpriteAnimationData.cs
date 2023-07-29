using System;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	[Serializable]
	public class SpriteAnimationData
	{
		public string Name;
		public Sprite[] Frames;
		[Range(0.01f, 60)]
		public float FramesPerSpeed = 5;
	}
}