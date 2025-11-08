namespace InsaneOne.Core.Locales
{
	/// <summary> Allows to replace special symbols in string with your text. </summary>
	public sealed class LocalizationReplacer
	{
		public string Result { get; private set; }
		public string Preprocess { get; set; }
		public string Postprocess { get; set; }
		
		readonly string replacementSymbol;
		int changeId;

		public LocalizationReplacer(string originalString, string replacementSymbol = "#")
		{
			Result = originalString;
			this.replacementSymbol = replacementSymbol;
			ResetChangeId();
		}

		/// <summary> Replaces non-unique keys, for which is numbers used to. For example, #0, #1, #2, etc. Every replace increases counter by 1. </summary>
		public void ReplaceNext(string replaceTo)
		{
			Result = Result.Replace($"{replacementSymbol}{changeId}", $"{Preprocess}{replaceTo}{Postprocess}");
			changeId++;
		}

		/// <summary> Replaces specific key with your string. Keys always starts with replacementSymbol (# by default), but you do not need to write it in the key — it will be added automatically. </summary>
		public void ReplaceKey(string key, string replaceTo)
		{
			if (!key.StartsWith(replacementSymbol))
				key = $"{replacementSymbol}{key}";

			Result = Result.Replace(key, $"{Preprocess}{replaceTo}{Postprocess}");
		}
		
		/// <summary> Allows to replace occurrences of #LOC_someLocalizationId with other localization with same id. </summary>
		public void ReplaceLocalizedStrings()
		{
			const string symbol = "#LOC_";

			if (!Result.Contains(symbol))
				return;
			
			foreach (var (id, text) in Localization.CachedTexts)
				Result = Result.Replace($"{symbol}{id}", $"{Preprocess}{text}{Postprocess}");
		}

		public void ResetChangeId() => changeId = 0;
	}
}