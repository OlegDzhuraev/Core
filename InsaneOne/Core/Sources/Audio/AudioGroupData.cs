using UnityEngine.Audio;

namespace InsaneOne.Core
{
	public struct AudioGroupData
	{
		public bool Is3D;
		public float MinDistance3D;
		public float MaxDistance3D;
		public float DopplerLevel;

		public AudioMixerGroup Mixer;

		public static AudioGroupData GetDefault3D()
		{
			return new AudioGroupData
			{
				Is3D = true,
				MinDistance3D = 5f,
				MaxDistance3D = 100f,
				DopplerLevel = 0f,
			};
		}

		public static AudioGroupData GetDefault2D()
		{
			return new AudioGroupData { Is3D = false };
		}
	}
}