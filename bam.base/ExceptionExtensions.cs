/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Bam.Net
{
    public static class ExceptionExtensions
    {
        public static void ThrowIfNull(this object objectToCheck, string paramName = "objectToCheck")
        {
            if (objectToCheck == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ThrowIfEmptyOrNull(this string stringToCheck, string paramName = "stringToCheck")
        {
            if (string.IsNullOrEmpty(stringToCheck))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ThrowIfIsNotOfType<T>(this object objectToCheck, string paramName = "objectToCheck")
        {
            ThrowIfNull(objectToCheck, paramName);

            if (!(objectToCheck is T))
            {
                throw new InvalidOperationException($"The specified object is of concrete type {objectToCheck.GetType().Name} which is not of generic type {typeof(T).Name}.");
            }
        }

        public static void Throw<TException>(string messageFormat, params object[] args) where TException : Exception
        {
            throw CreateException<TException>(messageFormat, args);
        }

        public static TException CreateException<TException>(string messageFormat, params object[] args) where TException : Exception
        {
            Type exceptionType = typeof(TException);
            ConstructorInfo ctor = exceptionType.GetConstructor(new Type[] { typeof(string)});
            return (TException)ctor.Invoke(new object[] { string.Format(messageFormat, args) });
        }

        public static void ThrowInvalidOperation(string messageFormat, params object[] args)
        {
            throw new InvalidOperationException(string.Format(messageFormat, args));
        }

		public static Exception GetInnerException(this Exception exception)
        {
            return exception.GetBaseException();
		}
    }
}
