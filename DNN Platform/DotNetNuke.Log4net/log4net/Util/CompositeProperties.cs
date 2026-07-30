// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// 
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;
using System.Collections;

namespace log4net.Util
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <summary>
    /// This class aggregates several PropertiesDictionary collections together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides a dictionary style lookup over an ordered list of
    /// <see cref="PropertiesDictionary"/> collections.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    public sealed class CompositeProperties
    {
        private PropertiesDictionary m_flattened = null;
        private ArrayList m_nestedProperties = new ArrayList();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="CompositeProperties" /> class.
        /// </para>
        /// </remarks>
        internal CompositeProperties()
        {
		Logger.LogDebug(">>> Entering CompositeProperties()");
		Logger.LogDebug("<<< Exiting CompositeProperties()");
        }

        /// <summary>
        /// Gets the value of a property
        /// </summary>
        /// <value>
        /// The value for the property with the specified key
        /// </value>
        /// <remarks>
        /// <para>
        /// Looks up the value for the <paramref name="key" /> specified.
        /// The <see cref="PropertiesDictionary"/> collections are searched
        /// in the order in which they were added to this collection. The value
        /// returned is the value held by the first collection that contains
        /// the specified key.
        /// </para>
        /// <para>
        /// If none of the collections contain the specified key then
        /// <c>null</c> is returned.
        /// </para>
        /// </remarks>
        public object this[string key]
        {
            get 
            {
                // Look in the flattened properties first
                if (this.m_flattened != null)
                {
                    return this.m_flattened[key];
                }

                // Look for the key in all the nested properties
                foreach(ReadOnlyPropertiesDictionary cur in this.m_nestedProperties)
                {
                    if (cur.Contains(key))
                    {
                        return cur[key];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Add a Properties Dictionary to this composite collection
        /// </summary>
        /// <param name="properties">the properties to add</param>
        /// <remarks>
        /// <para>
        /// Properties dictionaries added first take precedence over dictionaries added
        /// later.
        /// </para>
        /// </remarks>
        public void Add(ReadOnlyPropertiesDictionary properties)
        {
			Logger.LogDebug(">>> Entering Add(properties={Properties})", properties);
            this.m_flattened = null;
            this.m_nestedProperties.Add(properties);
			Logger.LogDebug("<<< Exiting Add(properties={Properties})", properties);
			Logger.LogDebug("Add(properties={Properties}): this.m_flattened → {Arg1}", properties, this.m_flattened);
			Logger.LogDebug("Add(properties={Properties}): m_flattened → {M_flattened}", properties, m_flattened);
        }

        /// <summary>
        /// Flatten this composite collection into a single properties dictionary
        /// </summary>
        /// <returns>the flattened dictionary</returns>
        /// <remarks>
        /// <para>
        /// Reduces the collection of ordered dictionaries to a single dictionary
        /// containing the resultant values for the keys.
        /// </para>
        /// </remarks>
        public PropertiesDictionary Flatten()
        {
			Logger.LogDebug(">>> Entering Flatten()");
            if (this.m_flattened == null)
            {
                this.m_flattened = new PropertiesDictionary();
				Logger.LogDebug("Flatten(): m_flattened → {M_flattened}", m_flattened);

                for(int i = this.m_nestedProperties.Count; --i >= 0; )
                {
                    ReadOnlyPropertiesDictionary cur = (ReadOnlyPropertiesDictionary)this.m_nestedProperties[i];

					Logger.LogDebug("Flatten(): cur → {Cur}", cur);
                    foreach(DictionaryEntry entry in cur)
                    {
                        this.m_flattened[(string)entry.Key] = entry.Value;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting Flatten()");
            return this.m_flattened;
        }
    }
}

