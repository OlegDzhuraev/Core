using System.IO;
using InsaneOne.Core.Development;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class SerializeReferenceMigratorWindowTests
	{
		const string TestFolder = "Assets/_SerializeReferenceMigratorTests";
		const string AssetPath = TestFolder + "/TestHostAsset.asset";

		[TearDown]
		public void TearDown()
		{
			if (AssetDatabase.IsValidFolder(TestFolder))
				AssetDatabase.DeleteAsset(TestFolder);
		}

		[Test]
		public void PatchManagedReferenceType_ReplacesOnlyMatchingRid_AndKeepsDataUntouched()
		{
			var yaml =
				"    RefIds:\n" +
				"    - rid: 1000\n" +
				"      type: {class: OldClass, ns: Old.Namespace, asm: OldAssembly}\n" +
				"      data:\n" +
				"        value: 5\n" +
				"    - rid: 2000\n" +
				"      type: {class: OtherClass, ns: Other.Namespace, asm: OtherAssembly}\n" +
				"      data:\n" +
				"        value: 7\n";

			var result = SerializeReferenceMigratorWindow.PatchManagedReferenceType(yaml, 1000, "NewClass", "New.Namespace", "NewAssembly");

			StringAssert.Contains("rid: 1000", result);
			StringAssert.Contains("type: {class: NewClass, ns: New.Namespace, asm: NewAssembly}", result);
			StringAssert.Contains("value: 5", result); // data block for the patched rid must survive untouched

			// the other RefIds entry must not be affected at all
			StringAssert.Contains("rid: 2000", result);
			StringAssert.Contains("type: {class: OtherClass, ns: Other.Namespace, asm: OtherAssembly}", result);
			StringAssert.Contains("value: 7", result);
		}

		[Test]
		public void PatchManagedReferenceType_ReturnsTextUnchanged_WhenRidNotFound()
		{
			const string yaml = "    - rid: 1000\n      type: {class: OldClass, ns: Old, asm: OldAssembly}\n      data:\n        value: 5\n";

			var result = SerializeReferenceMigratorWindow.PatchManagedReferenceType(yaml, 9999, "NewClass", "New", "NewAssembly");

			Assert.AreEqual(yaml, result);
		}

		[Test]
		public void Migrator_FixesRealAssetWithMissingType_AndPreservesFieldValue()
		{
			if (EditorSettings.serializationMode is SerializationMode.ForceBinary)
			{
				Assert.Ignore("This test requires the project's Asset Serialization mode to be Force Text or Mixed.");
				return;
			}

			if (!AssetDatabase.IsValidFolder(TestFolder))
				AssetDatabase.CreateFolder("Assets", "_SerializeReferenceMigratorTests");

			var host = ScriptableObject.CreateInstance<TestHostAsset>();
			host.Data = new TestReferencedData { Value = 42 };

			AssetDatabase.CreateAsset(host, AssetPath);
			AssetDatabase.SaveAssets();

			// sanity check: a freshly created asset has no missing types yet
			Assert.False(SerializationUtility.HasManagedReferencesWithMissingTypes(host));

			var referenceId = ManagedReferenceUtility.GetManagedReferenceIdForObject(host, host.Data);

			// simulate a rename by rewriting the on-disk type to something that doesn't exist,
			// using the same patch method the tool itself uses to apply a fix
			var corrupted = SerializeReferenceMigratorWindow.PatchManagedReferenceType(
				File.ReadAllText(AssetPath), referenceId, "RenamedGoneClass", "Renamed.Gone.Namespace", "Renamed.Gone.Assembly");

			File.WriteAllText(AssetPath, corrupted);
			AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);

			host = AssetDatabase.LoadAssetAtPath<TestHostAsset>(AssetPath);

			Assert.True(SerializationUtility.HasManagedReferencesWithMissingTypes(host), "Asset should now report a missing SerializeReference type.");

			var missingTypes = SerializationUtility.GetManagedReferencesWithMissingTypes(host);
			Assert.AreEqual(1, missingTypes.Length);
			Assert.AreEqual("RenamedGoneClass", missingTypes[0].className);
			Assert.AreEqual(referenceId, missingTypes[0].referenceId);

			// now migrate it back to the real type, as a user would via the tool's Apply button
			var fixedText = SerializeReferenceMigratorWindow.PatchManagedReferenceType(
				File.ReadAllText(AssetPath), referenceId, nameof(TestReferencedData), typeof(TestReferencedData).Namespace, typeof(TestReferencedData).Assembly.GetName().Name);

			File.WriteAllText(AssetPath, fixedText);
			AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);

			host = AssetDatabase.LoadAssetAtPath<TestHostAsset>(AssetPath);

			Assert.False(SerializationUtility.HasManagedReferencesWithMissingTypes(host), "Migrated asset should no longer report a missing type.");
			Assert.IsInstanceOf<TestReferencedData>(host.Data);
			Assert.AreEqual(42, ((TestReferencedData) host.Data).Value, "Original field value must survive the rename round-trip.");
		}
	}
}
