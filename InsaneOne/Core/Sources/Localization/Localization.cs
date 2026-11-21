using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InsaneOne.Core.Utility;
using UnityEngine;
#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
using UnityEngine.Networking;
#endif
using ILogger = InsaneOne.Core.Utility.ILogger;

namespace InsaneOne.Core.Locales
{
	/// <summary> Use to localize game text with .csv-file. Supports fallback to original language. </summary>
	public static class Localization
	{
		const char SplitSymbol = ';';
		const string DefaultLanguage = "English";
		static readonly Encoding TextEncoding = Encoding.UTF8;

		public static event Action WasInitialized;

		public static string Language { get; private set; } = DefaultLanguage;
		public static List<string> Languages = new ();
		public static bool IsLoaded { get; private set; }
		public static bool IsLanguageSetAtLeastOnce { get; private set; }
		
		public static readonly Dictionary<string, string> CachedTexts = new ();

		static string LanguagesRow => loadedLocalizationTexts[0];

		static string[] loadedLocalizationTexts;

		static ILogger logger = new CoreUnityLogger();

		/// <summary>Initializes localization. Call this once at the game initialization. </summary>
		public static void Initialize(string fileName = "Localization.csv", ILogger customLogger = default)
		{
			if (customLogger != default)
				logger = customLogger;

			if (IsLoaded)
			{
				logger.Log("Localization already loaded — don't needed to call initialize method again.", LogLevel.Warning);
				return;
			}

			var path = Path.Combine(Application.streamingAssetsPath, fileName);

			CachedTexts.Clear();

			loadedLocalizationTexts = ReadAllLines(path);

			if (loadedLocalizationTexts.Length == 0 || string.IsNullOrWhiteSpace(LanguagesRow))
				throw new Exception("Localization file is empty or has no languages header row!");

			var languages = LanguagesRow.Split(SplitSymbol, StringSplitOptions.None);

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
			var isLanguageFound = false;

			for (var i = 0; i < Languages.Count; i++)
				if (Languages[i] == lang)
				{
					columnId = i + 1; // + 1 because Languages list doesn't contain first column
					isLanguageFound = true;
					break;
				}

			if (!isLanguageFound)
				logger.Log($"Language '{lang}' was not found in the loaded localization file! Falling back to the first language column.", LogLevel.Warning);

			for (int i = 1; i < loadedLocalizationTexts.Length; i++)
			{
				var str = loadedLocalizationTexts[i];

				if (str == "")
					continue;

				var elements = str.Split(SplitSymbol, StringSplitOptions.None);

				if (elements.Length is 0 || string.IsNullOrWhiteSpace(elements[0]))
					continue;

				if (elements.Length < 2) // row has no translations at all, not even for the fallback language
					continue;

				var translatedWord = elements.Length - 1 >= columnId ? elements[columnId] : elements[1]; // fallback to first language

				if (string.IsNullOrWhiteSpace(translatedWord)) // empty cell for the chosen language - fallback to first language too
					translatedWord = elements[1];

				CachedTexts[elements[0]] = translatedWord;
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

		/// <summary> Reloads localization on all currently loaded (scene) objects implementing <see cref="ILocalized"/>,
		/// including inactive ones. Unlike Resources.FindObjectsOfTypeAll, this doesn't touch assets/prefabs sitting in memory. </summary>
		public static void ReloadLocalization()
		{
			var localizedTexts = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ILocalized>();
			foreach (var locText in localizedTexts)
				locText.ReloadLocalization();
		}

		public static string GetText(string id) => CachedTexts.GetValueOrDefault(id, id);
		
		public static bool TryGetText(string id, out string result)
		{
			if (CachedTexts.TryGetValue(id, out result))
				return true;

			result = id;
			return false;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetOnLoad() => Unload();

		/// <summary> Call this on game closing. </summary>
		public static void Unload()
		{
			Languages.Clear();
			CachedTexts.Clear();
			IsLoaded = false;
			IsLanguageSetAtLeastOnce = false;
			Language = DefaultLanguage;
			loadedLocalizationTexts = null;
			WasInitialized = null;
		}

#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
		/// <summary> On Android, Streaming Assets are packed inside the compressed APK/AAB, and on WebGL they are served
		/// as a remote URL - in both cases plain System.IO can't read them, so UnityWebRequest has to be used instead. </summary>
		static string[] ReadAllLines(string path)
		{
			using var request = UnityWebRequest.Get(path);
			request.SendWebRequest();

			while (!request.isDone) { }

			if (request.result != UnityWebRequest.Result.Success)
				throw new Exception($"Localization file not found at Streaming Assets folder! ({request.error})");

			return request.downloadHandler.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		}
#else
		static string[] ReadAllLines(string path)
		{
			if (!File.Exists(path))
				throw new Exception("Localization file not found at Streaming Assets folder!");

			return File.ReadAllLines(path, TextEncoding);
		}
#endif
	}
}