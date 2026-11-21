using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using InsaneOne.Core.Development;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
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

			var result = SerializeReferenceMigrator.PatchManagedReferenceType(yaml, 1000, "NewClass", "New.Namespace", "NewAssembly");

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

			var result = SerializeReferenceMigrator.PatchManagedReferenceType(yaml, 9999, "NewClass", "New", "NewAssembly");

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
			var corrupted = SerializeReferenceMigrator.PatchManagedReferenceType(
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
			// (the AI bridge drives the exact same ApplyFixes API)
			var fix = new SerializeReferenceMigrator.FixEntry
			{
				AssetPath = AssetPath,
				ReferenceId = referenceId,
				NewClassName = nameof(TestReferencedData),
				NewNamespace = typeof(TestReferencedData).Namespace,
				NewAssemblyName = typeof(TestReferencedData).Assembly.GetName().Name,
			};

			var applyResult = SerializeReferenceMigrator.ApplyFixes(new[] { fix });

			Assert.AreEqual(1, applyResult.AppliedCount);
			Assert.AreEqual(0, applyResult.FailedCount);
			Assert.IsEmpty(applyResult.Errors);

			host = AssetDatabase.LoadAssetAtPath<TestHostAsset>(AssetPath);

			Assert.False(SerializationUtility.HasManagedReferencesWithMissingTypes(host), "Migrated asset should no longer report a missing type.");
			Assert.IsInstanceOf<TestReferencedData>(host.Data);
			Assert.AreEqual(42, ((TestReferencedData) host.Data).Value, "Original field value must survive the rename round-trip.");
		}

		[Test]
		public void ScanProject_FindsMissingTypeInPrefab()
		{
			if (EditorSettings.serializationMode is SerializationMode.ForceBinary)
			{
				Assert.Ignore("This test requires the project's Asset Serialization mode to be Force Text or Mixed.");
				return;
			}

			if (!AssetDatabase.IsValidFolder(TestFolder))
				AssetDatabase.CreateFolder("Assets", "_SerializeReferenceMigratorTests");

			var prefabPath = TestFolder + "/TestPrefab.prefab";

			var go = new GameObject("Prefab Mb Host");
			go.AddComponent<TestReferencedMB>().Data = new TestReferencedData { Value = 11 };
			PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
			Object.DestroyImmediate(go);

			var prefabText = File.ReadAllText(prefabPath);
			var ridMatch = Regex.Match(prefabText, @"rid:\s*(\d+)\s*\r?\n\s*type:\s*\{class: TestReferencedData");
			Assert.IsTrue(ridMatch.Success, "Test prefab should contain a managed reference.");

			var rid = long.Parse(ridMatch.Groups[1].Value);
			var corrupted = SerializeReferenceMigrator.PatchManagedReferenceType(prefabText, rid, "RenamedGoneClass", "Renamed.Gone.Namespace", "Renamed.Gone.Assembly");

			File.WriteAllText(prefabPath, corrupted);
			AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);

			var found = SerializeReferenceMigrator.ScanProject();

			Assert.IsTrue(found.Any(e => e.AssetPath == prefabPath && e.ReferenceId == rid && e.ClassName == "RenamedGoneClass"),
				"ScanProject should find the broken reference inside a prefab component.");
		}

		[Test]
		public void ScanProject_FindsMissingTypeInSavedScene()
		{
			if (EditorSettings.serializationMode is SerializationMode.ForceBinary)
			{
				Assert.Ignore("This test requires the project's Asset Serialization mode to be Force Text or Mixed.");
				return;
			}

			if (!AssetDatabase.IsValidFolder(TestFolder))
				AssetDatabase.CreateFolder("Assets", "_SerializeReferenceMigratorTests");

			var scenePath = TestFolder + "/TestScene.unity";
			var previousScene = EditorSceneManager.GetActiveScene();
			var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

			GameObject hostGo = null;

			try
			{
				EditorSceneManager.SetActiveScene(scene);

				hostGo = new GameObject("Scene Mb Host");
				var mb = hostGo.AddComponent<TestReferencedMB>();
				mb.Data = new TestReferencedData { Value = 13 };

				EditorSceneManager.SaveScene(scene, scenePath);

				var rid = ManagedReferenceUtility.GetManagedReferenceIdForObject(mb, mb.Data);

				var corrupted = SerializeReferenceMigrator.PatchManagedReferenceType(
					File.ReadAllText(scenePath), rid, "RenamedGoneClass", "Renamed.Gone.Namespace", "Renamed.Gone.Assembly");

				File.WriteAllText(scenePath, corrupted);
				AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceUpdate);
			}
			finally
			{
				if (hostGo != null)
					Object.DestroyImmediate(hostGo);

				EditorSceneManager.SetActiveScene(previousScene); // restore the previous active scene, then close the test one
				EditorSceneManager.CloseScene(scene, true);
			}

			var found = SerializeReferenceMigrator.ScanProject();

			Assert.IsTrue(found.Any(e => e.AssetPath == scenePath && e.ClassName == "RenamedGoneClass"),
				"ScanProject should find the broken reference inside a scene component.");
		}
	}
}
