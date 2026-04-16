/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Reflection;
using Bam.Incubation;
using Bam.Logging;
using Bam.Services;

namespace Bam.DependencyInjection
{
    /// <summary>
    /// A simple dependency injection container.
    /// </summary>
    public class DependencyProvider: IDependencyProvider
    {
        readonly object _accessLock = new object();
        readonly Dictionary<Type, object> _typeInstanceDictionary;
        readonly Dictionary<string, Type> _classNameTypeDictionary;
        readonly Dictionary<Type, Dictionary<string, object>> _ctorParams;

        static DependencyProvider()
        {
            Default = new DependencyProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyProvider"/> class with empty registrations.
        /// </summary>
        public DependencyProvider()
        {
            _typeInstanceDictionary = new Dictionary<Type, object>();
            _classNameTypeDictionary = new Dictionary<string, Type>();
            _ctorParams = new Dictionary<Type, Dictionary<string, object>>();
        }
        
        // TODO: implement Circular dependency check
        
        /// <summary>
        /// Gets or sets the default global dependency provider instance.
        /// </summary>
        public static DependencyProvider Default
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a shallow copy of this dependency provider, duplicating all type-instance mappings,
        /// class name mappings, and constructor parameter registrations.
        /// </summary>
        /// <returns>A new <see cref="DependencyProvider"/> with the same registrations.</returns>
        public virtual DependencyProvider Clone()
        {
            lock (_accessLock)
            {
                DependencyProvider val = new DependencyProvider();
                foreach (Type t in _typeInstanceDictionary.Keys)
                {
                    val._typeInstanceDictionary.Add(t, _typeInstanceDictionary[t]);
                }
                foreach (string s in _classNameTypeDictionary.Keys)
                {
                    val._classNameTypeDictionary.Add(s, _classNameTypeDictionary[s]);
                }
                foreach (Type type in _ctorParams.Keys)
                {
                    val._ctorParams.Add(type, _ctorParams[type]);
                }

                return val;
            }
        }

        /// <summary>
        /// Copy the values from the specified dependencyProvider to the current; the same as CopyFrom
        /// </summary>
        /// <param name="dependencyProvider">The dependencyProvider to copy from</param>
        /// <param name="overwrite">If true, values in the current dependencyProvider
        /// will be overwritten by values of the same types from the specified
        /// dependencyProvider otherwise the current value is kept.</param>
        public void CombineWith(DependencyProvider dependencyProvider, bool overwrite = true)
        {
            CopyFrom(dependencyProvider, overwrite);
        }
        
        /// <summary>
        /// Copy the values from the specified dependencyProvider to the current; the same as CombineWith.
        /// </summary>
        /// <param name="dependencyProvider">The dependencyProvider to copy from</param>
        /// <param name="overwrite">If true, values in the current dependencyProvider
        /// are overwritten by values of the same types from the specified
        /// dependencyProvider otherwise the current value is kept.</param>
        public void CopyFrom(DependencyProvider dependencyProvider, bool overwrite = true)
        {
            if (dependencyProvider == null)
            {
                return;
            }
            lock (_accessLock)
            {
                foreach (Type t in dependencyProvider._typeInstanceDictionary.Keys)
                {
                    if (!this._typeInstanceDictionary.ContainsKey(t) || overwrite)
                    {
                        this._typeInstanceDictionary[t] = dependencyProvider._typeInstanceDictionary[t];
                    }
                }
                foreach (string s in dependencyProvider._classNameTypeDictionary.Keys)
                {
                    if (!this._classNameTypeDictionary.ContainsKey(s) || overwrite)
                    {
                        this._classNameTypeDictionary[s] = dependencyProvider._classNameTypeDictionary[s];
                    }
                }
                foreach(Type type in dependencyProvider._ctorParams.Keys)
                {
                    CopyCtorParams(type, dependencyProvider);
                }
            }
        }

        /// <summary>
        /// Copies constructor parameter registrations for the specified type from the given dependency provider into this one.
        /// </summary>
        /// <param name="type">The type whose constructor parameters should be copied.</param>
        /// <param name="dependencyProvider">The source dependency provider to copy constructor parameters from.</param>
        public void CopyCtorParams(Type type, DependencyProvider dependencyProvider)
        {
            if (dependencyProvider._ctorParams.ContainsKey(type))
            {
                Dictionary<string, object> ctorArgs = dependencyProvider._ctorParams[type];
                foreach (string parameterName in ctorArgs.Keys)
                {
                    SetCtorParam(type, parameterName, ctorArgs[parameterName]);
                }
            }
        }

        /// <summary>
        /// Copies the specified generic type from the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The dependencyProvider.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        public void CopyTypeFrom<T>(DependencyProvider source, bool overwrite = true)
        {
            CopyTypeFrom(typeof(T), source, overwrite);
        }

        /// <summary>
        /// Copies the registration for the specified type from the source dependency provider into this one.
        /// </summary>
        /// <param name="type">The type whose registration should be copied.</param>
        /// <param name="source">The source dependency provider to copy from.</param>
        /// <param name="overwrite">If true, existing registrations of the same type are overwritten; otherwise they are kept.</param>
        public void CopyTypeFrom(Type type, DependencyProvider source, bool overwrite = true)
        {
            if (!_typeInstanceDictionary.ContainsKey(type) || overwrite)
            {
                _typeInstanceDictionary[type] = source._typeInstanceDictionary[type];
            }
            string className = type.Name;
            if (!_classNameTypeDictionary.ContainsKey(className) || overwrite)
            {
                _classNameTypeDictionary[className] = source._classNameTypeDictionary[className];
            }
            CopyCtorParams(type, source);
        }

        /// <summary>
        /// Constructs and sets an instance of type T by finding a constructor
        /// that takes constructor arguments of types already 
        /// constructed or set.  If the constructor arguments are not 
        /// already instantiated an InvalidOperationException is thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Construct<T>()
        {
            return (T)Construct(typeof(T));
        }

        /// <summary>
        /// Construct an instance of the specified type
        /// injecting constructor arguments from the current 
        /// dependencyProvider
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Construct(Type type)
        {
            GetCtorAndParams(type, out ConstructorInfo ctor, out List<object> ctorParams);
            this[type] = ctor.Invoke(ctorParams.ToArray());
            return this[type];
        }
        
        /// <summary>
        /// Set writable properties of the specified instance to 
        /// values in the current dependencyProvider.
        /// </summary>
        /// <param name="instance"></param>
        public void SetProperties(object instance)
        {
            Type type = instance.GetType();            
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                object? value = this[prop.PropertyType];
                Delegate? getter = value as Delegate;
                value = getter != null ? getter.DynamicInvoke() : value;

                if (value == null && prop.HasCustomAttributeOfType(out InjectAttribute attr))
                {
                    value = GetInjectValue(prop, attr);
                }

                if (prop.CanWrite && value != null)
                {
                    prop.SetValue(instance, value, null);
                }
            }
        }

