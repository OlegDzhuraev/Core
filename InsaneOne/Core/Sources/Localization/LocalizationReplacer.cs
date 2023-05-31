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
			changeId = 0;
		}
		
		public void ReplaceNext(string replaceTo)
		{
			Result = Result.Replace($"{replacementSymbol}{changeId}", $"{Preprocess}{replaceTo}{Postprocess}");
			changeId++;
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
	}
}