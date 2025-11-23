using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using URnd = UnityEngine.Random;

namespace InsaneOne.Core
{
    public static class RandomExtensions
    {
        static readonly List<object> tempRandomElements = new ();

        /// <summary>Gets random element from list of objects of any type.</summary>
        public static T Random<T>(this List<T> list) => list[URnd.Range(0, list.Count)];

        /// <summary>Gets random element from array of objects of any type.</summary>
        public static T Random<T>(this T[] array) => array[URnd.Range(0, array.Length)];

        /// <summary>Gets random element from array of objects of any type. Random index limited by maxLength param.</summary>
        public static T Random<T>(this T[] array, int maxLength) => array[URnd.Range(0, Math.Min(array.Length, maxLength))];

        /// <summary>Gets random element from list of objects of any type. Random index limited by maxCount param.</summary>
        public static T Random<T>(this List<T> list, int maxCount) => list[URnd.Range(0, Math.Min(list.Count, maxCount))];

        /// <summary>Gets random element from list of objects of any type, only from elements, for which condition is true.</summary>
        public static bool TryRandom<T>(this IEnumerable<T> collection, Func<T, bool> condition, out T result)
        {
            foreach (var e in collection.Where(condition.Invoke))
                tempRandomElements.Add(e);

            if (tempRandomElements.Count == 0)
            {
                tempRandomElements.Clear();
                result = default;
                return false;
            }

            result = (T) tempRandomElements.Random();
            tempRandomElements.Clear();
            return true;
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
        
        /// <summary> Get item from dictionary by weighted random. </summary>
        /// <param name="items">Structure - key: item, value: its weight</param>
        public static T GetWeightedItem<T>(this Dictionary<T, float> items)
        {
            float totalWeight = 0;

            foreach (var (_, weight) in items)
                totalWeight += weight;

            var randomNumber = URnd.Range(0f, totalWeight);

            T selectedItem = default;
            foreach (var (item, weight) in items)
            {
                if (randomNumber < weight)
                {
                    selectedItem = item;
                    break;
                }

                randomNumber -= weight;
            }

            return selectedItem;
        }
    }
}