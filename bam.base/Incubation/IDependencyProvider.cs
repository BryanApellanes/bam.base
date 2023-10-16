/*
	Copyright © Bryan Apellanes 2015  
*/
using System;

namespace Bam.Services
{
    public interface IDependencyProvider
    {
        T Construct<T>(params object[] ctorParams);
        T Construct<T>(params Type[] ctorParamTypes);
        object Get(Type type);
        T Get<T>();
        T Get<T>(params object[] ctorParams);
        T Get<T>(params Type[] ctorParamTypes);
        void Set<T>(T instance);
        void Set<T>(Func<T> instanciator);
        object this[Type type] { get; set; }
    }
}
