using System;
using System.Collections.Generic;
using System.Reflection;

namespace InsaneOne.Core.Injection
{
	[Flags]
	public enum InjectionType
	{
		Method,
		Field,
		All = Method | Field,
	}

	public class Container
	{
		const int StartPoolSize = 5;
		const BindingFlags BindingAttribute = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		/// <summary> Used to prevent GC alloc on every injection. </summary>
		readonly Dictionary<int, object[]> arraysPool = new ();

		readonly List<object> targets = new ();
		readonly List<InjectData> dependenciesData = new ();

		readonly Type injectAttributeType = typeof(InjectAttribute);
		readonly InjectionType injectionType;

		readonly ILogger logger;

		public Container(InjectionType injectionType, ILogger logger = null)
		{
			this.injectionType = injectionType;
			this.logger = logger;

			for (var i = 1; i < StartPoolSize; i++)
				arraysPool.Add(i, new object[i]);
		}

		public InjectData AddDependency(object data)
		{
			if (data == null)
				throw new NullReferenceException("Data can't be null!");

			// todo disallow multiple additions
			var result = new InjectData(data);
			dependenciesData.Add(result);
			return result;
		}

		public InjectData AddDependencyAs<T>(T data) => AddDependencyAs(data, typeof(T));

		public InjectData AddDependencyAs(object data, Type type)
		{
			if (data == null)
				throw new NullReferenceException("Data can't be null!");

			var result = new InjectData(data, type);
			dependenciesData.Add(result);
			return result;
		}

		public void AddTarget(object target)
		{
			if (!targets.Contains(target))
				targets.Add(target);
		}

		/// <summary> Injects dependencies to all targets. </summary>
		public void Resolve()
		{
			foreach (var target in targets)
				InjectDirectly(target);
		}

		/// <summary> Can be used in runtime for any target.
		/// Currently, this method is not optimized to use at runtime, so be careful. </summary>
		public void InjectDirectly(object target)
		{
			if (injectionType.HasFlag(InjectionType.Method))
				InjectToMethod(target, dependenciesData);

			if (injectionType.HasFlag(InjectionType.Field) && target is not INoFieldsInject)
				foreach (var data in dependenciesData)
					InjectToField(target, data);
		}

		void InjectToMethod(object target, List<InjectData> data)
		{
			var systemType = target.GetType();

			foreach (var method in systemType.GetMethods(BindingAttribute))
			{
				if (method.IsStatic || !Attribute.IsDefined(method, injectAttributeType))
					continue;

				var injectAttribute = method.GetCustomAttribute<InjectAttribute>();
				var parameters = method.GetParameters();

				if (!arraysPool.TryGetValue(parameters.Length, out var input))
				{
					input = new object[parameters.Length];
					arraysPool.Add(parameters.Length, input);
				}

				for (var i = 0; i < parameters.Length; i++)
				{
					var parameter = parameters[i];
					input[i] = null;

					foreach (var injectData in data)
					{
						if (injectData.BindToIds.Count > 0 && !injectData.BindToIds.Contains(injectAttribute.Id)) // if binds only to specific attribute ids and attribute has no this id, skipping
							continue;

						var injectType = injectData.GetInjectType();

						if (parameter.ParameterType.IsAssignableFrom(injectType))
						{
							input[i] = injectData.Data;
							break;
						}
					}

					if (input[i] == null)
						logger?.Log($"[Injection] Not found required injection data of type {parameter.ParameterType}!");
				}

				method.Invoke(target, input);

				break;
			}
		}

		void InjectToField(object system, InjectData injectionData)
		{
			var dataType = injectionData.GetInjectType();
			var systemType = system.GetType();

			foreach (var field in systemType.GetFields(BindingAttribute))
			{
				if (field.IsStatic || !Attribute.IsDefined(field, injectAttributeType))
					continue;

				var injectAttribute = field.GetCustomAttribute<InjectAttribute>();

				if (injectionData.BindToIds.Count > 0 && !injectionData.BindToIds.Contains(injectAttribute.Id)) // if binds only to specific attribute ids and attribute has no this id, skipping
					continue;

				if (field.FieldType.IsAssignableFrom(dataType))
				{
					field.SetValue(system, injectionData.Data);
					break;
				}
			}
		}
	}
}