using UnityEngine;

namespace InsaneOne.Core.Components
{
    /// <summary> Use to make transform following without parenting. Can be used for camera follow or something else like this. </summary>
    public sealed class TargetFollow : MonoBehaviour
    {
        [Tooltip("Use lerp to smooth movement. Note that it is not fully correct lerp, it will never reach 100% same position. For another lerp make your own component.")]
        [SerializeField] bool lerp;
        [SerializeField] float lerpSpeed = 10f;
        [SerializeField] bool useLateUpdate;
        
        public Transform Target { get; set; }

        void Update()
        {
            if (!useLateUpdate)
                Process();
        }

        void LateUpdate()
        {
            if (useLateUpdate)
                Process();
        }

        void Process()
        {
            if (!Target)
                return;

            if (lerp)
                transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * lerpSpeed);
            else
                transform.position = Target.position;
        }
    }
}