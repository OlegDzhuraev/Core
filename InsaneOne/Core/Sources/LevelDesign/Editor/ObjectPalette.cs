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
