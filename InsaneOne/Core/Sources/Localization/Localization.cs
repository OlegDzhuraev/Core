using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InsaneOne.Core.Locales
{
	/// <summary> Use to localize game text with .csv-file. Supports fallback to original language. </summary>
	public static class Localization
	{
		const char SplitSymbol = ';';

		public static string Language { get; private set; } = "English";

		public static event Action WasInitialized;

		public static List<string> Languages = new ();
		public static bool IsLoaded { get; private set; }
		public static bool IsLanguageSetAtLeastOnce { get; private set; }
		
		public static readonly Dictionary<string, string> CachedTexts = new ();

		static string LanguagesRow => loadedLocalizationTexts[0];

		static string[] loadedLocalizationTexts;

		/// <summary>Initializes localization. Call this once at the game initialization. </summary>
		public static void Initialize(string fileName = "Localization.csv")
		{
			if (IsLoaded)
			{
				Debug.LogWarning("Localization already loaded - don't needed to call initialize method again.");
				return;
			}
			
			var path = Path.Combine(Application.streamingAssetsPath, fileName);

			if (!File.Exists(path))
				throw new Exception("Localization file not found at Streaming Assets folder!");

			CachedTexts.Clear();

			loadedLocalizationTexts = File.ReadAllLines(path, Encoding.UTF8);
			var languages = LanguagesRow.Split(SplitSymbol, StringSplitOptions.RemoveEmptyEntries);

			Languages = languages.TakeLast(languages.Length - 1).ToList();
			
			IsLoaded = true;
			WasInitialized?.Invoke();
		}

		public static void SetLanguage(string lang)
		{
			if (!IsLoaded)
				throw new Exception("Localization was not initialized!");
			
			CachedTexts.Clear();
			
			Language = lang;

			var columnId = 1;

			for (var i = 0; i < Languages.Count; i++)
				if (Languages[i] == lang)
				{
					columnId = i + 1; // + 1 because Languages list doesn't contain first column
					break;
				}

			for (int i = 1; i < loadedLocalizationTexts.Length; i++)
			{
				var str = loadedLocalizationTexts[i];
				
				if (str is "" or "\n")
					continue;
				
				var elements = str.Split(SplitSymbol, StringSplitOptions.RemoveEmptyEntries);

				if (elements.Length is 0 || string.IsNullOrWhiteSpace(elements[0]))
					continue;

				var translatedWord = elements.Length - 1 >= columnId ? elements[columnId] : elements[1]; // fallback to first language
				CachedTexts.Add(elements[0], translatedWord);
			}

			IsLanguageSetAtLeastOnce = true;
			ReloadLocalization();
		}

		/// <summary>Returns exist language id, even if wrong was passed as argument. </summary>
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

		public static string GetText(string id) => CachedTexts.GetValueOrDefault(id, "No localization");
		
		public static bool TryGetText(string id, out string result)
		{
			if (CachedTexts.TryGetValue(id, out result))
				return true;

			result = "No localization";
			return false;
		}

		/// <summary> Call this on game closing. </summary>
		public static void Unload()
		{
			Languages.Clear();
			CachedTexts.Clear();
			IsLoaded = false;
			IsLanguageSetAtLeastOnce = false;
		}
		
		/// <summary> Finds all objects of the desired type on the scene. It also returns objects from Resources. </summary>
		static T[] GetAllObjectsOfType<T>() => Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
	}
}