using UnityEngine;
using System;
using InsaneOne.Core.Architect;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;

namespace InsaneOne.Core.Ui
{
    /// <summary> Ui screen fader. Requires DOTween to work. If you're had DOTween installed,add DOTWEEN compile symbol
    /// to the Project Settings and check AsmDef InsaneOne.Core referenced to the DoTween. </summary>
    public sealed class Fader : MonoBehaviour
    {
        public event Action WasStartedFade, WasEndedFade, WasStartedUnfade, WasEndedUnfade;
        static Fader instance;

        [SerializeField] Image fadeImage;
        [SerializeField] bool hideOnStart = true;

        void Start()
        {
            if (hideOnStart)
            {
                fadeImage.raycastTarget = false;
                SetUnfadedState();
            }
        }

        void OnDestroy() => ServiceLocator.Unregister<Fader>();

        public void Fade(float duration = 0.5f)
        {
            fadeImage.DOKill();
            WasStartedFade?.Invoke();
            fadeImage.raycastTarget = true;
            
            var seq = DOTween.Sequence();
            seq.Append(fadeImage.DOFade(1f, duration));
            seq.AppendCallback(() => WasEndedFade?.Invoke());
        }

        public void SetFadedState()
        {
            var c = fadeImage.color;
            fadeImage.color = new Color(c.r, c.g, c.b, 1f);
        }
        
        public void SetUnfadedState()
        {
            var c = fadeImage.color;
            fadeImage.color = new Color(c.r, c.g, c.b, 0f);
        }
        
        public void Unfade(float duration)
        {
            fadeImage.DOKill();
            WasStartedUnfade?.Invoke();
            fadeImage.raycastTarget = false;
            
            var seq = DOTween.Sequence();
            seq.Append(fadeImage.DOFade(0f, duration));
            seq.AppendCallback(() => WasEndedUnfade?.Invoke());
        }

        public static bool Init(out Fader faderInstance)
        {
            if (instance)
            {
                faderInstance = instance;
                return false;
            }

            var go = Instantiate(ServiceLocator.Get<CoreData>().UiFaderTpl);
            DontDestroyOnLoad(go);

            instance = faderInstance = go.GetComponent<Fader>();
            ServiceLocator.Register(faderInstance);
            
            return true;
        }
        
        void OnApplicationQuit() => instance = null;
    }
}
#else 
namespace InsaneOne.Core.Ui
{
    /// <summary> Mock for prefab. Install DOTween. </summary>
    public sealed class Fader : MonoBehaviour { }
}
#endif