using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary>This class describes timer. Every time when you need to do some periodic action with delay using Time.deltaTime, but do not want to use coroutine, you can use this timer. </summary>
	[System.Serializable]
	public class Timer
	{
		float fullTime, timeLeft;
		TimerCallback timerCallback;

		public delegate void TimerCallback();

		public Timer(float timeValue, TimerCallback timerCallback = null)
		{
			SetFullTime(timeValue);
			timeLeft = timeValue;

			if (timerCallback != null)
				SetTimerCallBack(timerCallback);
		}
		
		/// <summary> Simple tick timer. Will reset after reaches zero and start again. In reset frame method IsReady will return true. </summary>
		public void Tick()
		{
			if (timeLeft > 0)
			{
				timeLeft -= Time.deltaTime;
			}
			else
			{
				timeLeft = fullTime;
				timerCallback?.Invoke();
			}
		}

		/// <summary> Tick only if not finished. It requires manual timer reset. Can be used for some mechanics like skills reload, which need to be reloaded without stable period. </summary>
		public void TickIfNotReady()
		{
			if (!IsReady())
				Tick();
		}

		/// <summary> Checks is timer delay ended. It should be called in same frame and Update, which calls Tick method of this timer. </summary>
		public bool IsReady() => timeLeft <= 0;

		/// <summary>Sets time in seconds which timer will wait for next action.</summary>
		public void SetFullTime(float timeValue) => fullTime = timeValue;

		/// <summary>Sets callback which will be called every time timer ticks</summary>
		public void SetTimerCallBack(TimerCallback callBackMethod) => timerCallback = callBackMethod;

		/// <summary> Restore timer full time left. </summary>
		public void Reset() => timeLeft = fullTime;
		
		/// <summary> Get timer reload percents. </summary>
		public float GetReloadPercents() => (fullTime - timeLeft) / fullTime;
	}
}