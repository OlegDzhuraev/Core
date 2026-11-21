using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneOne.Core.Tests
{
	/// <summary> Test-only ScriptableObject with a [SerializeReference] field, used by
	/// <see cref="SerializeReferenceMigratorWindowTests"/>. Must be a top-level (non-nested) type in a file matching
	/// its name - Unity can't create a MonoScript/asset reference for a nested ScriptableObject type. </summary>
	class TestHostAsset : ScriptableObject
	{
		[SerializeReference] public TestReferencedBase Data;
	}

	/// <summary> Test-only MonoBehaviour with a [SerializeReference] field - used to check that the migrator finds
	/// broken references on prefab and scene components, not only in ScriptableObject assets. </summary>
	class TestReferencedMB : MonoBehaviour
	{
		[SerializeReference] public TestReferencedBase Data;
	}

	abstract class TestReferencedBase { }

	[Serializable]
	class TestReferencedData : TestReferencedBase
	{
		public int Value;
	}
}
