using System;

namespace InsaneOne.Core.Architect
{
	[Obsolete("This class is obsolete. Recommended to use ServiceLocator instead.", false)]
	public class Service<T>
	{
		static T service;

		public static T Get() => service;
		public static void Set(T instance) => service = instance;
	}
}