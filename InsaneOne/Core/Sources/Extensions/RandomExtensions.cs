using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core
{
    public static class RandomExtensions
    {
        /// <summary>Gets random element from list of objects of any type.</summary>
        public static T Random<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
		
        /// <summary>Gets random element from array of objects of any type.</summary>
        public static T Random<T>(this T[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }
		
        /// <summary>Randomly fills Vector3 values.</summary>
        public static void Randomize(this ref Vector3 vector3, float minValue, float maxValue)
        {
            vector3.x = UnityEngine.Random.Range(minValue, maxValue);
            vector3.y = UnityEngine.Random.Range(minValue, maxValue);
            vector3.z = UnityEngine.Random.Range(minValue, maxValue);
        }
		
        /// <summary>Returns randomized Vector3.</summary>
        public static Vector3 GetRandomizedVector3(float minValue, float maxValue)
        {
            var x = UnityEngine.Random.Range(minValue, maxValue);
            var y = UnityEngine.Random.Range(minValue, maxValue);
            var z = UnityEngine.Random.Range(minValue, maxValue);

            return new Vector3(x, y, z);
        }

        /// <summary>Randomly fills Vector2 values.</summary>
        public static void Randomize(this ref Vector2 vector2, float minValue, float maxValue)
        {
            vector2.x = UnityEngine.Random.Range(minValue, maxValue);
            vector2.y = UnityEngine.Random.Range(minValue, maxValue);
        }
		
        /// <summary>Returns randomized Vector2.</summary>
        public static Vector2 GetRandomizedVector2(float minValue, float maxValue)
        {
            var x = UnityEngine.Random.Range(minValue, maxValue);
            var y = UnityEngine.Random.Range(minValue, maxValue);

            return new Vector2(x, y);
        }
    }
}