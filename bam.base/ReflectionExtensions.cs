/*
	Copyright © Bryan Apellanes 2015  
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bam.Data;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace Bam
{
    public static class ReflectionExtensions
    {
        public static bool HasObjectParameters(this MethodInfo methodInfo)
        {
            return HasObjectParameters(methodInfo, out ParameterInfo[] ignore);
        }
        
        public static bool HasObjectParameters(this MethodInfo methodInfo, out ParameterInfo[] objectParameters)
        {
            bool result = false;
            List<ParameterInfo> outParams = new List<ParameterInfo>();
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                if (!parameterInfo.ParameterType.IsPrimitive && parameterInfo.ParameterType != typeof(string))
                {
                    result = true;
                    outParams.Add(parameterInfo);
                }
            }

            objectParameters = outParams.ToArray();
            return result;
        }
        
        /// <summary>
        /// Returns a hash representing the specified
        /// types using the specified HashAlgorithm 
        /// and encoding.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="algorithm"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToInfoHash(this IEnumerable<Type> types, HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding? encoding = null)
        {
            return types.ToInfoString().HashHexString(algorithm, encoding);
        }

        public static string ToInfoString(this IEnumerable<Type> types)
        {
            StringBuilder output = new StringBuilder();
            types.Each(type =>
            {
                output.AppendLine(type.ToInfoString());
            });

            return output.ToString();
        }

        public static string ToInfoHash(this Type type, HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding? encoding = null)
        {
            return type.ToInfoString().HashHexString(algorithm, encoding);
        }

        /// <summary>
        /// Return a string representation of the specified 
        /// type.  This is primarily used for hashing the
        /// type for the purpose of uniquely identifying
        /// it across processes.  The resulting
        /// string cannot be easily converted back to 
        /// the original type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToInfoString(this Type type)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine($"{type.FullName}");
            List<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());
            if(type != typeof(RuntimeTypeHandle) && !type.Name.Equals("RuntimeType"))
            {
                props.Sort((p1, p2) => p1.Name.CompareTo(p2.Name));
            }
            props.Each(pi =>
            {
                output.AppendLine($"\t{pi.ToInfoString()}");
            });
            return output.ToString();
        }

        /// <summary>
        /// Return a string representation of the prop. This 
        /// is primarily used for hashing the property and
        /// the resulting string cannot be easily converted 
        /// back to the original PropertyInfo.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string ToInfoString(this PropertyInfo prop)
        {
            return $"{{{prop.Name}:{prop.PropertyType.ToTypeString(false)}}}";
        }
        /// <summary>
        /// Invoke the specified static method of the 
        /// specified (extension method "current") type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T InvokeStatic<T>(this Type type, string methodName, params object[] args)
        {
            Args.ThrowIfNull(type);
            Args.ThrowIfNull(methodName);
            return (T)type.GetMethod(methodName).Invoke(null, args);
        }

        public static bool HasGenericArguments(this Type type)
        {
            Type[] ignore;
            return type.HasGenericArguments(out ignore);
        }
        public static bool HasGenericArguments(this Type type, out Type[] genericArgumentTypes)
        {
            genericArgumentTypes = type.GetGenericArguments();
            return genericArgumentTypes.Length > 0;
        }

        /// <summary>
        /// Invoke the specified method on the specified instance 
        /// using the specified arguments
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public static T Invoke<T>(this object instance, string methodName, params object[] args)
        {
            Args.ThrowIfNull(instance, "instance");
            Args.ThrowIfNull(methodName, "methodName");
            return (T)instance.GetType().GetMethod(methodName, args.Select(a => a.GetType()).ToArray()).Invoke(instance, args);
        }

        /// <summary>
        /// Invoke the specified generic method with generic argument type TArg
        /// returning object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T InvokeGeneric<T, TArg>(this object instance, string methodName, params object[] args)
        {
            Args.ThrowIfNull(instance, "instance");
            Args.ThrowIfNull(methodName, "methodName");
            try
            {
                MethodInfo method = instance.GetType().GetMethod(methodName, args.Select(a => a.GetType()).ToArray());
                MethodInfo genericMethod = method.MakeGenericMethod(typeof(TArg));
                return (T)genericMethod.Invoke(instance, args);
            }
            catch (AmbiguousMatchException ame)
            {
                IEnumerable<MethodInfo> methods = instance.GetType().GetMethods().Where(mi => mi.Name.Equals(methodName) && mi.ContainsGenericParameters && mi.GetParameters().Length == args.Length);
                T response = default(T);
                bool gotOne = false;
                foreach (MethodInfo method in methods)
                {
                    ParameterInfo[] paramInfos = method.GetParameters();
                    bool useThisOne = true;
                    for (int i = 0; i < paramInfos.Length; i++)
                    {
                        Type argType = args[i].GetType();
                        Type paramType = paramInfos[i].ParameterType;
                        if(paramType == typeof(object))
                        {
                            continue;
                        }
                        if (!argType.Equals(paramType))
                        {
                            useThisOne = false;
                            break;
                        }
                    }
                    if (useThisOne)
                    {
                        MethodInfo genericMethod = method.MakeGenericMethod(typeof(TArg));
                        response = (T)genericMethod.Invoke(instance, args);
                        gotOne = true;
                        break;
                    }
                }
                if (!gotOne)
                {
                    throw ame;
                }
                return response;
            }
        }

        public static bool TryInvoke(this object instance, string methodName, Action<Exception> exceptionHandler, params object[] args)
        {
            try
            {
                Invoke(instance, methodName, args);
                return true;
            }
            catch (Exception ex)
            {
                exceptionHandler(ex);
            }

            return false;
        }
        
        /// <summary>
        /// Invoke the specified method on the specified instance 
        /// using the specified arguments
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public static object Invoke(this object instance, string methodName, params object[] args)
        {
            Args.ThrowIfNull(instance, "instance");
            Args.ThrowIfNull(methodName, "methodName");
            return instance.GetType().GetMethod(methodName).Invoke(instance, args);
        }

        /// <summary>
        /// Returns true if the specified instance is of a type
        /// that has the specified propertyName
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool HasProperty(this object instance, string propertyName)
        {
            Args.ThrowIfNull(instance, "instance");
            PropertyInfo ignore;
            return HasProperty(instance, propertyName, out ignore);
        }

        /// <summary>
        /// Returns true if the specified instance is of a type
        /// that has the specified propertyName
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool HasProperty(this object instance, string propertyName, out PropertyInfo prop)
        {
            Args.ThrowIfNull(instance, "instance");
            Type type = instance.GetType();
            prop = type.GetProperty(propertyName);
            return prop != null;
        }

        /// <summary>
        /// Subscribe the specified handler to the specified event.  This
        /// is mostly useful when the type is ambiguous because
        /// the underlying implementation uses reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static T On<T>(this T instance, string eventName, EventHandler handler)
        {
            return Subscribe<T>(instance, eventName, handler);
        }

        /// <summary>
        /// Subscribe the specified handler to the specified event 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static T Subscribe<T>(this T instance, string eventName, EventHandler handler)
        {
            return Subscribe(instance, eventName, (Delegate)handler);
        }

        public static T On<T>(this T instance, string eventName, Delegate handler)
        {
            return Subscribe<T>(instance, eventName, handler);
        }

        public static T Subscribe<T, TEventArgs>(this T instance, string eventName, EventHandler<TEventArgs> handler)
        {
            return Subscribe<T>(instance, eventName, handler);
        }

        public static T Subscribe<T>(this T instance, string eventName, Delegate handler)
        {
            EventInfo eventInfo = typeof(T).GetEvent(eventName);
            Args.ThrowIfNull(eventInfo, "eventName");
            eventInfo.AddEventHandler(instance, handler);
            return instance;
        }

        /// <summary>
        /// Subscribe the specified handler to the named event emitted by the instance, making sure it is only subscribed once.  The specified event handler will only fire once per event occurrance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="instance">The event emitting instance.</param>
        /// <param name="eventName">The name of the event to subscribe to .</param>
        /// <param name="handler">The event handler.</param>
        /// <returns></returns>
        public static T SubscribeOnce<T>(this T instance, string eventName, EventHandler handler)
        {
            return SubscribeOnce(instance, eventName, (Delegate)handler);
        }

        public static T SubscribeOnce<T>(this T instance, string eventName, Delegate handler)
        {
            return instance.SubscribeOnce<T>(eventName, handler, out IEventSubscription ignore);
        }

        public static T SubscribeOnce<T>(this T instance, string eventName, EventHandler handler, out IEventSubscription subscription)
        {
            return SubscribeOnce(instance, eventName, (Delegate)handler, out subscription);
        }

        public static T SubscribeOnce<T>(this T instance, string eventName, Delegate handler, out IEventSubscription subscription)
        {
            T result = instance.UnSubscribe(eventName, handler).Subscribe(eventName, handler);
            subscription = instance.GetEventSubscriptions(eventName).FirstOrDefault(es => es.Delegate.Equals(handler));
            return result;
        }

        public static object SubscribeOnce(this object instance, string eventName, EventHandler handler)
        {
            return SubscribeOnce(instance, eventName, (Delegate)handler);
        }

        public static object SubscribeOnce(this object instance, string eventName, Delegate handler)
        {
            return instance.SubscribeOnce(eventName, handler, out IEventSubscription ignore);
        }

        public static object SubscribeOnce(this object instance, string eventName, EventHandler handler, out IEventSubscription subscription)
        {
            return SubscribeOnce(instance, eventName, (Delegate)handler, out subscription);
        }

        public static object SubscribeOnce(this object instance, string eventName, Delegate handler, out IEventSubscription subscription)
        {
            object result = instance.UnSubscribe(eventName, handler).Subscribe(eventName, handler);
            subscription = instance.GetEventSubscriptions(eventName).FirstOrDefault(es => es.Delegate.Equals(handler));
            return result;
        }

        public static object Subscribe(this object instance, string eventName, EventHandler handler)
        {
            return Subscribe(instance, eventName, (Delegate)handler);
        }
        /// <summary>
        /// Subscribe the specified handler to the specified event
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static object Subscribe(this object instance, string eventName, Delegate handler)
        {
            EventInfo eventInfo = instance.GetType().GetEvent(eventName);
            Args.ThrowIfNull(eventInfo, "eventName");
            eventInfo.AddEventHandler(instance, handler);
            return instance;
        }

        public static T Off<T>(this T instance, string eventName, EventHandler handler)
        {
            return Off<T>(instance, eventName, (Delegate)handler);
        }
        /// <summary>
        /// Unsubscribe the specified handler from the specified event 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static T Off<T>(this T instance, string eventName, Delegate handler)
        {
            return UnSubscribe<T>(instance, eventName, handler);
        }
        public static object UnSubscribe(this object instance, string eventName, EventHandler handler)
        {
            return UnSubscribe(instance, eventName, (Delegate)handler);
        }
        /// <summary>
        /// Unsubscribe the specified handler from the specified event 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static object UnSubscribe(this object instance, string eventName, Delegate handler)
        {
            EventInfo eventInfo = instance.GetType().GetEvent(eventName);
            Args.ThrowIfNull(eventInfo, "eventName");
            eventInfo.RemoveEventHandler(instance, handler);
            return instance;
        }
        public static T UnSubscribe<T>(this T instance, string eventName, EventHandler handler)
        {
            return UnSubscribe<T>(instance, eventName, (Delegate)handler);
        }
        /// <summary>
        /// Unsubscribe the specified handler from the specified event 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static T UnSubscribe<T>(this T instance, string eventName, Delegate handler)
        {
            EventInfo eventInfo = typeof(T).GetEvent(eventName);
            Args.ThrowIfNull(eventInfo, "eventName");
            eventInfo.RemoveEventHandler(instance, handler);
            return instance;
        }

        /// <summary>
        /// If instance is null return the specified alternative otherwise return the instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="instead">The instead.</param>
        /// <returns></returns>
        public static T Or<T>(this T instance, T instead)
        {
            if(instance == null)
            {
                return instead;
            }
            if(instance as string == string.Empty)
            {
                return instead;
            }
            return instance;
        }

        /// <summary>
        /// Tries to get the value of the specified property, using the specified value if
        /// the property is not found on the specified instance. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="ifPropertyNotFound">If property not found.</param>
        /// <param name="value">The value.</param>
        public static bool TryGetPropertyValue<T>(this object instance, string propertyName, T ifPropertyNotFound, out T value)
        {
            Args.ThrowIfNull(instance, "instance");
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperties().FirstOrDefault(pi => pi.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if(property == null)
            {
                value = ifPropertyNotFound;
                return false;
            }
            else
            {
                value = (T)property.GetValue(instance);
                if(typeof(T) == typeof(string) && string.IsNullOrEmpty(value as string))
                {
                    value = ifPropertyNotFound;
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Get the property of the current instance with the specified name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyName">The name of the property value to retrieve</param>
        /// <returns></returns>
        public static T Property<T>(this object instance, string propertyName, bool throwIfPropertyNotFound = true)
        {
            object value = Property(instance, propertyName, throwIfPropertyNotFound);
            return value == null ? default(T) : (T)value;
        }

        /// <summary>
        /// Get the value of the specified propertyName from the current instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object Property(this object instance, string propertyName, bool throwIfPropertyNotFound = true)
        {
            Args.ThrowIfNull(instance, "instance");
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperties().FirstOrDefault(pi => pi.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (property == null)
            {
                if (throwIfPropertyNotFound)
                {
                    PropertyNotFound(propertyName, type);
                }
                else
                {
                    return null;
                }
            }
            return property.GetValue(instance);
        }

        /// <summary>
        /// Set the specified property if it is 
        /// null or an empty string
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="throwIfPropertyNotFound"></param>
        /// <returns></returns>
        public static object PropertyIfNullOrBlank(this object instance, string propertyName, object value, bool throwIfPropertyNotFound = true)
        {
            object currentValue = instance.Property(propertyName, throwIfPropertyNotFound);
            string stringValue = currentValue as string;
            return instance.PropertyIf(currentValue == null || string.IsNullOrEmpty(stringValue), propertyName, value, throwIfPropertyNotFound);
        }

        /// <summary>
        /// Set the specified property if the specified
        /// condition is true
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="condition"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="throwIfPropertyNotFound"></param>
        /// <returns></returns>
        public static object PropertyIf(this object instance, bool condition, string propertyName, object value, bool throwIfPropertyNotFound = true)
        {
            if (condition)
            {
                instance.Property(propertyName, value, throwIfPropertyNotFound);
            }
            return null;
        }

        /// <summary>
        /// Set the property with the specified name and return the instance 
        /// to enable chaining
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="throwIfPropertyNotFound"></param>
        /// <returns></returns>
        public static object Property(this object instance, string propertyName, object value, bool throwIfPropertyNotFound = true)
        {
            Args.ThrowIfNull(instance, "instance");
            Type type = instance.GetType();
            PropertyInfo property = GetPropertyOrThrow(type, propertyName, throwIfPropertyNotFound);
            SetProperty(instance, property, value);
            return instance;
        }
        /// <summary>      
        /// Set the property with the specified name and return the instance 
        /// to enable chaining
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="instanceType"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="throwIfPropertyNotFound"></param>
        /// <returns></returns>
        public static object Property(this object instance, Type instanceType, string propertyName, object value, bool throwIfPropertyNotFound = true)
        {
            Args.ThrowIfNull(instance, "instance");
            Args.ThrowIfNull(instanceType, "instanceType");
            PropertyInfo property = GetPropertyOrThrow(instanceType, propertyName, throwIfPropertyNotFound);
            SetProperty(instance, property, value);
            return instance;
        }

        /// <summary>      
        /// Set the property with the specified name and return the instance 
        /// to enable chaining
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="throwIfPropertyNotFound"></param>
        /// <returns></returns>
        public static object Property<T>(this T instance, string propertyName, object value, bool throwIfPropertyNotFound = true)
        {
            Args.ThrowIfNull(instance, "instance");
            Type type = instance.GetType(); // get the actual instance type since it may be an extender of T
            PropertyInfo property = GetPropertyOrThrow(type, propertyName, throwIfPropertyNotFound);
            SetProperty(instance, property, value);
            return instance;
        }

        private static PropertyInfo GetPropertyOrThrow(Type type, string propertyName, bool throwIfPropertyNotFound)
        {
            PropertyInfo property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = FindPropertyInfoOrThrow(type, propertyName, throwIfPropertyNotFound);
            }
            
            if (property == null && throwIfPropertyNotFound)
            {
                PropertyNotFound(propertyName, type);
            }

            return property;
        }

        public static PropertyInfo FindPropertyInfoOrThrow(Type type, string propertyName, bool throwIfPropertyNotFound)
        {
            PropertyInfo property = type.GetProperties().FirstOrDefault(t => t.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (property == null && throwIfPropertyNotFound)
            {
                PropertyNotFound(propertyName, type);
            }

            return property;
        }
        
        public static void SetProperty(this object instance, PropertyInfo property, object value)
        {
            if (property != null)
            {
                if (value == DBNull.Value)
                {
                    value = null;
                }
                else if ((value is int || value is decimal) &&
                   (property.PropertyType == typeof(long) ||
                   Nullable.GetUnderlyingType(property.PropertyType) == typeof(long)))
                {
                    value = Convert.ToInt64(value);
                }
                else if (value is long l &&
                         (property.PropertyType == typeof(ulong) || property.PropertyType == typeof(ulong?)))
                {
                    value = l.MapToUlong();//Dao.MapLongToUlong(l);
                }
                else if ((value is int || value is decimal) &&
                   (property.PropertyType == typeof(ulong) ||
                   property.PropertyType == typeof(ulong?)))
                {
                    value = Convert.ToUInt64(value);
                }
                property.SetValue(instance, value, null);
            }
        }


        public static void EachPropertyInfo(this object instance, Action<PropertyInfo> action)
        {
            Type type = instance.GetType();
            PropertyInfo[] properties = type.GetProperties();
            properties.Each(action);
        }

        public static void EachPropertyInfo(this object instance, Action<PropertyInfo, int> action)
        {
            Type type = instance.GetType();
            PropertyInfo[] properties = type.GetProperties();
            properties.Each(action);
        }

        public static void EachPropertyValue(this object instance, Action<PropertyInfo, object> action)
        {
            instance.EachPropertyInfo(pi =>
            {
                object value = pi.GetValue(instance, null);
                action(pi, value);
            });
        }

        public static void EachPropertyValue(this object instance, Action<PropertyInfo, object, int> action)
        {
            instance.EachPropertyInfo((pi, i) =>
            {
                object value = pi.GetValue(instance, null);
                action(pi, value, i);
            });
        }
        /// <summary>
        /// Return the results of the eacher by passing the PropertyInfo and value
        /// of each of the properties of instance where the property type is one
        /// of the base supported data types; bool, int, long, decimal, string, 
        /// byte[], DateTime or their nullable equivalent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="eacher"></param>
        /// <returns></returns>
        public static IEnumerable<T> EachDataProperty<T>(this object instance, Func<PropertyInfo, object, T> eacher)
        {
            return EachDataProperty(instance, instance.GetType(), (pi) => true, eacher);
        }
        /// <summary>
        /// Return the results of the eacher by passing the PropertyInfo and value
        /// of each of the properties of instance where the property type is one
        /// of the base supported data types; bool, int, long, decimal, string, 
        /// byte[], DateTime or their nullable equivalent 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        /// <param name="eacher"></param>
        /// <returns></returns>
        public static IEnumerable<T> EachDataProperty<T>(this object instance, Type type, Func<PropertyInfo, object, T> eacher)
        {
            return EachDataProperty(instance, type, (pi) => true, eacher);
        }
        /// <summary>
        /// Return the results of the eacher by passing the PropertyInfo and value
        /// of each of the properties of instance where the property type is one
        /// of the base supported data types; bool, int, long, decimal, string, 
        /// byte[], DateTime or their nullable equivalent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyPredicate"></param>
        /// <param name="eacher"></param>
        /// <returns></returns>
        public static IEnumerable<T> EachDataProperty<T>(this object instance, Func<PropertyInfo, bool> propertyPredicate, Func<PropertyInfo, object, T> eacher)
        {
            return EachDataProperty<T>(instance, instance.GetType(), propertyPredicate, eacher);
        }

        /// <summary>
        /// Return the results of the eacher by passing the PropertyInfo and value
        /// of each of the properties of instance where the property is declared
        /// on the specified type and the property type is one of the base 
        /// supported data types; bool, int, long, decimal, string, byte[], 
        /// DateTime or their nullable equivalent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        /// <param name="propertyPredicate"></param>
        /// <param name="eacher"></param>
        /// <returns></returns>
        public static IEnumerable<T> EachDataProperty<T>(this object instance, Type type, Func<PropertyInfo, bool> propertyPredicate, Func<PropertyInfo, object, T> eacher)
        {
            Args.ThrowIfNull(propertyPredicate, nameof(propertyPredicate));
            foreach (PropertyInfo pi in type.GetProperties().Where(pi => pi.DeclaringType == type && pi.PropertyType.In(typeof(bool?), typeof(bool), typeof(uint), typeof(uint?), typeof(int), typeof(int?), typeof(ulong), typeof(ulong?), typeof(long), typeof(long?), typeof(decimal), typeof(decimal?), typeof(string), typeof(byte[]), typeof(DateTime), typeof(DateTime?))))
            {
                if (propertyPredicate(pi))
                {
                    yield return eacher(pi, pi.GetValue(instance));
                }
            };
        }
        /// <summary>
        /// Return the Type as the string that can be used to 
        /// declare it in code
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToTypeString(this Type type, bool includeNamespace = true)
        {
            StringBuilder output = new StringBuilder();
            if (includeNamespace)
            {
                output.AppendFormat("{0}.", type.Namespace);
            }
            output.Append(type == typeof(int) || type == typeof(long) || type == typeof(uint) || type == typeof(ulong) ? type.Name: type.Name.DropTrailingNonLetters());
            if (type.IsGenericType)
            {
                output.AppendFormat("<{0}>", type.GetGenericArguments().ToDelimited(t => includeNamespace ? "{0}.{1}".Format(t.Namespace, t.ToTypeString(false)): t.ToTypeString(false)));
            }
            if (type.IsArray)
            {
                output.Append("[]");
            }
            return output.ToString();
        }

        public static bool IsOverridable(this MethodInfo method)
        {
            //https://msdn.microsoft.com/en-us/library/system.reflection.methodbase.isvirtual(v=vs.110).aspx
            return method.IsVirtual && !method.IsFinal;
        }

        private static void PropertyNotFound(string propertyName, Type type)
        {
            Args.Throw<InvalidOperationException>("Specified property ({0}) was not found on object of type ({1})", propertyName, type.Name);
        }


        public static bool IsEnumerable(this PropertyInfo property)
        {
            return property.PropertyType.IsEnumerable();
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == typeof(string))
            {
                return false; // it is but that's not what we're looking for
            }

            return type.IsArray ||
                   typeof(IEnumerable).IsAssignableFrom(type) ||
                   type.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }

        /// <summary>
        /// Get the type of the array or enumerable of the specified 
        /// property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Type? GetEnumerableType(this PropertyInfo property)
        {
            Type? result = null;
            if (property.PropertyType.IsArray)
            {
                result = property.PropertyType.GetElementType();
            }
            else if (property.PropertyType != typeof(string) &&
                     property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null)
            {
                result = property.PropertyType.GetInterfaces()
                    .Where(t => t.IsGenericType == true && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(t => t.GetGenericArguments()[0]).FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// Returns true if the specified toCheck type has 
        /// an enumerable property that is of the current type
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool HasEnumerableOfMe(this Type self, Type toCheck)
        {
            PropertyInfo ignore;
            return HasEnumerableOfMe(self, toCheck, out ignore);
        }

        /// <summary>
        /// Returns true if the specified toCheck type has 
        /// an enumerable property that is of the current type
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool HasEnumerableOfMe(this Type self, Type toCheck, out PropertyInfo enumerableProperty)
        {
            bool result = false;
            enumerableProperty = null;
            foreach (PropertyInfo property in toCheck.GetProperties())
            {
                Type enumerableType = property.GetEnumerableType();
                if (enumerableType != null)
                {
                    if (enumerableType == self)
                    {
                        enumerableProperty = property;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Determines if the method is a special property method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsProperty(this MethodInfo method)
        {
            return method.GetProperty() != null;
        }

        public static PropertyInfo GetProperty(this MethodInfo method)
        {
            if (!method.IsSpecialName) return null;
            string propertyName = method.Name.Substring(4);
            PropertyInfo p = method.DeclaringType.GetProperty(propertyName);

            return p;
        }


        public static PropertyInfo TryGetSourcePropNamed(this Type sourceType, string propertyName)
        {
            try
            {
                return sourceType.GetProperty(propertyName);
            }
            catch (AmbiguousMatchException ame)
            {
                return sourceType.GetProperties().FirstOrDefault(p => p.DeclaringType == sourceType && p.Name == propertyName);
            }
        }

        public static bool IsCompatibleWith(this PropertyInfo prop, PropertyInfo other)
        {
            return AreCompatibleProperties(prop, other);
        }

        public static bool AreCompatibleProperties(PropertyInfo destProp, PropertyInfo sourceProp)
        {
            return (sourceProp.PropertyType == destProp.PropertyType ||
                    sourceProp.PropertyType == Nullable.GetUnderlyingType(destProp.PropertyType) ||
                    Nullable.GetUnderlyingType(sourceProp.PropertyType) == destProp.PropertyType)
                   && destProp.CanWrite;
        }

        /// <summary>
        /// Determines whether the specified type is compatible with the specified other type.
        /// Compatibility means that they are of the same type or same nullable type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="otherType">Type of the other.</param>
        /// <returns>
        ///   <c>true</c> if [is compatible with] [the specified other type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCompatibleWith(this Type type, Type otherType)
        {
            return AreCompatibleTypes(type, otherType);
        }

        public static bool AreCompatibleTypes(this Type type, Type otherType)
        {
            return (type == otherType ||
                    otherType == Nullable.GetUnderlyingType(type) ||
                    Nullable.GetUnderlyingType(otherType) == type);
        }

        public static bool IsPrimitiveNullableOrString(this Type type)
        {
            return type == typeof(string) || IsPrimitiveOrNullable(type);
        }

        public static bool IsPrimitiveOrNullable(this Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            return type.IsPrimitive || underlyingType == null ? type.IsPrimitive : underlyingType.IsPrimitive;
        }

        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        public static bool IsForEachable(this Type type)
        {
            return IsForEachable(type, out Type ignore);
        }

        /// <summary>
        /// Determines whether the specified type is a list like type, like an array or generic
        /// List.  Excludes string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is for eachable] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsForEachable(this Type type, out Type underlyingType)
        {
            underlyingType = null;
            if (type == typeof(string))
            {
                return false;
            }

            if (type.IsArray)
            {
                underlyingType = type.GetElementType();
                return true;
            }

            underlyingType = type.GetGenericArguments().FirstOrDefault();
            return type.GetInterfaces().Contains(typeof(IEnumerable));
        }

        /// <summary>
        /// Determines whether the specified type is nullable of the specified
        /// generic argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is nullable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullable<T>(this Type type)
        {
            return IsNullable<T>(type, out Type ignore);
        }

        /// <summary>
        /// Determines whether the specified type is nullable of the specified generic argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="underlyingType">Type of the underlying.</param>
        /// <returns>
        ///   <c>true</c> if the specified underlying type is nullable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullable<T>(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return underlyingType == typeof(T);
            }

            return false;
        }


    }
}
