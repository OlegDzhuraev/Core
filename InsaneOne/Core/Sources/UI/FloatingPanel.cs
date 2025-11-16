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

namespace InsaneOne.Core.UI
{
    /// <summary> FloatingPanel Can be used for healthbars and some other informers, which should be drawn near 3D world object but in 2D space. </summary>
    [DisallowMultipleComponent]
    public sealed class FloatingPanel : Element<FloatingPanelViewModel>
    {
        public Transform CanvasTransform;
        public RectTransform RectTransform;

        void Awake()
        {
            if (!RectTransform)
                RectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            RectTransform.anchorMin = Vector3.zero;
            RectTransform.anchorMax = Vector3.zero;
        }

        void Update()
        {
            if (!ViewModel.IsTargetExist)
                return;

            var worldTargetPos = ViewModel.FollowTarget.transform.position;
            var screenPos = MainCamera.Cached.WorldToScreenPoint(worldTargetPos + Vector3.up * ViewModel.VerticalOffset);

            RectTransform.anchoredPosition = screenPos / CanvasTransform.localScale.x;
        }
    }

    public class FloatingPanelViewModel
    {
        public bool IsTargetExist => FollowTarget != null;

        public Transform FollowTarget;
        public float VerticalOffset;

        public FloatingPanelViewModel(Transform followTarget, float verticalOffset = 0)
        {
            FollowTarget = followTarget;
            VerticalOffset = verticalOffset;
        }
    }
}