// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Instrumentation
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Compilation;

    using DotNetNuke.Internal.SourceGenerators;
    using log4net.Config;

    /// <summary>Provides access to logging methods.  Obsolete, use <see cref="LoggerSource"/> instead.</summary>
    [DnnDeprecated(7, 0, 1, "Use LoggerSource.Instance", RemovalVersion = 11)]
    public static partial class DnnLog
    {
        private const string ConfigFile = "DotNetNuke.log4net.config";
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(DnnLog));

        private static readonly object ConfigLock = new object();
        private static bool isConfigured;

        // use a single static logger to avoid the performance impact of type reflection on every call for logging
        private static StackFrame CallingFrame
        {
            get
            {
                StackFrame frame = null;
                StackFrame[] stack = new StackTrace().GetFrames();

                int frameDepth = 0;
                if (stack != null)
                {
                    Type reflectedType = stack[frameDepth].GetMethod().ReflectedType;
                    while (reflectedType == BuildManager.GetType("DotNetNuke.Services.Exceptions.Exceptions", false) || reflectedType == typeof(DnnLogger) || reflectedType == typeof(DnnLog))
                    {
                        frameDepth++;
                        reflectedType = stack[frameDepth].GetMethod().ReflectedType;
                    }

                    frame = stack[frameDepth];
                }

                return frame;
            }
        }

        private static Type CallingType
        {
            get
            {
                return CallingFrame.GetMethod().DeclaringType;
            }
        }

        /// <summary>  Standard method to use on method entry.</summary>
        public static void MethodEntry()
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(CultureInfo.InvariantCulture, "Entering Method [{0}]", CallingFrame.GetMethod().Name);
            }
			Logger.LogDebug("<<< Exiting MethodEntry()");
        }

        /// <summary>Standard method to use on method exit.</summary>
        /// <param name="returnObject">The return value of the method.</param>
        public static void MethodExit(object returnObject)
        {
			Logger.LogDebug(">>> Entering MethodExit(returnObject={ReturnObject})", returnObject);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                if (returnObject == null)
                {
                    returnObject = "NULL";
                }

                Logger.TraceFormat(CultureInfo.InvariantCulture, "Method [{0}] Returned [{1}]", CallingFrame.GetMethod().Name, returnObject);
            }
			Logger.LogDebug("<<< Exiting MethodExit(returnObject={ReturnObject})", returnObject);
			Logger.LogDebug(">>> Entering MethodExit()");
			Logger.LogDebug("<<< Exiting MethodExit()");
        }

        /// <summary>  Standard method to use on method exit.</summary>
        public static void MethodExit()
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(CultureInfo.InvariantCulture, "Method [{0}] Returned", CallingFrame.GetMethod().Name);
            }
        }

        /// <summary>Logs a message at the Trace log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Trace(string message)
        {
			Logger.LogDebug(">>> Entering Trace(message={Message})", message);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.Trace(message);
            }
			Logger.LogDebug("<<< Exiting Trace(message={Message})", message);
        }

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(string format, params object[] args)
            => Trace(CultureInfo.InvariantCulture, format, args);
			Logger.LogDebug(">>> Entering Trace(format={Format},args={Args})", format, args);

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
		Logger.LogDebug("<<< Exiting Trace(format={Format},args={Args})", format, args);
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(IFormatProvider provider, string format, params object[] args)
        {
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(provider, format, args);
            }
        }

        /// <summary>Logs a message at the Debug log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
			Logger.LogDebug(">>> Entering Debug(message={Message})", message);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.Debug(message);
            }
			Logger.LogDebug("<<< Exiting Debug(message={Message})", message);
        }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(string format, params object[] args)
        {
			Logger.LogDebug(">>> Entering Debug(format={Format},args={Args})", format, args);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                if (args.Length == 0)
                {
                    Logger.Debug(format);
                }
                else
                {
                    Logger.DebugFormat(CultureInfo.InvariantCulture, format, args);
                }
            }
			Logger.LogDebug("<<< Exiting Debug(format={Format},args={Args})", format, args);
        }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogDebug(">>> Entering Debug(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.DebugFormat(provider, format, args);
            }
			Logger.LogDebug("<<< Exiting Debug(provider={Provider},format={Format},args={Args})", provider, format, args);
        }

        /// <summary>Logs a message at the Info log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Info(object message)
        {
			Logger.LogDebug(">>> Entering Info(message={Message})", message);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.Info(message);
            }
			Logger.LogDebug("<<< Exiting Info(message={Message})", message);
        }

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Info(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogDebug(">>> Entering Info(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.InfoFormat(provider, format, args);
            }
			Logger.LogDebug(">>> Entering Info(format={Format},args={Args})", format, args);
			Logger.LogDebug("<<< Exiting Info(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogDebug("<<< Exiting Info(format={Format},args={Args})", format, args);
        }

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Info(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                if (args.Length == 0)
                {
                    Logger.Info(format);
                }
                else
                {
                    Logger.InfoFormat(CultureInfo.InvariantCulture, format, args);
                }
            }
        }

        /// <summary>Log a message and exception details at the Warn log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Warn(string message, Exception exception)
        {
			Logger.LogDebug(">>> Entering Warn(message={Message},exception={Exception})", message, exception);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message, exception);
            }
			Logger.LogDebug("<<< Exiting Warn(message={Message},exception={Exception})", message, exception);
			Logger.LogDebug(">>> Entering Warn(message={Message})", message);
			Logger.LogDebug("<<< Exiting Warn(message={Message})", message);
        }

        /// <summary>Logs a message at the Warn log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Warn(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message);
            }
        }

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warn(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogDebug(">>> Entering Warn(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.WarnFormat(provider, format, args);
            }
			Logger.LogDebug(">>> Entering Warn(format={Format},args={Args})", format, args);
			Logger.LogDebug("<<< Exiting Warn(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogDebug("<<< Exiting Warn(format={Format},args={Args})", format, args);
        }

        /// <summary>Log a message at the Warn log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warn(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                if (args.Length == 0)
                {
                    Logger.Warn(format);
                }
                else
                {
                    Logger.WarnFormat(CultureInfo.InvariantCulture, format, args);
                }
            }
        }

        /// <summary>Log a message and exception details at the Error log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Error(string message, Exception exception)
        {
			Logger.LogDebug(">>> Entering Error(message={Message},exception={Exception})", message, exception);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message, exception);
            }
			Logger.LogDebug("<<< Exiting Error(message={Message},exception={Exception})", message, exception);
			Logger.LogDebug(">>> Entering Error(message={Message})", message);
			Logger.LogDebug("<<< Exiting Error(message={Message})", message);
        }

        /// <summary>Logs a message at the Error log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Error(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message);
            }
        }

        /// <summary>Log exception details at the Error log level.</summary>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Error(Exception exception)
        {
			Logger.LogDebug(">>> Entering Error(exception={Exception})", exception);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(exception.Message, exception);
            }
			Logger.LogDebug("<<< Exiting Error(exception={Exception})", exception);
        }

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogDebug(">>> Entering Error(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.ErrorFormat(provider, format, args);
            }
			Logger.LogDebug(">>> Entering Error(format={Format},args={Args})", format, args);
			Logger.LogDebug("<<< Exiting Error(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogDebug("<<< Exiting Error(format={Format},args={Args})", format, args);
        }

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                if (args.Length == 0)
                {
                    Logger.Error(format);
                }
                else
                {
                    Logger.ErrorFormat(CultureInfo.InvariantCulture, format, args);
                }
            }
        }

        /// <summary>Log a message and exception details at the Fatal log level.</summary>
        /// <param name="message">An object to display as the message.</param>
        /// <param name="exception">An exception to include in the log.</param>
        public static void Fatal(string message, Exception exception)
        {
			Logger.LogDebug(">>> Entering Fatal(message={Message},exception={Exception})", message, exception);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message, exception);
            }
			Logger.LogDebug("<<< Exiting Fatal(message={Message},exception={Exception})", message, exception);
			Logger.LogDebug(">>> Entering Fatal(message={Message})", message);
			Logger.LogDebug("<<< Exiting Fatal(message={Message})", message);
        }

        /// <summary>Logs a message at the Fatal log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Fatal(object message)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message);
            }
        }

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Fatal(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogDebug(">>> Entering Fatal(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.FatalFormat(provider, format, args);
            }
			Logger.LogDebug(">>> Entering Fatal(format={Format},args={Args})", format, args);
			Logger.LogDebug("<<< Exiting Fatal(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogDebug("<<< Exiting Fatal(format={Format},args={Args})", format, args);
        }

        /// <summary>Log a message at the Fatal log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Fatal(string format, params object[] args)
        {
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                if (args.Length == 0)
                {
                    Logger.Fatal(format);
                }
                else
                {
                    Logger.FatalFormat(CultureInfo.InvariantCulture, format, args);
                }
            }
        }

        private static void EnsureConfig()
        {
			Logger.LogDebug(">>> Entering EnsureConfig()");
            if (!isConfigured)
            {
                lock (ConfigLock)
                {
                    if (!isConfigured)
                    {
                        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
                        var originalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + ConfigFile);
						Logger.LogDebug("EnsureConfig(): configPath → {ConfigPath}", configPath);
						Logger.LogDebug("EnsureConfig(): originalPath → {OriginalPath}", originalPath);
                        if (!File.Exists(configPath) && File.Exists(originalPath))
                        {
                            File.Copy(originalPath, configPath);
                        }

                        if (File.Exists(configPath))
                        {
                            XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
                        }

                        isConfigured = true;
						Logger.LogDebug("EnsureConfig(): isConfigured → {IsConfigured}", isConfigured);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting EnsureConfig()");
        }
    }
}
