using System.Collections.Generic;
using InsaneOne.Core.Utility;
using UnityEngine;
using ILogger = InsaneOne.Core.Utility.ILogger;

namespace InsaneOne.Core
{
	internal class GlobalAudioData
	{
		internal readonly Dictionary<int, List<AudioSource>> cachedSources = new ();
		internal readonly Dictionary<int, AudioGroupData> layerDatas = new ();

		/// <summary> All cached sounds parent. </summary>
		internal GameObject parent;

		internal ILogger logger = new CoreUnityLogger();
	}
}