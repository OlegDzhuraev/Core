using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.LevelDesign
{
	/// <summary> Scene-level registry of objects created by the Object Placer tool, used by its Erase Mode to tell them
	/// apart from other scene content without modifying the placed objects themselves. </summary>
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	public class ObjectPlacerRegistry : MonoBehaviour
	{
		public List<GameObject> PlacedObjects = new ();
	}
}
