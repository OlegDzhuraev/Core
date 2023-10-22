using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> Use this instead of AudioClip to more complex situations than just clip play. </summary>
	[System.Serializable]
	public class AudioData
	{
		[SerializeField] AudioClip[] clipVariations;
		[SerializeField, Range(0f, 1f)] float pitchRandom;
		[Tooltip("Sometimes sounds is louder or too silent. If you don't want to open some external audio editor for fix, use this field instead.")]
		[SerializeField, Range(0f, 1f)] float volume = 1f;
		[SerializeField] bool loop;

		public float Volume => volume;
		public float PitchRandom => pitchRandom;
		public bool Loop => loop;

		public float GetRandomPitch() => 1f + Random.Range(-pitchRandom, pitchRandom);
		
		public AudioClip GetRandomClip() => clipVariations.Random();
		public AudioClip GetClipById(int id) => clipVariations[id];
		public int GetClipsVariationsAmount() => clipVariations.Length;
	}
}