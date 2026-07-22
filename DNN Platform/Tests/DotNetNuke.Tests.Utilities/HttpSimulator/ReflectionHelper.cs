// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities.HttpSimulator
{
    using System;
    using System.Reflection;

    /// <summary>Helper class to simplify common reflection tasks.</summary>
    public static class ReflectionHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Returns the value of the private member specified.</summary>
        /// <param name="fieldName">Name of the member.</param>
        /// <param name="type">Type of the member.</param>
        /// <returns></returns>
        public static T GetStaticFieldValue<T>(string fieldName, Type type)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                return (T)field.GetValue(type);
            }

            return default(T);
        }

        /// <summary>Returns the value of the private member specified.</summary>
        /// <param name="fieldName">Name of the member.</param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static T GetStaticFieldValue<T>(string fieldName, string typeName)
        {
			Logger.LogDebug(">>> Entering GetStaticFieldValue(fieldName={FieldName},typeName={TypeName})", fieldName, typeName);
            var type = Type.GetType(typeName, true);
			Logger.LogDebug("<<< Exiting GetStaticFieldValue(fieldName={FieldName},type={Type})", fieldName, type);
			Logger.LogDebug("GetStaticFieldValue(fieldName={FieldName},typeName={TypeName}): type → {Type}", fieldName, typeName, type);
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
			Logger.LogDebug("GetStaticFieldValue(fieldName={FieldName},typeName={TypeName}): field → {Field}", fieldName, typeName, field);
            {
                return (T)field.GetValue(type);
            }

            return default(T);
        }

        /// <summary>Sets the value of the private static member.</summary>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public static void SetStaticFieldValue<T>(string fieldName, Type type, T value)
        {
			Logger.LogDebug(">>> Entering SetStaticFieldValue(fieldName={FieldName},type={Type},value={Value})", fieldName, type, value);
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
			Logger.LogDebug("SetStaticFieldValue(fieldName={FieldName},type={Type},value={Value}): field → {Field}", fieldName, type, value, field);
            {
                throw new ArgumentException(string.Format("Could not find the private instance field '{0}'", fieldName));
            }

            field.SetValue(null, value);
        }

        /// <summary>Sets the value of the private static member.</summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <param name="value"></param>
        public static void SetStaticFieldValue<T>(string fieldName, string typeName, T value)
        {
            var type = Type.GetType(typeName, true);
			Logger.LogDebug("<<< Exiting SetStaticFieldValue(fieldName={FieldName},type={Type},value={Value})", fieldName, type, value);
			Logger.LogDebug("SetStaticFieldValue(fieldName={FieldName},typeName={TypeName},value={Value}): type → {Type}", fieldName, typeName, value, type);
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
			Logger.LogDebug("SetStaticFieldValue(fieldName={FieldName},typeName={TypeName},value={Value}): field → {Field}", fieldName, typeName, value, field);
            {
                throw new ArgumentException(string.Format("Could not find the private instance field '{0}'", fieldName));
            }

            field.SetValue(null, value);
        }

        /// <summary>Returns the value of the private member specified.</summary>
        /// <param name="fieldName">Name of the member.</param>
        /// <param name="source">The object that contains the member.</param>
        /// <returns></returns>
        public static T GetPrivateInstanceFieldValue<T>(string fieldName, object source)
        {
			Logger.LogDebug(">>> Entering GetPrivateInstanceFieldValue(fieldName={FieldName},source={Source})", fieldName, source);
            var field = source.GetType().GetField(fieldName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
			Logger.LogDebug("GetPrivateInstanceFieldValue(fieldName={FieldName},source={Source}): field → {Field}", fieldName, source, field);
            {
                return (T)field.GetValue(source);
            }

            return default(T);
        }

        /// <summary>Returns the value of the private member specified.</summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="source">The object that contains the member.</param>
        /// <param name="value">The value to set the member to.</param>
        public static void SetPrivateInstanceFieldValue(string memberName, object source, object value)
        {
			Logger.LogDebug(">>> Entering SetPrivateInstanceFieldValue(memberName={MemberName},source={Source},value={Value})", memberName, source, value);
            var field = source.GetType().GetField(memberName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
			Logger.LogDebug("SetPrivateInstanceFieldValue(memberName={MemberName},source={Source},value={Value}): field → {Field}", memberName, source, value, field);
            {
                throw new ArgumentException(string.Format("Could not find the private instance field '{0}'", memberName));
            }

            field.SetValue(source, value);
			Logger.LogDebug("<<< Exiting SetPrivateInstanceFieldValue(memberName={MemberName},source={Source},value={Value})", memberName, source, value);
        }

        public static object Instantiate(string typeName)
        {
			Logger.LogDebug(">>> Entering Instantiate(typeName={TypeName})", typeName);
			Logger.LogDebug("<<< Exiting Instantiate(typeName={TypeName})", typeName);
            return Instantiate(typeName, null, null);
        }

        public static object Instantiate(string typeName, Type[] constructorArgumentTypes, params object[] constructorParameterValues)
        {
			Logger.LogDebug("<<< Exiting Instantiate(typeName={TypeName},constructorArgumentTypes={ConstructorArgumentTypes},constructorParameterValues={ConstructorParameterValues})", typeName, constructorArgumentTypes, constructorParameterValues);
            return Instantiate(Type.GetType(typeName, true), constructorArgumentTypes, constructorParameterValues);
        }

        public static object Instantiate(Type type, Type[] constructorArgumentTypes, params object[] constructorParameterValues)
        {
            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, constructorArgumentTypes, null);
            return constructor.Invoke(constructorParameterValues);
        }

        /// <summary>Invokes a non-public static method.</summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static TReturn InvokeNonPublicMethod<TReturn>(Type type, string methodName, params object[] parameters)
        {
			Logger.LogDebug(">>> Entering InvokeNonPublicMethod(type={Type},methodName={MethodName},parameters={Parameters})", type, methodName, parameters);
            var paramTypes = Array.ConvertAll(parameters, o => o.GetType());
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null);
			Logger.LogDebug("InvokeNonPublicMethod(type={Type},methodName={MethodName},parameters={Parameters}): paramTypes → {ParamTypes}", type, methodName, parameters, paramTypes);
			Logger.LogDebug("InvokeNonPublicMethod(type={Type},methodName={MethodName},parameters={Parameters}): method → {Method}", type, methodName, parameters, method);

            if (method == null)
            {
                throw new ArgumentException(string.Format("Could not find a method with the name '{0}'", methodName), "methodName");
            }

            return (TReturn)method.Invoke(null, parameters);
        }

        public static TReturn InvokeNonPublicMethod<TReturn>(object source, string methodName, params object[] parameters)
        {
            var paramTypes = Array.ConvertAll(parameters, o => o.GetType());
			Logger.LogDebug("<<< Exiting InvokeNonPublicMethod(type={Type},methodName={MethodName},parameters={Parameters})", type, methodName, parameters);
			Logger.LogDebug("InvokeNonPublicMethod(source={Source},methodName={MethodName},parameters={Parameters}): paramTypes → {ParamTypes}", source, methodName, parameters, paramTypes);
            var method = source.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null);

			Logger.LogDebug("InvokeNonPublicMethod(source={Source},methodName={MethodName},parameters={Parameters}): method → {Method}", source, methodName, parameters, method);
            if (method == null)
            {
                throw new ArgumentException(string.Format("Could not find a method with the name '{0}'", methodName), "methodName");
            }

            return (TReturn)method.Invoke(source, parameters);
        }

        public static TReturn InvokeProperty<TReturn>(object source, string propertyName)
        {
			Logger.LogDebug(">>> Entering InvokeProperty(source={Source},propertyName={PropertyName})", source, propertyName);
            var propertyInfo = source.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
			Logger.LogDebug("InvokeProperty(source={Source},propertyName={PropertyName}): propertyInfo → {PropertyInfo}", source, propertyName, propertyInfo);
            {
                throw new ArgumentException(string.Format("Could not find a propertyName with the name '{0}'", propertyName), "propertyName");
            }

            return (TReturn)propertyInfo.GetValue(source, null);
        }

        public static TReturn InvokeNonPublicProperty<TReturn>(object source, string propertyName)
        {
			Logger.LogDebug(">>> Entering InvokeNonPublicProperty(source={Source},propertyName={PropertyName})", source, propertyName);
            var propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance, null, typeof(TReturn), new Type[0], null);
            if (propertyInfo == null)
			Logger.LogDebug("InvokeNonPublicProperty(source={Source},propertyName={PropertyName}): propertyInfo → {PropertyInfo}", source, propertyName, propertyInfo);
            {
                throw new ArgumentException(string.Format("Could not find a propertyName with the name '{0}'", propertyName), "propertyName");
            }

            return (TReturn)propertyInfo.GetValue(source, null);
        }

        public static object InvokeNonPublicProperty(object source, string propertyName)
        {
            var propertyInfo = source.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                throw new ArgumentException(string.Format("Could not find a propertyName with the name '{0}'", propertyName), "propertyName");
            }

			Logger.LogDebug("<<< Exiting InvokeNonPublicProperty(source={Source},propertyName={PropertyName})", source, propertyName);
            return propertyInfo.GetValue(source, null);
        }
    }
}
