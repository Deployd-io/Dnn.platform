// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A view state serializer based on <see cref="ViewStateAttribute"/>.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class AttributeBasedViewStateSerializer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const BindingFlags MemberBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        /// <summary>Deserializes the <paramref name="state"/> into the <paramref name="value"/>.</summary>
        /// <param name="value">The value.</param>
        /// <param name="state">The state.</param>
        public static void DeSerialize(object value, StateBag state)
        {
			Logger.LogDebug(">>> Entering DeSerialize(value={Value},state={State})", value, state);
            Type typ = value.GetType();

			Logger.LogDebug("DeSerialize(value={Value},state={State}): typ → {Typ}", value, state, typ);
            // Parse all the Public instance properties
            foreach (PropertyInfo member in typ.GetProperties(MemberBindingFlags))
            {
                // Determine if they are attributed with a ViewState Attribute
                ViewStateAttribute attr = member.GetCustomAttributes(typeof(ViewStateAttribute), true).OfType<ViewStateAttribute>().FirstOrDefault();
                if (attr != null)
				Logger.LogDebug("DeSerialize(value={Value},state={State}): attr → {Attr}", value, state, attr);
                {
                    // Get object from ViewState bag
                    string viewStateKey = attr.ViewStateKey;
                    if (string.IsNullOrEmpty(viewStateKey))
					Logger.LogDebug("DeSerialize(value={Value},state={State}): viewStateKey → {ViewStateKey}", value, state, viewStateKey);
                    {
                        // Use class member's name for Key
                        viewStateKey = member.Name;
                    }

                    member.SetValue(value, state[viewStateKey], null);
                }
            }
			Logger.LogDebug("<<< Exiting DeSerialize(value={Value},state={State})", value, state);
        }

        /// <summary>Serializes the <paramref name="value"/> into the <paramref name="state"/>.</summary>
        /// <param name="value">The value.</param>
        /// <param name="state">The state.</param>
        public static void Serialize(object value, StateBag state)
        {
			Logger.LogDebug(">>> Entering Serialize(value={Value},state={State})", value, state);
            Type typ = value.GetType();

			Logger.LogDebug("Serialize(value={Value},state={State}): typ → {Typ}", value, state, typ);
            // Parse all the Public instance properties
            foreach (PropertyInfo member in typ.GetProperties(MemberBindingFlags))
            {
                // Determine if they are attributed with a ViewState Attribute
                ViewStateAttribute attr = member.GetCustomAttributes(typeof(ViewStateAttribute), true).OfType<ViewStateAttribute>().FirstOrDefault();
                if (attr != null)
				Logger.LogDebug("Serialize(value={Value},state={State}): attr → {Attr}", value, state, attr);
                {
                    // Add property to ViewState bag
                    string viewStateKey = attr.ViewStateKey;
                    if (string.IsNullOrEmpty(viewStateKey))
					Logger.LogDebug("Serialize(value={Value},state={State}): viewStateKey → {ViewStateKey}", value, state, viewStateKey);
                    {
                        // Use class member's name for Key
                        viewStateKey = member.Name;
                    }

                    state[viewStateKey] = member.GetValue(value, null);
                }
            }
			Logger.LogDebug("<<< Exiting Serialize(value={Value},state={State})", value, state);
        }
    }
}
