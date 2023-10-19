using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneOne.Core
{
	/// <summary> Allows to play sound without using your own GameObjects with AudioSource,
	/// you need to setup audio layer in this class and then just pass audioclip to Play method. </summary>
	public static class Audio
	{
		const int MaxSourcesInLayer = 24;
		
		static readonly Dictionary<int, List<AudioSource>> cachedSources = new Dictionary<int, List<AudioSource>>();
		static readonly Dictionary<int, AudioGroupData> layerDatas = new Dictionary<int, AudioGroupData>();
		
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
		public static void UpdateLayer(int layer, AudioGroupData data)
		{
			if (!cachedSources.TryGetValue(layer, out var list))
			{
				list = new List<AudioSource>();
				cachedSources.Add(layer, list);
			}
			
			if (!layerDatas.ContainsKey(layer))
			{
				layerDatas.Add(layer, data);
			}
			else
			{
				layerDatas[layer] = data;
				foreach (var source in list)
					SetupAudioFromData(source, data);
			}
		}

		/// <summary> To fully init layer, you need to cache some audio sources for this layer. Call this after UpdateLayer. </summary>
		public static void AddSourcesInLayer(int layer, int amount)
		{
			if (!layerDatas.TryGetValue(layer, out var data))
				throw new NullReferenceException("No audio data for this layer set! Call CreateLayer first.");
			
			if (!cachedSources.TryGetValue(layer, out var list))
				throw new NullReferenceException("No audio list for this layer set! Call CreateLayer first.");

			var soundsLeft = MaxSourcesInLayer - list.Count;
			amount = Math.Min(amount, soundsLeft);
			
			for (int q = 0; q < amount; q++)
			{
				var go = new GameObject($"AudioInLayer_{layer}");
				var source = go.AddComponent<AudioSource>();

				go.transform.SetParent(parent.transform);

				source.playOnAwake = false;
				source.loop = false;

				SetupAudioFromData(source, data);

				list.Add(source);
			}
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

		/// <summary> Destroys all cached audio sources in specified layer. </summary>
		/// <param name="layer"></param>
		public static void ClearLayer(int layer)
		{
			if (!cachedSources.TryGetValue(layer, out var list)) 
				return;
			
			foreach (var audioSource in list)
				GameObject.Destroy(audioSource.gameObject);
				
			list.Clear();
		}
		
		/// <summary> Plays sound with specified parameters. </summary>
		/// <returns>AudioSource, choosen to play. You can handle its changes manually by your code (stop after N seconds, for example, etc). </returns>
		public static AudioSource Play(int layer, AudioClip clip, Vector3 position = default, float volume = 1f, float pitchRandom = 0f, bool loop = false)
		{
			if (clip == null)
			{
				Debug.LogWarning("You passed empty clip in Play method - nothing will be played.");
				return null;
			}
			
			if (TryGetFreeSource(layer, out var source))
			{
				pitchRandom = Math.Clamp(pitchRandom, -1f, 1f);
				
				source.transform.position = position;
				source.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
				source.clip = clip;
				source.volume = volume;
				source.loop = loop;
				source.Play();

				return source;
			}

			return null;
		}

		static bool TryGetFreeSource(int layer, out AudioSource audioSource)
		{
			audioSource = default;
			
			if (!cachedSources.TryGetValue(layer, out var sources))
				return false;

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
	}
}