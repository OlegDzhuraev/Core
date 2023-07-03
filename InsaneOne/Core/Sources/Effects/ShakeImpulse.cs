using UnityEngine;

namespace InsaneOne.Core.Effects
{
	[System.Serializable]
	public sealed class ShakeImpulse
	{
		public ShakeImpulse()
		{ }

		public ShakeImpulse(AnimationCurve shakeAmplitude, AnimationCurve shakeFrequency, float shakeTime)
		{
			amplitude = shakeAmplitude;
			frequency = shakeFrequency;
			timeLength = shakeTime;
		}
		
		[SerializeField] AnimationCurve amplitude = AnimationCurve.Linear(0f, 0.25f, 1f, 0.1f);
		[SerializeField] AnimationCurve frequency = AnimationCurve.Constant(0f, 1f, 0.05f);
		[SerializeField] float timeLength = 0.25f;

		public float TimeLength => timeLength;
		
		public float GetAmplitude(float progress) => amplitude.Evaluate(progress);
		public float GetFrequency(float progress) => frequency.Evaluate(progress);
	}
}