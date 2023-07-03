using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> Shakes camera in local coordinates. </summary>
	public sealed class CameraShake
	{
		Timer shakeTimer, frequencyTimer;

		bool isActive;

		Vector2 direction;
		Transform camTransform;

		Vector3 actualPos, targetPos;
		readonly Vector3 startPos;

		ShakeImpulse impulse;

		public CameraShake(Vector3 defaultCameraLocalPos)
		{
			startPos = defaultCameraLocalPos;
		}
		
		public void Shake(ShakeImpulse withImpulse)
		{
			impulse = withImpulse;
			
			frequencyTimer = new Timer(impulse.GetFrequency(0));
			shakeTimer = new Timer(impulse.TimeLength);

			direction = Vector2.zero;
			camTransform = MainCamera.Cached.transform;

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