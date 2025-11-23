using UnityEngine;

namespace InsaneOne.Core.Components
{
    /// <summary> Use to make transform following without parenting. Can be used for camera follow or something else like this. </summary>
    [DisallowMultipleComponent]
    public sealed class TargetFollow : MonoBehaviour
    {
        [Tooltip("Use lerp to smooth movement. Note that this is not fully correct lerp, it will never reach 100% same position. Better not use it for game logic.")]
        [SerializeField] bool lerp;
        [SerializeField] float lerpSpeed = 10f;
        [SerializeField] bool smoothDeltaTime;
        [SerializeField] bool useLateUpdate;
        
        public Transform Target { get; set; }
        public bool Lerp { get => lerp; set => lerp = value; }

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

            var dTime = smoothDeltaTime ? Time.smoothDeltaTime : Time.deltaTime;

            if (lerp)
                transform.position = Vector3.Lerp(transform.position, Target.position, dTime * lerpSpeed);
            else
                transform.position = Target.position;
        }
    }
}