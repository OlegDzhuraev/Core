using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary>You can derive from this class any modal window which can be shown and hidden.</summary>
	public class Modal : MonoBehaviour, IHideable
	{
		[SerializeField] protected GameObject selfObject;

		protected virtual void Awake()
		{
			if (!selfObject)
				selfObject = gameObject;
		}

		public virtual void Show() => selfObject.SetActive(true);
		public virtual void Hide() => selfObject.SetActive(false);
	}
}