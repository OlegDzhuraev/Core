using InsaneOne.Core.UI;
using InsaneOne.Core.Utility;
using TMPro;
using UnityEngine;

namespace InsaneOne.Core.Locales
{
    [DisallowMultipleComponent]
    public sealed class LocalizedTMPText : Element<LocalizedTextViewModel>, ILocalized
    {
        [SerializeField, Localized] string localizationId;
        [SerializeField] bool addUiTextIfNotFound = true;
        [SerializeField] bool initializeOnStart = true;

        TMP_Text text;

        void Awake()
        {
            text = GetComponent<TMP_Text>();

            if (addUiTextIfNotFound && !text)
                text = gameObject.AddComponent<TextMeshProUGUI>();
        }

        void Start()
        {
            if (ViewModel is { AutoReload: true } || initializeOnStart && ViewModel == null)
                ReloadLocalization();
        }

        public void ReloadLocalization()
        {
            if (string.IsNullOrWhiteSpace(localizationId) && gameObject.activeSelf)
            {
                CoreUnityLogger.I.Log($"Empty localization id on text object [{name}]!", LogLevel.Warning);
                return;
            }

            if (ViewModel == null)
                SetViewModel(new LocalizedTextViewModel(localizationId));

            if (!Localization.IsLoaded)
            {
                CoreUnityLogger.I.Log($"No localization is loaded, can't apply localization on text object [{name}]!", LogLevel.Error);
                return;
            }
            
            if (text == null && !TryGetComponent(out text))
            {
                CoreUnityLogger.I.Log($"No localization TMP text on object [{name}]", LogLevel.Error);
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