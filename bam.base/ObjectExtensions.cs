using Bam.Net.Data.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace Bam.Net    
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Double null check the specified toInit locking on the current
        /// object using the specified ifNull function to instantiate if 
        /// toInit is null.  This guarantees thread safe access to the
        /// resulting object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sync"></param>
        /// <param name="toInit"></param>
        /// <param name="ifNull"></param>
        /// <returns></returns>
        public static T DoubleCheckLock<T>(this object sync, ref T toInit, Func<T> ifNull)
        {
            if (toInit == null)
            {
                lock (sync)
                {
                    if (toInit == null)
                    {
                        toInit = ifNull();
                    }
                }
            }

            return toInit;
        }

        public static string TryPropertiesToString(this object obj, string separator = "\r\n")
        {
            try
            {
                return obj.PropertiesToString(separator);
            }
            catch
            {
                // don't crash
            }

            return string.Empty;
        }

        public static string PropertiesToString(this object obj, string separator = "\r\n")
        {
            Args.ThrowIfNull(obj);

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            return PropertiesToString(obj, properties, separator);
        }

        /// <summary>
        /// Read the properties of the specified object and return the 
        /// values as a string on a single line
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string PropertiesToLine(this object obj)
        {
            return PropertiesToString(obj, "~");
        }

        public static Dictionary<string, object> PropertiesToDictionary(this object obj, PropertyInfo[] properties)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        if (property.PropertyType == typeof(string[]))
                        {
                            string[] values = ((string[])property.GetValue(obj, null)) ?? new string[] { };
                            result.Add(property.Name, string.Join(", ", values));
                        }
#if NET472
                        else if (property.PropertyType == typeof(HttpCookieCollection))
                        {
                            object value = property.GetValue(obj, null);
                            if (value != null)
                            {
                                HttpCookieCollection values = (HttpCookieCollection)value;
                                List<string> strings = new List<string>();
                                foreach (HttpCookie cookie in values)
                                {
                                    strings.Add(string.Format("{0}={1}", cookie.Name, cookie.Value));
                                }
                                result.Add(property.Name, string.Join("\r\n\t", strings.ToArray()));
                            }
                            else
                            {
                                result.Add(property.Name, "[null]");
                            }
                        }
