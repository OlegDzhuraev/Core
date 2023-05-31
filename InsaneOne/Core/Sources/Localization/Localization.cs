using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InsaneOne.Core.Locales
{
	public static class Localization
	{
		public static string Language { get; private set; } = "English";
		public static string Filename { get; set; } = "Localization.csv";

		public static event Action WasInitialized;
		
		static string[] loadedLocalizationTexts;

		public static readonly List<string> Languages = new ();
		public static bool IsLoaded;
		
		public static readonly Dictionary<string, string> CachedTexts = new ();

		public static void Initialize()
		{
			if (IsLoaded)
				return;
			
			var path = $"{Application.streamingAssetsPath}/{Filename}";

			if (!File.Exists(path))
				return;
			
			loadedLocalizationTexts = File.ReadAllLines(path, Encoding.UTF8);

			CachedTexts.Clear();
			
			var langs = loadedLocalizationTexts[0].Split(";", StringSplitOptions.RemoveEmptyEntries);
			
			for (var i = 1; i < langs.Length; i++)
				Languages.Add(langs[i]);
			
			IsLoaded = true;
			
			WasInitialized?.Invoke();
		}

		public static void SetLanguage(string lang)
		{
			if (!IsLoaded)
				return;
			
			CachedTexts.Clear();
			
			Language = lang;
			
			var langs = loadedLocalizationTexts[0].Split(";", StringSplitOptions.RemoveEmptyEntries);
			
			int columnId = 1;
			
			for (var i = 1; i < langs.Length; i++)
				if (langs[i] == lang)
				{
					columnId = i;
					break;
				}

			for (int i = 1; i < loadedLocalizationTexts.Length; i++)
			{
				var str = loadedLocalizationTexts[i];
				
				if (str is "" or "\n")
					continue;
				
				var words = str.Split(";", StringSplitOptions.RemoveEmptyEntries);

				var translatedWord = words.Length - 1 >= columnId ? words[columnId] : words[1];
				
				CachedTexts.Add(words[0], translatedWord);
			}
			
			ReloadLocalization();
		}

		public static int GetSafeId(int originalId)
		{
			if (originalId < 0 || originalId >= Languages.Count)
				originalId = 0;

			return originalId;
		}

		public static string GetLangName(int id) => Languages[GetSafeId(id)];

		public static void ReloadLocalization()
		{
			var localizedtexts = GetAllObjectsOfType<MonoBehaviour>().OfType<ILocalized>();
			foreach (var locText in localizedtexts)
				locText.ReloadLocalization();
		}

		public static string GetText(string id) => CachedTexts.TryGetValue(id, out var result) ? result : "No text";
		
		public static bool TryGetText(string id, out string result)
		{
			if (CachedTexts.TryGetValue(id, out result))
				return true;

			result = "No text";

			return false;
		}

		public static void Unload()
		{
			Languages.Clear();
			CachedTexts.Clear();
			IsLoaded = false;
		}
		
		/// <summary> Finds all objects of the desired type on the scene. It also returns objects from Resources. </summary>
		static T[] GetAllObjectsOfType<T>() => Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
	}
}