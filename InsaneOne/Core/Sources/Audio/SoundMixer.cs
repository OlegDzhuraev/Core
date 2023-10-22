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