        /// <summary>
        /// Sets writable properties adorned with the Inject attribute of the specified instance to
        /// values in the current dependencyProvider.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void SetInjectionProperties(object instance) 
        {
            Type type = instance.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if(prop.HasCustomAttributeOfType(out InjectAttribute attr))
                {
                    if (!prop.CanWrite)
                    {
                        Log.Warn("Property {0}.{1} is addorned with the Inject attribute but it is read only");
                        continue;
                    }
                    prop.SetValue(instance, GetInjectValue(prop, attr));
                }
            }
        }

        private object GetInjectValue(PropertyInfo prop, InjectAttribute attr)
        {
            object value;
            Type tryType = attr.TypeToUse ?? prop.PropertyType;
            value = Get(tryType);
            if (value == null && attr.Required)
            {
                string msgFormat = "Unable to construct required injection property: Name = {0}, Type = {1}";
                string message = string.Format(msgFormat, $"{prop!.DeclaringType!.Name}.{prop.Name}", tryType.FullName);
                throw new InvalidOperationException(message);
            }

            return value!;
        }

        /// <summary>
        /// Constructs an object of type T passing the specified ctorParams to the 
        /// contructor.
        /// </summary>
        /// <typeparam name="T">The type of the object to instantiate.</typeparam>
        /// <param name="ctorParams">The object values to pass to the constructor of type T.</param>
        /// <exception cref="InvalidOperationException">If the constructor with a signature matching
        /// the types of the specified ctorParams is not found.</exception>
        public T Construct<T>(params object[] ctorParams)
        {
            Type type = typeof(T);

            Construct(type, ctorParams);
            return (T)this[type];
        }

