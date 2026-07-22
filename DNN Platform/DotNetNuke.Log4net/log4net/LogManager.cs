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
using System.Reflection;

using log4net.Core;
using log4net.Repository;

namespace log4net
{
    /// <summary>This class is used by client applications to request logger instances.</summary>
    /// <remarks>
    /// <para>
    /// This class has static methods that are used by a client to request
    /// a logger instance. The <see cref="M:GetLogger(string)"/> method is 
    /// used to retrieve a logger.
    /// </para>
    /// <para>
    /// See the <see cref="ILog"/> interface for more details.
    /// </para>
    /// </remarks>
    /// <example>Simple example of logging messages
    /// <code lang="C#">
    /// ILog log = LogManager.GetLogger("application-log");
    /// 
    /// log.Info("Application Start");
    /// log.Debug("This is a debug message");
    /// 
    /// if (log.IsDebugEnabled)
    /// {
    ///         log.Debug("This is another debug message");
    /// }
    /// </code>
    /// </example>
    /// <threadsafety static="true" instance="true" />
    /// <seealso cref="ILog"/>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public sealed class LogManager
    {
        /// <summary>Initializes a new instance of the <see cref="LogManager" /> class. </summary>
        /// <remarks>
        /// Uses a private access modifier to prevent instantiation of this class.
        /// </remarks>
        private LogManager()
        {
		cref.DebugFormat(">>> Entering LogManager()");
		cref.DebugFormat("<<< Exiting LogManager()");
        }

#if !NETSTANDARD1_3 // Excluded because GetCallingAssembly() is not available in CoreFX (https://github.com/dotnet/corefx/issues/2221).
        /// <overloads>Returns the named logger if it exists.</overloads>
        /// <summary>Returns the named logger if it exists.</summary>
        /// <remarks>
        /// <para>
        /// If the named logger exists (in the default repository) then it
        /// returns a reference to the logger, otherwise it returns <c>null</c>.
        /// </para>
        /// </remarks>
        /// <param name="name">The fully qualified logger name to look for.</param>
        /// <returns>The logger found, or <c>null</c> if no logger could be found.</returns>
        public static ILog Exists(string name) 
        {
			cref.DebugFormat(">>> Entering Exists(name={0})", name);
			cref.DebugFormat("<<< Exiting Exists(name={0})", name);
            return Exists(Assembly.GetCallingAssembly(), name);
        }
        
        /// <overloads>Get the currently defined loggers.</overloads>
        /// <summary>Returns all the currently defined loggers in the default repository.</summary>
        /// <remarks>
        /// <para>The root logger is <b>not</b> included in the returned array.</para>
        /// </remarks>
        /// <returns>All the defined loggers.</returns>
        public static ILog[] GetCurrentLoggers()
        {
			cref.DebugFormat(">>> Entering GetCurrentLoggers()");
			cref.DebugFormat("<<< Exiting GetCurrentLoggers()");
            return GetCurrentLoggers(Assembly.GetCallingAssembly());
        }
        
        /// <overloads>Get or create a logger.</overloads>
        /// <summary>Retrieves or creates a named logger.</summary>
        /// <remarks>
        /// <para>
        /// Retrieves a logger named as the <paramref name="name"/>
        /// parameter. If the named logger already exists, then the
        /// existing instance will be returned. Otherwise, a new instance is
        /// created.
        /// </para>
        /// <para>By default, loggers do not have a set level but inherit
        /// it from the hierarchy. This is one of the central features of
        /// log4net.
        /// </para>
        /// </remarks>
        /// <param name="name">The name of the logger to retrieve.</param>
        /// <returns>The logger with the name specified.</returns>
        public static ILog GetLogger(string name)
        {
			cref.DebugFormat(">>> Entering GetLogger(name={0})", name);
			cref.DebugFormat("<<< Exiting GetLogger(name={0})", name);
            return GetLogger(Assembly.GetCallingAssembly(), name);
        }
#endif // !NETSTANDARD1_3

        /// <summary>Returns the named logger if it exists.</summary>
        /// <remarks>
        /// <para>
        /// If the named logger exists (in the specified repository) then it
        /// returns a reference to the logger, otherwise it returns
        /// <c>null</c>.
        /// </para>
        /// </remarks>
        /// <param name="repository">The repository to lookup in.</param>
        /// <param name="name">The fully qualified logger name to look for.</param>
        /// <returns>
        /// The logger found, or <c>null</c> if the logger doesn't exist in the specified 
        /// repository.
        /// </returns>
        public static ILog Exists(string repository, string name) 
        {
			cref.DebugFormat("<<< Exiting Exists(repository={0},name={1})", repository, name);
            return WrapLogger(LoggerManager.Exists(repository, name));
        }

        /// <summary>Returns the named logger if it exists.</summary>
        /// <remarks>
        /// <para>
        /// If the named logger exists (in the repository for the specified assembly) then it
        /// returns a reference to the logger, otherwise it returns
        /// <c>null</c>.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        /// <param name="name">The fully qualified logger name to look for.</param>
        /// <returns>
        /// The logger, or <c>null</c> if the logger doesn't exist in the specified
        /// assembly's repository.
        /// </returns>
        public static ILog Exists(Assembly repositoryAssembly, string name) 
        {
			cref.DebugFormat("<<< Exiting Exists(repositoryAssembly={0},name={1})", repositoryAssembly, name);
            return WrapLogger(LoggerManager.Exists(repositoryAssembly, name));
        }

        /// <summary>Returns all the currently defined loggers in the specified repository.</summary>
        /// <param name="repository">The repository to lookup in.</param>
        /// <remarks>
        /// The root logger is <b>not</b> included in the returned array.
        /// </remarks>
        /// <returns>All the defined loggers.</returns>
        public static ILog[] GetCurrentLoggers(string repository)
        {
			cref.DebugFormat("<<< Exiting GetCurrentLoggers(repository={0})", repository);
            return WrapLoggers(LoggerManager.GetCurrentLoggers(repository));
        }

        /// <summary>Returns all the currently defined loggers in the specified assembly's repository.</summary>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        /// <remarks>
        /// The root logger is <b>not</b> included in the returned array.
        /// </remarks>
        /// <returns>All the defined loggers.</returns>
        public static ILog[] GetCurrentLoggers(Assembly repositoryAssembly)
        {
			cref.DebugFormat("<<< Exiting GetCurrentLoggers(repositoryAssembly={0})", repositoryAssembly);
            return WrapLoggers(LoggerManager.GetCurrentLoggers(repositoryAssembly));
        }

        /// <summary>Retrieves or creates a named logger.</summary>
        /// <remarks>
        /// <para>
        /// Retrieve a logger named as the <paramref name="name"/>
        /// parameter. If the named logger already exists, then the
        /// existing instance will be returned. Otherwise, a new instance is
        /// created.
        /// </para>
        /// <para>
        /// By default, loggers do not have a set level but inherit
        /// it from the hierarchy. This is one of the central features of
        /// log4net.
        /// </para>
        /// </remarks>
        /// <param name="repository">The repository to lookup in.</param>
        /// <param name="name">The name of the logger to retrieve.</param>
        /// <returns>The logger with the name specified.</returns>
        public static ILog GetLogger(string repository, string name)
        {
			cref.DebugFormat("<<< Exiting GetLogger(repository={0},name={1})", repository, name);
            return WrapLogger(LoggerManager.GetLogger(repository, name));
        }

        /// <summary>Retrieves or creates a named logger.</summary>
        /// <remarks>
        /// <para>
        /// Retrieve a logger named as the <paramref name="name"/>
        /// parameter. If the named logger already exists, then the
        /// existing instance will be returned. Otherwise, a new instance is
        /// created.
        /// </para>
        /// <para>
        /// By default, loggers do not have a set level but inherit
        /// it from the hierarchy. This is one of the central features of
        /// log4net.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        /// <param name="name">The name of the logger to retrieve.</param>
        /// <returns>The logger with the name specified.</returns>
        public static ILog GetLogger(Assembly repositoryAssembly, string name)
        {
			cref.DebugFormat("<<< Exiting GetLogger(repositoryAssembly={0},name={1})", repositoryAssembly, name);
            return WrapLogger(LoggerManager.GetLogger(repositoryAssembly, name));
        }	

        /// <summary>Shorthand for <see cref="M:LogManager.GetLogger(string)"/>.</summary>
        /// <remarks>
        /// Get the logger for the fully qualified name of the type specified.
        /// </remarks>
        /// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
        /// <returns>The logger with the name specified.</returns>
        public static ILog GetLogger(Type type) 
        {
#if NETSTANDARD1_3
			cref.DebugFormat("<<< Exiting GetLogger(type={0})", type);
            return GetLogger(type.GetTypeInfo().Assembly, type.FullName);
#else
            return GetLogger(Assembly.GetCallingAssembly(), type.FullName);
#endif
        }

        /// <summary>Shorthand for <see cref="M:LogManager.GetLogger(string)"/>.</summary>
        /// <remarks>
        /// Gets the logger for the fully qualified name of the type specified.
        /// </remarks>
        /// <param name="repository">The repository to lookup in.</param>
        /// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
        /// <returns>The logger with the name specified.</returns>
        public static ILog GetLogger(string repository, Type type) 
        {
			cref.DebugFormat("<<< Exiting GetLogger(repository={0},type={1})", repository, type);
            return WrapLogger(LoggerManager.GetLogger(repository, type));
        }

        /// <summary>Shorthand for <see cref="M:LogManager.GetLogger(string)"/>.</summary>
        /// <remarks>
        /// Gets the logger for the fully qualified name of the type specified.
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        /// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
        /// <returns>The logger with the name specified.</returns>
        public static ILog GetLogger(Assembly repositoryAssembly, Type type) 
        {
			cref.DebugFormat("<<< Exiting GetLogger(repositoryAssembly={0},type={1})", repositoryAssembly, type);
            return WrapLogger(LoggerManager.GetLogger(repositoryAssembly, type));
        }

        /// <summary>Shuts down the log4net system.</summary>
        /// <remarks>
        /// <para>
        /// Calling this method will <b>safely</b> close and remove all
        /// appenders in all the loggers including root contained in all the
        /// default repositories.
        /// </para>
        /// <para>
        /// Some appenders need to be closed before the application exists. 
        /// Otherwise, pending logging events might be lost.
        /// </para>
        /// <para>The <c>shutdown</c> method is careful to close nested
        /// appenders before closing regular appenders. This is allows
        /// configurations where a regular appender is attached to a logger
        /// and again to a nested appender.
        /// </para>
        /// </remarks>
        public static void Shutdown() 
        {
			cref.DebugFormat(">>> Entering Shutdown()");
            LoggerManager.Shutdown();
			cref.DebugFormat("<<< Exiting Shutdown()");
        }

#if !NETSTANDARD1_3
        /// <overloads>Shutdown a logger repository.</overloads>
        /// <summary>Shuts down the default repository.</summary>
        /// <remarks>
        /// <para>
        /// Calling this method will <b>safely</b> close and remove all
        /// appenders in all the loggers including root contained in the
        /// default repository.
        /// </para>
        /// <para>Some appenders need to be closed before the application exists. 
        /// Otherwise, pending logging events might be lost.
        /// </para>
        /// <para>The <c>shutdown</c> method is careful to close nested
        /// appenders before closing regular appenders. This is allows
        /// configurations where a regular appender is attached to a logger
        /// and again to a nested appender.
        /// </para>
        /// </remarks>
        public static void ShutdownRepository() 
        {
			cref.DebugFormat(">>> Entering ShutdownRepository()");
            ShutdownRepository(Assembly.GetCallingAssembly());
			cref.DebugFormat("<<< Exiting ShutdownRepository()");
        }
#endif

        /// <summary>Shuts down the repository for the repository specified.</summary>
        /// <remarks>
        /// <para>
        /// Calling this method will <b>safely</b> close and remove all
        /// appenders in all the loggers including root contained in the
        /// <paramref name="repository"/> specified.
        /// </para>
        /// <para>
        /// Some appenders need to be closed before the application exists. 
        /// Otherwise, pending logging events might be lost.
        /// </para>
        /// <para>The <c>shutdown</c> method is careful to close nested
        /// appenders before closing regular appenders. This is allows
        /// configurations where a regular appender is attached to a logger
        /// and again to a nested appender.
        /// </para>
        /// </remarks>
        /// <param name="repository">The repository to shutdown.</param>
        public static void ShutdownRepository(string repository) 
        {
			cref.DebugFormat(">>> Entering ShutdownRepository(repository={0})", repository);
            LoggerManager.ShutdownRepository(repository);
			cref.DebugFormat("<<< Exiting ShutdownRepository(repository={0})", repository);
        }

        /// <summary>Shuts down the repository specified.</summary>
        /// <remarks>
        /// <para>
        /// Calling this method will <b>safely</b> close and remove all
        /// appenders in all the loggers including root contained in the
        /// repository. The repository is looked up using
        /// the <paramref name="repositoryAssembly"/> specified.
        /// </para>
        /// <para>
        /// Some appenders need to be closed before the application exists. 
        /// Otherwise, pending logging events might be lost.
        /// </para>
        /// <para>
        /// The <c>shutdown</c> method is careful to close nested
        /// appenders before closing regular appenders. This is allows
        /// configurations where a regular appender is attached to a logger
        /// and again to a nested appender.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        public static void ShutdownRepository(Assembly repositoryAssembly) 
        {
			cref.DebugFormat(">>> Entering ShutdownRepository(repositoryAssembly={0})", repositoryAssembly);
            LoggerManager.ShutdownRepository(repositoryAssembly);
			cref.DebugFormat("<<< Exiting ShutdownRepository(repositoryAssembly={0})", repositoryAssembly);
        }

#if !NETSTANDARD1_3
        /// <overloads>Reset the configuration of a repository</overloads>
        /// <summary>Resets all values contained in this repository instance to their defaults.</summary>
        /// <remarks>
        /// <para>
        /// Resets all values contained in the repository instance to their
        /// defaults.  This removes all appenders from all loggers, sets
        /// the level of all non-root loggers to <c>null</c>,
        /// sets their additivity flag to <c>true</c> and sets the level
        /// of the root logger to <see cref="Level.Debug"/>. Moreover,
        /// message disabling is set to its default "off" value.
        /// </para>		
        /// </remarks>
        public static void ResetConfiguration() 
        {
			cref.DebugFormat(">>> Entering ResetConfiguration()");
            ResetConfiguration(Assembly.GetCallingAssembly());
			cref.DebugFormat("<<< Exiting ResetConfiguration()");
        }
#endif

        /// <summary>Resets all values contained in this repository instance to their defaults.</summary>
        /// <remarks>
        /// <para>
        /// Reset all values contained in the repository instance to their
        /// defaults.  This removes all appenders from all loggers, sets
        /// the level of all non-root loggers to <c>null</c>,
        /// sets their additivity flag to <c>true</c> and sets the level
        /// of the root logger to <see cref="Level.Debug"/>. Moreover,
        /// message disabling is set to its default "off" value.
        /// </para>		
        /// </remarks>
        /// <param name="repository">The repository to reset.</param>
        public static void ResetConfiguration(string repository) 
        {
			cref.DebugFormat(">>> Entering ResetConfiguration(repository={0})", repository);
            LoggerManager.ResetConfiguration(repository);
			cref.DebugFormat("<<< Exiting ResetConfiguration(repository={0})", repository);
        }

        /// <summary>Resets all values contained in this repository instance to their defaults.</summary>
        /// <remarks>
        /// <para>
        /// Reset all values contained in the repository instance to their
        /// defaults.  This removes all appenders from all loggers, sets
        /// the level of all non-root loggers to <c>null</c>,
        /// sets their additivity flag to <c>true</c> and sets the level
        /// of the root logger to <see cref="Level.Debug"/>. Moreover,
        /// message disabling is set to its default "off" value.
        /// </para>		
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository to reset.</param>
        public static void ResetConfiguration(Assembly repositoryAssembly) 
        {
			cref.DebugFormat(">>> Entering ResetConfiguration(repositoryAssembly={0})", repositoryAssembly);
            LoggerManager.ResetConfiguration(repositoryAssembly);
			cref.DebugFormat("<<< Exiting ResetConfiguration(repositoryAssembly={0})", repositoryAssembly);
        }

#if !NETSTANDARD1_3
        /// <overloads>Get the logger repository.</overloads>
        /// <summary>Returns the default <see cref="ILoggerRepository"/> instance.</summary>
        /// <remarks>
        /// <para>
        /// Gets the <see cref="ILoggerRepository"/> for the repository specified
        /// by the callers assembly (<see cref="M:Assembly.GetCallingAssembly()"/>).
        /// </para>
        /// </remarks>
        /// <returns>The <see cref="ILoggerRepository"/> instance for the default repository.</returns>
        [Obsolete("Use GetRepository instead of GetLoggerRepository")]
        public static ILoggerRepository GetLoggerRepository()
        {
			cref.DebugFormat(">>> Entering GetLoggerRepository()");
			cref.DebugFormat("<<< Exiting GetLoggerRepository()");
            return GetRepository(Assembly.GetCallingAssembly());
        }
#endif

        /// <summary>Returns the default <see cref="ILoggerRepository"/> instance.</summary>
        /// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Gets the <see cref="ILoggerRepository"/> for the repository specified
        /// by the <paramref name="repository"/> argument.
        /// </para>
        /// </remarks>
        /// <param name="repository">The repository to lookup in.</param>
        [Obsolete("Use GetRepository instead of GetLoggerRepository")]
        public static ILoggerRepository GetLoggerRepository(string repository)
        {
			cref.DebugFormat("<<< Exiting GetLoggerRepository(repository={0})", repository);
            return GetRepository(repository);
        }

        /// <summary>Returns the default <see cref="ILoggerRepository"/> instance.</summary>
        /// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Gets the <see cref="ILoggerRepository"/> for the repository specified
        /// by the <paramref name="repositoryAssembly"/> argument.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        [Obsolete("Use GetRepository instead of GetLoggerRepository")]
        public static ILoggerRepository GetLoggerRepository(Assembly repositoryAssembly)
        {
			cref.DebugFormat("<<< Exiting GetLoggerRepository(repositoryAssembly={0})", repositoryAssembly);
            return GetRepository(repositoryAssembly);
        }

#if !NETSTANDARD1_3
        /// <overloads>Get a logger repository.</overloads>
        /// <summary>Returns the default <see cref="ILoggerRepository"/> instance.</summary>
        /// <remarks>
        /// <para>
        /// Gets the <see cref="ILoggerRepository"/> for the repository specified
        /// by the callers assembly (<see cref="M:Assembly.GetCallingAssembly()"/>).
        /// </para>
        /// </remarks>
        /// <returns>The <see cref="ILoggerRepository"/> instance for the default repository.</returns>
        public static ILoggerRepository GetRepository()
        {
			cref.DebugFormat(">>> Entering GetRepository()");
			cref.DebugFormat("<<< Exiting GetRepository()");
            return GetRepository(Assembly.GetCallingAssembly());
        }
#endif

        /// <summary>Returns the default <see cref="ILoggerRepository"/> instance.</summary>
        /// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Gets the <see cref="ILoggerRepository"/> for the repository specified
        /// by the <paramref name="repository"/> argument.
        /// </para>
        /// </remarks>
        /// <param name="repository">The repository to lookup in.</param>
        public static ILoggerRepository GetRepository(string repository)
        {
			cref.DebugFormat("<<< Exiting GetRepository(repository={0})", repository);
            return LoggerManager.GetRepository(repository);
        }

        /// <summary>Returns the default <see cref="ILoggerRepository"/> instance.</summary>
        /// <returns>The default <see cref="ILoggerRepository"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Gets the <see cref="ILoggerRepository"/> for the repository specified
        /// by the <paramref name="repositoryAssembly"/> argument.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to lookup the repository.</param>
        public static ILoggerRepository GetRepository(Assembly repositoryAssembly)
        {
			cref.DebugFormat("<<< Exiting GetRepository(repositoryAssembly={0})", repositoryAssembly);
            return LoggerManager.GetRepository(repositoryAssembly);
        }

#if !NETSTANDARD1_3
        /// <overloads>Create a domain</overloads>
        /// <summary>Creates a repository with the specified repository type.</summary>
        /// <remarks>
        /// <para>
        /// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
        /// </para>
        /// <para>
        /// The <see cref="ILoggerRepository"/> created will be associated with the repository
        /// specified such that a call to <see cref="M:GetRepository()"/> will return 
        /// the same repository instance.
        /// </para>
        /// </remarks>
        /// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
        /// and has a no arg constructor. An instance of this type will be created to act
        /// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        [Obsolete("Use CreateRepository instead of CreateDomain")]
        public static ILoggerRepository CreateDomain(Type repositoryType)
        {
			cref.DebugFormat(">>> Entering CreateDomain(repositoryType={0})", repositoryType);
			cref.DebugFormat("<<< Exiting CreateDomain(repositoryType={0})", repositoryType);
            return CreateRepository(Assembly.GetCallingAssembly(), repositoryType);
        }

        /// <overloads>Create a logger repository.</overloads>
        /// <summary>Creates a repository with the specified repository type.</summary>
        /// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
        /// and has a no arg constructor. An instance of this type will be created to act
        /// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="ILoggerRepository"/> created will be associated with the repository
        /// specified such that a call to <see cref="M:GetRepository()"/> will return 
        /// the same repository instance.
        /// </para>
        /// </remarks>
        public static ILoggerRepository CreateRepository(Type repositoryType)
        {
			cref.DebugFormat(">>> Entering CreateRepository(repositoryType={0})", repositoryType);
			cref.DebugFormat("<<< Exiting CreateRepository(repositoryType={0})", repositoryType);
            return CreateRepository(Assembly.GetCallingAssembly(), repositoryType);
        }
#endif

        /// <summary>Creates a repository with the specified name.</summary>
        /// <remarks>
        /// <para>
        /// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
        /// </para>
        /// <para>
        /// Creates the default type of <see cref="ILoggerRepository"/> which is a
        /// <see cref="log4net.Repository.Hierarchy.Hierarchy"/> object.
        /// </para>
        /// <para>
        /// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
        /// An <see cref="Exception"/> will be thrown if the repository already exists.
        /// </para>
        /// </remarks>
        /// <param name="repository">The name of the repository, this must be unique amongst repositories.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        /// <exception cref="LogException">The specified repository already exists.</exception>
        [Obsolete("Use CreateRepository instead of CreateDomain")]
        public static ILoggerRepository CreateDomain(string repository)
        {
			cref.DebugFormat("<<< Exiting CreateDomain(repository={0})", repository);
            return LoggerManager.CreateRepository(repository);
        }

        /// <summary>Creates a repository with the specified name.</summary>
        /// <remarks>
        /// <para>
        /// Creates the default type of <see cref="ILoggerRepository"/> which is a
        /// <see cref="log4net.Repository.Hierarchy.Hierarchy"/> object.
        /// </para>
        /// <para>
        /// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
        /// An <see cref="Exception"/> will be thrown if the repository already exists.
        /// </para>
        /// </remarks>
        /// <param name="repository">The name of the repository, this must be unique amongst repositories.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        /// <exception cref="LogException">The specified repository already exists.</exception>
        public static ILoggerRepository CreateRepository(string repository)
        {
			cref.DebugFormat("<<< Exiting CreateRepository(repository={0})", repository);
            return LoggerManager.CreateRepository(repository);
        }

        /// <summary>Creates a repository with the specified name and repository type.</summary>
        /// <remarks>
        /// <para>
        /// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
        /// </para>
        /// <para>
        /// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
        /// An <see cref="Exception"/> will be thrown if the repository already exists.
        /// </para>
        /// </remarks>
        /// <param name="repository">The name of the repository, this must be unique to the repository.</param>
        /// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
        /// and has a no arg constructor. An instance of this type will be created to act
        /// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        /// <exception cref="LogException">The specified repository already exists.</exception>
        [Obsolete("Use CreateRepository instead of CreateDomain")]
        public static ILoggerRepository CreateDomain(string repository, Type repositoryType)
        {
			cref.DebugFormat("<<< Exiting CreateDomain(repository={0},repositoryType={1})", repository, repositoryType);
            return LoggerManager.CreateRepository(repository, repositoryType);
        }

        /// <summary>Creates a repository with the specified name and repository type.</summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="repository"/> name must be unique. Repositories cannot be redefined.
        /// An <see cref="Exception"/> will be thrown if the repository already exists.
        /// </para>
        /// </remarks>
        /// <param name="repository">The name of the repository, this must be unique to the repository.</param>
        /// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
        /// and has a no arg constructor. An instance of this type will be created to act
        /// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        /// <exception cref="LogException">The specified repository already exists.</exception>
        public static ILoggerRepository CreateRepository(string repository, Type repositoryType)
        {
			cref.DebugFormat("<<< Exiting CreateRepository(repository={0},repositoryType={1})", repository, repositoryType);
            return LoggerManager.CreateRepository(repository, repositoryType);
        }

        /// <summary>Creates a repository for the specified assembly and repository type.</summary>
        /// <remarks>
        /// <para>
        /// <b>CreateDomain is obsolete. Use CreateRepository instead of CreateDomain.</b>
        /// </para>
        /// <para>
        /// The <see cref="ILoggerRepository"/> created will be associated with the repository
        /// specified such that a call to <see cref="M:GetRepository(Assembly)"/> with the
        /// same assembly specified will return the same repository instance.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to get the name of the repository.</param>
        /// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
        /// and has a no arg constructor. An instance of this type will be created to act
        /// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        [Obsolete("Use CreateRepository instead of CreateDomain")]
        public static ILoggerRepository CreateDomain(Assembly repositoryAssembly, Type repositoryType)
        {
			cref.DebugFormat("<<< Exiting CreateDomain(repositoryAssembly={0},repositoryType={1})", repositoryAssembly, repositoryType);
            return LoggerManager.CreateRepository(repositoryAssembly, repositoryType);
        }

        /// <summary>Creates a repository for the specified assembly and repository type.</summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ILoggerRepository"/> created will be associated with the repository
        /// specified such that a call to <see cref="M:GetRepository(Assembly)"/> with the
        /// same assembly specified will return the same repository instance.
        /// </para>
        /// </remarks>
        /// <param name="repositoryAssembly">The assembly to use to get the name of the repository.</param>
        /// <param name="repositoryType">A <see cref="Type"/> that implements <see cref="ILoggerRepository"/>
        /// and has a no arg constructor. An instance of this type will be created to act
        /// as the <see cref="ILoggerRepository"/> for the repository specified.</param>
        /// <returns>The <see cref="ILoggerRepository"/> created for the repository.</returns>
        public static ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
        {
			cref.DebugFormat("<<< Exiting CreateRepository(repositoryAssembly={0},repositoryType={1})", repositoryAssembly, repositoryType);
            return LoggerManager.CreateRepository(repositoryAssembly, repositoryType);
        }

        /// <summary>Gets the list of currently defined repositories.</summary>
        /// <remarks>
        /// <para>
        /// Get an array of all the <see cref="ILoggerRepository"/> objects that have been created.
        /// </para>
        /// </remarks>
        /// <returns>An array of all the known <see cref="ILoggerRepository"/> objects.</returns>
        public static ILoggerRepository[] GetAllRepositories()
        {
			cref.DebugFormat(">>> Entering GetAllRepositories()");
			cref.DebugFormat("<<< Exiting GetAllRepositories()");
            return LoggerManager.GetAllRepositories();
        }

            /// <summary>Flushes logging events buffered in all configured appenders in the default repository.</summary>
            /// <param name="millisecondsTimeout">The maximum time in milliseconds to wait for logging events from asycnhronous appenders to be flushed.</param>
            /// <returns><c>True</c> if all logging events were flushed successfully, else <c>false</c>.</returns>
            public static bool Flush(int millisecondsTimeout)
            {
			cref.DebugFormat(">>> Entering Flush(millisecondsTimeout={0})", millisecondsTimeout);
#if !NETSTANDARD1_3 // Excluded because GetCallingAssembly() is not available in CoreFX (https://github.com/dotnet/corefx/issues/2221).
                Appender.IFlushable flushableRepository = LoggerManager.GetRepository(Assembly.GetCallingAssembly()) as Appender.IFlushable;
                if (flushableRepository == null)
				cref.DebugFormat("Flush(millisecondsTimeout={0}): flushableRepository → {1}", millisecondsTimeout, flushableRepository);
                {
                    return false;
                }
                else
                {
                    return flushableRepository.Flush(millisecondsTimeout);
                }
#else
				cref.DebugFormat("<<< Exiting Flush(millisecondsTimeout={0})", millisecondsTimeout);
                return false;
#endif
            }

            /// <summary>Looks up the wrapper object for the logger specified.</summary>
        /// <param name="logger">The logger to get the wrapper for.</param>
        /// <returns>The wrapper for the logger specified.</returns>
        private static ILog WrapLogger(ILogger logger)
        {
			cref.DebugFormat(">>> Entering WrapLogger(logger={0})", logger);
			cref.DebugFormat("<<< Exiting WrapLogger(logger={0})", logger);
            return (ILog)s_wrapperMap.GetWrapper(logger);
        }

        /// <summary>Looks up the wrapper objects for the loggers specified.</summary>
        /// <param name="loggers">The loggers to get the wrappers for.</param>
        /// <returns>The wrapper objects for the loggers specified.</returns>
        private static ILog[] WrapLoggers(ILogger[] loggers)
        {
			cref.DebugFormat(">>> Entering WrapLoggers(loggers={0})", loggers);
            ILog[] results = new ILog[loggers.Length];
            for(int i = 0; i < loggers.Length; i++)
            {
                results[i] = WrapLogger(loggers[i]);
            }
			cref.DebugFormat("<<< Exiting WrapLoggers(loggers={0})", loggers);
            return results;
        }

        /// <summary>
        /// Create the <see cref="ILoggerWrapper"/> objects used by
        /// this manager.
        /// </summary>
        /// <param name="logger">The logger to wrap.</param>
        /// <returns>The wrapper for the logger specified.</returns>
        private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
        {
			cref.DebugFormat(">>> Entering WrapperCreationHandler(logger={0})", logger);
			cref.DebugFormat("<<< Exiting WrapperCreationHandler(logger={0})", logger);
            return new LogImpl(logger);
        }

        /// <summary>The wrapper map to use to hold the <see cref="LogImpl"/> objects.</summary>
        private static readonly WrapperMap s_wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandler));
    }
}
