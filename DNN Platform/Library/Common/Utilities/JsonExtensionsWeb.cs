// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Web.Script.Serialization;

    /// <summary>JSON Extensions based on the JavaScript Serializer in System.web.</summary>
    public static class JsonExtensionsWeb
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JsonExtensionsWeb));
        /// <summary>Serializes a type to JSON. Note the type must be marked Serializable or include a DataContract attribute.</summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The JSON string.</returns>
        public static string ToJsonString(object value)
        {
			Logger.LogDebug(">>> Entering ToJsonString(value={Value})", value);
            var ser = SerializerFactory();
            string json = ser.Serialize(value);
			Logger.LogDebug("ToJsonString(value={Value}): ser → {Ser}", value, ser);
			Logger.LogDebug("<<< Exiting ToJsonString(value={Value})", value);
            return json;
        }

        /// <summary>Extension method on object that serializes the value to JSON. Note the type must be marked Serializable or include a DataContract attribute.</summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The JSON string.</returns>
        public static string ToJson(this object value)
        {
			Logger.LogDebug(">>> Entering ToJson(value={Value})", value);
			Logger.LogDebug("<<< Exiting ToJson(value={Value})", value);
            return ToJsonString(value);
        }

        /// <summary>Deserializes a JSON string into a specific type. Note that the type specified must be serializable.</summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="type">The type to deserialize the JSON into.</param>
        /// <returns>The deserialized value.</returns>
        public static object FromJsonString(string json, Type type)
        {
			Logger.LogDebug(">>> Entering FromJsonString(json={Json},type={Type})", json, type);
            // *** Have to use Reflection with a 'dynamic' non constant type instance
            var ser = SerializerFactory();
			Logger.LogDebug("FromJsonString(json={Json},type={Type}): ser → {Ser}", json, type, ser);

            object result = ser.GetType().GetMethod("Deserialize").MakeGenericMethod(type).Invoke(ser, new object[1] { json });
			Logger.LogDebug("FromJsonString(json={Json},type={Type}): result → {Result}", json, type, result);
			Logger.LogDebug("<<< Exiting FromJsonString(json={Json},type={Type})", json, type);
            return result;
        }

        /// <summary>Extension method to string that deserializes a JSON string into a specific type. Note that the type specified must be serializable.</summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="type">The type to deserialize the JSON into.</param>
        /// <returns>The deserialized value.</returns>
        public static object FromJson(this string json, Type type)
        {
			Logger.LogDebug(">>> Entering FromJson(json={Json},type={Type})", json, type);
			Logger.LogDebug("<<< Exiting FromJson(json={Json})", json);
            return FromJsonString(json, type);
        }

        /// <summary>Extension method to string that deserializes a JSON string into a specific type. Note that the type specified must be serializable.</summary>
        /// <typeparam name="TType">The type to deserialize the JSON into.</typeparam>
        /// <param name="json">A JSON string.</param>
        /// <returns>The deserialized value.</returns>
        public static TType FromJson<TType>(this string json)
        {
            var ser = SerializerFactory();

            var result = ser.Deserialize<TType>(json);
            return result;
        }

        private static JavaScriptSerializer SerializerFactory()
        {
			Logger.LogDebug(">>> Entering SerializerFactory()");
            // Allow large JSON strings to be serialized and deserialized.
			Logger.LogDebug("<<< Exiting SerializerFactory()");
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }
    }
}
