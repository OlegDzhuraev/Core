using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Injection
{
	[Serializable]
	public class InjectData
	{
		public object Data;
		public Type AsType;
		public List<string> BindToIds;
		bool isCustomType;

		public InjectData(object data)
		{
			Data = data;
			BindToIds = new List<string>();

			AsType = null;
			isCustomType = false;
		}

		public InjectData(object data, Type asType)
		{
			Data = data;
			BindToIds = new List<string>();
			AsType = asType;
			isCustomType = true;
		}

		public InjectData Bind(string id)
		{
			BindToIds.Add(id);
			return this;
		}

		public Type GetInjectType() => isCustomType ? AsType : Data.GetType();
	}
}