#endif
                        else if (property.PropertyType == typeof(System.Net.CookieCollection))
                        {
                            object value = property.GetValue(obj, null);
                            if (value != null)
                            {
                                System.Net.CookieCollection values = (System.Net.CookieCollection)value;
                                List<string> strings = new List<string>();
                                foreach (System.Net.Cookie cookie in values)
                                {
                                    strings.Add($"{cookie.Name}={cookie.Value}");
                                }

                                result.Add(property.Name, string.Join("\r\n\t", strings.ToArray()));
                            }
                            else
                            {
                                result.Add(property.Name, "[null]");
                            }
                        }
                        else if (property.PropertyType == typeof(NameValueCollection))
                        {
                            object value = property.GetValue(obj, null);
                            if (value != null)
                            {
                                NameValueCollection values = (NameValueCollection)value;
                                List<string> strings = new List<string>();
                                foreach (string key in values.AllKeys)
                                {
                                    strings.Add($"{key}={values[key]}");
                                }

                                result.Add(property.Name, string.Join("\r\n\t", strings.ToArray()));
                            }
                            else
                            {
                                result.Add(property.Name, "[null]");
                            }
                        }
                        else if (property.GetIndexParameters().Length == 0)
                        {
                            object value = property.GetValue(obj, null);
                            string stringValue = "[null]";
                            if (value != null)
                            {
                                if (value is IEnumerable values && !(value is string))
                                {
                                    List<string> strings = new List<string>();
                                    foreach (object o in values)
                                    {
                                        strings.Add(o.ToString());
                                    }

                                    stringValue = string.Join("\r\n\t", strings.ToArray());
                                }
                                else
                                {
                                    stringValue = value.ToString();
                                }
                            }

                            result.Add(property.Name, stringValue);
                        }
                        else if (property.GetIndexParameters().Length > 0)
                        {
                            result.Add($"Indexed({property.Name})", string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Add(property.Name, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add("EXCEPTION", ex.Message);
            }

            return result;
        }

        public static string PropertiesToString(this object obj, PropertyInfo[] properties, string separator = "\r\n")
        {
            try
            {
                StringBuilder returnValue = new StringBuilder();
                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        if (property.PropertyType == typeof(string[]))
                        {
                            string[] values = ((string[])property.GetValue(obj, null)) ?? new string[] { };
                            returnValue.AppendFormat("{0}: {1}{2}", property.Name, string.Join(", ", values),
                                separator);
                        }
#if NET472
                        else if (property.PropertyType == typeof(HttpCookieCollection))
                        {
                            object value = property.GetValue(obj, null);
                            if (value != null)
                            {
                                HttpCookieCollection values = (HttpCookieCollection)value;
                                List<string> strings = new List<string>();
                                foreach (HttpCookie cookie in values)
                                {
                                    strings.Add(string.Format("{0}={1}", cookie.Name, cookie.Value));
                                }
                                returnValue.AppendFormat("{0}: {1}{2}", property.Name, string.Join("\r\n\t", strings.ToArray()), separator);
                            }
                            else
                            {
                                returnValue.AppendFormat("{0}: [null]{1}", property.Name, separator);
                            }
                        }
#endif
                        else if (property.PropertyType == typeof(System.Net.CookieCollection))
                        {
                            object value = property.GetValue(obj, null);
                            if (value != null)
                            {
                                System.Net.CookieCollection values = (System.Net.CookieCollection)value;
                                List<string> strings = new List<string>();
                                foreach (System.Net.Cookie cookie in values)
                                {
                                    strings.Add(string.Format("{0}={1}", cookie.Name, cookie.Value));
                                }

                                returnValue.AppendFormat("{0}: {1}{2}", property.Name,
                                    string.Join("\r\n\t", strings.ToArray()), separator);
                            }
                            else
                            {
                                returnValue.AppendFormat("{0}: [null]{1}", property.Name, separator);
                            }
                        }
                        else if (property.PropertyType == typeof(NameValueCollection))
                        {
                            object value = property.GetValue(obj, null);
                            if (value != null)
                            {
                                NameValueCollection values = (NameValueCollection)value;
                                List<string> strings = new List<string>();
                                foreach (string key in values.AllKeys)
                                {
                                    strings.Add(string.Format("{0}={1}", key, values[key]));
                                }

                                returnValue.AppendFormat("{0}: {1}{2}", property.Name,
                                    string.Join("\r\n\t", strings.ToArray()), separator);
                            }
                            else
                            {
                                returnValue.AppendFormat("{0}: [null]{1}", property.Name, separator);
                            }
                        }
                        else if (property.GetIndexParameters().Length == 0)
                        {
                            object value = property.GetValue(obj, null);
                            string stringValue = "[null]";
                            if (value != null)
                            {
                                if (value is IEnumerable values && !(value is string))
                                {
                                    List<string> strings = new List<string>();
                                    foreach (object o in values)
                                    {
                                        strings.Add(o.ToString());
                                    }

                                    stringValue = string.Join("\r\n\t", strings.ToArray());
                                }
                                else
                                {
                                    stringValue = value.ToString();
                                }
                            }

                            returnValue.AppendFormat("{0}: {1}{2}", property.Name, stringValue, separator);
                        }
                        else if (property.GetIndexParameters().Length > 0)
                        {
                            returnValue.AppendFormat("Indexed Property:{0}{1}", property.Name, separator);
                        }
                    }
                    catch (Exception ex)
                    {
                        returnValue.AppendFormat("{0}: ({1}){2}", property.Name, ex.Message, separator);
                    }
                }

                return returnValue.ToString();
            }
            catch (Exception ex)
            {
                return $"Error Getting Properties: {ex.Message}";
            }
        }
        /// <summary>
        /// Serialize the current object to json in the specified path
        /// </summary>
        /// <param name="value"></param>
        /// <param name="path"></param>
        public static void ToJsonFile(this object value, string path)
        {
            ToJsonFile(value, new FileInfo(path));
        }

        /// <summary>
        /// Serialize the current object as json to the specified file overwriting
        /// the existing file if there is one.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="file"></param>
        public static void ToJsonFile(this object value, FileInfo file)
        {
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            ToJson(value, Newtonsoft.Json.Formatting.Indented).SafeWriteToFile(file.FullName, true);
        }
        public static string ToJson(this object value, Newtonsoft.Json.Formatting formatting)
        {
            return JsonConvert.SerializeObject(value, formatting);
        }

        public static int GetHashCode(this object instance, params object[] propertiesToInclude)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (object property in propertiesToInclude)
                {
                    if (property != null)
                    {
                        hash = (hash * 16777619) ^ property.GetHashCode();
                    }
                }

                return hash;
            }
        }

        /// <summary>
        /// Copies all the subscribed event handlers from source to
        /// the destination
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object CopyEventHandlers(this object destination, object source)
        {
            GetEventSubscriptions(source).Each(es => { es.EventInfo.AddEventHandler(destination, es.Delegate); });
            return destination;
        }

        /// <summary>
        /// Gets all the subscribed event subscriptions for the specified event name
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public static IEnumerable<IEventSubscription> GetEventSubscriptions(this object instance, string eventName)
        {
            return GetEventSubscriptions(instance).Where(es => es.EventInfo.Name.Equals(eventName));
        }

        /// <summary>
        /// Gets All the subscribed event subscriptions
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IEnumerable<IEventSubscription> GetEventSubscriptions(this object instance)
        {
            Type type = instance.GetType();
            Func<System.Reflection.EventInfo, FieldInfo> ei2fi =
                ei => type.GetField(ei.Name,
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.GetField);

            // ** yuck **
            IEnumerable<IEventSubscription> results = from eventInfo in type.GetEvents()
                                                     let eventFieldInfo = ei2fi(eventInfo)
                                                     let eventFieldValue =
                                                         (System.Delegate)eventFieldInfo?.GetValue(instance)
                                                     from subscribedDelegate in eventFieldValue == null
                                                         ? new Delegate[] { }
                                                         : eventFieldValue.GetInvocationList()
                                                     select new EventSubscription
                                                     {
                                                         EventName = eventFieldInfo.Name,
                                                         Delegate = subscribedDelegate,
                                                         FieldInfo = eventFieldInfo,
                                                         EventInfo = eventInfo
                                                     };
            // ** /yuck **
            return results;
        }

        /// <summary>
        /// Copies all properties from source to destination where the name and
        /// type match.  Accounts for nullability and treats non nullable and
        /// nullable primitives as compatible
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object CopyProperties(this object destination, object source)
        {
            if (destination == null || source == null)
            {
                return destination;
            }

            ForEachProperty(destination, source, CopyProperty);

            return destination;
        }

        /// <summary>
        /// Same as CopyProperties but will clone properties
        /// whos type implements ICloneable
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object CloneProperties(this object destination, object source)
        {
            if (destination == null || source == null)
            {
                return destination;
            }

            ForEachProperty(destination, source, CloneProperty);

            return destination;
        }

        private static void ForEachProperty(object destination, object source,
            Action<object, object, PropertyInfo, PropertyInfo> action)
        {
            Type destinationType = destination.GetType();
            Type sourceType = source.GetType();

            foreach (PropertyInfo destProp in destinationType.GetProperties())
            {
                PropertyInfo sourceProp = sourceType.TryGetSourcePropNamed(destProp.Name);
                action(destination, source, destProp, sourceProp);
            }
        }

        internal static void CopyProperty(this object destination, object source, PropertyInfo destProp,
            PropertyInfo sourceProp)
        {
            if (sourceProp != null)
            {
                if (destProp.IsCompatibleWith(sourceProp))
                {
                    ParameterInfo[] indexParameters = sourceProp.GetIndexParameters();
                    if (indexParameters == null || indexParameters.Length == 0)
                    {
                        object value = sourceProp.GetValue(source, null);
                        destProp.SetValue(destination, value, null);
                    }
                }
            }
        }

        internal static void CloneProperty(this object destination, object source, PropertyInfo destProp,
            PropertyInfo sourceProp)
        {
            if (sourceProp != null)
            {
                if (destProp.IsCompatibleWith(sourceProp))
                {
                    object value = sourceProp.GetValue(source, null);
                    if (value is ICloneable cloneable)
                    {
                        value = cloneable.Clone();
                    }

                    destProp.SetValue(destination, value, null);
                }
            }
        }

        /// <summary>
        /// Serialize the specified object to yaml
        /// </summary>
        /// <param name="val"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static string ToYaml(this object val)
        {
            Serializer serializer = new Serializer();
            return serializer.Serialize(val);
        }

        public static void ToYamlFile(this object val, string path)
        {
            ToYamlFile(val, new FileInfo(path));
        }

        public static void ToYamlFile(this object val, FileInfo file)
        {
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            using (StreamWriter sw = new StreamWriter(file.FullName))
            {
                sw.Write(val.ToYaml());
            }
        }

        public static string ToJson(this object value, params JsonConverter[] converters)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            if (converters != null && converters.Length > 0)
            {
                settings.Converters = new List<JsonConverter>(converters);
            }

            return JsonConvert.SerializeObject(value, settings);
        }

        public static T TryCopyAs<T>(this object source) where T : new()
        {
            try
            {
                return CopyAs<T>(source);
            }
            catch
            {
                // don't crash
            }
            return default;
        }

        /// <summary>
        /// Copy the current source instance as the specified generic
        /// type T copying all properties that match in name and type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CopyAs<T>(this object source) where T : new()
        {
            T result = new T();
            result.CopyProperties(source);
            return result;
        }

        /// <summary>
        /// Copies the specified object as the specified generic repo data type.  The 
        /// new value has an Id of 0 so attempts to save it in a DaoRepository results
        /// in a new entry rather than an attempted update.
        /// </summary>
        /// <typeparam name="TRepoData"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TRepoData CopyAsNew<TRepoData>(this object source) where TRepoData : IRepoData, new()
        {
            TRepoData result = new TRepoData();
            result.CopyProperties(source);
            result.Id = 0;
            return result;
        }

        /// <summary>
        /// Copy the current source instance as the specified type
        /// copying all properties that match in name and type.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CopyAs(this object source, Type type, params object[] ctorParams)
        {
            if (source == null)
            {
                return source;
            }

            object result = type.Construct(ctorParams);
            result.CopyProperties(source);
            return result;
        }

        public static string ToJson(this object value, bool pretty,
            NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Newtonsoft.Json.Formatting formatting =
                pretty ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = formatting,
                NullValueHandling = nullValueHandling
            };
            return value.ToJson(settings);
        }

        public static string ToJson(this object value, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(value, settings);
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }

        public static IObjectEncoder ObjectEncoder
        {
            get;
            set;
        }

        public static IObjectEncoding Encode(this object value)
        {
            Args.ThrowIfNull(ObjectEncoder, $"{nameof(ObjectExtensions)}.{nameof(ObjectEncoder)}");
            return ObjectEncoder.Encode(value);
        }

        public static void EncodeToFile(this object value, string filePath)
        {
            IObjectEncoding encoding = value.Encode();
            File.WriteAllBytes(filePath, encoding.Bytes);
        }

        /// <summary>
        /// Clears the locks created for writing and appending
        /// to files
        /// </summary>
        public static void ClearFileAccessLocks(this object any)
        {
            FileLock.ClearLocks();
        }

        /// <summary>
        /// Serialize the object to the specified filePath.  The same as
        /// ToXmlFile().
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filePath"></param>
        public static void XmlSerialize(this object target, string filePath)
        {
            ToXmlFile(target, filePath);
        }

        /// <summary>
        /// Serialize the object to the specified filePath.  The same as XmlSerialzie()
        /// </summary>
        /// <param name="target"></param>
        /// <param name="filePath"></param>
        public static void ToXmlFile(this object target, string filePath)
        {
            ToXmlFile(target, filePath, true);
        }

        static object lockObj = new object();
        public static void ToXmlFile(this object target, string filePath, bool throwIfNotSerializable)
        {
            Type t = target.GetType();
            if (throwIfNotSerializable)
            {
                bool isSerializable = IsSerializable(t);
                if (!isSerializable)
                {
                    ThrowInvalidOperationException(t);
                }
            }

            XmlSerializer ser = new XmlSerializer(t);
            lock (lockObj)
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    ser.Serialize(sw, target);
                }
            }
        }

        public static string ToXml(this object target)
        {
            MemoryStream stream = new MemoryStream();
            ToXmlStream(target, stream);
            stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string ToXml(this object target, bool includeXmlDeclaration)
        {
            return ToXml(target, new XmlWriterSettings { OmitXmlDeclaration = !includeXmlDeclaration, Indent = true });
        }

        public static string ToXml(this object target, XmlWriterSettings settings)
        {
            MemoryStream stream = new MemoryStream();
            ToXmlStream(target, stream, settings);
            stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void ToXmlStream(this object target, Stream stream)
        {
            ToXmlStream(target, stream, new XmlWriterSettings { Indent = true });
        }

        public static void ToXmlStream(this object target, Stream stream, XmlWriterSettings settings)
        {
            XmlSerializer ser = new XmlSerializer(target.GetType());
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                ser.Serialize(writer, target);
            }
        }

        /// <summary>
        /// Return a Stream containing the current
        /// object as json
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Stream ToJsonStream(this object value)
        {
            MemoryStream stream = new MemoryStream();
            ToJsonStream(value, stream);
            return stream;
        }

        /// <summary>
        /// Write the specified value to the specified stream as json
        /// </summary>
        /// <param name="value"></param>
        /// <param name="stream"></param>
        public static void ToJsonStream(this object value, Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(value.ToJson());
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }
        public static Stream ToYamlStream(this object value)
        {
            MemoryStream stream = new MemoryStream();
            ToYamlStream(value, stream);
            return stream;
        }

        public static void ToYamlStream(this object value, Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(value.ToYaml());
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }

        private static void ThrowInvalidOperationException(Type t)
        {
            throw new InvalidOperationException("The target object specified is of type " + t.Name + " which is not serializable.  If this is a user defined type add the [Serializable] attribute to the class definition");
        }

        private static bool IsSerializable(Type t)
        {
            bool isSerializable = t.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0;
            return isSerializable;
        }
    }
}
