// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.IO;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;

    [ExpressionPrefix("dnnLoc")]
    [ExpressionEditor("DotNetNuke.Services.Localization.LocalizationExpressionBuilderEditor")]
    public class LocalizationExpressionBuilder : ExpressionBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalizationExpressionBuilder));
        /// <inheritdoc />
        public override bool SupportsEvaluate
        {
            get
            {
                return true;
            }
        }

        public static object GetLocalizedResource(string key, Type targetType, string propertyName, string virtualPath)
        {
			Logger.LogError(">>> Entering GetLocalizedResource(key={Key},targetType={TargetType},propertyName={PropertyName},virtualPath={VirtualPath})", key, targetType, propertyName, virtualPath);
            if (HttpContext.Current == null)
            {
                return null;
            }

            string localResourceFile = string.Empty;
			Logger.LogError("GetLocalizedResource(key={Key},targetType={TargetType},propertyName={PropertyName},virtualPath={VirtualPath}): localResourceFile → {LocalResourceFile}", key, targetType, propertyName, virtualPath, localResourceFile);
            if (!string.IsNullOrEmpty(virtualPath))
            {
                string filename = Path.GetFileName(virtualPath);
                if (filename != null)
                {
                    localResourceFile = virtualPath.Replace(filename, Localization.LocalResourceDirectory + "/" + filename);
                }
            }

            string value = Localization.GetString(key, localResourceFile);

            if (value == null)
            {
				Logger.LogError("Throwing InvalidOperationException from GetLocalizedResource(key={Key},targetType={TargetType},propertyName={PropertyName},virtualPath={VirtualPath})", key, targetType, propertyName, virtualPath);
                throw new InvalidOperationException($"Localized Value '{key}' not found.");
            }

            // check if value can be converted to property type
            if (targetType != null)
            {
                PropertyDescriptor propDesc = TypeDescriptor.GetProperties(targetType)[propertyName];
				Logger.LogError("GetLocalizedResource(key={Key},targetType={TargetType},propertyName={PropertyName},virtualPath={VirtualPath}): propDesc → {PropDesc}", key, targetType, propertyName, virtualPath, propDesc);
                if (propDesc != null && propDesc.PropertyType != value.GetType())
                {
                    // Type mismatch - make sure that the value can be converted to the Web control property type
                    if (propDesc.Converter != null)
                    {
                        if (!propDesc.Converter.CanConvertFrom(value.GetType()))
                        {
                            throw new InvalidOperationException(
                                $"Localized value '{key}' cannot be converted to type {propDesc.PropertyType}.");
                        }

                        return propDesc.Converter.ConvertFrom(value);
                    }
                }
            }

            // If we reach here, no type mismatch - return the value
			Logger.LogWarning("<<< Exiting GetLocalizedResource(key={Key},targetType={TargetType},propertyName={PropertyName},virtualPath={VirtualPath})", key, targetType, propertyName, virtualPath);
            return value;
        }

        /// <inheritdoc />
        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
			Logger.LogError(">>> Entering GetCodeExpression(entry={Entry},parsedData={ParsedData},context={Context})", entry, parsedData, context);
            var inputParams = new CodeExpression[]
                                  {
                                      new CodePrimitiveExpression(entry.Expression.Trim()),
                                      new CodeTypeOfExpression(entry.DeclaringType),
                                      new CodePrimitiveExpression(entry.PropertyInfo.Name),
                                      new CodePrimitiveExpression(context.VirtualPath),
                                  };

			Logger.LogWarning("<<< Exiting GetCodeExpression(entry={Entry},parsedData={ParsedData},context={Context})", entry, parsedData, context);
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetType()), "GetLocalizedResource", inputParams);
        }

        /// <inheritdoc />
        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
			Logger.LogError(">>> Entering EvaluateExpression(target={Target},entry={Entry},parsedData={ParsedData},context={Context})", target, entry, parsedData, context);
            string key = entry.Expression.Trim();
            Type targetType = entry.DeclaringType;
            string propertyName = entry.PropertyInfo.Name;
            string virtualPath = context.VirtualPath;

			Logger.LogWarning("<<< Exiting EvaluateExpression(target={Target},entry={Entry},parsedData={ParsedData},context={Context})", target, entry, parsedData, context);
            return GetLocalizedResource(key, targetType, propertyName, virtualPath);
        }
    }
}
