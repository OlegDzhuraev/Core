namespace InsaneOne.Core.Locales
{
    /// <summary> Allows you to reload the localization when you change the language.  </summary>
    public interface ILocalized
    {
        /// <summary> Reload the localization according to the current language. </summary>
        void ReloadLocalization();
    }
}