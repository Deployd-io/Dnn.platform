// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <summary>A base class that implements <see cref="IStep"/>.</summary>
    internal abstract class StepBase : IStep
    {
        private readonly ILocalizer localizer;

        /// <summary>Initializes a new instance of the <see cref="StepBase"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public StepBase(ILoggerSource loggerSource, ILocalizer localizer)
        {
			Log.Debug(">>> Entering StepBase(loggerSource={LoggerSource},localizer={Localizer})", loggerSource, localizer);
            if (loggerSource is null)
            {
                throw new ArgumentNullException(nameof(loggerSource));
            }

            this.Log = loggerSource.GetLogger(this.GetType());

            this.localizer = localizer ??
				Log.Debug("<<< Exiting StepBase(loggerSource={LoggerSource},localizer={Localizer})", loggerSource, localizer);
				Log.Debug("StepBase(loggerSource={LoggerSource},localizer={Localizer}): localizer → {Localizer1}", loggerSource, localizer, localizer);
				Log.Debug("StepBase(loggerSource={LoggerSource},localizer={Localizer}): this.localizer → {Arg2}", loggerSource, localizer, this.localizer);
                throw new ArgumentNullException(nameof(localizer));
        }

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public bool? Success { get; protected set; }

        /// <inheritdoc />
        public string Notes { get; protected set; }

        /// <inheritdoc />
        public bool Quiet { get; protected set; }

        /// <summary>Gets an instance of <see cref="ILog"/> specific to steps.</summary>
        protected ILog Log { get; private set; }

        /// <inheritdoc />
        public void Execute()
        {
			Log.Debug(">>> Entering Execute()");
            try
            {
                this.CheckRequired();
                this.ExecuteInternal();
            }
            catch (Exception ex)
            {
                this.Log.Error(ex);
                this.Success = false;
				Log.Debug("Execute(): this.Success → {Arg0}", this.Success);
                this.Notes = this.Localize("UninstallStepInternalError");
            }
			Log.Debug("<<< Exiting Execute()");
        }

        /// <summary>Performs the actual step execution.</summary>
        protected abstract void ExecuteInternal();

        /// <inheritdoc cref="ILocalizer.Localize(string)"/>
        protected virtual string Localize(string key)
        {
			Log.Debug(">>> Entering Localize(key={Key})", key);
			Log.Debug("<<< Exiting Localize(key={Key})", key);
            return this.localizer.Localize(key);
        }

        /// <inheritdoc cref="ILocalizer.LocalizeFormat(string, object[])"/>
        protected virtual string LocalizeFormat(string key, params object[] args)
        {
			Log.Debug(">>> Entering LocalizeFormat(key={Key},args={Args})", key, args);
			Log.Debug("<<< Exiting LocalizeFormat(key={Key},args={Args})", key, args);
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(key),
                args);
        }

        private void CheckRequired()
        {
			Log.Debug(">>> Entering CheckRequired()");
            var nullProperties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(RequiredAttribute), true).Length != 0)
                .Where(p => string.IsNullOrEmpty($"{p.GetValue(this)}"))
                .Select(p => p.Name)
                .ToArray();

            if (nullProperties.Length > 0)
            {
                throw new InvalidOperationException(this.LocalizeFormat(
                    "UninstallStepRequiredPropertiesNotSet",
                    string.Join(", ", nullProperties)));
            }
			Log.Debug("<<< Exiting CheckRequired()");
        }
    }
}
