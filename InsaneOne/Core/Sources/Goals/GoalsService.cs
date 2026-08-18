using System;
using System.Collections.Generic;
using System.IO;
using InsaneOne.Core.Architect;
using InsaneOne.Core.Utility;
using UnityEngine;

namespace InsaneOne.Core.Goals
{
	/// <summary> Tracks runtime progress of goals (achievement-like objectives) configured in a <see cref="GoalsConfig"/>.
	/// Register an instance via <see cref="Architect.ServiceLocator"/> (e.g. <c>ServiceLocator.Register(new GoalsService(config))</c>),
	/// then use AddProgress/SetProgress to update goals from gameplay code, TryGetProgress/GetAllProgress to read it for UI,
	/// ProgressChanged/Completed to react to changes (e.g. to show a notification popup), and Save/Load to persist progress
	/// as a json file on disk (under Application.persistentDataPath, not PlayerPrefs). </summary>
	public class GoalsService : IInitService
	{
		/// <summary> Default file name used by Save/Load/HasSave/DeleteSave when no explicit one is passed. </summary>
		public const string DefaultSaveFileName = "goals_progress.json";

		/// <summary> Full path of the save file for the given file name, under Application.persistentDataPath. </summary>
		public static string GetSavePath(string fileName = DefaultSaveFileName) => Path.Combine(Application.persistentDataPath, fileName);

		/// <summary> Is there a save file at the given path. </summary>
		public static bool HasSave(string fileName = DefaultSaveFileName) => File.Exists(GetSavePath(fileName));

		/// <summary> Deletes the save file at the given path, if any. </summary>
		public static void DeleteSave(string fileName = DefaultSaveFileName)
		{
			var path = GetSavePath(fileName);

			if (File.Exists(path))
				File.Delete(path);
		}

		/// <summary> Fired every time progress of any goal changes, including when it becomes completed. </summary>
		public event Action<GoalProgress> ProgressChanged;

		/// <summary> Fired once, when a goal first reaches its RequiredAmount. </summary>
		public event Action<GoalProgress> Completed;

		readonly GoalsConfig config;
		readonly Dictionary<string, int> progress = new ();
		readonly HashSet<string> completedIds = new ();

		public GoalsService(GoalsConfig config)
		{
			this.config = config;
		}

		/// <summary> Called automatically by ServiceLocator.Register - fills progress of all configured goals with zero. </summary>
		public void Init() => ResetProgress();

		/// <summary> Resets progress of every configured goal back to zero and clears completed marks. Doesn't fire any events. </summary>
		public void ResetProgress()
		{
			progress.Clear();
			completedIds.Clear();

			foreach (var definition in config.Goals)
				progress[definition.Id] = 0;
		}

		/// <summary> Adds (or, with a negative amount, subtracts) progress to the goal with the given id, clamped to 0..RequiredAmount. </summary>
		public GoalProgress AddProgress(string id, int amount = 1) => SetProgress(id, progress.GetValueOrDefault(id) + amount);

		/// <summary> Sets progress of the goal with the given id to an exact value, clamped to 0..RequiredAmount. </summary>
		public GoalProgress SetProgress(string id, int amount)
		{
			if (!config.TryGetGoal(id, out var definition))
			{
				CoreUnityLogger.I.Log($"[{nameof(GoalsService)}] No goal with id [ {id} ] found in config! Progress not changed.", LogLevel.Warning);
				return default;
			}

			amount = Math.Clamp(amount, 0, definition.RequiredAmount);
			progress[id] = amount;

			var result = new GoalProgress(definition, amount);
			ProgressChanged?.Invoke(result);

			if (result.IsCompleted && completedIds.Add(id))
				Completed?.Invoke(result);

			return result;
		}

		/// <summary> Reads current progress of the goal with the given id. </summary>
		public bool TryGetProgress(string id, out GoalProgress result)
		{
			if (config.TryGetGoal(id, out var definition) && progress.TryGetValue(id, out var current))
			{
				result = new GoalProgress(definition, current);
				return true;
			}

			result = default;
			return false;
		}

		/// <summary> Reads current progress of every configured goal, useful to fill a UI list on open. </summary>
		public IEnumerable<GoalProgress> GetAllProgress()
		{
			foreach (var definition in config.Goals)
				yield return new GoalProgress(definition, progress.GetValueOrDefault(definition.Id));
		}

		/// <summary> Writes current progress of all goals as json to a file under Application.persistentDataPath. </summary>
		public void Save(string fileName = DefaultSaveFileName)
		{
			var path = GetSavePath(fileName);
			var data = new GoalsSaveData();

			foreach (var (id, amount) in progress)
				data.Entries.Add(new GoalSaveEntry { Id = id, Amount = amount });

			try
			{
				var directory = Path.GetDirectoryName(path);

				if (!string.IsNullOrEmpty(directory))
					Directory.CreateDirectory(directory);

				File.WriteAllText(path, JsonUtility.ToJson(data, true));
			}
			catch (Exception ex)
			{
				CoreUnityLogger.I.Log($"[{nameof(GoalsService)}] Failed to save progress to '{path}': {ex.Message}", LogLevel.Error);
			}
		}

		/// <summary> Reads progress previously written by Save from a file under Application.persistentDataPath and applies it
		/// (values for goals missing from config are ignored, values for goals missing from the save are left as-is).
		/// Doesn't fire ProgressChanged/Completed - call this once right after registering the service, before anything reads progress.
		/// Returns false if there's no save file, or it couldn't be read. </summary>
		public bool Load(string fileName = DefaultSaveFileName)
		{
			var path = GetSavePath(fileName);

			if (!File.Exists(path))
				return false;

			try
			{
				var data = JsonUtility.FromJson<GoalsSaveData>(File.ReadAllText(path));
				ApplyLoadedData(data);

				return true;
			}
			catch (Exception ex)
			{
				CoreUnityLogger.I.Log($"[{nameof(GoalsService)}] Failed to load progress from '{path}': {ex.Message}", LogLevel.Error);
				return false;
			}
		}

		void ApplyLoadedData(GoalsSaveData data)
		{
			if (data?.Entries == null)
				return;

			foreach (var entry in data.Entries)
			{
				if (!config.TryGetGoal(entry.Id, out var definition))
				{
					CoreUnityLogger.I.Log($"[{nameof(GoalsService)}] Saved progress has unknown goal id [ {entry.Id} ], ignored.", LogLevel.Warning);
					continue;
				}

				var amount = Math.Clamp(entry.Amount, 0, definition.RequiredAmount);
				progress[entry.Id] = amount;

				if (amount >= definition.RequiredAmount)
					completedIds.Add(entry.Id);
			}
		}
	}
}
