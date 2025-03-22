using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Injection
{
	[Serializable]
	public class InjectData
	{
		public object Data;
		public List<string> BindToIds = new List<string>();

		public InjectData(object data)
		{
			Data = data;
		}

		public InjectData Bind(string id)
		{
			BindToIds.Add(id);
			return this;
		}
	}
}