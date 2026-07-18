using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> A set of prefabs, selectable by icon in the Object Placer tool. </summary>
	[CreateAssetMenu(menuName = "InsaneOne/Level Design/Object Palette")]
	public class ObjectPalette : ScriptableObject
	{
		[Serializable]
		public class Entry
		{
			public GameObject Prefab;
			[Tooltip("Optional icon for the palette button. If not set, an asset preview of the prefab is used.")]
			public Texture2D Icon;
		}

		public List<Entry> Entries = new ();
	}
}
