using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InsaneOne.Core.Development
{
	/// <summary> Common methods for editor scripts.</summary>
	public static class EditorHelpers
	{
		/// <summary> Loads all assets, found in project by search filter, in List of type T. </summary>
		public static void LoadAssetsToList<T>(List<T> listToAddIn, string searchFilter) where T : Object
		{
			listToAddIn.Clear();
            
			var assets = AssetDatabase.FindAssets(searchFilter);

			for (int i = 0; i < assets.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(T)) as T;
				listToAddIn.Add(asset);
			}
		}
		
		/// <summary> Creates 2D texture with selected size and color. Useful for editor gui styles. </summary>
		public static Texture2D MakeTexture(int width, int height, Color color)
		{
			var pixels = new Color[width * height];
 
			for (int i = 0; i < pixels.Length; i++)
				pixels[i] = color;
 
			var resultTexture = new Texture2D(width, height);
			resultTexture.SetPixels(pixels);
			resultTexture.Apply();
 
			return resultTexture;
		}
	}
}