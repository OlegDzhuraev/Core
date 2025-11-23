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

		/// <summary> Call this at first to initialize new layer. You can use enum instead of int to make code easier to read.</summary>
		public static void AddLayer(int layer, AudioGroupData groupData, int capacity = 8)
		{
			if (data.cachedSources.ContainsKey(layer) || data.layerDatas.ContainsKey(layer))
			{
				CoreUnityLogger.I.Log($"Audio Layer {layer} already initialized.");
				return;
			}

			var collection = new List<AudioSource>(8);

			data.cachedSources.Add(layer, collection);
			data.layerDatas.Add(layer, groupData);

			var soundsLeft = MaxSourcesInLayer - collection.Count;
			capacity = Math.Min(capacity, soundsLeft);

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
			if (clip == null)
			{
				data.logger.Log("You passed empty clip in Play method — nothing will be played.", LogLevel.Warning);
				return null;
			}

			if (!TryGetFreeSource(layer, out var source))
			{
				var list = Audio.data.cachedSources[layer];
				var data = Audio.data.layerDatas[layer];
				source = AddSourceToLayer(layer, data, list);

				Audio.data.logger.Log($"No free sources in Audio Layer {layer}! Created new. Recommended to increase default capacity.", LogLevel.Warning);
			}

			pitchRandom = Math.Clamp(pitchRandom, -1f, 1f);

			source.transform.position = position;
			source.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
			source.clip = clip;
			source.volume = volume;
			source.loop = loop;
			source.Play();

			return source;
		}

		/// <summary> Plays sound with parameters, specified in the AudioData. </summary>
		/// <returns>AudioSource, chosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc.). </returns>
		public static AudioSource Play(int layer, AudioData audioData, Vector3 position = default)
		{
			if (audioData.GetClipsVariationsAmount() == 0)
			{
				data.logger.Log("No audio clips set in the passed AudioData! Nothing will be played.", LogLevel.Warning);
				return null;
			}
			
			return Play(layer, audioData.GetRandomClip(), position, audioData.Volume, audioData.PitchRandom, audioData.Loop);
		}

		public static bool TryGetFreeSource(int layer, out AudioSource audioSource)
		{
			audioSource = default;
			
			if (!data.cachedSources.TryGetValue(layer, out var sources))
				throw new NullReferenceException($"No audio sources in layer {layer}!");

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
			if (!data.cachedSources.TryGetValue(layer, out var sources))
				return;

			foreach (var audioSource in sources)
				audioSource.Stop();
		}
		
		/// <summary> Is any source in this layer playing sound. </summary>
		public static bool IsPlaying(int layer)
		{
			if (!data.cachedSources.TryGetValue(layer, out var sources))
				return false;

			foreach (var audioSource in sources)
				if (audioSource.isPlaying)
					return true;

			return false;
		}

		public static void SetLogMixerValue(AudioMixer mixer, string exposedParam, float linearValue)
		{
			mixer?.SetFloat(exposedParam, Mathf.Log(linearValue) * 20);
		}
	}
}