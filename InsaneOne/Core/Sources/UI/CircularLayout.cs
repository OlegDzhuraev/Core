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

using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.Ui
{
    public sealed class CircularLayout : MonoBehaviour
    {
        [Tooltip("Layout radius in screen pixels.")]
        [SerializeField, Range(0, 2048)] float radius = 512;
        
        readonly List<RectTransform> layoutElements = new ();
        
        void OnValidate() => ApplyLayout();
        void Start() => ApplyLayout();

        public void ApplyLayout()
        {
            layoutElements.Clear();
            
            for (int i = 0; i < transform.childCount; i++)
            {
                var rectTr = transform.GetChild(i).GetComponent<RectTransform>();
                
                if (rectTr)
                    layoutElements.Add(rectTr);
            }
            
            var amount = layoutElements.Count;
            var scaledPi = Mathf.PI * 2;
            
            for (var i = 0; i < amount; i++)
            {
                var value = i / (float) amount * scaledPi;
                
                var x = Mathf.Sin(value);
                var y = Mathf.Cos(value);
                
                var element = layoutElements[i];
                element.anchoredPosition = new Vector2(x, y) * radius;
            }
        }

        public RectTransform GetNearestToPos(Vector2 pos)
        {
            var minSqrDist = float.MaxValue - 1;
            RectTransform selectedElement = null;
            
            foreach (var elem in layoutElements)
            {
                var sqrMag = ((Vector2)elem.position - pos).sqrMagnitude;
                if (sqrMag < minSqrDist)
                {
                    minSqrDist = sqrMag;
                    selectedElement = elem;
                }
            }

            return selectedElement;
        }
    }
}