        /// <summary>
        /// Constructs an object of the specified type passing the specified
        /// ctorParams to the constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctorParams"></param>
        /// <returns></returns>
        public object Construct(Type type, params object[] ctorParams)
        {
            Type[] ctorTypes = new Type[ctorParams.Length];
            for (int i = 0; i < ctorTypes.Length; i++)
            {
                ctorTypes[i] = ctorParams[i].GetType();
            }

            ConstructorInfo ctor = type.GetConstructor(ctorTypes)!;
            if (ctor == null)
            {
                Throw(type, ctorTypes);
            }

            this[type] = ctor!.Invoke(ctorParams);
            return this[type];
        }

        private static void Throw(Type type, Type[] ctorTypes)
        {
            if (type.IsInterface)
            {
                throw new BindingNotFoundException(type);
            }
            else
            {
                throw new ConstructFailedException(type, ctorTypes);
            }
        }

        /// <summary>
        /// Constructs an object of type T using existing instances
        /// of the specified ctorParamTypes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctorParamTypes"></param>
        /// <returns></returns>
        public T Construct<T>(params Type[] ctorParamTypes)
        {
            object[] ctorParams = GetCtorArgumentsFromTypes(ctorParamTypes);

            return Construct<T>(ctorParams);
        }

        /// <summary>
        /// Constructs an instance of the specified type using registered instances of the specified constructor parameter types.
        /// </summary>
        /// <param name="type">The type to construct.</param>
        /// <param name="ctorParamTypes">Types whose registered instances are passed to the constructor.</param>
        /// <returns>The newly constructed instance.</returns>
        public object Construct(Type type, Type[] ctorParamTypes)
        {
            object[] ctorParams = GetCtorArgumentsFromTypes(ctorParamTypes);

            return Construct(type, ctorParams);
        }

        private object[] GetCtorArgumentsFromTypes(Type[] ctorParamTypes)
        {
            if (ctorParamTypes == null)
            {
                return new object[] { };
            }
            object[] ctorParams = new object[ctorParamTypes.Length];
            for (int i = 0; i < ctorParamTypes.Length; i++)
            {
                Type type = ctorParamTypes[i];
                object instance = this[type];
                ctorParams[i] = instance ?? throw new InvalidOperationException(string.Format("An object of type {0} has not been instantiated in the current container context.", type.Name));
            }
            return ctorParams;
        }

        private T GetInternal<T>()
        {
            if (this[typeof(T)] is Func<T> f)
            {
                return f();
            }
            else if (this[typeof(T)] is Func<Type, T> fp)
            {
                return fp(typeof(T));
            }
            else
            {
                return (T)this[typeof(T)];
            }
        }
        
        /// <summary>
        /// Gets an instance registered under the specified class name, cast to type T.
        /// </summary>
        /// <typeparam name="T">The type to cast the resolved instance to.</typeparam>
        /// <param name="className">The class name used to look up the registered type.</param>
        /// <returns>The resolved instance cast to T.</returns>
        public T Get<T>(string className)
        {
            return (T)Get(className);
        }

