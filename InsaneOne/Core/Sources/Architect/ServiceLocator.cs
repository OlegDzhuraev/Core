using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Architect
{
	public static class ServiceLocator
	{
		static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

		public static void Reset()
		{
			foreach (var (_, service) in services)
				if (service is IDisposableService disposable)
					disposable.Dispose();

			services.Clear();
		}
        
		public static bool Register<T>(T service)
		{
			var isAdded = services.TryAdd(typeof(T), service);

			if (isAdded && service is IInitService initService)
				initService.Init();

			return isAdded;
		}

		public static void Unregister<T>()
		{
			services.Remove(typeof(T));
		}

		public static T Get<T>()
		{
			if (services.TryGetValue(typeof(T), out var value))
				return (T)value;

			throw new NullReferenceException("No such service registered");
		}
        
		public static bool TryGet<T>(out T result)
		{
			if (services.TryGetValue(typeof(T), out var value))
			{
				result = (T) value;
				return true;
			}

			result = default;
			return false;
		}
	}
}