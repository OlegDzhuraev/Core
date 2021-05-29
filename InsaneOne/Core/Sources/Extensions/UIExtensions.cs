using UnityEngine;

namespace InsaneOne.Core
{
    public static class UIExtensions
    {
        /// <summary>Returns UI-element position relative to canvas. Used for anchoredPosition with Camera and World Space Canvases. </summary>
        public static Vector2 GetPositionRelativeToCanvas(this RectTransform rectTransform, RectTransform parentCanvas)
        {
            var canvasSize = parentCanvas.sizeDelta;

            return parentCanvas.InverseTransformPoint(rectTransform.position) + new Vector3(canvasSize.x, canvasSize.y, 0) / 2f;
        }
    }
}