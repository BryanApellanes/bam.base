/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Reflection;

namespace Bam.Data.Repositories
{
    public interface IMeta
    {
        object Data { get; }
        string Hash { get; }
        ulong Id { get; }
        string IdHash { get; }
        bool IsSerializable { get; }
        IObjectPersister ObjectPersister { get; set; }
        bool RequireIdProperty { get; set; }
        Type Type { get; set; }
        string Uuid { get; set; }
        string UuidHash { get; }

        IMetaProperty Property(string propertyName);
        Meta Property(string propertyName, object value);
        T ReadProperty<T>(PropertyInfo propInfo);
        T ReadPropertyVersion<T>(PropertyInfo propInfo, int version);
        void SetMeta();
        void SetMeta(object data);
        void WriteProperty(PropertyInfo propInfo, object propertyValue);
    }
}