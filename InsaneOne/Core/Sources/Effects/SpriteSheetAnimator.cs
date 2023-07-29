using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> You can use this for flipbook-animations for 2D game sprites. </summary>
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class SpriteSheetAnimator : SheetAnimator
	{
		[SerializeField] bool keepTiledSize = true;
		
		SpriteRenderer spriteRenderer;

		void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			defaultSprite = spriteRenderer.sprite;
		}

		protected override void Init()
		{
			spriteRenderer.sprite = defaultSprite;
			
			base.Init();
		}

		protected override void SetSprite(Sprite sprite)
		{
			var originalSize = spriteRenderer.size;
			spriteRenderer.sprite = sprite;

			if (keepTiledSize)
				spriteRenderer.size = originalSize;
		}
	}
}