        /// <summary>
        /// Gets an instance registered under the specified class name.
        /// </summary>
        /// <param name="className">The class name used to look up the registered type.</param>
        /// <returns>The resolved instance, or null if the class name is not registered.</returns>
        public object Get(string className)
        {
            return Get(className, out Type t);
        }

        /// <summary>
        /// Attempts to get an instance of the specified type without throwing on failure.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="value">When this method returns, contains the resolved instance, or null if resolution failed.</param>
        /// <returns>True if the instance was successfully resolved; otherwise false.</returns>
        public bool TryGet(Type type, out object value)
        {
            return TryGet(type, out value, out Exception e);
        }

        /// <summary>
        /// Attempts to get an instance of the specified type without throwing on failure.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="value">When this method returns, contains the resolved instance, or null if resolution failed.</param>
        /// <param name="e">When this method returns, contains the exception that occurred during resolution, or null if successful.</param>
        /// <returns>True if the instance was successfully resolved; otherwise false.</returns>
        public bool TryGet(Type type, out object value, out Exception e)
        {
            try
            {
                value = Get(type);
                e = null!;
                return true;
            }
            catch (Exception ex)
            {
                value = null!;
                e = ex;
                return false;
            }
        }

        /// <summary>
        /// Gets an instance of the specified type, constructing it if not already registered.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The resolved or newly constructed instance.</returns>
        public virtual object Get(Type type)
        {
            return Get(type, GetCtorParams(type).ToArray());
        }

        /// <summary>
        /// Gets an instance registered under the specified class name, resolving the associated type.
        /// </summary>
        /// <param name="className">The class name used to look up the registered type.</param>
        /// <param name="type">When this method returns, contains the resolved type, or null if the class name is not registered.</param>
        /// <returns>The resolved instance, or null if the class name is not registered.</returns>
        public object Get(string className, out Type type)
        {
            type = this[className];
            if (type != null)
            {
                object result = this[type];
                if (result is Func<object> fn)
                {
                    return fn() ?? Get(type, GetCtorParams(type));
                }
                else if(result is Func<Type, object> typeFn)
                {
                    return typeFn(type) ?? Get(type, GetCtorParams(type));
                }
                else if(result == null)
                {
                    result = Get(type, GetCtorParams(type));
                }
                return result;
            }

            return null!;
        }

        /// <summary>
        /// Gets an instance of the specified type, constructing it using instances of the specified constructor parameter types if not already registered.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <param name="ctorParamTypes">Types whose registered instances are passed to the constructor.</param>
        /// <returns>The resolved or newly constructed instance.</returns>
        public object Get(Type type, params Type[] ctorParamTypes)
        {
            if (this[type] == null)
            {
                Construct(type, ctorParamTypes);
            }

            return this[type];
        }

        /// <summary>
        /// Gets an object of type T if it has been instantiated otherwise
        /// calls Construct and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the object get.</typeparam>
        /// <param name="ctorParamTypes">Array of types used to retrieve the parameters passed to the contructor of
        /// type T</param>
        /// <returns>T</returns>
        public T Get<T>(params Type[] ctorParamTypes)
        {
            if (this[typeof(T)] == null)
            {
                return Construct<T>(ctorParamTypes);
            }
            else
            {
                return GetInternal<T>();
            }
        }

        /// <summary>
        /// Attempts to get an instance of type T without throwing on failure.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="value">When this method returns, contains the resolved instance, or the default value of T if resolution failed.</param>
        /// <returns>True if the instance was successfully resolved; otherwise false.</returns>
		public bool TryGet<T>(out T value)
		{
            return TryGet<T>(out value, out Exception ignore);
        }

        /// <summary>
        /// Attempts to get an instance of type T without throwing on failure.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <param name="value">When this method returns, contains the resolved instance, or the default value of T if resolution failed.</param>
        /// <param name="ex">When this method returns, contains the exception that occurred during resolution, or null if successful.</param>
        /// <returns>True if the instance was successfully resolved; otherwise false.</returns>
		public bool TryGet<T>(out T value, out Exception ex)
		{
			ex = null!;
			value = default(T)!;
			bool result = false;
			try
			{
				value = Get<T>();
				result = true;
			}
			catch (Exception e)
			{
				ex = e;
			}
			return result;
		}

