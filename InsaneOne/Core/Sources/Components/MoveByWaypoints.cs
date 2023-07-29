using System;
using UnityEngine;

namespace Features.Effects
{
    /// <summary> Will move this object by a given waypoints list. This can be most useful for simple animation effects, etc. Like basic tween.</summary>
    public sealed class MoveByWaypoints : MonoBehaviour
    {
        public event Action StartedMove;
        public event Action<Transform> ReachedWaypoint;

        public float Speed
        {
            get => speed;
            set => speed = value;
        }
        
        /// <summary> Can be used to randomize movement a bit. Path will be offseted with this value. </summary>
        public Vector3 Offset { get; set; }
   
        [SerializeField] Transform[] waypoints = Array.Empty<Transform>();
        [SerializeField] bool activateOnStart = true;
        [SerializeField] float speed = 3f;
        [Tooltip("Minimum distance to the current waypoint to start move to the next waypoint.")]
        [SerializeField] float minDist = 0.25f;
        [SerializeField] bool repeat = true;
        [SerializeField] bool startFromStartPoint;
        
        [Header("Rotation")]
        [SerializeField] bool rotateToNextPoint;
        [Tooltip("In angle/sec.")]
        [SerializeField] float rotationSpeed = 360f;

        [Header("Threading")]
        [Tooltip("If you want to run some of calculations manually (for multi-thread as example), check this.")]
        bool calculateManually;

        float sqrMinDist;

        int currentWaypointId;

        float sqrDistance;
        bool isReachedWaypoint;

        void Start()
        {
            sqrMinDist = (float) Math.Pow(minDist, 2);
            
            if (!activateOnStart)
            {
                enabled = false;
                return;
            }
            
            Restart();
        }

        void Update()
        {
            if (!calculateManually)
                CalculateDistance();
            
            if (isReachedWaypoint)
                ReachWaypoint();
            else
                Move();
        }

        void ReachWaypoint()
        {
            currentWaypointId++;
                
            if (currentWaypointId >= waypoints.Length)
            {
                if (repeat)
                    currentWaypointId = 0;
                else
                    enabled = false;
            }
            
            ReachedWaypoint?.Invoke(waypoints[currentWaypointId]);
        }
        
        void Move()
        {
            var smoothDt = Time.smoothDeltaTime;
            var pos = transform.position;
            var nextPos = waypoints[currentWaypointId].position + Offset;

            transform.position = Vector3.MoveTowards(pos, nextPos, smoothDt * speed);

            if (rotateToNextPoint)
            {
                var direction = (nextPos - pos).normalized;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), smoothDt * rotationSpeed);
            }
        }

        public void CalculateDistance()
        {
            sqrDistance = (transform.position - (waypoints[currentWaypointId].position + Offset)).sqrMagnitude;

            isReachedWaypoint = sqrDistance <= sqrMinDist;
        }

        public void SetWaypoints(Transform[] newWaypoints, bool restart = false)
        {
            waypoints = newWaypoints;

            if (restart)
                Restart();
        }
        
        public void Restart()
        {
            currentWaypointId = 0;

            if (startFromStartPoint)
                transform.position = waypoints[currentWaypointId].position;
            
            StartedMove?.Invoke();
        }

        void OnDrawGizmosSelected()
        {
            if (waypoints.Length == 0)
                return;
            
            for (int i = 1; i < waypoints.Length; i++)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i - 1].position);
            
            Gizmos.DrawLine(waypoints[0].position, waypoints[^1].position);
        }
    }
}