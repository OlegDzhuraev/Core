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

		readonly List<DrawerRenderPart> drawerParts = new List<DrawerRenderPart>();

		static readonly int _colorId = Shader.PropertyToID("_Color");

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
			if (UseCustomColor)
			{
				foreach (var part in drawerParts)
					part.MeshRenderer.material.SetColor(_colorId, Color);
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
		}

		/// <summary> Enable with renderers from passed in prefab template. </summary>
		public void Enable(GameObject templatePrefab, MaterialUsageMode materialUsageMode = MaterialUsageMode.None)
		{
			Cleanup();
			gameObject.SetActive(true);

			AddPartRecursive(templatePrefab.transform, transform, materialUsageMode);

			if (useTargetPrefabScale)
				transform.localScale = templatePrefab.transform.localScale;
		}

		void AddPartRecursive(Transform templatePart, Transform attachTo, MaterialUsageMode materialUsageMode = MaterialUsageMode.None)
		{
			for (int i = 0; i < templatePart.childCount; i++)
			{
				var templatePartChild = templatePart.GetChild(i);

				var meshFilter = templatePartChild.GetComponent<MeshFilter>();

				if (!meshFilter)
				{
					var emptyPartGo = new GameObject(templatePart.name);
					emptyPartGo.transform.SetParent(attachTo);
					CloneLocalTransform(templatePartChild, emptyPartGo.transform);
					AddPartRecursive(templatePartChild, emptyPartGo.transform);
					continue;
				}

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
					var originalRenderer = templatePartChild.GetComponent<Renderer>();
					if (originalRenderer)
					{
						var cloneMat = materialUsageMode == MaterialUsageMode.CloneMaterial;
						part.MeshRenderer.material = cloneMat ? Instantiate(originalRenderer.sharedMaterial) : originalRenderer.sharedMaterial;
					}
				}

				CloneLocalTransform(templatePartChild, meshPartGo.transform);
				AddPartRecursive(templatePartChild, meshPartGo.transform);

				drawerParts.Add(part);
			}
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