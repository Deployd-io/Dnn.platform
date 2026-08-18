// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common
{
    using System.Runtime.Serialization;

    using DotNetNuke.Common.Utilities;

    [DataContract]
    public class SerializableKeyValuePair<TKey, TValue>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SerializableKeyValuePair));
        /// <summary>Initializes a new instance of the <see cref="SerializableKeyValuePair{TKey, TValue}"/> class.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public SerializableKeyValuePair(TKey key, TValue value)
        {
			Logger.LogDebug(">>> Entering SerializableKeyValuePair(key={Key},value={Value})", key, value);
            this.Key = key;
            this.Value = value;
			Logger.LogDebug("SerializableKeyValuePair(key={Key},value={Value}): this.Key → {Arg2}", key, value, this.Key);
			Logger.LogDebug("<<< Exiting SerializableKeyValuePair(key={Key},value={Value})", key, value);
        }

        [DataMember(Name = "key")]
        public TKey Key { get; set; }

        [DataMember(Name = "value")]
        public TValue Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
			Logger.LogDebug(">>> Entering ToString()");
			Logger.LogDebug("<<< Exiting ToString()");
            return Json.Serialize(this);
        }
    }
}
