using System;
using System.IO;
using System.Linq;
using InsaneOne.Core.Utility;
using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Development
{
	/// <summary> Lets an AI agent (or any external tool) drive the SerializeReference Migrator while the Unity Editor
	/// has the project open - no batch mode or editor restart needed. The bridge (auto-loaded via [InitializeOnLoad])
	/// polls a command file and answers with a result file, both under Library/InsaneOne/SerializeReferenceMigrator/:
	/// <para>- `{"id":"...","command":"ping"}` - health check, replies with the editor version.</para>
	/// <para>- `{"id":"...","command":"scan"}` - lists all broken SerializeReference types found in the project.</para>
	/// <para>- `{"id":"...","command":"apply","fixes":[{assetPath, referenceId, className, namespace, assembly}]}` -
	/// remaps the listed references to their new types and returns the remaining broken entries after a re-scan.</para>
	/// <para> The agent writes a unique `id` into command.json, then polls result.json until its `id` matches - the
	/// editor picks a command up within ~0.5s and deletes command.json when done processing it. See agents.md. </para></summary>
	[InitializeOnLoad]
	public static class SerializeReferenceMigratorAiBridge
	{
		const float PollIntervalSeconds = 0.5f;

		static double nextCheckTime;

		static SerializeReferenceMigratorAiBridge()
		{
			EditorApplication.update += Tick;
		}

		/// <summary> Protocol directory, under the project's Library folder (never imported, never committed). </summary>
		public static string CommandDirectory
		{
			get
			{
				var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
				return Path.Combine(projectRoot ?? string.Empty, "Library", "InsaneOne", "SerializeReferenceMigrator");
			}
		}

		public static string CommandFilePath => Path.Combine(CommandDirectory, "command.json");
		public static string ResultFilePath => Path.Combine(CommandDirectory, "result.json");

		static void Tick()
		{
			if (EditorApplication.timeSinceStartup < nextCheckTime)
				return;

			nextCheckTime = EditorApplication.timeSinceStartup + PollIntervalSeconds;

			if (!File.Exists(CommandFilePath))
				return;

			ProcessCommandFile();
		}

		/// <summary> Reads and executes the current command.json, writes result.json and deletes the command file.
		/// Public so it can also be invoked directly (tests, other tooling). </summary>
		public static void ProcessCommandFile()
		{
			Directory.CreateDirectory(CommandDirectory);

			AiCommand command = null;

			try
			{
				command = JsonUtility.FromJson<AiCommand>(File.ReadAllText(CommandFilePath));
			}
			catch (Exception ex)
			{
				WriteResult(new AiResult { success = false, error = $"Could not parse command.json: {ex.Message}" });
			}
			finally
			{
				TryDeleteCommandFile(); // always removed first, so a broken command never re-runs every poll
			}

			if (command == null)
				return;

			switch (command.command)
			{
				case "ping":
					WriteResult(new AiResult { id = command.id, command = "ping", success = true, editorVersion = Application.unityVersion });
					break;

				case "scan":
					ProcessScan(command);
					break;

				case "apply":
					ProcessApply(command);
					break;

				default:
					WriteResult(new AiResult
					{
						id = command.id,
						command = command.command,
						success = false,
						error = $"Unknown command '{command.command}'. Supported commands: ping, scan, apply.",
					});
					break;
			}
		}

		static void ProcessScan(AiCommand command)
		{
			var found = SerializeReferenceMigrator.ScanProject();

			WriteResult(new AiResult
			{
				id = command.id,
				command = "scan",
				success = true,
				entries = found.Select(ToResultEntry).ToArray(),
			});
		}

		static void ProcessApply(AiCommand command)
		{
			if (command.fixes == null || command.fixes.Length == 0)
			{
				WriteResult(new AiResult { id = command.id, command = "apply", success = false, error = "Apply command requires a non-empty 'fixes' array." });
				return;
			}

			var fixes = command.fixes
				.Select(fix => new SerializeReferenceMigrator.FixEntry
				{
					AssetPath = fix.assetPath,
					ReferenceId = fix.referenceId,
					NewAssemblyName = fix.assembly,
					NewNamespace = fix.@namespace,
					NewClassName = fix.className,
				})
				.ToList();

			var applyResult = SerializeReferenceMigrator.ApplyFixes(fixes);
			var remaining = SerializeReferenceMigrator.ScanProject();

			WriteResult(new AiResult
			{
				id = command.id,
				command = "apply",
				success = applyResult.FailedCount == 0 && applyResult.Errors.Count == 0,
				appliedCount = applyResult.AppliedCount,
				failedCount = applyResult.FailedCount,
				errors = applyResult.Errors.ToArray(),
				entries = remaining.Select(ToResultEntry).ToArray(), // so the AI can verify what's left after the fix
			});
		}

		static AiResultEntry ToResultEntry(SerializeReferenceMigrator.MissingType missing) => new ()
		{
			assetPath = missing.AssetPath,
			referenceId = missing.ReferenceId,
			oldAssemblyName = missing.AssemblyName,
			oldNamespaceName = missing.NamespaceName,
			oldClassName = missing.ClassName,
		};

		static void TryDeleteCommandFile()
		{
			try
			{
				File.Delete(CommandFilePath);
			}
			catch (Exception ex)
			{
				CoreUnityLogger.I.Log($"[SerializeReference Migrator] Could not delete the AI command file: {ex.Message}", LogLevel.Warning);
			}
		}

		static void WriteResult(AiResult result)
		{
			try
			{
				Directory.CreateDirectory(CommandDirectory);
				File.WriteAllText(ResultFilePath, JsonUtility.ToJson(result, true));
			}
			catch (Exception ex)
			{
				CoreUnityLogger.I.Log($"[SerializeReference Migrator] Could not write the AI result file: {ex.Message}", LogLevel.Error);
			}
		}
	}

	[Serializable]
	public class AiCommand
	{
		/// <summary> Arbitrary string set by the caller - copied into <see cref="AiResult.id"/> so results can be matched to commands. </summary>
		public string id;
		/// <summary> One of: ping, scan, apply. </summary>
		public string command;
		/// <summary> Required for the apply command only. </summary>
		public AiFix[] fixes;
	}

	[Serializable]
	public class AiFix
	{
		public string assetPath;
		public long referenceId;
		public string className;
		public string @namespace;
		public string assembly;
	}

	[Serializable]
	public class AiResult
	{
		public string id;
		public string command;
		public bool success;
		public string error;
		public string editorVersion;
		public int appliedCount;
		public int failedCount;
		public string[] errors;
		/// <summary> Broken references found by a scan, or still remaining after an apply. </summary>
		public AiResultEntry[] entries;
	}

	[Serializable]
	public class AiResultEntry
	{
		public string assetPath;
		public long referenceId;
		public string oldAssemblyName;
		public string oldNamespaceName;
		public string oldClassName;
	}
}
