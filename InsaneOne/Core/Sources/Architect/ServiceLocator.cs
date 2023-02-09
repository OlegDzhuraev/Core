using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Architect
{
	public static class ServiceLocator
	{
		// todo dict with registered type?
		static readonly List<object> services = new List<object>();

		public static void Reset()
		{
			foreach (var service in services)
				if (service is IDisposableService disposable)
					disposable.Dispose();

			services.Clear();
		}
        
		public static void Register<T>(T service)
		{
			if (TryGet<T>(out _) || services.Contains(service))
				return;
            
			services.Add(service);
            
			if (service is IInitService initService)
				initService.Init();
		}

		public static void Unregister<T>()
		{
			if (TryGet<T>(out var service))
				services.Remove(service);
		}

		public static T Get<T>()
		{
			foreach (var service in services)
			{
				if (service is T typedService)
					return typedService;
			}

			throw new NullReferenceException("No such service registered");
		}
        
		public static bool TryGet<T>(out T result)
		{
			result = default;
            
			foreach (var service in services)
			{
				if (service is T typedService)
				{
					result = typedService;
					return true;
				}
			}

			return false;
		}
	}
}