using System;
using System.Collections.Generic;
using InsaneOne.Core.Utility;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace InsaneOne.Core
{
	/// <summary> Allows to play sound without using your own GameObjects with AudioSource,
	/// you need to set up audio layer in this class and then just pass AudioClip to Play method. </summary>
	public static class Audio
	{
		const int MaxSourcesInLayer = 24;

		static GlobalAudioData data;

		/// <summary> Call this at game start to init system. </summary>
		public static void Init()
		{
			if (data != null && data.parent)
				return;

			data = new GlobalAudioData { parent = new GameObject("[Sounds]") };
			Object.DontDestroyOnLoad(data.parent);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetOnLoad() => data = null;

		static void CheckInit()
		{
			if (data == null || !data.parent)
				throw new InvalidOperationException("Audio system is not initialized! Call Audio.Init() first.");
		}

		/// <summary> Call this at first to initialize new layer. You can use enum instead of int to make code easier to read.</summary>
		public static void AddLayer(int layer, AudioGroupData groupData, int capacity = 8)
		{
			CheckInit();

			if (data.cachedSources.ContainsKey(layer) || data.layerDatas.ContainsKey(layer))
			{
				CoreUnityLogger.I.Log($"Audio Layer {layer} is already initialized.");
				return;
			}

			var collection = new List<AudioSource>(8);

			data.cachedSources.Add(layer, collection);
			data.layerDatas.Add(layer, groupData);

			capacity = Math.Min(capacity, MaxSourcesInLayer);

			for (var q = 0; q < capacity; q++)
				AddSourceToLayer(layer, groupData, collection);
		}

		static AudioSource AddSourceToLayer(int layer, AudioGroupData groupData, List<AudioSource> layerSources)
		{
			var go = new GameObject($"AudioInLayer_{layer}");
			var source = go.AddComponent<AudioSource>();

			go.transform.SetParent(data.parent.transform);

			source.playOnAwake = false;
			source.loop = false;

			SetupAudioFromData(source, groupData);

			layerSources.Add(source);

			return source;
		}

		static void SetupAudioFromData(AudioSource source, AudioGroupData groupData)
		{
			if (groupData.Mixer != null)
				source.outputAudioMixerGroup = groupData.Mixer;
				
			source.spatialBlend = groupData.Is3D ? 1f : 0f;
			source.dopplerLevel = groupData.DopplerLevel;
			source.minDistance = groupData.MinDistance3D;
			source.maxDistance = groupData.MaxDistance3D;
		}

		/// <summary> Removes layer. Destroys all cached audio sources in specified layer. After remove, you can re-create layer with new settings</summary>
		public static void RemoveLayer(int layer)
		{
			CheckInit();

			if (!data.cachedSources.TryGetValue(layer, out var list))
				return;
			
			foreach (var audioSource in list)
				Object.Destroy(audioSource.gameObject);

			data.cachedSources.Remove(layer);
			data.layerDatas.Remove(layer);
		}

		/// <summary> Plays sound with specified parameters. </summary>
		/// <returns>AudioSource, chosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc). </returns>
		public static AudioSource Play(int layer, AudioClip clip, Vector3 position = default, float volume = 1f,
			float pitchRandom = 0f, bool loop = false)
		{
			CheckInit();

			if (clip == null)
			{
				data.logger.Log("You passed empty clip in Play method — nothing will be played.", LogLevel.Warning);
				return null;
			}

			if (!TryGetFreeSource(layer, out var source))
			{
				var list = Audio.data.cachedSources[layer];

				if (list.Count >= MaxSourcesInLayer)
				{
					data.logger.Log($"No free sources in Audio Layer {layer} and max sources capacity ({MaxSourcesInLayer}) reached! Nothing will be played. Recommended to increase the layer capacity.", LogLevel.Warning);
					return null;
				}

				var groupData = Audio.data.layerDatas[layer];
				source = AddSourceToLayer(layer, groupData, list);

				Audio.data.logger.Log($"No free sources in Audio Layer {layer}! Created new. Recommended to increase default capacity.", LogLevel.Warning);
			}

			pitchRandom = Math.Clamp(pitchRandom, -1f, 1f);

			source.transform.position = position;
			source.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
			source.clip = clip;
			source.volume = Mathf.Clamp(volume, 0f, 1f);
			source.loop = loop;
			source.Play();

			return source;
		}

		/// <summary> Plays sound with parameters, specified in the AudioData. </summary>
		/// <returns>AudioSource, chosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc.). </returns>
		public static AudioSource Play(int layer, AudioData audioData, Vector3 position = default)
		{
			CheckInit();

			if (audioData == null || audioData.GetClipsVariationsAmount() == 0)
			{
				data.logger.Log("Passed AudioData is null or has no audio clips set! Nothing will be played.", LogLevel.Warning);
				return null;
			}
			
			return Play(layer, audioData.GetRandomClip(), position, audioData.Volume, audioData.PitchRandom, audioData.Loop);
		}

		/// <summary> Plays sound with parameters, specified in the AudioData stored inside the passed AudioDataAsset. </summary>
		/// <returns>AudioSource, chosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc.). </returns>
		public static AudioSource Play(int layer, AudioDataAsset audioDataAsset, Vector3 position = default)
		{
			CheckInit();

			if (audioDataAsset == null || audioDataAsset.Data == null)
			{
				data.logger.Log("Passed AudioDataAsset is null or has no AudioData assigned! Nothing will be played.", LogLevel.Warning);
				return null;
			}

			return Play(layer, audioDataAsset.Data, position);
		}

		public static bool TryGetFreeSource(int layer, out AudioSource audioSource)
		{
			audioSource = default;

			CheckInit();

			if (!data.cachedSources.TryGetValue(layer, out var sources))
				throw new InvalidOperationException($"Audio layer {layer} is not initialized! Call Audio.AddLayer first.");

			foreach (var source in sources)
			{
				if (source.isPlaying)
					continue;

				audioSource = source;
				return true;
			}

			return false;
		}

		public static void StopAll(int layer)
		{
			CheckInit();

			if (!data.cachedSources.TryGetValue(layer, out var sources))
				return;

			foreach (var audioSource in sources)
				audioSource.Stop();
		}
		
		/// <summary> Is any source in this layer playing sound. </summary>
		public static bool IsPlaying(int layer)
		{
			CheckInit();

			if (!data.cachedSources.TryGetValue(layer, out var sources))
				return false;

			foreach (var audioSource in sources)
				if (audioSource.isPlaying)
					return true;

			return false;
		}

		public static void SetLogMixerValue(AudioMixer mixer, string exposedParam, float linearValue)
		{
			mixer?.SetFloat(exposedParam, Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f);
		}
	}
}