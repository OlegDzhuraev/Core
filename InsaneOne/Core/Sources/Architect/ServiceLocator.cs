using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Architect
{
	public static class ServiceLocator
	{
		static readonly Dictionary<Type, object> services = new ();
		static readonly Dictionary<string, HashSet<Type>> groups = new ();

		public static void Reset()
		{
			foreach (var (type, service) in services)
				ResetService(type, service);

			services.Clear();
			groups.Clear();
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
			if (service is IDisposableService disposable)
				disposable.Dispose();

			if (remove)
				services.Remove(type);
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

		public static void Unregister<T>() => services.Remove(typeof(T));

		public static T Get<T>() => Get<T>(typeof(T));

		static T Get<T>(Type type)
		{
			if (services.TryGetValue(type, out var value))
				return (T)value;

			throw new NullReferenceException($"No service of type '{type}' registered!");
		}

		/// <summary> When using it, keep in mind the peculiarities of Unity.Object, which may not be null when destroyed,
		/// and therefore needs to be unregistered when destroyed.  </summary>
		public static bool TryGet<T>(out T typedResult)
		{
			var isExist = TryGet(typeof(T), out var result);
			typedResult = (T) result;
			return isExist;
		}

		static bool TryGet(Type type, out object result)
		{
			if (services.TryGetValue(type, out var value))
			{
				result = value;
				return true;
			}

			result = default;
			return false;
		}
	}
}