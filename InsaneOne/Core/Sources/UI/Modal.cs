using System;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary>You can add this component to any window which should be modal with possibility to be shown and hidden.</summary>
	public sealed class Modal : MonoBehaviour, IHideable
	{
		public event Action WasShown, WasHidden;
		
		[SerializeField] GameObject selfObject;
		[SerializeField] bool hideOnStart;
		
		public GameObject SelfObject => selfObject;
		public bool IsShown { get; private set; }
		
		bool isInitialized;
		
		void Awake()
		{
			if (!selfObject)
				selfObject = gameObject;
		}

		void Start()
		{
			if (hideOnStart)
				Hide();

			isInitialized = true;
		}

		public void Show()
		{
			if (isInitialized && IsShown)
				return;
			
			WasShown?.Invoke();
			
			IsShown = true;
			selfObject.SetActive(true);
		}

		public void Hide()
		{
			if (isInitialized && !IsShown)
				return;
			
			WasHidden?.Invoke();
			
			IsShown = false;
			selfObject.SetActive(false);
		}
	}
}