/*
 * Copyright 2025 Oleg Dzhuraev <godlikeaurora@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using System;
using InsaneOne.Core.Architect;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;

namespace InsaneOne.Core.Ui
{
    /// <summary> Ui screen fader. Requires DOTween to work. If you've DOTween installed, add DOTWEEN compile symbol
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
        
        public void Unfade(float duration = 0.5f)
        {
            fadeImage.DOKill();
            WasStartedUnfade?.Invoke();
            fadeImage.raycastTarget = false;
            
            var seq = DOTween.Sequence();
            seq.Append(fadeImage.DOFade(0f, duration));
            seq.AppendCallback(() => WasEndedUnfade?.Invoke());
        }

        /// <summary> This Init version uses CoreData Fader Tpl prefab as template (you can use other with your own prefab). After Init you also can use ServiceLocator Get method with Fader as T to get inited instance.</summary>
        public static bool Init(out Fader faderInstance)
        {
            return Init(CoreData.Load().UiFaderTpl, out faderInstance);
        }

        /// <summary> After Init you also can use ServiceLocator Get method with Fader as T to get inited instance.</summary>
        public static bool Init(GameObject prefab, out Fader faderInstance)
        {
            if (instance)
            {
                faderInstance = instance;
                return false;
            }

            var go = Instantiate(prefab);
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
    /// <summary> Mock for prefab. Install DOTween to use it. </summary>
    public sealed class Fader : MonoBehaviour { }
}
#endif