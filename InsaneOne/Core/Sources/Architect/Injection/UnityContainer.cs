using UnityEngine;

namespace InsaneOne.Core.Injection
{
	public class UnityContainer : Container
	{
		public UnityContainer(InjectionType injectionType, ILogger logger = null) : base(injectionType, logger) { }

		/// <summary> Creates an object and resolves all dependencies in its MonoBehaviour classes methods with Inject attribute.
		/// Currently, this method is not optimized to use at runtime, so be careful.</summary>
		/// <param name="injectHierarchy"> If true, injects into IRequireInject components on child objects as well,
		/// not only on the root instance. Useful for prefabs with nested injectable components. </param>
		public virtual GameObject Spawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool injectHierarchy = false)
		{
			if (rotation == default)
				rotation = Quaternion.identity;

			var instance = Object.Instantiate(prefab, position, rotation, parent);

			if (injectHierarchy)
				ApplyToHierarchy(instance);
			else
				ApplyTo(instance);

			return instance;
		}

		public virtual void ApplyTo(GameObject instance)
		{
			var targets = instance.GetComponents<IRequireInject>();

			foreach (var target in targets)
				InjectDirectly(target);
		}

		/// <summary> Same as ApplyTo, but also injects into IRequireInject components found on child objects. </summary>
		public virtual void ApplyToHierarchy(GameObject instance, bool includeInactive = true)
		{
			var targets = instance.GetComponentsInChildren<IRequireInject>(includeInactive);

			foreach (var target in targets)
				InjectDirectly(target);
		}
	}
}