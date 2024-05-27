using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public static class TypeExtensions
    {
        public static void ForEachPublicStaticField(this Type type, Action<object> forEach)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                forEach(field.GetRawConstantValue());
            }
        }
        /// <summary>
        /// Construct an instance of the type
        /// </summary>
        /// <typeparam name="T">The type to cast the result as</typeparam>
        /// <param name="type">The type whose constructor will be called</param>
        /// <param name="ctorArgs">The parameters to pass to the constructor if any</param>
        /// <returns></returns>
        public static T Construct<T>(this Type type, params object[] ctorArgs)
        {
            return (T)type.Construct(ctorArgs);
        }

        /// <summary>
        /// Construct an instance of the specified type passing in the
        /// specified parameters to the constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ctorArgs"></param>
        /// <returns></returns>
        public static object Construct(this Type type, params object[] ctorArgs)
        {
            ConstructorInfo ctor = GetConstructor(type, ctorArgs);
            object val = null;
            if (ctor != null)
            {
                val = ctor.Invoke(ctorArgs);
            }

            return val;
        }

        public static bool TryConstruct(this Type type, out object constructed, params object[] ctorArgs)
        {
            return type.TryConstruct(out constructed, ex => { }, ctorArgs);
        }

        public static bool TryConstruct(this Type type, out object constructed, Action<Exception> catcher,
            params object[] ctorArgs)
        {
            bool result = false;
            constructed = null;
            try
            {
                constructed = Construct(type, ctorArgs);
                result = constructed != null;
            }
            catch (Exception ex)
            {
                catcher(ex);
                result = false;
            }

            return result;
        }

        public static bool TryConstruct<T>(this Type type, out T constructed, params object[] ctorArgs)
        {
            return type.TryConstruct(out constructed, ex => { }, ctorArgs);
        }

        public static bool TryConstruct<T>(this Type type, out T constructed, Action<Exception> catcher,
            params object[] ctorArgs)
        {
            bool result = true;
            constructed = default(T);
            try
            {
                constructed = Construct<T>(type, ctorArgs);
            }
            catch (Exception ex)
            {
                catcher(ex);
                result = false;
            }

            return result;
        }


        private delegate T CompiledLambdaCtor<T>(params object[] ctorArgs);

        /// <summary>
        /// Construct an instance of the type using a dynamically defined and
        /// compiled lambda.  This "should" replace existing Construct&lt;T&gt;
        /// implementation after benchmarks prove this one is faster.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="ctorArgs"></param>
        /// <returns></returns>
        public static T DynamicConstruct<T>(this Type type, params object[] ctorArgs)
        {
            ParameterExpression param;
            NewExpression newExp;
            GetExpressions(type, ctorArgs, out param, out newExp);

            LambdaExpression lambda = Expression.Lambda(typeof(CompiledLambdaCtor<T>), newExp, param);
            CompiledLambdaCtor<T> compiled = (CompiledLambdaCtor<T>)lambda.Compile();
            return compiled(ctorArgs);
        }

        private delegate object CompiledLambdaCtor(params object[] ctorArgs);

        /// <summary>
        /// Construct an instance of the type using a dynamically defined and
        /// compiled lambda.  This "should" replace existing Construct&lt;T&gt;
        /// implementation after benchmarks prove this one is faster.
        /// Testing shows this is actually roughly 2x slower than the existing 
        /// Construct methods.  Keeping here for novelty reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="ctorArgs"></param>
        /// <returns></returns>
        public static object DynamicConstruct(this Type type, params object[] ctorArgs)
        {
            ParameterExpression param;
            NewExpression newExp;
            GetExpressions(type, ctorArgs, out param, out newExp);

            LambdaExpression lambda = Expression.Lambda(typeof(CompiledLambdaCtor), newExp, param);
            CompiledLambdaCtor compiled = (CompiledLambdaCtor)lambda.Compile();
            return compiled(ctorArgs);
        }

        private static void GetExpressions(Type type, object[] ctorArgs, out ParameterExpression param,
            out NewExpression newExp)
        {
            ConstructorInfo ctor = GetConstructor(type, ctorArgs);
            ParameterInfo[] parameterInfos = ctor.GetParameters();

            param = Expression.Parameter(typeof(object[]), "args");
            Expression[] argsExp = new Expression[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = parameterInfos[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            ;

            newExp = Expression.New(ctor, argsExp);
        }

        private static ConstructorInfo GetConstructor(Type type, object[] ctorArgs)
        {
            List<Type> paramTypes = new List<Type>();
            foreach (object o in ctorArgs)
            {
                paramTypes.Add(o.GetType());
            }

            ConstructorInfo ctor = type.GetConstructor(paramTypes.ToArray());
            return ctor;
        }
    }
}