        /// <summary>
        /// Gets an object of type T if it has been instantiated otherwise
        /// calls Construct and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the object to get.</typeparam>
        /// <returns>T</returns>
        public virtual T Get<T>()
        {
            if (this[typeof(T)] == null)
            {
                T getInternal = GetInternal<T>();
                if(getInternal == null)
                {
                    this[typeof(T)] = Construct<T>()!;
                }
            }

            return GetInternal<T>();
        }
        
        /// <summary>
        /// Gets an object of type T if it has been instantiated otherwise
        /// sets the inner instance to the specified setToIfNull and returns
        /// it.  This results in the specified setToIfNull being returned
        /// for subsequent calls to this method.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="setToIfNull">The instance to set the inner instance to if
        /// it has not been previously set</param>
        /// <returns>T</returns>
        public T Get<T>(T setToIfNull)
        {
            if (this[typeof(T)] == null)
            {
                this[typeof(T)] = setToIfNull!;
            }

            return GetInternal<T>();
        }
        /// <summary>
        /// Gets an object of type T if it has been instantiated otherwise
        /// calls Construct and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the object to get.</typeparam>
        /// <param name="ctorParams">Array of objects to pass to the constructor of type T</param>
        /// <returns>T</returns>
        public virtual T Get<T>(params object[] ctorParams)
        {
            if (this[typeof(T)] == null)
            {
                return Construct<T>(ctorParams);
            }
            else
            {
                return (T)this[typeof(T)];
            }
        }

        /// <summary>
        /// Gets an object of the specified type if it has been instantiated otherwise constructs a new instance.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctorParams"></param>
        /// <returns></returns>
        public virtual object Get(Type type, params object[] ctorParams)
        {
            if (this[type] == null)
            {
                return Construct(type, ctorParams);
            }
            else
            {
                return this[type];
            }
        }

        /// <summary>
        /// Adds an instance of type T by constructing it with the default constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Add<T>() where T: new()
        {
            Set<T>(new T());
        }

        /// <summary>
        /// Registers the specified instance as the resolution for type T, allowing overwrites.
        /// </summary>
        /// <typeparam name="T">The type to register the instance for.</typeparam>
        /// <param name="instance">The instance to register.</param>
        public void Set<T>(T instance)
        {
            Set<T>(instance, false);
        }

        /// <summary>
        /// Registers the specified instance as the resolution for type T.
        /// </summary>
        /// <typeparam name="T">The type to register the instance for.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <param name="throwIfSet">If true, throws an <see cref="InvalidOperationException"/> when type T is already registered.</param>
        public void Set<T>(T instance, bool throwIfSet)
        {
            Check<T>(throwIfSet);

            this[typeof(T)] = instance!;
        }

        /// <summary>
        /// Registers a factory function that creates instances of type T on each resolution.
        /// </summary>
        /// <typeparam name="T">The type to register the factory for.</typeparam>
        /// <param name="instanciator">The factory function that creates instances of type T.</param>
        public void Set<T>(Func<T> instanciator)
        {
            Set<T>(instanciator, false);
        }

        /// <summary>
        /// Registers a factory function that creates instances of type T on each resolution.
        /// </summary>
        /// <typeparam name="T">The type to register the factory for.</typeparam>
        /// <param name="instanciator">The factory function that creates instances of type T.</param>
        /// <param name="throwIfSet">If true, throws an <see cref="InvalidOperationException"/> when type T is already registered.</param>
        public void Set<T>(Func<T> instanciator, bool throwIfSet = false)
        {
            Check<T>(throwIfSet);

            this[typeof(T)] = instanciator;
        }

