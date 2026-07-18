using System;
using System.Collections.Generic;
using System.Reflection;

namespace InsaneOne.Core.Architect
{
	public static class ServiceLocator
	{
		static readonly Dictionary<Type, object> services = new ();
		static readonly Dictionary<string, HashSet<Type>> groups = new ();

		/// <summary> Maps an alias type to the canonical type it was registered under, see <see cref="Alias{T,TAlias}"/>. </summary>
		static readonly Dictionary<Type, Type> aliases = new ();

		public static void Reset()
		{
			foreach (var (type, service) in services)
				ResetService(type, service);

			services.Clear();
			groups.Clear();
			aliases.Clear();
		}

		public static void ResetGroup(string group)
		{
			if (!groups.TryGetValue(group, out var set))
				return;

			foreach (var type in set)
				if (TryGet(type, out var service))
					ResetService(type, service, true);

			set.Clear();
		}

		static void ResetService(Type type, object service, bool remove = false)
		{
			if (service is IDisposable disposable)
				disposable.Dispose();

			if (remove)
			{
				services.Remove(type);
				RemoveAliasesOf(type);
			}
		}

		static void RemoveAliasesOf(Type type)
		{
			List<Type> toRemove = null;

			foreach (var (alias, canonical) in aliases)
				if (canonical == type)
					(toRemove ??= new List<Type>()).Add(alias);

			if (toRemove == null)
				return;

			foreach (var alias in toRemove)
				aliases.Remove(alias);
		}
        
		public static bool Register<T>(T service, string group = "")
		{
			var type = typeof(T);
			var isAdded = services.TryAdd(type, service);
			AddToGroup(type, group);

			if (isAdded && service is IInitService initService)
				initService.Init();

			return isAdded;
		}

		static void AddToGroup(Type serviceType, string group)
		{
			if (string.IsNullOrWhiteSpace(group))
				return;

			if (!groups.TryGetValue(group, out var set))
			{
				set = new HashSet<Type>();
				groups[group] = set;
			}

			set.Add(serviceType);
		}

		/// <summary> Registers an additional type this already-registered service of type <typeparamref name="T"/> can also be resolved by
		/// (e.g. an interface or base class it implements), so <see cref="Get{T}"/>/<see cref="TryGet{T}"/> will return the same instance
		/// for both types. Returns false if <typeparamref name="T"/> isn't registered yet, or <typeparamref name="TAlias"/> is already taken. </summary>
		public static bool Alias<T, TAlias>() where T : TAlias
		{
			var type = typeof(T);
			var aliasType = typeof(TAlias);

			if (!services.ContainsKey(type))
				return false;

			if (services.ContainsKey(aliasType) || aliases.ContainsKey(aliasType))
				return false;

			aliases.Add(aliasType, type);
			return true;
		}

		public static T Get<T>() => Get<T>(typeof(T));

		static T Get<T>(Type type)
		{
			if (TryGet(type, out var value))
				return (T)value;

			throw new NullReferenceException($"No service of type '{type}' registered!");
		}

		public static void Unregister<T>() => Unregister(typeof(T));

		static void Unregister(Type type)
		{
			services.Remove(type);
			RemoveAliasesOf(type);
		}

		/// <summary> When using it, keep in mind the peculiarities of Unity.Object, which may not be null when destroyed,
		/// and therefore needs to be unregistered when destroyed. </summary>
		public static bool TryGet<T>(out T typedResult)
		{
			var isExist = TryGet(typeof(T), out var result);
			typedResult = (T) result;
			return isExist;
		}

		/// <summary> Automatically finds services for all type fields with attribute <see cref="LocateAttribute"/> and fills it. </summary>
		public static void AutoLocate(object target)
		{
			const BindingFlags BindingAttribute = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var locateAttributeType = typeof(LocateAttribute);
			var targetType = target.GetType();

			foreach (var field in targetType.GetFields(BindingAttribute))
			{
				if (Attribute.IsDefined(field, locateAttributeType) && TryGet(field.FieldType, out var service))
					field.SetValue(target, service);
			}
		}

		static bool TryGet(Type type, out object result)
		{
			result = null;

			if (aliases.TryGetValue(type, out var canonicalType))
				type = canonicalType;

			if (services.TryGetValue(type, out var value))
			{
#if UNITY_5_3_OR_NEWER
				if (value is UnityEngine.Object unityObject && unityObject == null)
				{
					UnityEngine.Debug.LogWarning($"[ServiceLocator] Object of type {type} is {nameof(UnityEngine.Object)}, and it was destroyed. Can't get it from ServiceLocator.");
					Unregister(type);
					return false;
				}
#endif
				result = value;
				return true;
			}

			return false;
		}
	}
}