using System;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary>You can add this component to any window which should be modal with possibility to be shown and hidden.</summary>
	public sealed class Panel : MonoBehaviour
	{
		public event Action WasShown, WasHidden;
		
		[SerializeField] GameObject selfObject;
		[SerializeField] bool hideOnStart;
		
		public GameObject SelfObject => selfObject;
		public RectTransform RectTransform { get; private set; }
		public bool IsShown { get; private set; }

		bool wasShown;
		
		void Awake()
		{
			if (!selfObject)
				selfObject = gameObject;

			RectTransform = selfObject.GetComponent<RectTransform>();
		}

		void Start()
		{
			if (hideOnStart && !wasShown)
				Hide(true);
		}

		public void Show()
		{
			if (IsShown)
				return;
			
			WasShown?.Invoke();
			
			wasShown = true;
			IsShown = true;
			
			selfObject.SetActive(true);
		}

		public void Hide(bool ignoreHiddenState = false)
		{
			if (!IsShown && !ignoreHiddenState)
				return;
			
			WasHidden?.Invoke();
			
			IsShown = false;
			selfObject.SetActive(false);
		}
	}
}