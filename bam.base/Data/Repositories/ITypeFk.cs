/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Reflection;

namespace Bam.Net.Data.Repositories
{
    public interface ITypeFk
    {
        PropertyInfo ChildParentProperty { get; set; }
        PropertyInfo CollectionProperty { get; set; }
        PropertyInfo ForeignKeyProperty { get; set; }
        Type ForeignKeyType { get; set; }
        string Hash { get; }
        PropertyInfo PrimaryKeyProperty { get; set; }
        Type PrimaryKeyType { get; set; }
    }
}