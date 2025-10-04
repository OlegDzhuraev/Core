using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace InsaneOne.Core
{
	/// <summary> Allows to play sound without using your own GameObjects with AudioSource,
	/// you need to set up audio layer in this class and then just pass AudioClip to Play method. </summary>
	public static class Audio
	{
		const int MaxSourcesInLayer = 24;
		
		static readonly Dictionary<int, List<AudioSource>> cachedSources = new ();
		static readonly Dictionary<int, AudioGroupData> layerDatas = new ();
		
		/// <summary> All cached sounds parent. </summary>
		static GameObject parent;
		
		/// <summary> Call this at game start to init system. </summary>
		public static void Init()
		{
			if (parent)
				return;
			
			cachedSources.Clear();
			layerDatas.Clear();
			
			parent = new GameObject("[Sounds]");
			GameObject.DontDestroyOnLoad(parent);
		}

		/// <summary> Call this at first to initialize new layer. You can use enum instead of int to make code easier to read.</summary>
		public static void AddLayer(int layer, AudioGroupData data, int capacity = 8)
		{
			if (cachedSources.ContainsKey(layer) || layerDatas.ContainsKey(layer))
			{
				Debug.Log($"Audio Layer {layer} already initialized.");
				return;
			}

			var list = new List<AudioSource>();
			cachedSources.Add(layer, list);

			layerDatas.Add(layer, data);

			var soundsLeft = MaxSourcesInLayer - list.Count;
			capacity = Math.Min(capacity, soundsLeft);

			for (var q = 0; q < capacity; q++)
				AddSourceToLayer(layer, data, list);
		}

		static AudioSource AddSourceToLayer(int layer, AudioGroupData data, List<AudioSource> layerSources)
		{
			var go = new GameObject($"AudioInLayer_{layer}");
			var source = go.AddComponent<AudioSource>();

			go.transform.SetParent(parent.transform);

			source.playOnAwake = false;
			source.loop = false;

			SetupAudioFromData(source, data);

			layerSources.Add(source);

			return source;
		}

		static void SetupAudioFromData(AudioSource source, AudioGroupData data)
		{
			if (data.Mixer != null)
				source.outputAudioMixerGroup = data.Mixer;
				
			source.spatialBlend = data.Is3D ? 1f : 0f;
			source.dopplerLevel = data.DopplerLevel;
			source.minDistance = data.MinDistance3D;
			source.maxDistance = data.MaxDistance3D;
		}

		/// <summary> Removes layer. Destroys all cached audio sources in specified layer. After remove, you can re-create layer with new settings</summary>
		public static void RemoveLayer(int layer)
		{
			if (!cachedSources.TryGetValue(layer, out var list))
				return;
			
			foreach (var audioSource in list)
				GameObject.Destroy(audioSource.gameObject);

			cachedSources.Remove(layer);
			layerDatas.Remove(layer);
		}

		/// <summary> Plays sound with specified parameters. </summary>
		/// <returns>AudioSource, chosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc). </returns>
		public static AudioSource Play(int layer, AudioClip clip, Vector3 position = default, float volume = 1f,
			float pitchRandom = 0f, bool loop = false)
		{
			if (clip == null)
			{
				Debug.LogWarning("You passed empty clip in Play method - nothing will be played.");
				return null;
			}

			if (!TryGetFreeSource(layer, out var source))
			{
				var list = cachedSources[layer];
				var data = layerDatas[layer];
				source = AddSourceToLayer(layer, data, list);

				Debug.LogWarning($"No free sources in Audio Layer {layer}! Created new. Recommended to increase default capacity.");
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
		/// <returns>AudioSource, choosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc). </returns>
		public static AudioSource Play(int layer, AudioData data, Vector3 position = default)
		{
			if (data.GetClipsVariationsAmount() == 0)
			{
				Debug.LogWarning("No audio clips set in the passed AudioData! Nothing will be played.");
				return null;
			}
			
			return Play(layer, data.GetRandomClip(), position, data.Volume, data.PitchRandom, data.Loop);
		}

		public static bool TryGetFreeSource(int layer, out AudioSource audioSource)
		{
			audioSource = default;
			
			if (!cachedSources.TryGetValue(layer, out var sources))
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
			if (!cachedSources.TryGetValue(layer, out var sources))
				return;

			foreach (var audioSource in sources)
				audioSource.Stop();
		}
		
		/// <summary> Is any source in this layer playing sound. </summary>
		public static bool IsPlaying(int layer)
		{
			if (!cachedSources.TryGetValue(layer, out var sources))
				return false;

			foreach (var audioSource in sources)
				if (audioSource.isPlaying)
					return true;

			return false;
		}

		public static void SetLogMixerValue(AudioMixer mixer, string exposedParam, float linearValue)
		{
			mixer.SetFloat(exposedParam, Mathf.Log(linearValue) * 20);
		}
	}
}