        /// <summary>
        /// Registers a factory function that receives a <see cref="Type"/> parameter and creates instances of type T.
        /// </summary>
        /// <typeparam name="T">The type to register the factory for.</typeparam>
        /// <param name="instanciator">The factory function that receives the requested type and creates instances of type T.</param>
        /// <param name="throwIfSet">If true, throws an <see cref="InvalidOperationException"/> when type T is already registered.</param>
        public void Set<T>(Func<Type, T> instanciator, bool throwIfSet = false)
        {
            Check<T>(throwIfSet);

            this[typeof(T)] = instanciator;
        }

        /// <summary>
        /// Registers a factory function that creates instances for the specified type.
        /// </summary>
        /// <param name="type">The type to register the factory for.</param>
        /// <param name="instanciator">The factory function that creates instances.</param>
        /// <param name="throwIfSet">If true, throws an <see cref="InvalidOperationException"/> when the type is already registered.</param>
        public void Set(Type type, Func<object> instanciator, bool throwIfSet = false)
        {
            Check(type, throwIfSet);

            this[type] = instanciator;
        }

        /// <summary>
        /// Registers a mapping from one type to another by constructing an instance of the implementation type.
        /// </summary>
        /// <param name="forType">The type to register (typically an interface or abstract class).</param>
        /// <param name="useType">The implementation type to construct and register.</param>
        /// <param name="throwIfSet">If true, throws an <see cref="InvalidOperationException"/> when the type is already registered.</param>
        public void Set(Type forType, Type useType, bool throwIfSet = false)
        {
            Set(forType, Construct(useType), throwIfSet);
        }

        /// <summary>
        /// Registers the specified instance for the given type.
        /// </summary>
        /// <param name="type">The type to register the instance for.</param>
        /// <param name="instance">The instance to register.</param>
        /// <param name="throwIfSet">If true, throws an <see cref="InvalidOperationException"/> when the type is already registered.</param>
        public void Set(Type type, object instance, bool throwIfSet = false)
        {
            Check(type, throwIfSet);

            this[type] = instance;
        }
        
        private void Check(Type t, bool throwIfSet)
        {
            if (throwIfSet && Contains(t))
            {
                throw new InvalidOperationException(
                    $"Type of ({t.Name}) already set in this {nameof(DependencyProvider)}");
            }
        }

        private void Check<T>(bool throwIfSet)
        {
            if (throwIfSet && Contains<T>())
            {
                throw new InvalidOperationException(
                    $"Type of <{typeof(T).Name}> already set in this dependencyProvider");
            }
        }

        /// <summary>
        /// Gets all registered class names (simple or fully-qualified type names).
        /// </summary>
        public string[] ClassNames => _classNameTypeDictionary.Keys.ToArray();

        /// <summary>
        /// Types as they would be resolved when using 
        /// the values in ClassNames
        /// </summary>
        public Type[] ClassNameTypes
        {
            get
            {
                HashSet<Type> types = new HashSet<Type>();
                foreach (string cn in ClassNames)
                {
                    Type type = this[cn];
                    if (type != null)
                    {
                        types.Add(type);
                    }
                }
                return types.ToArray();
            }
        }

        /// <summary>
        /// Gets the type registered under the specified class name, or null if not found.
        /// </summary>
        /// <param name="className">The class name to look up.</param>
        /// <returns>The registered type, or null if no type is registered under the specified class name.</returns>
        public Type this[string className]
        {
            get
            {
                if (_classNameTypeDictionary.ContainsKey(className))
                {
                    return _classNameTypeDictionary[className];
                }
                else
                {
                    return null!;
                }
            }
        }

        /// <summary>
        /// All the Types that are mapped to instances
        /// or instanciators
        /// </summary>
        public Type[] MappedTypes => _typeInstanceDictionary.Keys.ToArray();

        /// <summary>
        /// Removes the registration for type T from this provider.
        /// </summary>
        /// <typeparam name="T">The type to remove.</typeparam>
        public void Remove<T>()
        {
            Remove(typeof(T));
        }

