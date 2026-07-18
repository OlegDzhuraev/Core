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

			if (dependenciesData.Exists(d => ReferenceEquals(d.Data, data)))
				throw new InvalidOperationException($"Dependency [ {data} ] of type [ {data.GetType()} ] is already added to the container!");

			var result = new InjectData(data, asTypes);
			dependenciesData.Add(result);
			return result;
		}

		/// <summary> Removes the dependency, using the handle returned by AddDependency/AddDependencyAs. </summary>
		public bool RemoveDependency(InjectData data) => dependenciesData.Remove(data);

		/// <summary> Removes the dependency, previously registered with the same data reference. </summary>
		public bool RemoveDependency(object data) => dependenciesData.RemoveAll(d => ReferenceEquals(d.Data, data)) > 0;

		public void ClearDependencies() => dependenciesData.Clear();

		public void AddTarget(object target)
		{
			if (!targets.Contains(target))
				targets.Add(target);
		}

		public bool RemoveTarget(object target) => targets.Remove(target);

		public void ClearTargets() => targets.Clear();

		/// <summary> Clears both registered dependencies and pending targets.
		/// Reflection caches are kept, as they don't depend on container state. </summary>
		public void Clear()
		{
			ClearDependencies();
			ClearTargets();
		}

		/// <summary> Resolves the currently registered dependency of type T, or throws if none is found. </summary>
		public T Resolve<T>(string id = null)
		{
			if (TryResolve<T>(out var value, id))
				return value;

			throw new InvalidOperationException($"Dependency of type [ {typeof(T)} (id: {id ?? "none"}) ] is not registered in the container!");
		}

		/// <summary> Tries to resolve the currently registered dependency of type T. </summary>
		public bool TryResolve<T>(out T value, string id = null)
		{
			foreach (var injectData in dependenciesData)
			{
				if (injectData.BindToIds.Count > 0 && !injectData.BindToIds.Contains(id))
					continue;

				if (injectData.CanInjectTo(typeof(T)))
				{
					value = (T) injectData.Data;
					return true;
				}
			}

			value = default;
			return false;
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

					if (input[i] == null && !injectAttribute.Optional)
						logger?.Log($"[Injection] Not found required injection data of type [ {parameter.ParameterType} ]!");
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

				if (!isInjected && !injectAttribute.Optional)
					logger?.Log($"[Injection] Not found required injection data of type [ {field.FieldType} ] for field [ {field.Name} ] in [ {target.GetType()} ]!");
			}
		}

		/// <summary> Walks the whole type hierarchy (declaring type included) to find Inject-methods, because
		/// GetMethods(Public | NonPublic) does not return private methods declared in base classes. Overridden
		/// virtual methods are de-duplicated via GetBaseDefinition, so an override is invoked only once. </summary>
		List<MethodInfo> GetInjectableMethods(Type type)
		{
			if (methodsCache.TryGetValue(type, out var methods))
				return methods;

			var result = new List<MethodInfo>();
			var seenBaseDefinitions = new HashSet<MethodInfo>();

			for (var current = type; current != null; current = current.BaseType)
			{
				foreach (var method in current.GetMethods(BindingAttribute | BindingFlags.DeclaredOnly))
				{
					if (method.IsStatic || !Attribute.IsDefined(method, injectAttributeType))
						continue;

					if (!seenBaseDefinitions.Add(method.GetBaseDefinition()))
						continue; // a more derived override of this method was already added

					result.Add(method);
				}
			}

			methodsCache[type] = result;
			return result;
		}

		/// <summary> Walks the whole type hierarchy (declaring type included) to find Inject-fields, because
		/// GetFields(Public | NonPublic) does not return private fields declared in base classes. </summary>
		List<FieldInfo> GetInjectableFields(Type type)
		{
			if (fieldsCache.TryGetValue(type, out var fields))
				return fields;

			var result = new List<FieldInfo>();

			for (var current = type; current != null; current = current.BaseType)
			{
				foreach (var field in current.GetFields(BindingAttribute | BindingFlags.DeclaredOnly))
				{
					if (!field.IsStatic && Attribute.IsDefined(field, injectAttributeType))
						result.Add(field);
				}
			}

			fieldsCache[type] = result;
			return result;
		}
	}
}