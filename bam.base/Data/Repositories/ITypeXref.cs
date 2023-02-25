/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Reflection;

namespace Bam.Net.Data.Repositories
{
    public interface ITypeXref
    {
        string Hash { get; }
        Type Left { get; set; }
        string LeftArrayOrList { get; }
        PropertyInfo LeftCollectionProperty { get; set; }
        string LeftCollectionTypeName { get; }
        string LeftDaoName { get; }
        string LeftLengthOrCount { get; }
        Type Right { get; set; }
        string RightArrayOrList { get; }
        PropertyInfo RightCollectionProperty { get; set; }
        string RightCollectionTypeName { get; }
        string RightDaoName { get; }
        string RightLengthOrCount { get; }
        ITypeTableNameProvider TableNameProvider { get; set; }
    }
}