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
using System.Linq;
using UnityEditor;

namespace InsaneOne.Core.Editor
{
    public sealed class AssetDependencyGraph
    {
        [Serializable]
        public sealed class Entry
        {
            public string Guid;
            public string Path;
            public string Hash;
            public string[] Dependencies;
        }

        readonly Dictionary<string, Entry> _entries = new(StringComparer.Ordinal);
        readonly Dictionary<string, HashSet<string>> _users = new(StringComparer.Ordinal);
        public IEnumerable<Entry> Entries => _entries.Values;
        public int Count => _entries.Count;

        public bool TryGet(string guid, out Entry entry) => _entries.TryGetValue(guid, out entry);

        public void Set(Entry entry)
        {
            RemoveOutgoing(entry.Guid);
            entry.Dependencies = entry.Dependencies.Where(guid => !string.IsNullOrEmpty(guid) && guid != entry.Guid)
                .Distinct(StringComparer.Ordinal).ToArray();
            _entries[entry.Guid] = entry;
            foreach (var dependency in entry.Dependencies)
            {
                if (!_users.TryGetValue(dependency, out var users))
                    _users.Add(dependency, users = new HashSet<string>(StringComparer.Ordinal));
                users.Add(entry.Guid);
            }
        }

        public void Remove(string guid)
        {
            RemoveOutgoing(guid);
            _entries.Remove(guid);
            if (!_users.TryGetValue(guid, out var users))
                return;
            foreach (var user in users)
                _entries[user].Dependencies = _entries[user].Dependencies.Where(dependency => dependency != guid).ToArray();
            _users.Remove(guid);
        }

        void RemoveOutgoing(string guid)
        {
            if (!_entries.TryGetValue(guid, out var old))
                return;
            foreach (var dependency in old.Dependencies)
            {
                if (!_users.TryGetValue(dependency, out var users))
                    continue;
                users.Remove(guid);
                if (users.Count == 0)
                    _users.Remove(dependency);
            }
        }

        public bool TryGetCounts(string guid, out int dependencies, out int users)
        {
            dependencies = users = 0;
            if (!_entries.TryGetValue(guid, out var entry))
                return false;
            dependencies = entry.Dependencies.Length;
            users = _users.TryGetValue(guid, out var incoming) ? incoming.Count : 0;
            return true;
        }

        public List<string> Find(string guid, bool incoming, bool recursive)
        {
            var found = new HashSet<string>(StringComparer.Ordinal) { guid };
            var pending = new Queue<string>();
            pending.Enqueue(guid);
            while (pending.Count != 0)
            {
                var current = pending.Dequeue();
                IEnumerable<string> next = Array.Empty<string>();
                if (incoming)
                {
                    if (_users.TryGetValue(current, out var users))
                        next = users;
                }
                else if (_entries.TryGetValue(current, out var entry))
                    next = entry.Dependencies;
                foreach (var dependency in next)
                    if (found.Add(dependency) && recursive)
                        pending.Enqueue(dependency);
            }
            found.Remove(guid);
            return found.Where(_entries.ContainsKey).OrderBy(id => _entries[id].Path, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static bool IsAssetPath(string path) => !string.IsNullOrEmpty(path)
            && (path.StartsWith("Assets/", StringComparison.Ordinal) || path.StartsWith("Packages/", StringComparison.Ordinal))
            && !AssetDatabase.IsValidFolder(path);

        public void RefreshAsset(string path, string guid, string hash)
        {
            Set(new Entry
            {
                Guid = guid,
                Path = path,
                Hash = hash,
                Dependencies = AssetDatabase.GetDependencies(path, false)
                    .Where(IsAssetPath).Select(AssetDatabase.AssetPathToGUID).ToArray()
            });
        }
    }
}
