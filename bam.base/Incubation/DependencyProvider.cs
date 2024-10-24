/*
	Copyright © Bryan Apellanes 2015  
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bam.Logging;
using Bam.Services;

namespace Bam.Incubation
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

        public DependencyProvider()
        {
            _typeInstanceDictionary = new Dictionary<Type, object>();
            _classNameTypeDictionary = new Dictionary<string, Type>();
            _ctorParams = new Dictionary<Type, Dictionary<string, object>>();
        }
        
        // TODO: implement Circular dependency check
        
        public static DependencyProvider Default
        {
            get;
            set;
        }

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
        /// will be over written by values of the same types from the specified
        /// dependencyProvider otherwise the current value will be kept</param>
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
                object value = this[prop.PropertyType];
                Delegate getter = value as Delegate;
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
                string message = string.Format(msgFormat, $"{prop.DeclaringType.Name}.{prop.Name}", tryType.FullName);
                throw new InvalidOperationException(message);
            }

            return value;
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

            ConstructorInfo ctor = type.GetConstructor(ctorTypes);
            if (ctor == null)
            {
                Throw(type, ctorTypes);
            }

            this[type] = ctor.Invoke(ctorParams);
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
        
        public T Get<T>(string className)
        {
            return (T)Get(className);
        }

        public object Get(string className)
        {
            return Get(className, out Type t);
        }

        public bool TryGet(Type type, out object value)
        {
            return TryGet(type, out value, out Exception e);
        }

        public bool TryGet(Type type, out object value, out Exception e)
        {
            try
            {
                value = Get(type);
                e = null;
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                e = ex;
                return false;
            }
        }

        public object Get(Type type)
        {
            return Get(type, GetCtorParams(type).ToArray());
        }

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

            return null;
        }

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

		public bool TryGet<T>(out T value)
		{
            return TryGet<T>(out value, out Exception ignore);
        }

		public bool TryGet<T>(out T value, out Exception ex)
		{
			ex = null;
			value = default(T);
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
        public T Get<T>()
        {
            if (this[typeof(T)] == null)
            {
                T getInternal = GetInternal<T>();
                if(getInternal == null)
                {
                    this[typeof(T)] = Construct<T>();
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
                this[typeof(T)] = setToIfNull;
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
        public T Get<T>(params object[] ctorParams)
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

        public object Get(Type type, params object[] ctorParams)
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
        
        public void Add<T>() where T: new()
        {
            Set<T>(new T());
        }

        public void Set<T>(T instance)
        {
            Set<T>(instance, false);
        }

        /// <summary>
        /// Sets the inner instance of type T to the specified
        /// instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public void Set<T>(T instance, bool throwIfSet)
        {
            Check<T>(throwIfSet);

            this[typeof(T)] = instance;
        }

        public void Set<T>(Func<T> instanciator)
        {
            Set<T>(instanciator, false);
        }

        public void Set<T>(Func<T> instanciator, bool throwIfSet = false)
        {
            Check<T>(throwIfSet);

            this[typeof(T)] = instanciator;
        }

        public void Set<T>(Func<Type, T> instanciator, bool throwIfSet = false)
        {
            Check<T>(throwIfSet);

            this[typeof(T)] = instanciator;
        }

        public void Set(Type type, Func<object> instanciator, bool throwIfSet = false)
        {
            Check(type, throwIfSet);

            this[type] = instanciator;
        }

        public void Set(Type forType, Type useType, bool throwIfSet = false)
        {
            Set(forType, Construct(useType), throwIfSet);
        }

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
                    return null;
                }
            }
        }

        /// <summary>
        /// All the Types that are mapped to instances
        /// or instanciators
        /// </summary>
        public Type[] MappedTypes => _typeInstanceDictionary.Keys.ToArray();

        public void Remove<T>()
        {
            Remove(typeof(T));
        }

        public void Remove(string className)
        {
            Remove(className, out Type ignore);
        }

        public void Remove(string className, out Type type)
        {
            type = this[className];
            if (type != null)
            {
                Remove(type);
            }
        }

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

        public bool HasClass(string className)
        {
            return this[className] != null;
        }

        public bool Contains<T>()
        {
            return Contains(typeof(T));
        }

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

                    return result;
                }
                else
                {
                    return null;
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
            return null;
        }

        private void GetCtorAndParams(Type type, out ConstructorInfo ctor, out List<object> ctorParams)
        {
            ctorParams = GetCtorParams(type, out ctor);
            if (ctor == null)
            {
                Throw(type, ctorParams.Select(p => p.GetType()).ToArray());
            }
        }
        
        public List<object> GetCtorParams(Type type)
        {
            return GetCtorParams(type, out _);
        }
        
        public List<object> GetCtorParams(Type type, out ConstructorInfo ctorInfo)
        {
            ctorInfo = null;
            ConstructorInfo[] ctors = type.GetConstructors();
            List<object> ctorParams = new List<object>();
            foreach (ConstructorInfo ctor in ctors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length > 0)
                {
                    foreach (ParameterInfo paramInfo in parameters)
                    {
                        object ctorParam = GetCtorParameterValue(type, paramInfo.Name);
                        if (ctorParam != null)
                        {
                            if (ctorParam is Delegate d)
                            {
                                ctorParam = d.DynamicInvoke();
                            }
                            ctorParams.Add(ctorParam);
                        }
                        else
                        {
                            try
                            {
                                object existing = this[paramInfo.ParameterType] ?? Get(paramInfo.ParameterType,
                                    GetCtorParams(paramInfo.ParameterType).ToArray());
                                if (existing != null)
                                {
                                    if (existing is Delegate d)
                                    {
                                        existing = d.DynamicInvoke();
                                    }

                                    ctorParams.Add(existing);
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