        /// <summary>
        /// Removes the registration for the specified class name from this provider.
        /// </summary>
        /// <param name="className">The class name whose registration should be removed.</param>
        public void Remove(string className)
        {
            Remove(className, out Type ignore);
        }

        /// <summary>
        /// Removes the registration for the specified class name from this provider and outputs the associated type.
        /// </summary>
        /// <param name="className">The class name whose registration should be removed.</param>
        /// <param name="type">When this method returns, contains the type that was registered under the class name, or null if not found.</param>
        public void Remove(string className, out Type type)
        {
            type = this[className];
            if (type != null)
            {
                Remove(type);
            }
        }

        /// <summary>
        /// Removes the registration for the specified type, including both simple and fully-qualified class name mappings.
        /// </summary>
        /// <param name="type">The type whose registration should be removed.</param>
        public void Remove(Type type)
        {            
            string fullyQualifiedTypeName = string.Format("{0}.{1}", type.Namespace, type.Name);

            lock (_accessLock)
            {
                if (_typeInstanceDictionary.ContainsKey(type))
                {
                    _typeInstanceDictionary.Remove(type);
                }

                if (_classNameTypeDictionary.ContainsKey(type.Name))
                {
                    _classNameTypeDictionary.Remove(type.Name);
                }

                if (_classNameTypeDictionary.ContainsKey(fullyQualifiedTypeName))
                {
                    _classNameTypeDictionary.Remove(fullyQualifiedTypeName);
                }
            }
        }

        /// <summary>
        /// Determines whether a type is registered under the specified class name.
        /// </summary>
        /// <param name="className">The class name to check.</param>
        /// <returns>True if a type is registered under the specified class name; otherwise false.</returns>
        public bool HasClass(string className)
        {
            return this[className] != null;
        }

        /// <summary>
        /// Determines whether an instance or factory is registered for type T.
        /// </summary>
        /// <typeparam name="T">The type to check for.</typeparam>
        /// <returns>True if type T is registered; otherwise false.</returns>
        public bool Contains<T>()
        {
            return Contains(typeof(T));
        }

        /// <summary>
        /// Determines whether an instance or factory is registered for the specified type.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns>True if the type is registered; otherwise false.</returns>
        public bool Contains(Type type)
        {
            return this[type] != null;
        }

