// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>  Helper class to enable the data binding logic generate column names at runtime.</summary>
    internal sealed class RssElementCustomTypeDescriptor : ICustomTypeDescriptor
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Dictionary<string, string> attributes;

        /// <summary>Initializes a new instance of the <see cref="RssElementCustomTypeDescriptor"/> class.</summary>
        /// <param name="attributes">The attributes.</param>
        public RssElementCustomTypeDescriptor(Dictionary<string, string> attributes)
        {
            this.attributes = attributes;
        }

        /// <inheritdoc />
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        /// <inheritdoc />
        string ICustomTypeDescriptor.GetClassName()
        {
            return this.GetType().Name;
        }

        /// <inheritdoc />
        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        /// <inheritdoc />
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        /// <inheritdoc />
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        /// <inheritdoc />
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        /// <inheritdoc />
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        /// <inheritdoc />
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return null;
        }

        /// <inheritdoc />
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return null;
        }

        /// <inheritdoc />
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.GetPropertyDescriptors();
        }

        /// <inheritdoc />
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.GetPropertyDescriptors();
        }

        /// <inheritdoc />
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return (pd is RssElementCustomPropertyDescriptor) ? this : null;
        }

        private PropertyDescriptorCollection GetPropertyDescriptors()
        {
			Logger.LogDebug(">>> Entering GetPropertyDescriptors()");
            var propertyDescriptors = new PropertyDescriptor[this.attributes.Count];
            int i = 0;

            foreach (KeyValuePair<string, string> a in this.attributes)
            {
                propertyDescriptors[i++] = new RssElementCustomPropertyDescriptor(a.Key);
            }

			Logger.LogDebug("<<< Exiting GetPropertyDescriptors()");
            return new PropertyDescriptorCollection(propertyDescriptors);
        }

        private sealed class RssElementCustomPropertyDescriptor : PropertyDescriptor
        {
            public RssElementCustomPropertyDescriptor(string propertyName)
                : base(propertyName, null)
            {
			Logger.LogDebug(">>> Entering RssElementCustomPropertyDescriptor(propertyName={PropertyName})", propertyName);
			Logger.LogDebug("<<< Exiting RssElementCustomPropertyDescriptor(propertyName={PropertyName})", propertyName);
            }

            public override Type ComponentType
            {
                get
                {
                    return typeof(RssElementCustomTypeDescriptor);
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return typeof(string);
                }
            }

            public override bool CanResetValue(object o)
            {
				Logger.LogDebug(">>> Entering CanResetValue(o={O})", o);
				Logger.LogDebug(">>> Entering ResetValue(o={O})", o);
				Logger.LogDebug("<<< Exiting ResetValue(o={O})", o);
                return false;
            }

            public override void ResetValue(object o)
            {
            }

            public override void SetValue(object o, object value)
            {
			Logger.LogDebug(">>> Entering SetValue(o={O},value={Value})", o, value);
			Logger.LogDebug("<<< Exiting SetValue(o={O},value={Value})", o, value);
            }

            public override bool ShouldSerializeValue(object o)
            {
				Logger.LogDebug(">>> Entering ShouldSerializeValue(o={O})", o);
				Logger.LogDebug("<<< Exiting ShouldSerializeValue(o={O})", o);
                return true;
            }

            public override object GetValue(object o)
            {
				Logger.LogDebug(">>> Entering GetValue(o={O})", o);
                if (o is RssElementCustomTypeDescriptor element)
                {
                    if (element.attributes.TryGetValue(this.Name, out var propertyValue))
                    {
                        return propertyValue;
                    }
                }

				Logger.LogDebug("<<< Exiting GetValue(o={O})", o);
                return string.Empty;
            }
        }
    }
}
