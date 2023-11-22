using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.UI
{
	/// <summary> Use it for some icons lists, where some data should be passed to some icon.
	/// It can be inventory items, rts units buy icons, skills icons, etc. </summary>
	public abstract class IconsPanel<TIcon, TData> : MonoBehaviour where TIcon : MonoBehaviour where TData : class
	{
		public event Action WasRedrawn;
		
		/// <summary> Use to override default icon template, which can be setup from the inspector. </summary>
		public GameObject IconTemplate
		{
			get => iconTemplate;
			set => iconTemplate = value;
		}
		
		[Tooltip("Icons will be placed onto this panel. Recommended to use some Layout Group.")]
		[SerializeField] Transform panel;
		[Tooltip("Default icon template to spawn. Can be overriden via code by using public property.")]
		[SerializeField] GameObject iconTemplate;

		readonly List<GameObject> drawnIcons = new List<GameObject>();
		
		/// <summary> Use to redraw icons with a new datas. </summary>
		public void Redraw(List<TData> datas)
		{
			ClearDrawn();

			foreach (var data in datas)
			{
				if (data == null)
				{
					Debug.LogWarning("Some data in input list is NULL! Skipped.");
					continue;
				}
				
				var iconGo = Instantiate(iconTemplate, panel);
				var icon = iconGo.GetComponent<TIcon>();
				
				drawnIcons.Add(iconGo);
				InitIcon(icon, data);
			}
			
			WasRedrawn?.Invoke();
		}

		void ClearDrawn()
		{
			foreach (var drawnIcon in drawnIcons)
				Destroy(drawnIcon);

			drawnIcons.Clear();
		}

		/// <summary> Apply all icon initialization here. Load data into icon, etc. </summary>
		protected abstract void InitIcon(TIcon icon, TData data);
	}
}