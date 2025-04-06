using TMPro;
using UnityEngine;

namespace InsaneOne.Core.Locales
{
    public sealed class LocalizedTMPText : MonoBehaviour, ILocalized
    {
        [SerializeField] string localizationId;

        TMP_Text text;

        void Awake() => text = GetComponent<TMP_Text>();
        void Start() => ReloadLocalization();

        public void ReloadLocalization()
        {
            if (!Localization.IsLoaded)
                return;
            
            if (text == null)
                text = GetComponent<TMP_Text>();

            if (text == null)
            {
                Debug.LogWarning("No localization TMP text on object " + name);
                return;
            }

            var locale = localizationId.Localize();
            
            if (locale != Localization.NoLocalizationText)
                text.text = localizationId.Localize();

            text.parseCtrlCharacters = true;
        }

        public void SetCustomId(string id, bool reload = false)
        {
            localizationId = id;

            if (reload)
                ReloadLocalization();
        }
    }
}