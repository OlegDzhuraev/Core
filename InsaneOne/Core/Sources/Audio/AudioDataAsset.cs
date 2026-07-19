using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> ScriptableObject container for <see cref="AudioData"/>, so it can be authored, shared and referenced as a standalone asset.
	/// <see cref="AudioData"/> itself stays a plain serializable class, usable inline (as a field) as before. </summary>
	[CreateAssetMenu(menuName = "InsaneOne/Audio/Audio Data")]
	public sealed class AudioDataAsset : ScriptableObject
	{
		[SerializeField] AudioData data;

		public AudioData Data => data;
	}
}
