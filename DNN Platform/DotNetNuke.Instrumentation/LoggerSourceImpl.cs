// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Internal.SourceGenerators;
    using log4net;
    using log4net.Config;
    using log4net.Core;
    using log4net.Repository;
    using log4net.Util;

    /// <summary>An <see cref="ILoggerSource"/> implementation.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class LoggerSourceImpl : ILoggerSource
    {
        /// <inheritdoc />
        public ILog GetLogger(Type type)
        {
			log.DebugFormat(">>> Entering GetLogger(type={0})", type);
			log.DebugFormat(">>> Entering Logger(logger={0},type={1})", logger, type);
			log.DebugFormat("<<< Exiting Logger(logger={0},type={1})", logger, type);
			log.DebugFormat("<<< Exiting GetLogger(type={0})", type);
            return new Logger(LogManager.GetLogger(type).Logger, type);
        }

        /// <inheritdoc />
        public ILog GetLogger(string name)
        {
			log.DebugFormat(">>> Entering GetLogger(name={0})", name);
            return new Logger(LogManager.GetLogger(name).Logger, null);
        }

        private sealed class Logger : LoggerWrapperImpl, ILog
        {
            private const string ConfigFile = "DotNetNuke.log4net.config";
            private static readonly object ConfigLock = new object();
            private static Level levelTrace;
            private static Level levelDebug;
            private static Level levelInfo;
            private static Level levelWarn;
            private static Level levelError;
            private static Level levelFatal;
            private static bool configured;

            // add custom logging levels (below trace value of 20000)
            //            internal static Level LevelLogInfo = new Level(10001, "LogInfo");
            //            internal static Level LevelLogError = new Level(10002, "LogError");
            private readonly Type stackBoundary = typeof(Logger);

            internal Logger(ILogger logger, Type type)
                : base(logger)
            {
                this.stackBoundary = type ?? typeof(Logger);
                EnsureConfig();
                ReloadLevels(logger.Repository);
            }

            public bool IsDebugEnabled
            {
                get { return this.Logger.IsEnabledFor(levelDebug); }
            }

            public bool IsInfoEnabled
            {
                get { return this.Logger.IsEnabledFor(levelInfo); }
            }

            public bool IsTraceEnabled
            {
                get { return this.Logger.IsEnabledFor(levelTrace); }
            }

            public bool IsWarnEnabled
            {
                get { return this.Logger.IsEnabledFor(levelWarn); }
            }

            public bool IsErrorEnabled
            {
                get { return this.Logger.IsEnabledFor(levelError); }
            }

            public bool IsFatalEnabled
            {
                get { return this.Logger.IsEnabledFor(levelFatal); }
            }

            public void Debug(object message)
            {
				log.DebugFormat(">>> Entering Debug(message={0})", message);
                this.Debug(message, null);
				log.DebugFormat("<<< Exiting Debug(message={0})", message);
            }

            public void Debug(object message, Exception exception)
            {
				log.DebugFormat(">>> Entering Debug(message={0},exception={1})", message, exception);
                this.Logger.Log(this.stackBoundary, levelDebug, message, exception);
				log.DebugFormat("<<< Exiting Debug(message={0},exception={1})", message, exception);
            }

            public void DebugFormat(string format, params object[] args)
            {
				log.DebugFormat(">>> Entering DebugFormat(format={0},args={1})", format, args);
                this.DebugFormat(CultureInfo.InvariantCulture, format, args);
				log.DebugFormat("<<< Exiting DebugFormat(format={0},args={1})", format, args);
            }

            public void DebugFormat(IFormatProvider provider, string format, params object[] args)
            {
				log.DebugFormat(">>> Entering DebugFormat(provider={0},format={1},args={2})", provider, format, args);
                this.Logger.Log(this.stackBoundary, levelDebug, new SystemStringFormat(provider, format, args), null);
				log.DebugFormat("<<< Exiting DebugFormat(provider={0},format={1},args={2})", provider, format, args);
            }

            public void Info(object message)
            {
				log.DebugFormat(">>> Entering Info(message={0})", message);
                this.Info(message, null);
				log.DebugFormat("<<< Exiting Info(message={0})", message);
            }

            public void Info(object message, Exception exception)
            {
				log.DebugFormat(">>> Entering Info(message={0},exception={1})", message, exception);
                this.Logger.Log(this.stackBoundary, levelInfo, message, exception);
				log.DebugFormat("<<< Exiting Info(message={0},exception={1})", message, exception);
            }

            public void InfoFormat(string format, params object[] args)
            {
				log.DebugFormat(">>> Entering InfoFormat(format={0},args={1})", format, args);
                this.InfoFormat(CultureInfo.InvariantCulture, format, args);
				log.DebugFormat("<<< Exiting InfoFormat(format={0},args={1})", format, args);
            }

            public void InfoFormat(IFormatProvider provider, string format, params object[] args)
            {
				log.DebugFormat(">>> Entering InfoFormat(provider={0},format={1},args={2})", provider, format, args);
                this.Logger.Log(this.stackBoundary, levelInfo, new SystemStringFormat(provider, format, args), null);
				log.DebugFormat("<<< Exiting InfoFormat(provider={0},format={1},args={2})", provider, format, args);
            }

            public void Trace(object message)
            {
				log.DebugFormat(">>> Entering Trace(message={0})", message);
                this.Trace(message, null);
				log.DebugFormat("<<< Exiting Trace(message={0})", message);
            }

            public void Trace(object message, Exception exception)
            {
				log.DebugFormat(">>> Entering Trace(message={0},exception={1})", message, exception);
                this.Logger.Log(this.stackBoundary, levelTrace, message, exception);
				log.DebugFormat("<<< Exiting Trace(message={0},exception={1})", message, exception);
            }

            public void TraceFormat(string format, params object[] args)
            {
				log.DebugFormat(">>> Entering TraceFormat(format={0},args={1})", format, args);
                this.TraceFormat(CultureInfo.InvariantCulture, format, args);
				log.DebugFormat("<<< Exiting TraceFormat(format={0},args={1})", format, args);
            }

            public void TraceFormat(IFormatProvider provider, string format, params object[] args)
            {
				log.DebugFormat(">>> Entering TraceFormat(provider={0},format={1},args={2})", provider, format, args);
                this.Logger.Log(this.stackBoundary, levelTrace, new SystemStringFormat(provider, format, args), null);
				log.DebugFormat("<<< Exiting TraceFormat(provider={0},format={1},args={2})", provider, format, args);
            }

            public void Warn(object message)
            {
				log.DebugFormat(">>> Entering Warn(message={0})", message);
                this.Warn(message, null);
				log.DebugFormat("<<< Exiting Warn(message={0})", message);
            }

            public void Warn(object message, Exception exception)
            {
				log.DebugFormat(">>> Entering Warn(message={0},exception={1})", message, exception);
                this.Logger.Log(this.stackBoundary, levelWarn, message, exception);
				log.DebugFormat("<<< Exiting Warn(message={0},exception={1})", message, exception);
            }

            public void WarnFormat(string format, params object[] args)
            {
				log.DebugFormat(">>> Entering WarnFormat(format={0},args={1})", format, args);
                this.WarnFormat(CultureInfo.InvariantCulture, format, args);
				log.DebugFormat("<<< Exiting WarnFormat(format={0},args={1})", format, args);
            }

            public void WarnFormat(IFormatProvider provider, string format, params object[] args)
            {
				log.DebugFormat(">>> Entering WarnFormat(provider={0},format={1},args={2})", provider, format, args);
                this.Logger.Log(this.stackBoundary, levelWarn, new SystemStringFormat(provider, format, args), null);
				log.DebugFormat("<<< Exiting WarnFormat(provider={0},format={1},args={2})", provider, format, args);
            }

            public void Error(object message)
            {
				log.DebugFormat(">>> Entering Error(message={0})", message);
                this.Error(message, null);
				log.DebugFormat("<<< Exiting Error(message={0})", message);
            }

            public void Error(object message, Exception exception)
            {
				log.DebugFormat(">>> Entering Error(message={0},exception={1})", message, exception);
                this.Logger.Log(this.stackBoundary, levelError, message, exception);
				log.DebugFormat("<<< Exiting Error(message={0},exception={1})", message, exception);
            }

            public void ErrorFormat(string format, params object[] args)
            {
				log.DebugFormat(">>> Entering ErrorFormat(format={0},args={1})", format, args);
                this.ErrorFormat(CultureInfo.InvariantCulture, format, args);
				log.DebugFormat("<<< Exiting ErrorFormat(format={0},args={1})", format, args);
            }

            public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
            {
				log.DebugFormat(">>> Entering ErrorFormat(provider={0},format={1},args={2})", provider, format, args);
                this.Logger.Log(this.stackBoundary, levelError, new SystemStringFormat(provider, format, args), null);
				log.DebugFormat("<<< Exiting ErrorFormat(provider={0},format={1},args={2})", provider, format, args);
            }

            public void Fatal(object message)
            {
				log.DebugFormat(">>> Entering Fatal(message={0})", message);
                this.Fatal(message, null);
				log.DebugFormat("<<< Exiting Fatal(message={0})", message);
            }

            public void Fatal(object message, Exception exception)
            {
				log.DebugFormat(">>> Entering Fatal(message={0},exception={1})", message, exception);
                this.Logger.Log(this.stackBoundary, levelFatal, message, exception);
				log.DebugFormat("<<< Exiting Fatal(message={0},exception={1})", message, exception);
            }

            public void FatalFormat(string format, params object[] args)
            {
				log.DebugFormat(">>> Entering FatalFormat(format={0},args={1})", format, args);
                this.FatalFormat(CultureInfo.InvariantCulture, format, args);
				log.DebugFormat("<<< Exiting FatalFormat(format={0},args={1})", format, args);
            }

            public void FatalFormat(IFormatProvider provider, string format, params object[] args)
            {
				log.DebugFormat(">>> Entering FatalFormat(provider={0},format={1},args={2})", provider, format, args);
                this.Logger.Log(this.stackBoundary, levelFatal, new SystemStringFormat(provider, format, args), null);
				log.DebugFormat("<<< Exiting FatalFormat(provider={0},format={1},args={2})", provider, format, args);
            }

            private static void EnsureConfig()
            {
				log.DebugFormat(">>> Entering EnsureConfig()");
                if (!configured)
                {
                    lock (ConfigLock)
                    {
                        if (!configured)
                        {
                            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
							log.DebugFormat("EnsureConfig(): configPath → {0}", configPath);
                            var originalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + ConfigFile);
							log.DebugFormat("EnsureConfig(): originalPath → {0}", originalPath);
                            if (!File.Exists(configPath) && File.Exists(originalPath))
                            {
                                File.Copy(originalPath, configPath);
                            }

                            if (File.Exists(configPath))
                            {
                                AddGlobalContext();
                                XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
                            }

                            configured = true;
							log.DebugFormat("EnsureConfig(): configured → {0}", configured);
                        }
                    }
                }
				log.DebugFormat("<<< Exiting EnsureConfig()");
            }

            private static void ReloadLevels(ILoggerRepository repository)
            {
				log.DebugFormat(">>> Entering ReloadLevels(repository={0})", repository);
                LevelMap levelMap = repository.LevelMap;

                levelTrace = levelMap.LookupWithDefault(Level.Trace);
                levelDebug = levelMap.LookupWithDefault(Level.Debug);
				log.DebugFormat("ReloadLevels(repository={0}): levelDebug → {1}", repository, levelDebug);
				log.DebugFormat("ReloadLevels(repository={0}): levelTrace → {1}", repository, levelTrace);
                levelInfo = levelMap.LookupWithDefault(Level.Info);
				log.DebugFormat("ReloadLevels(repository={0}): levelInfo → {1}", repository, levelInfo);
                levelWarn = levelMap.LookupWithDefault(Level.Warn);
				log.DebugFormat("ReloadLevels(repository={0}): levelWarn → {1}", repository, levelWarn);
                levelError = levelMap.LookupWithDefault(Level.Error);
				log.DebugFormat("ReloadLevels(repository={0}): levelError → {1}", repository, levelError);
                levelFatal = levelMap.LookupWithDefault(Level.Fatal);
				log.DebugFormat("ReloadLevels(repository={0}): levelFatal → {1}", repository, levelFatal);
				log.DebugFormat("<<< Exiting ReloadLevels(repository={0})", repository);
            }

            private static void AddGlobalContext()
            {
				log.DebugFormat(">>> Entering AddGlobalContext()");
                try
                {
                    GlobalContext.Properties["appdomain"] = AppDomain.CurrentDomain.Id.ToString("D", CultureInfo.InvariantCulture);
                }
                catch
                {
					log.ErrorFormat("Exception in AddGlobalContext(): {0}", ex.Message);
                    // do nothing but just make sure no exception here.
                }
				log.DebugFormat("<<< Exiting AddGlobalContext()");
            }
        }
    }
}
