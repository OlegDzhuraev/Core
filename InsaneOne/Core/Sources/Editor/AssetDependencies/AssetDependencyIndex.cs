/*
 * Copyright 2026 Oleg Dzhuraev <godlikeaurora@gmail.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Editor
{
    [InitializeOnLoad]
    public static class AssetDependencyIndex
    {
        const string CachePath = "Library/InsaneOne/AssetDependencies.json";
        const int CacheVersion = 1;
        const double UpdateBudget = 0.006;

        [Serializable]
        sealed class Cache
        {
            public int Version;
            public AssetDependencyGraph.Entry[] Entries;
        }

        public static AssetDependencyGraph Graph { get; private set; } = new();
        public static event Action Changed;
        public static bool IsUpdating => _requested || _paths != null;
        public static string Error { get; private set; }
        public static int Processed => _position;
        public static int Total => _paths?.Length ?? 0;

        static string[] _paths;
        static HashSet<string> _seen;
        static int _position;
        static bool _requested;
        static bool _dirty;
        static double _scanAfter;
        static double _notifyAfter;

        static AssetDependencyIndex()
        {
            EditorApplication.update += Update;
            EditorApplication.projectChanged += RequestScan;
            EditorApplication.delayCall += Initialize;
        }

        static void Initialize()
        {
            try
            {
                if (File.Exists(CachePath))
                {
                    var cache = JsonUtility.FromJson<Cache>(File.ReadAllText(CachePath));
                    if (cache != null && cache.Version == CacheVersion && cache.Entries != null)
                        foreach (var entry in cache.Entries)
                            Graph.Set(entry);
                }
            }
            catch (Exception exception)
            {
                Graph = new AssetDependencyGraph();
                Debug.LogWarning($"Asset dependency cache will be rebuilt: {exception.Message}");
            }
            RequestScan();
        }

        public static void RequestScan()
        {
            _requested = true;
            _scanAfter = EditorApplication.timeSinceStartup + 0.3;
        }

        [MenuItem("Tools/InsaneOne/Asset Dependencies/Rebuild Index")]
        public static void Rebuild()
        {
            Graph = new AssetDependencyGraph();
            _paths = null;
            _seen = null;
            _position = 0;
            _dirty = true;
            Error = null;
            RequestScan();
            Notify();
        }

        static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (_paths == null)
            {
                if (!_requested || EditorApplication.timeSinceStartup < _scanAfter)
                    return;
                _requested = false;
                Error = null;
                _paths = AssetDatabase.GetAllAssetPaths();
                _position = 0;
                _seen = new HashSet<string>(StringComparer.Ordinal);
            }

            var until = EditorApplication.timeSinceStartup + UpdateBudget;
            while (_position < _paths.Length && EditorApplication.timeSinceStartup < until)
            {
                var path = _paths[_position++];
                if (!AssetDependencyGraph.IsAssetPath(path))
                    continue;
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                    continue;
                _seen.Add(guid);
                try
                {
                    var hash = AssetDatabase.GetAssetDependencyHash(path).ToString();
                    if (Graph.TryGet(guid, out var entry) && entry.Path == path && entry.Hash == hash)
                        continue;
                    Graph.RefreshAsset(path, guid, hash);
                    _dirty = true;
                }
                catch (Exception exception)
                {
                    Error = $"Could not index {path}: {exception.Message}";
                    Debug.LogWarning(Error);
                }
            }

            if (_position == _paths.Length)
            {
                foreach (var guid in Graph.Entries.Select(entry => entry.Guid).Where(guid => !_seen.Contains(guid)).ToArray())
                {
                    Graph.Remove(guid);
                    _dirty = true;
                }
                _paths = null;
                _seen = null;
                if (_dirty && !_requested && Error == null)
                    Save();
                Notify();
            }
            else if (EditorApplication.timeSinceStartup > _notifyAfter)
                Notify();
        }

        static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
                var temporary = CachePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(new Cache
                {
                    Version = CacheVersion,
                    Entries = Graph.Entries.ToArray()
                }));
                if (File.Exists(CachePath))
                    File.Replace(temporary, CachePath, null);
                else
                    File.Move(temporary, CachePath);
                _dirty = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not save asset dependency cache: {exception.Message}");
            }
        }

        static void Notify()
        {
            _notifyAfter = EditorApplication.timeSinceStartup + 0.25;
            EditorApplication.RepaintProjectWindow();
            Changed?.Invoke();
        }
    }

    sealed class AssetDependencyPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Length + deleted.Length + moved.Length + movedFrom.Length != 0)
                AssetDependencyIndex.RequestScan();
        }
    }
}
