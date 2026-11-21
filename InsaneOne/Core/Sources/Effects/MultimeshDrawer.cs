using System.Collections.Generic;
using InsaneOne.Core.Architect;
using UnityEngine;

namespace InsaneOne.Core.Effects
{
	/// <summary> Can be used to draw some prefab meshes without any other logics, useful for some holograms or object preview. </summary>
	public class MultimeshDrawer : MonoBehaviour
	{
		public bool UseCustomColor { get; set; }
		public Color Color { get; set; } = Color.white;

		[Tooltip("Setup renderer template here (should have MeshRenderer component attached). In template you can set material, which will be used to draw model.")]
		[SerializeField] GameObject drawerPartTemplate;
		[SerializeField] bool useTargetPrefabScale = true;
		[SerializeField] bool disableAtStart = true;
		[Tooltip("Use to add this drawer to Service Locator. Can be useful when there only ony Model Drawer in game.")]
		[SerializeField] bool registerAsService;

		readonly List<DrawerRenderPart> drawerParts = new ();
		readonly List<Material> clonedMaterials = new ();

		static readonly int colorId = Shader.PropertyToID("_Color");

		MaterialPropertyBlock propertyBlock;

		void Awake()
		{
			Debug.Assert(drawerPartTemplate);

			if (registerAsService)
				ServiceLocator.Register(this);

			if (disableAtStart)
				Disable();
		}

		void Update()
		{
			if (!UseCustomColor)
				return;

			propertyBlock ??= new MaterialPropertyBlock();

			foreach (var part in drawerParts)
			{
				part.MeshRenderer.GetPropertyBlock(propertyBlock);
				propertyBlock.SetColor(colorId, Color);
				part.MeshRenderer.SetPropertyBlock(propertyBlock);
			}
		}

		public void Disable()
		{
			gameObject.SetActive(false);
			transform.localScale = Vector3.one;
		}

		void Cleanup()
		{
			for (int i = 0; i < transform.childCount; i++)
				Destroy(transform.GetChild(i).gameObject);

			drawerParts.Clear();

			foreach (var material in clonedMaterials)
				Destroy(material);

			clonedMaterials.Clear();
		}

		/// <summary> Enable with renderers from passed in prefab template. </summary>
		public void Enable(GameObject templatePrefab, MaterialUsageMode materialUsageMode = MaterialUsageMode.None)
		{
			if (!drawerPartTemplate)
			{
				Debug.LogError($"[{nameof(MultimeshDrawer)}] No drawer part template assigned on '{name}' - assign a renderer template first.", this);
				return;
			}

			Cleanup();
			gameObject.SetActive(true);

			AddPartRecursive(templatePrefab.transform, transform, materialUsageMode);

			if (useTargetPrefabScale)
				transform.localScale = templatePrefab.transform.localScale;
		}

		void AddPartRecursive(Transform templatePart, Transform attachTo, MaterialUsageMode materialUsageMode = MaterialUsageMode.None)
		{
			var partAttachTo = attachTo;
			var meshFilter = templatePart.GetComponent<MeshFilter>();

			if (!meshFilter)
			{
				var emptyPartGo = new GameObject(templatePart.name);
				emptyPartGo.transform.SetParent(attachTo);
				CloneLocalTransform(templatePart, emptyPartGo.transform);
				partAttachTo = emptyPartGo.transform;
			}
			else
			{
				var mesh = meshFilter.sharedMesh;

				var meshPartGo = Instantiate(drawerPartTemplate, attachTo);
				meshPartGo.name = templatePart.gameObject.name;

				var part = new DrawerRenderPart()
				{
					MeshFilter = meshPartGo.GetOrAddComponent<MeshFilter>(),
					MeshRenderer = meshPartGo.GetOrAddComponent<MeshRenderer>(),
				};

				part.MeshFilter.sharedMesh = mesh;

				if (materialUsageMode != MaterialUsageMode.None)
				{
					var originalRenderer = templatePart.GetComponent<Renderer>();
					if (originalRenderer)
					{
						if (materialUsageMode == MaterialUsageMode.CloneMaterial)
						{
							var clonedMaterial = Instantiate(originalRenderer.sharedMaterial);
							clonedMaterials.Add(clonedMaterial); // destroyed in Cleanup to not leak a material per Enable call
							part.MeshRenderer.material = clonedMaterial;
						}
						else
						{
							part.MeshRenderer.material = originalRenderer.sharedMaterial;
						}
					}
				}

				CloneLocalTransform(templatePart, meshPartGo.transform);
				partAttachTo = meshPartGo.transform;

				drawerParts.Add(part);
			}

			for (int i = 0; i < templatePart.childCount; i++)
				AddPartRecursive(templatePart.GetChild(i), partAttachTo, materialUsageMode);
		}

		static void CloneLocalTransform(Transform from, Transform to)
		{
			to.localPosition = from.localPosition;
			to.localRotation = from.localRotation;
			to.localScale = from.localScale;
		}
	}

	/// <summary> None - will be used drawer template material. Shared - will be used same material instance. Cloned - will be created new material instance.
	/// <para>Supports only one material per renderer</para> </summary>
	public enum MaterialUsageMode
	{
		None, Shared, CloneMaterial
	}

	class DrawerRenderPart
	{
		public MeshFilter MeshFilter;
		public MeshRenderer MeshRenderer;
	}
}