using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Injection
{
	[Serializable]
	public class InjectData
	{
		public object Data;
		public HashSet<string> BindToIds;

		/// <summary> Types (aliases) this dependency is explicitly registered as. If empty, the actual runtime type of <see cref="Data"/> is used instead. </summary>
		readonly List<Type> asTypes = new ();

		public InjectData(object data) : this(data, (Type[]) null) { }

		public InjectData(object data, params Type[] asTypes)
		{
			Data = data;
			BindToIds = new HashSet<string>();

			if (asTypes == null)
				return;

			foreach (var asType in asTypes)
				if (asType != null)
					this.asTypes.Add(asType);
		}

		public InjectData Bind(string id)
		{
			BindToIds.Add(id);
			return this;
		}

		/// <summary> Registers an additional type (alias) this dependency can be resolved by, on top of any already registered ones. </summary>
		public InjectData As<T>() => As(typeof(T));

		/// <summary> Registers an additional type (alias) this dependency can be resolved by, on top of any already registered ones. </summary>
		public InjectData As(Type type)
		{
			if (type != null && !asTypes.Contains(type))
				asTypes.Add(type);

			return this;
		}

		/// <summary> Checks whether this dependency can be injected into a member of the given type, considering all registered aliases,
		/// or the actual runtime type of <see cref="Data"/> if no aliases were registered. </summary>
		public bool CanInjectTo(Type targetType)
		{
			if (asTypes.Count == 0)
				return targetType.IsAssignableFrom(Data.GetType());

			foreach (var asType in asTypes)
				if (targetType.IsAssignableFrom(asType))
					return true;

			return false;
		}
	}
}