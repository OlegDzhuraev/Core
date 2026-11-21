using System;
using System.Collections.Generic;
using UnityEngine;
using URnd = UnityEngine.Random;

namespace InsaneOne.Core
{
	public static class RandomExtensions
	{
		/// <summary>Gets random element from list of objects of any type. Returns default if the list is null or empty.</summary>
		public static T Random<T>(this List<T> list) => list != null && list.Count > 0 ? list[URnd.Range(0, list.Count)] : default;

		/// <summary>Gets random element from array of objects of any type. Returns default if the array is null or empty.</summary>
		public static T Random<T>(this T[] array) => array != null && array.Length > 0 ? array[URnd.Range(0, array.Length)] : default;

		/// <summary>Gets random element from array of objects of any type. Random index limited by maxLength param. Returns default if there are no elements.</summary>
		public static T Random<T>(this T[] array, int maxLength)
		{
			var count = Math.Min(array?.Length ?? 0, maxLength);
			return count > 0 ? array[URnd.Range(0, count)] : default;
		}

		/// <summary>Gets random element from list of objects of any type. Random index limited by maxCount param. Returns default if there are no elements.</summary>
		public static T Random<T>(this List<T> list, int maxCount)
		{
			var count = Math.Min(list?.Count ?? 0, maxCount);
			return count > 0 ? list[URnd.Range(0, count)] : default;
		}

		/// <summary>Gets random element from list of objects of any type, only from elements, for which condition is true.</summary>
		public static bool TryRandom<T>(this IEnumerable<T> collection, Func<T, bool> condition, out T result)
		{
			result = default;
			var matchesCount = 0;

			foreach (var e in collection)
			{
				if (!condition(e))
					continue;

				matchesCount++;

				if (URnd.Range(0, matchesCount) == 0) // reservoir sampling - each matching element has an equal chance, no allocations
					result = e;
			}

			return matchesCount > 0;
		}

		/// <summary>Randomly fills Vector3 values.</summary>
		public static void Randomize(this ref Vector3 vector3, float minValue, float maxValue)
		{
			vector3.x = URnd.Range(minValue, maxValue);
			vector3.y = URnd.Range(minValue, maxValue);
			vector3.z = URnd.Range(minValue, maxValue);
		}

		/// <summary>Returns randomized Vector3.</summary>
		public static Vector3 GetRandomizedVector3(float minValue, float maxValue)
		{
			var x = URnd.Range(minValue, maxValue);
			var y = URnd.Range(minValue, maxValue);
			var z = URnd.Range(minValue, maxValue);

			return new Vector3(x, y, z);
		}

		/// <summary>Randomly fills Vector2 values.</summary>
		public static void Randomize(this ref Vector2 vector2, float minValue, float maxValue)
		{
			vector2.x = URnd.Range(minValue, maxValue);
			vector2.y = URnd.Range(minValue, maxValue);
		}

		/// <summary>Returns randomized Vector2.</summary>
		public static Vector2 GetRandomizedVector2(float minValue, float maxValue)
		{
			var x = URnd.Range(minValue, maxValue);
			var y = URnd.Range(minValue, maxValue);

			return new Vector2(x, y);
		}

		/// <summary> Get item from dictionary by weighted random. Items with zero or negative weight are never picked. Returns default if all weights are zero. </summary>
		/// <param name="items">Structure - key: item, value: its weight</param>
		public static T GetWeightedItem<T>(this Dictionary<T, float> items)
		{
			float totalWeight = 0;

			foreach (var (_, weight) in items)
				totalWeight += weight;

			if (totalWeight <= 0)
				return default;

			var randomNumber = URnd.Range(0f, totalWeight);
			T selectedItem = default;

			foreach (var (item, weight) in items)
			{
				if (weight <= 0)
					continue;

				randomNumber -= weight;

				if (randomNumber <= 0)
				{
					selectedItem = item;
					break;
				}
			}

			return selectedItem;
		}
	}
}
