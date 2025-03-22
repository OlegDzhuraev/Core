using System;

namespace InsaneOne.Core.Injection
{
	public sealed class InjectAttribute : Attribute
	{
		public readonly string Id;

		public InjectAttribute() { }

		public InjectAttribute(string id)
		{
			Id = id;
		}
	}
}