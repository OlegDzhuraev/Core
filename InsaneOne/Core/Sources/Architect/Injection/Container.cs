using System;
using System.Collections.Generic;
using System.Reflection;

namespace InsaneOne.Core.Injection
{
	[Flags]
	public enum InjectionType
	{
		Method = 1,
		Field = 2,
		All = Method | Field,
	}

	public class Container
	{
		const int StartPoolSize = 5;
		const BindingFlags BindingAttribute = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		/// <summary> Used to prevent GC alloc on every injection. </summary>
		readonly Dictionary<int, object[]> arraysPool = new ();
		readonly Dictionary<Type, List<MethodInfo>> methodsCache = new ();
		readonly Dictionary<Type, List<FieldInfo>> fieldsCache = new ();

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

		public InjectData AddDependency(object data) => AddDependencyAs(data);
		public InjectData AddDependencyAs<T>(T data) => AddDependencyAs(data, typeof(T));

		/// <summary> Registers the dependency, optionally as an alias of one or more types (e.g. base classes or interfaces),
		/// so it can be resolved by any of them in addition to its actual runtime type. </summary>
		public InjectData AddDependencyAs(object data, params Type[] asTypes)
		{
			if (data == null)
				throw new NullReferenceException("Data can't be null!");

			// todo disallow multiple additions of same dep
			var result = new InjectData(data, asTypes);
			dependenciesData.Add(result);
			return result;
		}

		public void AddTarget(object target)
		{
			if (!targets.Contains(target))
				targets.Add(target);
		}

		/// <summary> Injects dependencies to all targets. </summary>
		/// <param name="clearTargetsAfterResolve"> If true (default), targets list is cleared after resolving,
		/// so the next Resolve() call won't re-inject already processed targets. </param>
		public void Resolve(bool clearTargetsAfterResolve = true)
		{
			foreach (var target in targets)
				InjectDirectly(target);

			if (clearTargetsAfterResolve)
				targets.Clear();
		}

		/// <summary> Can be used in runtime for any target.
		/// Currently, this method is not optimized to use at runtime, so be careful. </summary>
		public void InjectDirectly(object target)
		{
			if (injectionType.HasFlag(InjectionType.Method))
				InjectToMethod(target, dependenciesData);

			if (injectionType.HasFlag(InjectionType.Field) && target is not INoFieldsInject)
				InjectToFields(target, dependenciesData);
		}

		/// <param name="stopAfterFirst"> If true, only the first found Inject-method is invoked (optimization for
		/// the common case of a single Inject-method per target). Default is false, so all Inject-methods are invoked. </param>
		void InjectToMethod(object target, IList<InjectData> data, bool stopAfterFirst = false)
		{
			var methods = GetInjectableMethods(target.GetType());

			foreach (var method in methods)
			{
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

						if (injectData.CanInjectTo(parameter.ParameterType))
						{
							input[i] = injectData.Data;
							break;
						}
					}

					if (input[i] == null)
						logger?.Log($"[Injection] Not found required injection data of type {parameter.ParameterType}!");
				}

				method.Invoke(target, input);

				if (stopAfterFirst)
					break;
			}
		}

		void InjectToFields(object target, IList<InjectData> data)
		{
			var fields = GetInjectableFields(target.GetType());

			foreach (var field in fields)
			{
				var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
				var isInjected = false;

				foreach (var injectionData in data)
				{
					if (injectionData.BindToIds.Count > 0 && !injectionData.BindToIds.Contains(injectAttribute.Id)) // if binds only to specific attribute ids and attribute has no this id, skipping
						continue;

					if (injectionData.CanInjectTo(field.FieldType))
					{
						field.SetValue(target, injectionData.Data);
						isInjected = true;
						break; // field can be assigned from only one dependency. Other next dependencies, associated with same type will be skipped
					}
				}

				if (!isInjected)
					logger?.Log($"[Injection] Not found required injection data of type {field.FieldType} for field '{field.Name}' in '{target.GetType()}'!");
			}
		}

		List<MethodInfo> GetInjectableMethods(Type type)
		{
			if (methodsCache.TryGetValue(type, out var methods))
				return methods;

			var result = new List<MethodInfo>();
			foreach (var method in type.GetMethods(BindingAttribute))
			{
				if (!method.IsStatic && Attribute.IsDefined(method, injectAttributeType))
					result.Add(method);
			}

			methodsCache[type] = result;
			return result;
		}

		List<FieldInfo> GetInjectableFields(Type type)
		{
			if (fieldsCache.TryGetValue(type, out var fields))
				return fields;

			var result = new List<FieldInfo>();
			foreach (var field in type.GetFields(BindingAttribute))
			{
				if (!field.IsStatic && Attribute.IsDefined(field, injectAttributeType))
					result.Add(field);
			}

			fieldsCache[type] = result;
			return result;
		}
	}
}