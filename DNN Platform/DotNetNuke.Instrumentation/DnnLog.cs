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
			Logger.LogWarning("<<< Exiting MethodEntry()");
        }

        /// <summary>Standard method to use on method exit.</summary>
        /// <param name="returnObject">The return value of the method.</param>
        public static void MethodExit(object returnObject)
        {
			Logger.LogError(">>> Entering MethodExit(returnObject={ReturnObject})", returnObject);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                if (returnObject == null)
                {
                    returnObject = "NULL";
                }

                Logger.TraceFormat(CultureInfo.InvariantCulture, "Method [{0}] Returned [{1}]", CallingFrame.GetMethod().Name, returnObject);
            }
			Logger.LogWarning("<<< Exiting MethodExit(returnObject={ReturnObject})", returnObject);
			Logger.LogError(">>> Entering MethodExit()");
			Logger.LogWarning("<<< Exiting MethodExit()");
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
			Logger.LogError(">>> Entering Trace(message={Message})", message);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.Trace(message);
            }
			Logger.LogWarning("<<< Exiting Trace(message={Message})", message);
        }

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(string format, params object[] args)
            => Trace(CultureInfo.InvariantCulture, format, args);

        /// <summary>Log a message at the Trace log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogError(">>> Entering Trace(format={Format},args={Args})", format, args);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelTrace))
            {
                Logger.TraceFormat(provider, format, args);
            }
			Logger.LogWarning("<<< Exiting Trace(provider={Provider},format={Format},args={Args})", provider, format, args);
        }

        /// <summary>Logs a message at the Debug log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Debug(object message)
        {
			Logger.LogError(">>> Entering Debug(message={Message})", message);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.Debug(message);
            }
			Logger.LogWarning("<<< Exiting Debug(message={Message})", message);
        }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(string format, params object[] args)
        {
			Logger.LogError(">>> Entering Debug(format={Format},args={Args})", format, args);
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
			Logger.LogWarning("<<< Exiting Debug(format={Format},args={Args})", format, args);
        }

        /// <summary>Log a message at the Debug log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogError(">>> Entering Debug(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelDebug))
            {
                Logger.DebugFormat(provider, format, args);
            }
			Logger.LogWarning("<<< Exiting Debug(provider={Provider},format={Format},args={Args})", provider, format, args);
        }

        /// <summary>Logs a message at the Info log level.</summary>
        /// <param name="message">The message to log.</param>
        public static void Info(object message)
        {
			Logger.LogError(">>> Entering Info(message={Message})", message);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.Info(message);
            }
			Logger.LogWarning("<<< Exiting Info(message={Message})", message);
        }

        /// <summary>Log a message at the Info log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Info(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogError(">>> Entering Info(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelInfo))
            {
                Logger.InfoFormat(provider, format, args);
            }
			Logger.LogError(">>> Entering Info(format={Format},args={Args})", format, args);
			Logger.LogWarning("<<< Exiting Info(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogWarning("<<< Exiting Info(format={Format},args={Args})", format, args);
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
			Logger.LogError(">>> Entering Warn(message={Message},exception={Exception})", message, exception);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.Warn(message, exception);
            }
			Logger.LogWarning("<<< Exiting Warn(message={Message},exception={Exception})", message, exception);
			Logger.LogError(">>> Entering Warn(message={Message})", message);
			Logger.LogWarning("<<< Exiting Warn(message={Message})", message);
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
			Logger.LogError(">>> Entering Warn(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelWarn))
            {
                Logger.WarnFormat(provider, format, args);
            }
			Logger.LogError(">>> Entering Warn(format={Format},args={Args})", format, args);
			Logger.LogWarning("<<< Exiting Warn(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogWarning("<<< Exiting Warn(format={Format},args={Args})", format, args);
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
			Logger.LogError(">>> Entering Error(message={Message},exception={Exception})", message, exception);
            EnsureConfig();

            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(message, exception);
            }
			Logger.LogWarning("<<< Exiting Error(message={Message},exception={Exception})", message, exception);
			Logger.LogError(">>> Entering Error(message={Message})", message);
			Logger.LogWarning("<<< Exiting Error(message={Message})", message);
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
			Logger.LogError(">>> Entering Error(exception={Exception})", exception);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.Error(exception.Message, exception);
            }
			Logger.LogWarning("<<< Exiting Error(exception={Exception})", exception);
        }

        /// <summary>Log a message at the Error log level.</summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">A <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting">composite format string</see>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(IFormatProvider provider, string format, params object[] args)
        {
			Logger.LogError(">>> Entering Error(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelError))
            {
                Logger.ErrorFormat(provider, format, args);
            }
			Logger.LogError(">>> Entering Error(format={Format},args={Args})", format, args);
			Logger.LogWarning("<<< Exiting Error(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogWarning("<<< Exiting Error(format={Format},args={Args})", format, args);
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
			Logger.LogError(">>> Entering Fatal(message={Message},exception={Exception})", message, exception);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.Fatal(message, exception);
            }
			Logger.LogWarning("<<< Exiting Fatal(message={Message},exception={Exception})", message, exception);
			Logger.LogError(">>> Entering Fatal(message={Message})", message);
			Logger.LogWarning("<<< Exiting Fatal(message={Message})", message);
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
			Logger.LogError(">>> Entering Fatal(provider={Provider},format={Format},args={Args})", provider, format, args);
            EnsureConfig();
            if (Logger.Logger.IsEnabledFor(DnnLogger.LevelFatal))
            {
                Logger.FatalFormat(provider, format, args);
            }
			Logger.LogError(">>> Entering Fatal(format={Format},args={Args})", format, args);
			Logger.LogWarning("<<< Exiting Fatal(provider={Provider},format={Format},args={Args})", provider, format, args);
			Logger.LogWarning("<<< Exiting Fatal(format={Format},args={Args})", format, args);
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
			Logger.LogError(">>> Entering EnsureConfig()");
            if (!isConfigured)
            {
                lock (ConfigLock)
                {
                    if (!isConfigured)
                    {
                        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFile);
						Logger.LogError("EnsureConfig(): configPath → {ConfigPath}", configPath);
                        var originalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\" + ConfigFile);
						Logger.LogError("EnsureConfig(): originalPath → {OriginalPath}", originalPath);
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
			Logger.LogWarning("<<< Exiting EnsureConfig()");
        }
    }
}
