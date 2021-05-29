using TMPro;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> This class allows to show panel with info near cursor. </summary>
	public class Hint : MonoBehaviour, IHideable
	{
		[SerializeField] Transform canvasTransform;
		[SerializeField] GameObject selfObject;
		[SerializeField] TMP_Text nameText;

		RectTransform rectTransform;
		bool isShown;
		
		void Awake()
		{
			rectTransform = selfObject.GetComponent<RectTransform>();
		}

		void Start() => Hide();

		void Update()
		{ 
			if (isShown)
				rectTransform.anchoredPosition = Input.mousePosition;
		}

		public void ShowOnCursor(string text, Vector2 offset)
		{
			Show(text, (Vector2)Input.mousePosition + offset);
		}
		
		public void Show(string text, Vector2 position)
		{
			Show();

			rectTransform.anchoredPosition = position * canvasTransform.localScale.x;

			nameText.text = text;
		}

		public void Show()
		{
			selfObject.SetActive(true);
			isShown = true;
		}

		public void Hide()
		{
			selfObject.SetActive(false);
			isShown = false;
		}
	}
}