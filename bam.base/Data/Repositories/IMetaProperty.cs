/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bam.Net.Data.Repositories
{
    public interface IMetaProperty
    {
        object CurrentValue { get; }
        int HighestVersion { get; }
        Meta Meta { get; }
        PropertyInfo PropertyInfo { get; }
        Dictionary<int, DateTime> VersionDates { get; }
        int[] Versions { get; }

        IMetaPropertyVersionInfo GetVersionInfo();
        IMetaPropertyVersionInfo GetVersionInfo(int version);
        IMetaPropertyVersionInfo[] GetVersionInfos();
        object GetVersionValue(int version);
        void RollValueBack(DateTime to);
        void SetValue(object propertyValue);
        void SetValueToVersion(int version);
    }
}