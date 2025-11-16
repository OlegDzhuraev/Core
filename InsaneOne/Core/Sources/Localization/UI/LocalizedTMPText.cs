using InsaneOne.Core.UI;
using TMPro;
using UnityEngine;

namespace InsaneOne.Core.Locales
{
    public sealed class LocalizedTMPText : Element<LocalizedTextViewModel>, ILocalized
    {
        [SerializeField] string localizationId;

        TMP_Text text;

        void Awake() => text = GetComponent<TMP_Text>();
        void Start()
        {
            if (ViewModel != null && ViewModel.AutoReload)
                ReloadLocalization();
        }

        public void ReloadLocalization()
        {
            if (ViewModel == null)
                SetViewModel(new LocalizedTextViewModel(localizationId));

            if (!Localization.IsLoaded)
            {
                Debug.LogError($"No localization is loaded, can't apply localization on text object [{name}]!");
                return;
            }
            
            if (text == null && !TryGetComponent(out text))
            {
                Debug.LogError($"No localization TMP text on object [{name}]");
                return;
            }

            text.text = ViewModel!.LocalizedText;
            text.parseCtrlCharacters = true;
        }

        public override void OnViewModelChanged(LocalizedTextViewModel viewModel)
        {
            if (viewModel.AutoReload)
                ReloadLocalization();
        }
    }

    public class LocalizedTextViewModel
    {
        public string LocalizedText => LocaleId.Localize();

        public readonly string LocaleId;
        public readonly bool AutoReload;

        public LocalizedTextViewModel(string localeId, bool autoReload = false)
        {
            LocaleId = localeId;
            AutoReload = autoReload;
        }
    }
}