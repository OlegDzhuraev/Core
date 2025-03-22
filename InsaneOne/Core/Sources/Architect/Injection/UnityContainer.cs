using UnityEngine;

namespace InsaneOne.Core.Injection
{
	public class UnityContainer : Container
	{
		public UnityContainer(InjectionType injectionType) : base(injectionType) { }

		/// <summary> Creates an object and resolves all dependencies in its MonoBehaviour classes methods with Inject attribute.
		/// Currently, this method is not optimized to use at runtime, so be careful.</summary>
		public virtual GameObject Spawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
		{
			if (rotation == default)
				rotation = Quaternion.identity;

			var instance = GameObject.Instantiate(prefab, position, rotation, parent);
			ApplyTo(instance);

			return instance;
		}

		public virtual void ApplyTo(GameObject instance)
		{
			var targets = instance.GetComponents<IRequireInject>();

			foreach (var target in targets)
				InjectDirectly(target);
		}
	}
}