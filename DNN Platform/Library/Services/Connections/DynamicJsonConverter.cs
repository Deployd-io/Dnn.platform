// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Script.Serialization;

    public sealed class DynamicJsonConverter : JavaScriptConverter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DynamicJsonConverter));
        /// <inheritdoc />
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>([typeof(object)])); }
        }

        /// <inheritdoc />
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
			Logger.LogError(">>> Entering Deserialize(dictionary={Dictionary},type={Type},serializer={Serializer})", dictionary, type, serializer);
            if (dictionary == null)
            {
				Logger.LogError("Throwing ArgumentNullException from Deserialize(dictionary={Dictionary},type={Type},serializer={Serializer})", dictionary, type, serializer);
                throw new ArgumentNullException(nameof(dictionary));
            }

			Logger.LogWarning("<<< Exiting Deserialize(dictionary={Dictionary},type={Type},serializer={Serializer})", dictionary, type, serializer);
            return type == typeof(object) ? new DynamicJsonObject(dictionary) : null;
        }

        /// <inheritdoc />
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private sealed class DynamicJsonObject : DynamicObject
        {
            private readonly IDictionary<string, object> dictionary;

            public DynamicJsonObject(IDictionary<string, object> dictionary)
            {
				Logger.LogError(">>> Entering DynamicJsonObject(dictionary={Dictionary})", dictionary);
                this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
				Logger.LogDebug("DynamicJsonObject(dictionary={Dictionary}): dictionary → {Dictionary1}", dictionary, dictionary);
            Logger.LogWarning("<<< Exiting DynamicJsonObject(dictionary={Dictionary})", dictionary);
            }

            public override string ToString()
            {
				Logger.LogError(">>> Entering ToString()");
                var sb = new StringBuilder("{");
                this.ToString(sb);
				Logger.LogError("ToString(): sb → {Sb}", sb);
				Logger.LogWarning("<<< Exiting ToString()");
                return sb.ToString();
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
				Logger.LogError(">>> Entering TryGetMember(binder={Binder})", binder);
                if (!this.dictionary.TryGetValue(binder.Name, out result))
                {
                    // return null to avoid exception.  caller can check for null this way...
                    result = null;
					Logger.LogError("TryGetMember(binder={Binder}): result → {Result}", binder, result);
                    return true;
                }

                result = WrapResultObject(result);
				Logger.LogWarning("<<< Exiting TryGetMember(binder={Binder})", binder);
                return true;
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
				Logger.LogError(">>> Entering TryGetIndex(binder={Binder},indexes={Indexes})", binder, indexes);
                if (indexes.Length == 1 && indexes[0] != null)
                {
                    if (!this.dictionary.TryGetValue(indexes[0].ToString(), out result))
                    {
                        // return null to avoid exception.  caller can check for null this way...
                        result = null;
						Logger.LogError("TryGetIndex(binder={Binder},indexes={Indexes}): result → {Result}", binder, indexes, result);
                        return true;
                    }

                    result = WrapResultObject(result);
                    return true;
                }

				Logger.LogWarning("<<< Exiting TryGetIndex(binder={Binder},indexes={Indexes})", binder, indexes);
                return base.TryGetIndex(binder, indexes, out result);
            }

            private static object WrapResultObject(object result)
            {
				Logger.LogError(">>> Entering WrapResultObject(result={Result})", result);
                var dictionary = result as IDictionary<string, object>;
                if (dictionary != null)
                {
                    return new DynamicJsonObject(dictionary);
                }

                var arrayList = result as ArrayList;
                if (arrayList != null && arrayList.Count > 0)
                {
                    return arrayList[0] is IDictionary<string, object>
                        ? new List<object>(arrayList.Cast<IDictionary<string, object>>().Select(x => new DynamicJsonObject(x)))
                        : new List<object>(arrayList.Cast<object>());
                }

				Logger.LogWarning("<<< Exiting WrapResultObject(result={Result})", result);
                return result;
            }

            private void ToString(StringBuilder sb)
            {
				Logger.LogError(">>> Entering ToString(sb={Sb})", sb);
                var firstInDictionary = true;
				Logger.LogError("ToString(sb={Sb}): firstInDictionary → {FirstInDictionary}", sb, firstInDictionary);
                foreach (var pair in this.dictionary)
                {
                    if (!firstInDictionary)
                    {
                        sb.Append(',');
                    }

                    firstInDictionary = false;
                    var value = pair.Value;
                    var name = pair.Key;
					Logger.LogError("ToString(sb={Sb}): name → {Name}", sb, name);
                    if (value is string stringValue)
                    {
                        sb.AppendFormat(
                            CultureInfo.InvariantCulture,
                            "{0}:{1}",
                            HttpUtility.JavaScriptStringEncode(name, addDoubleQuotes: true),
                            HttpUtility.JavaScriptStringEncode(stringValue, addDoubleQuotes: true));
                    }
                    else if (value is IDictionary<string, object> dictValue)
                    {
                        new DynamicJsonObject(dictValue).ToString(sb);
                    }
                    else if (value is ArrayList list)
                    {
                        sb.Append(name + ":[");
                        var firstInArray = true;
						Logger.LogError("ToString(sb={Sb}): firstInArray → {FirstInArray}", sb, firstInArray);
                        foreach (var arrayValue in list)
                        {
                            if (!firstInArray)
                            {
                                sb.Append(',');
                            }

                            firstInArray = false;

                            if (arrayValue is IDictionary<string, object> dictArrayValue)
                            {
                                sb.Append(new DynamicJsonObject(dictArrayValue));
                            }
                            else if (arrayValue is string stringArrayValue)
                            {
                                sb.Append(HttpUtility.JavaScriptStringEncode(stringArrayValue, addDoubleQuotes: true));
                            }
                            else
                            {
                                sb.Append(arrayValue);
                            }
                        }

                        sb.Append(']');
                    }
                    else
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}:{1}", name, value);
                    }
                }

                sb.Append('}');
            }
        }
    }
}
