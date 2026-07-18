using System;

namespace InsaneOne.Core.Injection
{
	public sealed class InjectAttribute : Attribute
	{
		public readonly string Id;

		/// <summary> If true, missing dependency for this member won't be logged as an error. </summary>
		public bool Optional { get; set; }

		public InjectAttribute() { }

		public InjectAttribute(string id)
		{
			Id = id;
		}
	}
}