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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static T FindParentControl<T>(Control control)
            where T : Control
        {
			Logger.LogDebug(">>> Entering FindParentControl(control={Control})", control);
            T parent = default(T);
            if (control.Parent == null)
			Logger.LogDebug("FindParentControl(control={Control}): parent → {Parent}", control, parent);
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

            return parent;
        }

        public static T FindControl<T>(Control control, string id, bool recursive)
            where T : Control
        {
			Logger.LogDebug(">>> Entering FindControl(control={Control},id={Id},recursive={Recursive})", control, id, recursive);
            T target = null;
            if (control.Parent != null)
			Logger.LogDebug("FindControl(control={Control},id={Id},recursive={Recursive}): target → {Target}", control, id, recursive, target);
            {
                target = control.Parent.FindControl(id) as T;

                if (target == null && recursive)
                {
                    target = FindControl<T>(control.Parent, id, true);
                }
            }

            return target;
        }

        public static T FindFirstDescendent<T>(Control control)
            where T : Control
        {
			Logger.LogDebug(">>> Entering FindFirstDescendent(control={Control})", control);
            return FindFirstDescendent<T>(control, idx => idx is T);
        }

        public static T FindFirstDescendent<T>(Control control, Predicate<Control> predicate)
            where T : Control
        {
            if (predicate(control))
			Logger.LogDebug("<<< Exiting FindFirstDescendent(control={Control})", control);
            {
                return control as T;
            }

            foreach (Control childControl in control.Controls)
            {
                T descendent = FindFirstDescendent<T>(childControl, predicate);
                if (descendent != null)
				Logger.LogDebug("FindFirstDescendent(control={Control},predicate={Predicate}): descendent → {Descendent}", control, predicate, descendent);
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
			Logger.LogDebug(">>> Entering LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc})", containerControl, controlSrc);
            T ctrl;

            // load the control dynamically
            if (controlSrc.EndsWith(".ascx", StringComparison.InvariantCultureIgnoreCase))
            {
                // load from a user control on the file system
                ctrl = (T)containerControl.LoadControl("~/" + controlSrc);
				Logger.LogDebug("LoadControl(containerControl={ContainerControl},controlSrc={ControlSrc}): ctrl → {Ctrl}", containerControl, controlSrc, ctrl);
            }
            else
            {
                // load from a typename in an assembly ( ie. server control )
                Type objType = Reflection.CreateType(controlSrc);
                ctrl = (T)containerControl.LoadControl(objType, null);
            }

            return ctrl;
        }
    }
}
