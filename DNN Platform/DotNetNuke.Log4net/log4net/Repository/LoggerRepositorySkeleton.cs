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
using log4net.ObjectRenderer;
using log4net.Core;
using log4net.Util;
using log4net.Plugin;
using System.Threading;

namespace log4net.Repository
{
    /// <summary>Base implementation of <see cref="ILoggerRepository"/></summary>
    /// <remarks>
    /// <para>
    /// Default abstract implementation of the <see cref="ILoggerRepository"/> interface.
    /// </para>
    /// <para>
    /// Skeleton implementation of the <see cref="ILoggerRepository"/> interface.
    /// All <see cref="ILoggerRepository"/> types can extend this type.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public abstract class LoggerRepositorySkeleton : ILoggerRepository, Appender.IFlushable
    {
        private string m_name;
        private RendererMap m_rendererMap;
        private PluginMap m_pluginMap;
        private LevelMap m_levelMap;
        private Level m_threshold;
        private bool m_configured;
        private ICollection m_configurationMessages;
        private event LoggerRepositoryShutdownEventHandler m_shutdownEvent;
        private event LoggerRepositoryConfigurationResetEventHandler m_configurationResetEvent;
        private event LoggerRepositoryConfigurationChangedEventHandler m_configurationChangedEvent;
        private PropertiesDictionary m_properties;

        /// <summary>Default Constructor</summary>
        /// <remarks>
        /// <para>
        /// Initializes the repository with default (empty) properties.
        /// </para>
        /// </remarks>
        protected LoggerRepositorySkeleton() : this(new PropertiesDictionary())
        {
        }

        /// <summary>Construct the repository using specific properties</summary>
        /// <param name="properties">the properties to set for this repository</param>
        /// <remarks>
        /// <para>
        /// Initializes the repository with specified properties.
        /// </para>
        /// </remarks>
        protected LoggerRepositorySkeleton(PropertiesDictionary properties)
        {
			log.ErrorFormat(">>> Entering LoggerRepositorySkeleton(properties={0})", properties);
            this.m_properties = properties;
			log.WarnFormat("<<< Exiting LoggerRepositorySkeleton(properties={0})", properties);
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): this.m_properties → {1}", properties, this.m_properties);
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): m_properties → {1}", properties, m_properties);
            this.m_rendererMap = new RendererMap();
            this.m_pluginMap = new PluginMap(this);
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): m_rendererMap → {1}", properties, m_rendererMap);
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): m_pluginMap → {1}", properties, m_pluginMap);
            this.m_levelMap = new LevelMap();
            this.m_configurationMessages = EmptyCollection.Instance;
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): m_levelMap → {1}", properties, m_levelMap);
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): m_configurationMessages → {1}", properties, m_configurationMessages);
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): this.m_configurationMessages → {1}", properties, this.m_configurationMessages);
            this.m_configured = false;

            this.AddBuiltinLevels();
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): this.m_configured → {1}", properties, this.m_configured);

            // Don't disable any levels by default.
            this.m_threshold = Level.All;
			log.DebugFormat("LoggerRepositorySkeleton(properties={0}): m_threshold → {1}", properties, m_threshold);
        }

        /// <summary>The name of the repository</summary>
        /// <value>
        /// The string name of the repository
        /// </value>
        /// <remarks>
        /// <para>
        /// The name of this repository. The name is
        /// used to store and lookup the repositories 
        /// stored by the <see cref="IRepositorySelector"/>.
        /// </para>
        /// </remarks>
        public virtual string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        /// <summary>The threshold for all events in this repository</summary>
        /// <value>
        /// The threshold for all events in this repository
        /// </value>
        /// <remarks>
        /// <para>
        /// The threshold for all events in this repository
        /// </para>
        /// </remarks>
        public virtual Level Threshold
        {
            get { return this.m_threshold; }
            set
            { 
                if (value != null)
                {
                    this.m_threshold = value; 
                }
                else
                {
                    // Must not set threshold to null
                    LogLog.Warn(declaringType, "LoggerRepositorySkeleton: Threshold cannot be set to null. Setting to ALL");
                    this.m_threshold = Level.All;
                }
            }
        }

        /// <summary>RendererMap accesses the object renderer map for this repository.</summary>
        /// <value>
        /// RendererMap accesses the object renderer map for this repository.
        /// </value>
        /// <remarks>
        /// <para>
        /// RendererMap accesses the object renderer map for this repository.
        /// </para>
        /// <para>
        /// The RendererMap holds a mapping between types and
        /// <see cref="IObjectRenderer"/> objects.
        /// </para>
        /// </remarks>
        public virtual RendererMap RendererMap
        {
            get { return this.m_rendererMap; }
        }

        /// <summary>The plugin map for this repository.</summary>
        /// <value>
        /// The plugin map for this repository.
        /// </value>
        /// <remarks>
        /// <para>
        /// The plugin map holds the <see cref="IPlugin"/> instances
        /// that have been attached to this repository.
        /// </para>
        /// </remarks>
        public virtual PluginMap PluginMap
        {
            get { return this.m_pluginMap; }
        }

        /// <summary>Get the level map for the Repository.</summary>
        /// <remarks>
        /// <para>
        /// Get the level map for the Repository.
        /// </para>
        /// <para>
        /// The level map defines the mappings between
        /// level names and <see cref="Level"/> objects in
        /// this repository.
        /// </para>
        /// </remarks>
        public virtual LevelMap LevelMap
        {
            get { return this.m_levelMap; }
        }

        /// <summary>Test if logger exists</summary>
        /// <param name="name">The name of the logger to lookup</param>
        /// <returns>The Logger object with the name specified</returns>
        /// <remarks>
        /// <para>
        /// Check if the named logger exists in the repository. If so return
        /// its reference, otherwise returns <c>null</c>.
        /// </para>
        /// </remarks>
        public abstract ILogger Exists(string name);

        /// <summary>Returns all the currently defined loggers in the repository</summary>
        /// <returns>All the defined loggers</returns>
        /// <remarks>
        /// <para>
        /// Returns all the currently defined loggers in the repository as an Array.
        /// </para>
        /// </remarks>
        public abstract ILogger[] GetCurrentLoggers();

        /// <summary>Return a new logger instance</summary>
        /// <param name="name">The name of the logger to retrieve</param>
        /// <returns>The logger object with the name specified</returns>
        /// <remarks>
        /// <para>
        /// Return a new logger instance.
        /// </para>
        /// <para>
        /// If a logger of that name already exists, then it will be
        /// returned. Otherwise, a new logger will be instantiated and
        /// then linked with its existing ancestors as well as children.
        /// </para>
        /// </remarks>
        public abstract ILogger GetLogger(string name);

        /// <summary>Shutdown the repository</summary>
        /// <remarks>
        /// <para>
        /// Shutdown the repository. Can be overridden in a subclass.
        /// This base class implementation notifies the <see cref="ShutdownEvent"/>
        /// listeners and all attached plugins of the shutdown event.
        /// </para>
        /// </remarks>
        public virtual void Shutdown() 
        {
			log.ErrorFormat(">>> Entering Shutdown()");
            // Shutdown attached plugins
            foreach(IPlugin plugin in this.PluginMap.AllPlugins)
            {
                plugin.Shutdown();
            }

            // Notify listeners
            this.OnShutdown(null);
			log.WarnFormat("<<< Exiting Shutdown()");
        }

        /// <summary>Reset the repositories configuration to a default state</summary>
        /// <remarks>
        /// <para>
        /// Reset all values contained in this instance to their
        /// default state.
        /// </para>
        /// <para>
        /// Existing loggers are not removed. They are just reset.
        /// </para>
        /// <para>
        /// This method should be used sparingly and with care as it will
        /// block all logging until it is completed.
        /// </para>
        /// </remarks>
        public virtual void ResetConfiguration() 
        {
			log.ErrorFormat(">>> Entering ResetConfiguration()");
            // Clear internal data structures
            this.m_rendererMap.Clear();
            this.m_levelMap.Clear();
            this.m_configurationMessages = EmptyCollection.Instance;
			log.DebugFormat("ResetConfiguration(): m_configurationMessages → {0}", m_configurationMessages);
			log.DebugFormat("ResetConfiguration(): this.m_configurationMessages → {0}", this.m_configurationMessages);

            // Add the predefined levels to the map
            this.AddBuiltinLevels();

            this.Configured = false;
			log.DebugFormat("ResetConfiguration(): this.Configured → {0}", this.Configured);

            // Notify listeners
            this.OnConfigurationReset(null);
			log.WarnFormat("<<< Exiting ResetConfiguration()");
        }

        /// <summary>Log the logEvent through this repository.</summary>
        /// <param name="logEvent">the event to log</param>
        /// <remarks>
        /// <para>
        /// This method should not normally be used to log.
        /// The <see cref="ILog"/> interface should be used 
        /// for routine logging. This interface can be obtained
        /// using the <see cref="M:log4net.LogManager.GetLogger(string)"/> method.
        /// </para>
        /// <para>
        /// The <c>logEvent</c> is delivered to the appropriate logger and
        /// that logger is then responsible for logging the event.
        /// </para>
        /// </remarks>
        public abstract void Log(LoggingEvent logEvent);

        /// <summary>Flag indicates if this repository has been configured.</summary>
        /// <value>
        /// Flag indicates if this repository has been configured.
        /// </value>
        /// <remarks>
        /// <para>
        /// Flag indicates if this repository has been configured.
        /// </para>
        /// </remarks>
        public virtual bool Configured 
        { 
            get { return this.m_configured; }
            set { this.m_configured = value; }
        }

        /// <summary>
        /// Contains a list of internal messages captures during the 
        /// last configuration.
        /// </summary>
        public virtual ICollection ConfigurationMessages
        {
            get { return this.m_configurationMessages; }
            set { this.m_configurationMessages = value; }
        }

        /// <summary>Event to notify that the repository has been shutdown.</summary>
        /// <value>
        /// Event to notify that the repository has been shutdown.
        /// </value>
        /// <remarks>
        /// <para>
        /// Event raised when the repository has been shutdown.
        /// </para>
        /// </remarks>
        public event LoggerRepositoryShutdownEventHandler ShutdownEvent
        {
            add { this.m_shutdownEvent += value; }
            remove { this.m_shutdownEvent -= value; }
        }

        /// <summary>Event to notify that the repository has had its configuration reset.</summary>
        /// <value>
        /// Event to notify that the repository has had its configuration reset.
        /// </value>
        /// <remarks>
        /// <para>
        /// Event raised when the repository's configuration has been
        /// reset to default.
        /// </para>
        /// </remarks>
        public event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset
        {
            add { this.m_configurationResetEvent += value; }
            remove { this.m_configurationResetEvent -= value; }
        }

        /// <summary>Event to notify that the repository has had its configuration changed.</summary>
        /// <value>
        /// Event to notify that the repository has had its configuration changed.
        /// </value>
        /// <remarks>
        /// <para>
        /// Event raised when the repository's configuration has been changed.
        /// </para>
        /// </remarks>
        public event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged
        {
            add { this.m_configurationChangedEvent += value; }
            remove { this.m_configurationChangedEvent -= value; }
        }

        /// <summary>Repository specific properties</summary>
        /// <value>
        /// Repository specific properties
        /// </value>
        /// <remarks>
        /// These properties can be specified on a repository specific basis
        /// </remarks>
        public PropertiesDictionary Properties 
        { 
            get { return this.m_properties; } 
        }

        /// <summary>Returns all the Appenders that are configured as an Array.</summary>
        /// <returns>All the Appenders</returns>
        /// <remarks>
        /// <para>
        /// Returns all the Appenders that are configured as an Array.
        /// </para>
        /// </remarks>
        public abstract Appender.IAppender[] GetAppenders();

        /// <summary>The fully qualified type of the LoggerRepositorySkeleton class.</summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(LoggerRepositorySkeleton);

        private void AddBuiltinLevels()
        {
			log.ErrorFormat(">>> Entering AddBuiltinLevels()");
            // Add the predefined levels to the map
            this.m_levelMap.Add(Level.Off);

            // Unrecoverable errors
            this.m_levelMap.Add(Level.Emergency);
            this.m_levelMap.Add(Level.Fatal);
            this.m_levelMap.Add(Level.Alert); 

            // Recoverable errors
            this.m_levelMap.Add(Level.Critical);
            this.m_levelMap.Add(Level.Severe);
            this.m_levelMap.Add(Level.Error);
            this.m_levelMap.Add(Level.Warn);

            // Information
            this.m_levelMap.Add(Level.Notice);
            this.m_levelMap.Add(Level.Info); 

            // Debug
            this.m_levelMap.Add(Level.Debug);
            this.m_levelMap.Add(Level.Fine);
            this.m_levelMap.Add(Level.Trace);
            this.m_levelMap.Add(Level.Finer);
            this.m_levelMap.Add(Level.Verbose);
            this.m_levelMap.Add(Level.Finest);

            this.m_levelMap.Add(Level.All);
			log.WarnFormat("<<< Exiting AddBuiltinLevels()");
        }

        /// <summary>Adds an object renderer for a specific class. </summary>
        /// <param name="typeToRender">The type that will be rendered by the renderer supplied.</param>
        /// <param name="rendererInstance">The object renderer used to render the object.</param>
        /// <remarks>
        /// <para>
        /// Adds an object renderer for a specific class. 
        /// </para>
        /// </remarks>
        public virtual void AddRenderer(Type typeToRender, IObjectRenderer rendererInstance) 
        {
			log.ErrorFormat(">>> Entering AddRenderer(typeToRender={0},rendererInstance={1})", typeToRender, rendererInstance);
            if (typeToRender == null)
            {
				log.ErrorFormat("Throwing ArgumentNullException from AddRenderer(typeToRender={0},rendererInstance={1})", typeToRender, rendererInstance);
                throw new ArgumentNullException("typeToRender");
            }
            if (rendererInstance == null)
            {
                throw new ArgumentNullException("rendererInstance");
            }

            this.m_rendererMap.Put(typeToRender, rendererInstance);
			log.WarnFormat("<<< Exiting AddRenderer(typeToRender={0},rendererInstance={1})", typeToRender, rendererInstance);
        }

        /// <summary>Notify the registered listeners that the repository is shutting down</summary>
        /// <param name="e">Empty EventArgs</param>
        /// <remarks>
        /// <para>
        /// Notify any listeners that this repository is shutting down.
        /// </para>
        /// </remarks>
        protected virtual void OnShutdown(EventArgs e)
        {
			log.ErrorFormat(">>> Entering OnShutdown(e={0})", e);
            if (e == null)
            {
                e = EventArgs.Empty;
            }

            LoggerRepositoryShutdownEventHandler handler = this.m_shutdownEvent;
            if (handler != null)
            {
                handler(this, e);
            }
			log.WarnFormat("<<< Exiting OnShutdown(e={0})", e);
        }

        /// <summary>Notify the registered listeners that the repository has had its configuration reset</summary>
        /// <param name="e">Empty EventArgs</param>
        /// <remarks>
        /// <para>
        /// Notify any listeners that this repository's configuration has been reset.
        /// </para>
        /// </remarks>
        protected virtual void OnConfigurationReset(EventArgs e)
        {
			log.ErrorFormat(">>> Entering OnConfigurationReset(e={0})", e);
            if (e == null)
            {
                e = EventArgs.Empty;
            }

            LoggerRepositoryConfigurationResetEventHandler handler = this.m_configurationResetEvent;
            if (handler != null)
            {
                handler(this, e);
            }
			log.WarnFormat("<<< Exiting OnConfigurationReset(e={0})", e);
        }

        /// <summary>Notify the registered listeners that the repository has had its configuration changed</summary>
        /// <param name="e">Empty EventArgs</param>
        /// <remarks>
        /// <para>
        /// Notify any listeners that this repository's configuration has changed.
        /// </para>
        /// </remarks>
        protected virtual void OnConfigurationChanged(EventArgs e)
        {
			log.ErrorFormat(">>> Entering OnConfigurationChanged(e={0})", e);
            if (e == null)
            {
                e = EventArgs.Empty;
            }

            LoggerRepositoryConfigurationChangedEventHandler handler = this.m_configurationChangedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
			log.WarnFormat("<<< Exiting OnConfigurationChanged(e={0})", e);
        }

        /// <summary>Raise a configuration changed event on this repository</summary>
        /// <param name="e">EventArgs.Empty</param>
        /// <remarks>
        /// <para>
        /// Applications that programmatically change the configuration of the repository should
        /// raise this event notification to notify listeners.
        /// </para>
        /// </remarks>
        public void RaiseConfigurationChanged(EventArgs e)
        {
			log.ErrorFormat(">>> Entering RaiseConfigurationChanged(e={0})", e);
            this.OnConfigurationChanged(e);
			log.WarnFormat("<<< Exiting RaiseConfigurationChanged(e={0})", e);
        }

        private static int GetWaitTime(DateTime startTimeUtc, int millisecondsTimeout)
        {
			log.ErrorFormat(">>> Entering GetWaitTime(startTimeUtc={0},millisecondsTimeout={1})", startTimeUtc, millisecondsTimeout);
            if (millisecondsTimeout == Timeout.Infinite)
            {
                return Timeout.Infinite;
            }

            if (millisecondsTimeout == 0)
            {
                return 0;
            }

            int elapsedMilliseconds = (int)(DateTime.UtcNow - startTimeUtc).TotalMilliseconds;
			log.ErrorFormat("GetWaitTime(startTimeUtc={0},millisecondsTimeout={1}): elapsedMilliseconds → {2}", startTimeUtc, millisecondsTimeout, elapsedMilliseconds);
            int timeout = millisecondsTimeout - elapsedMilliseconds;
			log.ErrorFormat("GetWaitTime(startTimeUtc={0},millisecondsTimeout={1}): timeout → {2}", startTimeUtc, millisecondsTimeout, timeout);
            if (timeout < 0)
            {
                timeout = 0;
            }

			log.WarnFormat("<<< Exiting GetWaitTime(startTimeUtc={0},millisecondsTimeout={1})", startTimeUtc, millisecondsTimeout);
            return timeout;
        }

        /// <summary>Flushes all configured Appenders that implement <see cref="log4net.Appender.IFlushable"/>.</summary>
        /// <param name="millisecondsTimeout">The maximum time in milliseconds to wait for logging events from asycnhronous appenders to be flushed,
        /// or <see cref="Timeout.Infinite"/> to wait indefinitely.</param>
        /// <returns><c>True</c> if all logging events were flushed successfully, else <c>false</c>.</returns>
        public bool Flush(int millisecondsTimeout)
        {
			log.ErrorFormat(">>> Entering Flush(millisecondsTimeout={0})", millisecondsTimeout);
            if (millisecondsTimeout < -1)
            {
				log.ErrorFormat("Throwing ArgumentOutOfRangeException from Flush(millisecondsTimeout={0})", millisecondsTimeout);
                throw new ArgumentOutOfRangeException("millisecondsTimeout", "Timeout must be -1 (Timeout.Infinite) or non-negative");
            }

            // Assume success until one of the appenders fails
            bool result = true;
			log.ErrorFormat("Flush(millisecondsTimeout={0}): result → {1}", millisecondsTimeout, result);

            // Use DateTime.UtcNow rather than a System.Diagnostics.Stopwatch for compatibility with .NET 1.x
            DateTime startTimeUtc = DateTime.UtcNow;

            // Do buffering appenders first.  These may be forwarding to other appenders
            foreach(Appender.IAppender appender in this.GetAppenders())
            {
                Appender.IFlushable flushable = appender as Appender.IFlushable;
                if (flushable == null)
                {
                    continue;
                }

                if (appender is Appender.BufferingAppenderSkeleton)
                {
                    int timeout = GetWaitTime(startTimeUtc, millisecondsTimeout);
					log.ErrorFormat("Flush(millisecondsTimeout={0}): timeout → {1}", millisecondsTimeout, timeout);
                    if (!flushable.Flush(timeout))
                    {
                        result = false;
                    }
                }
            }

            // Do non-buffering appenders.
            foreach (Appender.IAppender appender in this.GetAppenders())
            {
                Appender.IFlushable flushable = appender as Appender.IFlushable;
                if (flushable == null)
                {
                    continue;
                }

                if (!(appender is Appender.BufferingAppenderSkeleton))
                {
                    int timeout = GetWaitTime(startTimeUtc, millisecondsTimeout);
                    if (!flushable.Flush(timeout))
                    {
                        result = false;
                    }
                }
            }

			log.WarnFormat("<<< Exiting Flush(millisecondsTimeout={0})", millisecondsTimeout);
            return result;
        }
    }
}
