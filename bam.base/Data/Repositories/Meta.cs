/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Reflection;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;

namespace Bam.Data.Repositories
{
	/// <summary>
	/// Provides typed meta data about persisted or persistable objects.
	/// </summary>
	/// <typeparam name="T">The type of the wrapped data object.</typeparam>
	/// <seealso cref="Bam.Data.Repositories.Meta" />
	[Serializable]
	public class Meta<T>: Meta
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="Meta{T}"/> class.
        /// </summary>
        public Meta() : base()
        {
            Type = typeof(T);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Meta{T}"/> class with the specified data and object persister.
        /// </summary>
        /// <param name="data">The typed data object to wrap.</param>
        /// <param name="objectPersister">The object persister used for reading and writing.</param>
        public Meta(T data, IObjectPersister objectPersister) : base(data!, objectPersister)
        {
            Type = typeof(T);
        }

        /// <summary>
        /// Gets or sets the wrapped data object cast to type <typeparamref name="T"/>.
        /// </summary>
        public T TypedData
		{
			get
			{
				return (T)Data;
			}
			set
			{
				Data = value!;
			}
		}

        /// <summary>
        /// Implicitly converts a <see cref="Meta{T}"/> to its underlying typed data.
        /// </summary>
        /// <param name="meta">The meta instance to unwrap.</param>
        /// <returns>The underlying <typeparamref name="T"/> data object.</returns>
        public static implicit operator T(Meta<T> meta)
		{
			return meta.TypedData;
		}
	}

    /// <summary>
    /// Provides meta data (Id, Uuid, Cuid, hash values) about persisted or persistable objects,
    /// and encapsulates reading and writing object properties through an <see cref="IObjectPersister"/>.
    /// </summary>
    [Serializable]
	public class Meta
	{
		IObjectPersister _objectPersister = null!;
		object _objectPersisterLock = new object();
		/// <summary>
		/// Gets or sets the object persister used for reading and writing data. Resolved from the service registry if not explicitly set.
		/// </summary>
		public IObjectPersister ObjectPersister
		{
			get
			{
				return _objectPersisterLock.DoubleCheckLock(ref _objectPersister, () => ServiceRegistry.Default!.Get<IObjectPersister>());
			}
			set
			{
				_objectPersister = value;
			}
		}

