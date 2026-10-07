// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Framework;

    public class ControlUtilities
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ControlUtilities));
        public static T FindParentControl<T>(Control control)
            where T : Control
        {
			Logger.LogError(">>> Entering FindParentControl(control={Control})", control);
            T parent = default(T);
			Logger.LogError("FindParentControl(control={Control}): parent → {Parent}", control, parent);
            if (control.Parent == null)
            {
                parent = null;
            }
            else
            {
                var parentT = control.Parent as T;
                if (parentT != null)
                {
                    parent = parentT;
                }
                else
                {
                    parent = FindParentControl<T>(control.Parent);
                }
            }

			Logger.LogWarning("<<< Exiting FindParentControl(control={Control})", control);
            return parent;
        }

        public static T FindControl<T>(Control control, string id, bool recursive)
            where T : Control
        {
			Logger.LogError(">>> Entering FindControl(control={Control},id={Id},recursive={Recursive})", control, id, recursive);
            T target = null;
			Logger.LogError("FindControl(control={Control},id={Id},recursive={Recursive}): target → {Target}", control, id, recursive, target);
            if (control.Parent != null)
            {
                target = control.Parent.FindControl(id) as T;

                if (target == null && recursive)
                {
                    target = FindControl<T>(control.Parent, id, true);
                }
            }

			Logger.LogWarning("<<< Exiting FindControl(control={Control},id={Id},recursive={Recursive})", control, id, recursive);
            return target;
        }

        public static T FindFirstDescendent<T>(Control control)
            where T : Control
        {
			Logger.LogError(">>> Entering FindFirstDescendent(control={Control})", control);
			Logger.LogWarning("<<< Exiting FindFirstDescendent(control={Control})", control);
			Logger.LogWarning("<<< Exiting FindFirstDescendent(control={Control},predicate={Predicate})", control, predicate);
            return FindFirstDescendent<T>(control, idx => idx is T);
        }

        public static T FindFirstDescendent<T>(Control control, Predicate<Control> predicate)
            where T : Control
        {
            if (predicate(control))
            {
                return control as T;
            }

            foreach (Control childControl in control.Controls)
            {
                T descendent = FindFirstDescendent<T>(childControl, predicate);
				Logger.LogError("FindFirstDescendent(control={Control},predicate={Predicate}): descendent → {Descendent}", control, predicate, descendent);
                if (descendent != null)
                {
                    return descendent;
                }
            }

            return null;
        }

        /// <summary>LoadControl loads a control and returns a reference to the control.</summary>
        /// <typeparam name="T">The type of control to Load.</typeparam>
        /// <param name="containerControl">The parent Container Control.</param>
        /// <param name="controlSrc">The source for the control.  This can either be a User Control (.ascx) or a compiled
        /// control.</param>
        /// <returns>A Control of type T.</returns>
        public static T LoadControl<T>(TemplateControl containerControl, string controlSrc)
            where T : Control
        {
			Logger.LogError(">>> Entering LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc})", containerControl, controlSrc);
            T ctrl;

            // load the control dynamically
            if (controlSrc.EndsWith(".ascx", StringComparison.InvariantCultureIgnoreCase))
            {
                // load from a user control on the file system
                ctrl = (T)containerControl.LoadControl("~/" + controlSrc);
				Logger.LogError("LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc}): ctrl → {Ctrl}", containerControl, controlSrc, ctrl);
            }
            else
            {
				Logger.LogDebug("LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc}): ctrl → {Ctrl}", containerControl, controlSrc, ctrl);
                // load from a typename in an assembly ( ie. server control )
                Type objType = Reflection.CreateType(controlSrc);
				Logger.LogError("LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc}): objType → {ObjType}", containerControl, controlSrc, objType);
                ctrl = (T)containerControl.LoadControl(objType, null);
            }

			Logger.LogWarning("<<< Exiting LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc})", containerControl, controlSrc);
            return ctrl;
        }
    }
}
