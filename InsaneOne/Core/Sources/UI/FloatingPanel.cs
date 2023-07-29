using UnityEngine;

namespace InsaneOne.Core.UI
{
    /// <summary> FloatingPanel Can be used for healthbars and some other informers, which should be drawn near 3D world object but in 2D space. </summary>
    public sealed class FloatingPanel : MonoBehaviour
    {
        public Transform CanvasTransform;
        public RectTransform RectTransform;
        public Transform FollowTarget;
        public float VerticalOffset;

        void Start()
        {
            RectTransform.anchorMin = Vector3.zero;
            RectTransform.anchorMax = Vector3.zero;
        }

        void Update()
        {
            if (!FollowTarget)
                return;

            var worldTargetPos = FollowTarget.transform.position;
            var screenPos = MainCamera.Cached.WorldToScreenPoint(worldTargetPos + Vector3.up * VerticalOffset);

            RectTransform.anchoredPosition = screenPos / CanvasTransform.localScale.x;
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}