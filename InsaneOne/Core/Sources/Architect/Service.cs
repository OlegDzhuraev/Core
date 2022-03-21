namespace InsaneOne.Core.Architect
{
	public class Service<T>
	{
		static T service;

		public static T Get() => service;
		public static void Set(T instance) => service = instance;
	}
}