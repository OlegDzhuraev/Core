using System;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	/// <summary> Test-only ScriptableObject with a [SerializeReference] field, used by
	/// <see cref="SerializeReferenceMigratorWindowTests"/>. Must be a top-level (non-nested) type in a file matching
	/// its name - Unity can't create a MonoScript/asset reference for a nested ScriptableObject type. </summary>
	class TestHostAsset : ScriptableObject
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
