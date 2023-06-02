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
