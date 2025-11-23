using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneOne.Core.Development
{
	/// <summary> Common methods for editor scripts.</summary>
	public static class EditorHelpers
	{
		public static GUIStyle GetBigBlockStyle()
		{
			var bigBlockStyle = new GUIStyle(EditorStyles.helpBox)
			{
				margin = new RectOffset(10, 10, 10, 10),
				padding = new RectOffset(10, 10, 10, 10)
			};

			return bigBlockStyle;
		}
		
		/// <summary> Loads all assets, found in project by search filter, in List of type T. </summary>
		public static void LoadAssetsToList<T>(IList<T> target, string searchFilter) where T : Object
		{
			target.Clear();

			var type = typeof(T);
			var assets = AssetDatabase.FindAssets(searchFilter);

			for (var i = 0; i < assets.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), type) as T;
				target.Add(asset);
			}
		}
		
		/// <summary> Loads all assets, found in project by search filter, in List of type T. </summary>
		public static void LoadAssetsToList<T>(IList<T> target) where T : Object
		{
			target.Clear();

			var type = typeof(T);
			var assets = AssetDatabase.FindAssets($"t:{type}");

			for (var i = 0; i < assets.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), type) as T;
				target.Add(asset);
			}
		}
		
		/// <summary> Creates 2D texture with selected size and color. Useful for editor gui styles. </summary>
		[Obsolete("Bro, use UI Toolkit instead...")]
		public static Texture2D MakeTexture(int width, int height, Color color)
		{
			var pixels = new Color[width * height];
 
			for (var i = 0; i < pixels.Length; i++)
				pixels[i] = color;
 
			var resultTexture = new Texture2D(width, height);
			resultTexture.SetPixels(pixels);
			resultTexture.Apply();
 
			return resultTexture;
		}
	}
}