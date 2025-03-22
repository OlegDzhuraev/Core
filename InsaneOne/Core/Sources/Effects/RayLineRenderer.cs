using System;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> Draws a ray line from forward of object and to collider point which was hit. </summary>
	public class RayLineRenderer : MonoBehaviour
	{
		[SerializeField] bool drawAtAwake;
		[Tooltip("Will recheck ray in update and redraw")]
		[SerializeField] bool autoRedraw;

		[SerializeField] float maxDistance = 100;
		[SerializeField] LayerMask layerMask = ~0;
		[SerializeField] bool drawIfNoHit;

		[SerializeField] LineRenderer lineRenderer;

		bool customDirection;
		Vector3 direction;

		void Awake()
		{
			if (!lineRenderer)
				throw new NullReferenceException("No LineRenderer set!");

			lineRenderer.useWorldSpace = true;

			if (drawAtAwake)
				Redraw();

			OnAwake();
		}

		protected virtual void OnAwake() { }

		void Update()
		{
			if (!autoRedraw)
				return;

			Redraw();
		}

		public virtual void Setup(float maxDistance, LayerMask layerMask, bool autoRedraw = false)
		{
			this.maxDistance = maxDistance;
			this.autoRedraw = false;
			this.layerMask = layerMask;

			Redraw();
		}

		/// <summary> Sets custom direction instead of default forward. Redraws after was set.</summary>
		public virtual void SetDirection(Vector3 direction)
		{
			this.direction = direction;
			customDirection = true;

			Redraw();
		}

		public virtual void SetMaterial(Material material)
		{
			lineRenderer.material = material;
		}

		public virtual void Redraw()
		{
			var rayDir = customDirection ? direction : transform.forward;

			if (Physics.Raycast(transform.position, rayDir, out var hit, maxDistance, layerMask))
			{
				lineRenderer.positionCount = 2;
				lineRenderer.SetPosition(0, transform.position);
				lineRenderer.SetPosition(1, hit.point);
			}
			else if (drawIfNoHit)
			{
				lineRenderer.positionCount = 2;
				lineRenderer.SetPosition(0, transform.position);
				lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
			}
			else
			{
				lineRenderer.positionCount = 0;
			}
		}
	}
}