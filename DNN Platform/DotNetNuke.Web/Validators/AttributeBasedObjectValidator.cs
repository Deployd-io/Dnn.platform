// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Validators
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>A validator which applies attributes to a property.</summary>
    /// <typeparam name="TAttribute">The type of attribute to validate.</typeparam>
    public abstract class AttributeBasedObjectValidator<TAttribute> : PropertyBasedObjectValidator
        where TAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        protected override ValidationResult ValidateProperty(object target, PropertyInfo targetProperty)
        {
			Logger.LogDebug(">>> Entering ValidateProperty(target={Target},targetProperty={TargetProperty})", target, targetProperty);
			Logger.LogDebug("<<< Exiting ValidateProperty(target={Target},targetProperty={TargetProperty})", target, targetProperty);
            return targetProperty.GetCustomAttributes(true)
                .OfType<TAttribute>()
                .Aggregate(
                    ValidationResult.Successful,
                    (result, attribute) =>
                        result.CombineWith(
                            this.ValidateAttribute(target, targetProperty, attribute) ?? ValidationResult.Successful));
        }

        /// <summary>Validates an attribute.</summary>
        /// <param name="target">The target object.</param>
        /// <param name="targetProperty">The property to validate.</param>
        /// <param name="attribute">The attribute on the property.</param>
        /// <returns>A new <see cref="ValidationResult"/>.</returns>
        protected abstract ValidationResult ValidateAttribute(object target, PropertyInfo targetProperty, TAttribute attribute);
    }
}
