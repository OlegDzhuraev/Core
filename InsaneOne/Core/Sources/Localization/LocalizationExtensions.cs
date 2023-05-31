namespace InsaneOne.Core.Locales
{
    public static class LocalizationExtensions
    {
        /// <summary> Gets the text in the desired language, tied to this string id, from the localization parameters. </summary>
        public static string Localize(this string textId) => Localization.GetText(textId);
    }
}