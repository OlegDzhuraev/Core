using System;
using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> Runtime only. Not serialized. </summary>
	public sealed class SoundMixer
	{
		/// <summary> How strong will be mixed two audiosources, if MixValue is centered between them. 1 will give 50% volume, 0 will give 0%. </summary>
		public float OverMix { get; private set; }
		public float MixValue { get; private set; }

		readonly AudioSource[] mixedSources;
		readonly float maxDist;
		readonly float calculationAmount;

		float finalMaxDist;
		
		public SoundMixer(params AudioSource[] sourcesToMix)
		{
			mixedSources = sourcesToMix;
			
			maxDist = 1f / mixedSources.Length;
			calculationAmount = mixedSources.Length - 1f;
			
			SetOverMix(1f);
		}
		
		public void UpdateMix(float value)
		{
			MixValue = value;

			for (var q = 0; q < mixedSources.Length; q++)
			{
				var position = q / calculationAmount;
				var strength = finalMaxDist - Mathf.Abs(Mathf.Clamp(position - MixValue, -finalMaxDist, finalMaxDist));

				mixedSources[q].volume = strength / finalMaxDist;
			}
		}

		/// <summary> This method allows to mix directly two audio sources, ignoring others.
		/// To work correctly, your actual mix value should be equal to passed here start value.
		/// "start" and "target" - mix progress values (so, if there for example 3 Audio Sources - 0 for first source, 0.5 - second, 1 - third one)
		/// Note: This method ignores OverMix value for now.</summary>
		public void MixTwo(float start, float target, float progress)
		{
			const float threshold = 0.02f;

			if (Math.Abs(start - target) <= threshold)
				return;

			MixValue = Mathf.Lerp(start, target, progress);
			
			for (var q = 0; q < mixedSources.Length; q++)
			{
				var position = q / calculationAmount;
				var source = mixedSources[q];
				
				if (Mathf.Abs(position - start) <= threshold)
					source.volume = Mathf.Lerp(1f, 0, progress);
				else if (Math.Abs(position - target) <= threshold)
					source.volume = Mathf.Lerp(0f, 1f, progress);
				else
					source.volume = 0;
			}
		}

		public void Play()
		{
			foreach (var mixedSource in mixedSources)
				mixedSource.Play();
		}

		public void Stop()
		{
			foreach (var mixedSource in mixedSources)
				mixedSource.Stop();
		}

		/// <summary> Allowed only range between 0 - 2. 0 for 0% volume, 1 for 50%, 2 for 100%.</summary>
		public void SetOverMix(float value)
		{
			OverMix = Mathf.Clamp(value, 0f, 2f);
			finalMaxDist = maxDist + maxDist * OverMix;
		}
	}
}