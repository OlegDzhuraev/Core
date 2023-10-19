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
	}
}