		protected internal ulong GetNextId(Type type, IObjectPersister? objectReaderWriter = null)
		{
			objectReaderWriter = objectReaderWriter ?? this.ObjectPersister;
			DirectoryInfo dir = new DirectoryInfo(Path.Combine(objectReaderWriter.RootDirectory, type.Name));
			FileInfo metaFile = new FileInfo(Path.Combine(dir.FullName, "meta.id"));
			if (metaFile.Exists)
			{
				string idString = metaFile.FullName.SafeReadFile();
				idString = string.IsNullOrEmpty(idString) ? "0" : idString;
				ulong result = 0;
				if (ulong.TryParse(idString, out ulong parsed))
				{
					result = ++parsed;
				}
				else
				{
					++result;
				}
				metaFile.FullName.SafeWriteFile(result.ToString());
				return result;
			}
			else
			{
				"1".SafeWriteToFile(metaFile.FullName);
				return 1;
			}
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="Meta"/> class.
        /// </summary>
        public Meta()
		{
			RequireIdProperty = true;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Meta"/> class with the specified data and object persister.
        /// </summary>
        /// <param name="data">The data object to wrap.</param>
        /// <param name="objectPersister">The object persister used for reading and writing.</param>
        /// <param name="setMeta">If true, initializes the Id, Uuid, and Cuid on the data object.</param>
        public Meta(object data, IObjectPersister objectPersister, bool setMeta = true)
		{
			RequireIdProperty = true;
			ObjectPersister = objectPersister;
			if (setMeta)
			{
				SetMeta(data);
			}
		}


        Type _type = null!;
        /// <summary>
        /// Gets or sets the type of the wrapped data object. If not explicitly set, returns the runtime type of <see cref="Data"/>.
        /// </summary>
		public Type Type
		{
			get
			{
				return _type ?? Data?.GetType()!;
			}
            set
            {
                _type = value;
            }
		}

        /// <summary>
        /// Gets the wrapped data object.
        /// </summary>
		public object Data { get; internal set; } = null!;

        /// <summary>
        /// Gets a value indicating whether the wrapped data object has the <see cref="SerializableAttribute"/>.
        /// </summary>
		public bool IsSerializable
		{
			get
			{
				if (Data != null)
				{
					return Data.GetType().HasCustomAttributeOfType<SerializableAttribute>();
				}

				return false;
			}
		}

        /// <summary>
        /// Gets the hash algorithm used for computing hashes. Set internally during meta initialization.
        /// </summary>
        public HashAlgorithms HashAlgorithm
        {
            get;
            private set;
        }

        /// <summary>
        /// Reads the value of the specified property from persisted storage using the object persister.
        /// </summary>
        /// <typeparam name="T">The expected type of the property value.</typeparam>
        /// <param name="propInfo">The property to read.</param>
        /// <returns>The persisted property value, or default if the property info is null.</returns>
        public T? ReadProperty<T>(PropertyInfo propInfo)
		{
			if (propInfo != null)
			{
				T result = ObjectPersister.ReadProperty<T>(propInfo, Uuid);
				return result;
			}

			return default;
		}

        /// <summary>
        /// Reads a specific version of the specified property value from persisted storage.
        /// </summary>
        /// <typeparam name="T">The expected type of the property value.</typeparam>
        /// <param name="propInfo">The property to read.</param>
        /// <param name="version">The version number to retrieve.</param>
        /// <returns>The versioned property value, or default if the property info is null.</returns>
		public T ReadPropertyVersion<T>(PropertyInfo propInfo, int version)
		{
			if (propInfo != null)
			{
				T result = ObjectPersister.ReadPropertyVersion<T>(propInfo, Hash, version);
				return result;
			}

			return default(T)!;
		}

        /// <summary>
        /// Sets the specified property on the data object and persists the object asynchronously.
        /// </summary>
        /// <param name="propInfo">The property to write.</param>
        /// <param name="propertyValue">The value to set on the property.</param>
		public void WriteProperty(PropertyInfo propInfo, object? propertyValue)
		{
			if (propInfo != null)
			{
				propInfo.SetValue(Data, propertyValue);
				ObjectPersister.WriteAsync(Type, Data);
			}
		}

        /// <summary>
        /// Gets or sets whether an Id property is required on the data object. Defaults to true.
        /// </summary>
		public bool RequireIdProperty
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the hash for this meta instance, preferring <see cref="UuidHash"/> and falling back to <see cref="IdHash"/>.
		/// </summary>
		public string Hash
		{
			get
			{
				return GetHash();
			}
		}

        /// <summary>
        /// Gets the numeric Id of the wrapped data object, or 0 if the data is null.
        /// </summary>
		public ulong Id
		{
			get
			{
				if (Data != null)
				{
					return GetId(RequireIdProperty)!.Value;
				}
				return 0;
			}
		}

        /// <summary>
        /// Gets or sets the UUID of the wrapped data object, read from or written to the object's Uuid property via reflection.
        /// </summary>
		public string Uuid
		{
			get
			{
				return GetUuid(Data);
			}
			set
			{
				SetUuid(Data, value);
			}
		}

        /// <summary>
        /// Gets the MD5 hash computed from the Id and the full type name.
        /// </summary>
		public string IdHash
		{
			get
			{
				return GetIdHash();
			}
		}

        /// <summary>
        /// Gets the MD5 hash computed from the Uuid and the full type name.
        /// </summary>
		public string UuidHash
		{
			get
			{
				return GetUuidHash();
			}
		}

        /// <summary>
        /// Gets an <see cref="IMetaProperty"/> wrapper for the specified property on the data object.
        /// </summary>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        /// <returns>An <see cref="IMetaProperty"/> for the named property, or null if the property does not exist.</returns>
		public IMetaProperty Property(string propertyName)
		{
			PropertyInfo? prop = Data.GetType().GetProperty(propertyName);
			if (prop != null)
			{
				IMetaProperty property = new MetaProperty(this, prop);
				return property;
			}

			return null!;
		}

        /// <summary>
        /// Sets the value of the specified property on the data object.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>This <see cref="Meta"/> instance for method chaining.</returns>
		public Meta Property(string propertyName, object value)
		{
			Property(propertyName).SetValue(value);
			return this;
		}

        /// <summary>
        /// Initializes the meta data (Id, Uuid, Cuid) for the specified data object.
        /// If the data is itself a <see cref="Meta"/> instance, the inner data is unwrapped.
        /// </summary>
        /// <param name="data">The data object to initialize meta data for.</param>
		public void SetMeta(object data)
		{
			Meta meta = (data as Meta)!;
			if (meta != null)
			{
				data = meta.Data;
			}
			Data = data;
			SetMeta();
		}

        /// <summary>
        /// Determines whether two objects are equal by comparing their Uuid properties.
        /// </summary>
        /// <param name="one">The first object to compare.</param>
        /// <param name="two">The second object to compare.</param>
        /// <returns>True if the Uuid values of both objects are equal.</returns>
		public static bool AreEqual(object? one, object? two)
		{
			return UuidsAreEqual(one, two);
		}

        /// <summary>
        /// Determines whether two objects have the same Id property value.
        /// </summary>
        /// <param name="one">The first object to compare.</param>
        /// <param name="two">The second object to compare.</param>
        /// <returns>True if the Id values of both objects are equal.</returns>
		public static bool IdsAreEqual(object one, object two)
		{
			return Meta.GetId(one) == Meta.GetId(two);
		}

        /// <summary>
        /// Determines whether two objects have the same Uuid property value.
        /// </summary>
        /// <param name="one">The first object to compare.</param>
        /// <param name="two">The second object to compare.</param>
        /// <returns>True if the Uuid values of both objects are equal.</returns>
		public static bool UuidsAreEqual(object? one, object? two)
		{
			return Meta.GetUuid(one).Equals(Meta.GetUuid(two));
		}

        /// <summary>
        /// Determines whether this Meta is equal to the specified object by comparing Uuid values.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the Uuid values are equal.</returns>
		public override bool Equals(object? obj)
		{
			return AreEqual(this, obj);
		}

        /// <summary>
        /// Returns the hash code of the Uuid string.
        /// </summary>
        /// <returns>The hash code of the Uuid.</returns>
		public override int GetHashCode()
		{
			return Uuid.GetHashCode();
		}

        /// <summary>
        /// Initializes Id, Uuid, and Cuid on the wrapped data object if they are not already set.
        /// </summary>
		public virtual void SetMeta()
		{
			if (Data != null)
			{
				if (GetId(Data) <= 0)
				{
					SetId(Data);
				}
				if (string.IsNullOrEmpty(GetUuid(Data)))
				{
					SetUuid(Data);
				}
				SetCuid(Data);
			}
		}

        /// <summary>
        /// Sets the Created (if null) and Modified audit fields on the specified object to the current UTC time.
        /// </summary>
        /// <param name="value">The object whose audit fields should be set.</param>
		public static void SetAuditFields(object value)
		{
			Args.ThrowIfNull(value, "value");
			try
			{
				string created = "Created";
				string modified = "Modified";
				DateTime now = DateTime.UtcNow;
				DateTime? createdDate = value.Property<DateTime?>(created);
				if (createdDate == null)
				{
					value.Property(created, now);
				}
				value.Property(modified, now);
			}
			catch (Exception ex)
			{
				Log.AddEntry("An error occurred trying to set audit fields on object of type {0}", LogEventType.Error, ex, value.GetType().Name);
			}
		}

		protected string GetHash()
		{		
			string hash = GetUuidHash();
			if (string.IsNullOrEmpty(hash)) 
			{
				hash = GetIdHash();
			}

			return hash;
		}

		protected string GetIdHash()
		{
			Type type = Type ?? Type.Missing.GetType();
			string hash = $"{Id}{type.FullName}".Md5(); // TODO: make the algorithm configurable
			return hash;
		}
		
		protected string GetUuidHash()
		{
            Type type = Type ?? Type.Missing.GetType();
            return GetUuidHash(Uuid, type);
		}
    
        /// <summary>
        /// Computes an MD5 hash from the specified UUID and type full name.
        /// </summary>
        /// <param name="uuid">The UUID string.</param>
        /// <param name="type">The type whose full name is included in the hash input.</param>
        /// <returns>An MD5 hash string.</returns>
        public static string GetUuidHash(string uuid, Type type)
        {
			return $"{uuid}::{type.FullName}".Md5(); // TODO: make the algorithm configurable
        }

        /// <summary>
        /// Computes an MD5 hash from the Uuid property of the specified object and the given type full name.
        /// </summary>
        /// <param name="value">The object whose Uuid property is read via reflection. If null, an empty UUID is used.</param>
        /// <param name="type">The type whose full name is included in the hash input.</param>
        /// <returns>An MD5 hash string.</returns>
        public static string GetUuidHash(object? value, Type type)
        {
            string result = GetUuidHash("", Type.Missing.GetType());
            if (value != null)
            {
                result = GetUuidHash("", type);
                PropertyInfo? uuidProp = value.GetType().GetProperty("Uuid");
                if (uuidProp != null)
                {
                    string? uuid = uuidProp.GetValue(value) as string;
                    if (!string.IsNullOrEmpty(uuid))
                    {
                        result = GetUuidHash(uuid, type);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Computes an MD5 hash from the Id property of the specified object and its type full name.
        /// </summary>
        /// <param name="value">The object whose Id property is read via reflection.</param>
        /// <param name="type">The type to use for the hash. If null, the runtime type of <paramref name="value"/> is used.</param>
        /// <returns>An MD5 hash string.</returns>
        public static string GetIdHash(object value, Type? type = null)
        {
            type = type ?? value.GetType();
            ulong id = Meta.GetId(value)!.Value;
            return GetIdHash(id, type);
        }

        /// <summary>
        /// Computes an MD5 hash from the specified Id and type full name.
        /// </summary>
        /// <param name="id">The numeric Id.</param>
        /// <param name="type">The type whose full name is included in the hash input.</param>
        /// <returns>An MD5 hash string.</returns>
        public static string GetIdHash(long id, Type type)
        {
            return "{0}::{1}".Format(id, type.FullName!).Md5();
        }

        /// <summary>
        /// Computes an MD5 hash from the specified nullable Id and type full name.
        /// Returns an empty string if either parameter is null.
        /// </summary>
        /// <param name="id">The nullable numeric Id.</param>
        /// <param name="type">The type whose full name is included in the hash input.</param>
        /// <returns>An MD5 hash string, or an empty string if <paramref name="id"/> or <paramref name="type"/> is null.</returns>
        public static string GetIdHash(ulong? id, Type type)
        {
			if(id == null)
			{
				return string.Empty;
			}
			if(type == null)
			{
				return string.Empty;
			}

            return "{0}::{1}".Format(id, type.FullName!).Md5();
        }

        protected internal static string GetUuid(object? data, bool throwIfUuidPropertyMissing = false)
        {
            return GetPropValue(data, "Uuid", throwIfUuidPropertyMissing);
        }

        protected internal static string GetCuid(object? data, bool throwIfCuidPropertyMissing = false)
        {
            return GetPropValue(data, "Cuid", throwIfCuidPropertyMissing);
        }

        protected internal static string GetPropValue(object? data, string propName, bool throwIfPropertyMissing = false)
		{
			string result = string.Empty;
			if (data != null)
			{
				Type dataType = data.GetType();
				PropertyInfo? prop = dataType.GetProperty(propName);
				if (prop != null)
				{
					result = (string)prop.GetValue(data)!;
				}
				else if (throwIfPropertyMissing)
				{
					Args.Throw<InvalidOperationException>("The specified object of type {0} doesn't have a {1} property", dataType.Name, propName);
				}
			}
			return result!;
		}


		protected internal static bool HasKeyProperty(object data, out PropertyInfo prop)
		{
			prop = GetKeyProperty(data.GetType(), false)!;
			return prop != null;
		}

		protected internal static bool IsNew(object data)
		{
			return !IdIsSet(data) && !UuidIsSet(data);
		}

		protected internal static bool IdIsSet(object data)
		{
			bool result = false;
			PropertyInfo? key;
			if (HasKeyProperty(data, out key))
			{
				result = (long)key.GetValue(data)! > 0;
			}

			return result;
		}

		protected internal static bool UuidIsSet(object data)
		{
			bool result = false;
			PropertyInfo? uuidProp = data.GetType().GetProperty("Uuid");
			if (uuidProp != null)
			{
				result = !string.IsNullOrEmpty((string?)uuidProp.GetValue(data));
			}

			return result;
		}

		protected internal static PropertyInfo? GetKeyProperty(Type type, bool throwIfNoIdProperty = true)
		{
			PropertyInfo? keyProp = type.GetFirstProperyWithAttributeOfType<KeyAttribute>();
			if (keyProp == null)
			{
				keyProp = type.GetProperty("Id");
			}

			if (keyProp == null && throwIfNoIdProperty)
			{
				throw new NoIdPropertyException(type);
			}

            if(keyProp != null && keyProp.PropertyType != typeof(ulong) && keyProp.PropertyType != typeof(ulong?))
            {
                throw new InvalidIdPropertyTypeException(keyProp);
            }
			return keyProp;
		}

		protected virtual ulong? GetId(bool throwIfNoIdProperty = true)
        {
            return GetId(Data, throwIfNoIdProperty);
        }

        protected internal static ulong? GetId(object value, bool throwIfNoIdProperty = true)
        {
            PropertyInfo? pocoProp = GetKeyProperty(value.GetType(), throwIfNoIdProperty);
            if (pocoProp == null)
            {
				return null;
            }
            object? idValue = pocoProp.GetValue(value);   
            return (ulong?)idValue;
        }
		
		/// <summary>
		/// Sets the Id property of the specified value to the 
		/// next Id for it's type if it
		/// has not yet been set.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="objectReaderWriter"></param>
		internal void SetId(object value, IObjectPersister objectReaderWriter = null!)
		{
			objectReaderWriter = objectReaderWriter ?? this.ObjectPersister;
			Type type = value.GetType();
			PropertyInfo? idProp = type.GetProperty("Id");
			if (idProp != null)
			{
				ulong id = (ulong)idProp.GetValue(value)!;
                if (id == 0)
                {
                    ulong retrievedId = GetNextId(type, objectReaderWriter);

                    idProp.SetValue(value, retrievedId);
                }
			}
		}

		/// <summary>
		/// Sets the Uuid property of the specified data if
		/// it has not already been set
		/// </summary>
		/// <param name="data"></param>
		/// <param name="uuid"></param>
		internal static void SetUuid(object data, string? uuid = null)
		{
			Type type = data.GetType();
			PropertyInfo? uuidProp = type.GetProperty("Uuid");
			if (uuidProp != null)
			{
				if (string.IsNullOrEmpty(uuid))
				{
					uuid = uuidProp.GetValue(data) as string;
				}
                if (!Guid.TryParse(uuid, out Guid guid) || string.IsNullOrEmpty(uuid))
                {
                    guid = Guid.NewGuid();
                }
                uuidProp.SetValue(data, guid.ToString());
			}
		}

        internal static void SetCuid(object data, string? cuid = null, RandomSource randomSource = RandomSource.Simple)
        {
            Type type = data.GetType();
            PropertyInfo? cuidProp = type.GetProperty("Cuid");
            if(cuidProp != null)
            {
                if (string.IsNullOrEmpty(cuid))
                {
                    cuid = cuidProp.GetValue(data) as string;
                }
                if (string.IsNullOrEmpty(cuid))
                {
                    cuid = Cuid.Generate(randomSource);
                }
                cuidProp.SetValue(data, cuid);
            }
        }

		protected internal static int GetNextVersionNumber(DirectoryInfo propRoot, PropertyInfo prop, string hash)
		{
			int num = 0;
			int highest = GetHighestVersionNumber(propRoot, prop, hash);
			num = highest + 1;
			return num;
		}

		protected internal int GetHighestVersionNumber(PropertyInfo prop, string hash)
		{
			return GetHighestVersionNumber(this.ObjectPersister.GetPropertyDirectory(prop), prop, hash);
		}

		protected internal static int GetHighestVersionNumber(DirectoryInfo propRoot, PropertyInfo prop, string hash)
		{
			int highest = 0;
			if (!propRoot.Exists)
			{
				propRoot.Create();
			}
			List<FileInfo> files = GetPropertyFiles(propRoot, prop, hash);
			if (files.Count > 0)
			{
				files.Sort((one, two) => two.Name.CompareTo(one.Name));
				int.TryParse(files[0].Name, out highest);
			}
			return highest;
		}
		
		protected internal int[] GetVersions(PropertyInfo prop, string hash)
		{
			return GetVersions(this.ObjectPersister.GetPropertyDirectory(prop), prop, hash);
		}

		protected internal static int[] GetVersions(DirectoryInfo propertyDirectory, PropertyInfo prop, string hash)
		{
			List<FileInfo> files = GetPropertyFiles(propertyDirectory, prop, hash);
			return files.Select(f => int.Parse(f.Name)).ToArray();
		}

		protected internal Dictionary<int, DateTime> GetVersionDates(PropertyInfo prop, string hash)
		{
			Dictionary<int, DateTime> results = new Dictionary<int, DateTime>();
			foreach(FileInfo file in GetPropertyFiles(prop, hash))
			{
				results.Add(int.Parse(file.Name), file.LastWriteTime);
			}
			return results;
		}

		protected internal List<FileInfo> GetPropertyFiles(PropertyInfo prop, string hash)
		{
			return GetPropertyFiles(this.ObjectPersister.GetPropertyDirectory(prop), prop, hash);
		}

		protected internal static List<FileInfo> GetPropertyFiles(DirectoryInfo propertyDirectory, PropertyInfo prop, string hash)
		{
			DirectoryInfo propRootForHash = new DirectoryInfo(Path.Combine(propertyDirectory.FullName, hash, prop.PropertyType.Name));
			if (!propRootForHash.Exists)
			{
				propRootForHash.Create();
			}
			List<FileInfo> files = propRootForHash.GetFiles().Where(f => f.HasNoExtension()).ToList();
			return files;
		}
	}
}
