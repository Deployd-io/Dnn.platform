// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class ModuleActionItemsAttribute : ActionFilterAttribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleActionItemsAttribute));
        private const string MethodNameTemplate = "Get{0}Actions";

        public Type Type { get; set; }

        public string MethodName { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
			Logger.LogError(">>> Entering OnActionExecuting(filterContext={FilterContext})", filterContext);
            var controller = filterContext.Controller as IDnnController;
            Type type;
            string methodName;

            if (controller == null)
            {
				Logger.LogError("Throwing InvalidOperationException from OnActionExecuting(filterContext={FilterContext})", filterContext);
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            object instance;

            if (this.Type == null)
            {
                type = filterContext.Controller.GetType();
				Logger.LogError("OnActionExecuting(filterContext={FilterContext}): type → {Type}", filterContext, type);
                instance = controller;
				Logger.LogDebug("OnActionExecuting(filterContext={FilterContext}): instance → {Instance}", filterContext, instance);
            }
            else
            {
				Logger.LogError("OnActionExecuting(filterContext={FilterContext}): instance → {Instance}", filterContext, instance);
                type = this.Type;
				Logger.LogDebug("OnActionExecuting(filterContext={FilterContext}): type → {Type}", filterContext, type);
                instance = Reflection.CreateInstance(type);
            }

            if (string.IsNullOrEmpty(this.MethodName))
            {
                methodName = string.Format(CultureInfo.InvariantCulture, MethodNameTemplate, filterContext.ActionDescriptor.ActionName);
				Logger.LogError("OnActionExecuting(filterContext={FilterContext}): methodName → {MethodName}", filterContext, methodName);
            }
            else
            {
                methodName = this.MethodName;
				Logger.LogDebug("OnActionExecuting(filterContext={FilterContext}): methodName → {MethodName}", filterContext, methodName);
            }

            var method = GetMethod(type, methodName);
			Logger.LogError("OnActionExecuting(filterContext={FilterContext}): method → {Method}", filterContext, method);

            controller.ModuleActions = method.Invoke(instance, null) as ModuleActionCollection;
			Logger.LogWarning("<<< Exiting OnActionExecuting(filterContext={FilterContext})", filterContext);
        }

        private static MethodInfo GetMethod(Type type, string methodName)
        {
			Logger.LogError(">>> Entering GetMethod(type={Type},methodName={MethodName})", type, methodName);
            var method = type.GetMethod(methodName);

			Logger.LogError("GetMethod(type={Type},methodName={MethodName}): method → {Method}", type, methodName, method);
            if (method == null)
            {
				Logger.LogError("Throwing NotImplementedException from GetMethod(type={Type},methodName={MethodName})", type, methodName);
                throw new NotImplementedException($"The expected method to get the module actions cannot be found. Type: {type.FullName}, Method: {methodName}");
            }

            var returnType = method.ReturnType.FullName;
            if (returnType != "DotNetNuke.Entities.Modules.Actions.ModuleActionCollection")
            {
				Logger.LogError("Throwing InvalidOperationException from GetMethod(type={Type},methodName={MethodName})", type, methodName);
                throw new InvalidOperationException("The method must return an object of type ModuleActionCollection");
            }

			Logger.LogWarning("<<< Exiting GetMethod(type={Type},methodName={MethodName})", type, methodName);
            return method;
        }
    }
}
