using System;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> Shakes camera in local coordinates. Requires camera to be child of some other transform.
	/// <para>It is not static because you can want to use it with several cameras</para></summary>
	public sealed class CameraShake
	{
		/// <summary> Set false to disable all camera shakes in game. </summary>
		public static bool IsGlobalEnabled = true;

		readonly Transform camTransform;
		readonly Vector3 startPos;
		
		Timer shakeTimer, frequencyTimer;

		bool isActive;

		Vector2 direction;
		
		Vector3 actualPos, targetPos;
		
		ShakeImpulse impulse;

		public CameraShake(Transform cameraTransform)
		{
			camTransform = cameraTransform;
			startPos = cameraTransform.localPosition;
		}
		
		public void Shake(ShakeImpulse withImpulse)
		{
			if (!IsGlobalEnabled)
				return;

			if (!camTransform)
				throw new NullReferenceException("Camera Shake: no camera transform exist! Possibly, camera was destroyed. Cancelled shake.");
			
			if (camTransform.parent == null)
			{
				Debug.LogWarning("Camera Shake: requires camera to be child of some other transform. Actually it is not. Cancelled shake.");
				return;
			}
			
			impulse = withImpulse;
			
			frequencyTimer = new Timer(impulse.GetFrequency(0));
			shakeTimer = new Timer(impulse.TimeLength);
			direction = Vector2.zero;
			isActive = true;

			CalculateNewPosition();
		}

		/// <summary> Call it every frame outside. </summary>
		public void Update()
		{
			if (!isActive || Time.timeScale == 0)
				return;

			if (!shakeTimer.IsReady())
			{
				if (frequencyTimer.IsReady())
				{
					CalculateNewPosition();
					frequencyTimer.SetFullTime(impulse.GetFrequency(GetProgress()));
					frequencyTimer.Restart();
					actualPos = camTransform.localPosition;
				}
				
				camTransform.localPosition = Vector3.Lerp(actualPos, targetPos, GetFrequencyProgress());
				
				shakeTimer.TickIfNotReady();
				frequencyTimer.TickIfNotReady();
				
				return;
			}

			if (!frequencyTimer.IsReady())
			{
				camTransform.localPosition = Vector3.Lerp(actualPos, startPos, GetFrequencyProgress());
				
				frequencyTimer.TickIfNotReady();
				return;
			}
			
			camTransform.localPosition = startPos;
			isActive = false;
		}
		
		void CalculateNewPosition()
		{
			var targetAmplitude = impulse.GetAmplitude(GetProgress());
					
			direction.Randomize(-1f, 1f);
			direction.Normalize();
				
			targetPos = startPos + (Vector3)direction * targetAmplitude;
			actualPos = startPos;
		}

		float GetFrequencyProgress() => Mathf.Clamp(frequencyTimer.GetReloadPercents(), 0, 1);
		float GetProgress() => Mathf.Clamp(shakeTimer.GetReloadPercents(), 0, 1);
	}
}