        /// <summary>
        /// Gets the inner instance of the type specified or
        /// null if it has not been set through a call to Set(), Get() or 
        /// Construct().
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object this[Type type]
        {
            get
            {
                if (_typeInstanceDictionary.TryGetValue(type, out var result))
                {
                    if (result is Delegate d)
                    {
                        try
                        {
                            result = d.DynamicInvoke();
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
                            {
                                throw ex.InnerException;
                            }

                            throw;
                        }
                    }

                    return result!;
                }
                else
                {
                    return null!;
                }
            }
            set
            {
                if (_typeInstanceDictionary.ContainsKey(type))
                {
                    _typeInstanceDictionary[type] = value;
                }
                else
                {
                    lock (_accessLock)
                    {
                        _typeInstanceDictionary.Add(type, value);
                        string fullyQualifiedTypeName = $"{type.Namespace}.{type.Name}";
                        if (!_classNameTypeDictionary.ContainsKey(type.Name))
                        {
                            _classNameTypeDictionary.Add(type.Name, type);
                        }
                        else if (!_classNameTypeDictionary.ContainsKey(fullyQualifiedTypeName))
                        {
                            _classNameTypeDictionary.Add(fullyQualifiedTypeName, type);
                        }
                        else
                        {
                            throw new InvalidOperationException($"The specified type {type.Name} conflicts with an existing type registration.");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Set the value to pass into the constructor when 
        /// constructing the specified type
        /// </summary>
        /// <param name="forType"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public void SetCtorParam(Type forType, string parameterName, object value)
        {
            lock (_accessLock)
            {
                if (!_ctorParams.ContainsKey(forType))
                {
                    _ctorParams.Add(forType, new Dictionary<string, object>());
                }

                if (!_ctorParams[forType].ContainsKey(parameterName))
                {
                    _ctorParams[forType].Add(parameterName, value);
                }
            }
        }

        /// <summary>
        /// Gets the constructor parameter value.
        /// </summary>
        /// <param name="forType">For type.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        public object GetCtorParameterValue(Type forType, string parameterName)
        {
            if(_ctorParams.ContainsKey(forType) && _ctorParams[forType].ContainsKey(parameterName))
            {
                return _ctorParams[forType][parameterName];
            }
            return null!;
        }

        private void GetCtorAndParams(Type type, out ConstructorInfo ctor, out List<object> ctorParams)
        {
            ctorParams = GetCtorParams(type, out ctor);
            if (ctor == null)
            {
                Throw(type, ctorParams.Select(p => p.GetType()).ToArray());
            }
        }
        
        /// <summary>
        /// Resolves constructor parameters for the specified type from registered instances and constructor parameter values.
        /// </summary>
        /// <param name="type">The type whose constructor parameters should be resolved.</param>
        /// <returns>A list of resolved constructor parameter values.</returns>
        public List<object> GetCtorParams(Type type)
        {
            return GetCtorParams(type, out _);
        }

        protected List<object> GetCtorParams(Type type, HashSet<Type> constructingTypes)
        {
            return GetCtorParams(type, constructingTypes, out _);
        }
        
        /// <summary>
        /// Resolves constructor parameters for the specified type and outputs the matching constructor.
        /// </summary>
        /// <param name="type">The type whose constructor parameters should be resolved.</param>
        /// <param name="ctorInfo">When this method returns, contains the constructor whose parameters were successfully resolved, or null if none matched.</param>
        /// <returns>A list of resolved constructor parameter values.</returns>
        public List<object> GetCtorParams(Type type, out ConstructorInfo ctorInfo)
        {
            HashSet<Type> constructingTypes = new HashSet<Type>();
            return GetCtorParams(type, constructingTypes, out ctorInfo);
        }
        
        protected List<object> GetCtorParams(Type type, HashSet<Type> constructingTypes, out ConstructorInfo ctorInfo)
        {
            if (!constructingTypes.Contains(type))
            {
                constructingTypes.Add(type);
            }
            else
            {
                throw new DependencyLoopException(type, constructingTypes);
            }
            
            ctorInfo = null!;
            ConstructorInfo[] ctors = type.GetConstructors();
            List<object> ctorParams = new List<object>();
            foreach (ConstructorInfo ctor in ctors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length > 0)
                {
                    foreach (ParameterInfo paramInfo in parameters)
                    {
                        object ctorParam = GetCtorParameterValue(type, paramInfo.Name!);
                        if (ctorParam != null)
                        {
                            if (ctorParam is Delegate d)
                            {
                                ctorParam = d.DynamicInvoke()!;
                            }
                            ctorParams.Add(ctorParam!);
                        }
                        else
                        {
                            try
                            {
                                object existing = this[paramInfo.ParameterType] ?? Get(paramInfo.ParameterType, GetCtorParams(paramInfo.ParameterType, constructingTypes).ToArray());
                                if (existing != null)
                                {
                                    if (existing is Delegate d)
                                    {
                                        existing = d.DynamicInvoke()!;
                                    }

                                    ctorParams.Add(existing!);
                                }
                                else
                                {
                                    ctorParams.Clear();
                                    break;
                                }
                            }
                            catch (BindingNotFoundException bindingNotFoundException)
                            {
                                throw new TypedBindingNotFoundException(type, bindingNotFoundException.InterfaceType);
                            }
                        }
                    }
                }

                if (ctorParams.Count == parameters.Length)
                {
                    ctorInfo = ctor;
                    break;
                }
            }
            return ctorParams;
        }
    }
}
