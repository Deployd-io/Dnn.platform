// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
	using Microsoft.Extensions.Logging;

    /// <summary><see cref="Type"/> specific extensions to be used in Dependency Injection invocations.</summary>
    public static partial class TypeExtensions
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Safely Get all Types from the assembly. If there is an error while retrieving the types it will return an empty array of <see cref="Type"/>.</summary>
        /// <param name="assembly">The assembly to retrieve all types from.</param>
        /// <returns>An array of all <see cref="Type"/> in the given <see cref="Assembly"/>.</returns>
        /// <remarks>This is obsolete because logging is not added. Please use the SafeGetTypes with the ILog parameter.</remarks>
        [DnnDeprecated(9, 9, 0, "Please use the SafeGetTypes overload with the ILog parameter")]
        public static partial Type[] SafeGetTypes(this Assembly assembly)
        {
			_logger.LogError(">>> Entering SafeGetTypes(assembly={Assembly})", assembly);
			_logger.LogWarning("<<< Exiting SafeGetTypes(assembly={Assembly})", assembly);
            return assembly.SafeGetTypes(null);
        }

        /// <summary>Safely Get all Types from the assembly. If there is an error while retrieving the types it will return an empty array of <see cref="Type"/>.</summary>
        /// <param name="assembly">The assembly to retrieve all types from.</param>
        /// <param name="logger">A optional <see cref="ILog"/> object. This will log issues loading the types.</param>
        /// <returns>An array of all <see cref="Type"/> in the given <see cref="Assembly"/>.</returns>
        public static Type[] SafeGetTypes(this Assembly assembly, ILog logger)
        {
            var (types, exception) = assembly.GetTypesAndException();
            if (logger is null || exception is null)
            {
                return types.ToArray();
            }

            if (exception is ReflectionTypeLoadException loadException)
            {
                var messageBuilder = BuildLoaderExceptionMessage(
                    new StringBuilder($"Unable to get all types for {assembly.FullName}, see exception for details").AppendLine(),
                    assembly,
                    loadException);

                logger.Warn(messageBuilder.ToString(), loadException);
            }
            else
            {
                logger.Error($"Unable to get any types for {assembly.FullName}, see exception for details", exception);
            }

            return types.ToArray();
        }

        /// <summary>Safely get all types from all assemblies.</summary>
        /// <returns>A sequence of all available <see cref="Type"/> instances, along with errors and warnings.</returns>
        public static TypesResult SafeGetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(DotNetNukeFirstThenDnnThenOthers)
                .ThenBy(assembly => assembly.FullName)
                .SafeGetTypes();

            static int DotNetNukeFirstThenDnnThenOthers(Assembly assembly)
            {
				_logger.LogError(">>> Entering DotNetNukeFirstThenDnnThenOthers(assembly={Assembly})", assembly);
                if (assembly.FullName.StartsWith("DotNetNuke", StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }

                if (assembly.FullName.StartsWith("DNN", StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }

				_logger.LogWarning("<<< Exiting DotNetNukeFirstThenDnnThenOthers(assembly={Assembly})", assembly);
                return 2;
            }
        }

        /// <summary>Safely get all types from a set of assemblies.</summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>A sequence of all available <see cref="Type"/> instances, along with errors and warnings.</returns>
        public static TypesResult SafeGetTypes(this IEnumerable<Assembly> assemblies)
        {
            var loadExceptions = new Dictionary<Assembly, ReflectionTypeLoadException>();
            var otherExceptions = new Dictionary<Assembly, Exception>();
            var types = assemblies.SafeGetTypes(loadExceptions, otherExceptions);
			_logger.LogError("SafeGetTypes(assemblies={Assemblies}): loadExceptions → {LoadExceptions}", assemblies, loadExceptions);
            return new TypesResult(types.ToList(), loadExceptions, otherExceptions);
        }

        /// <summary>Logs the exceptions in <see cref="TypesResult.OtherExceptions"/> of <paramref name="result" /> using <paramref name="logger"/>.</summary>
        /// <param name="result">The types result to log.</param>
        /// <param name="logger">The logger.</param>
        public static void LogOtherExceptions(this TypesResult result, ILog logger)
        {
			_logger.LogError(">>> Entering LogOtherExceptions(result={Result},logger={Logger})", result, logger);
            if (logger is null)
            {
                return;
            }

            foreach (var exceptionPair in result.OtherExceptions)
            {
                var assembly = exceptionPair.Key;
                var exception = exceptionPair.Value;
                logger.Error($"Unable to get any types for {assembly.FullName}, see exception for details", exception);
            }
			_logger.LogWarning("<<< Exiting LogOtherExceptions(result={Result},logger={Logger})", result, logger);
        }

        /// <summary>Builds a message to load for a collection of <see cref="ReflectionTypeLoadException"/> instances.</summary>
        /// <param name="loadExceptions">A dictionary mapping <see cref="Assembly"/> to <see cref="ReflectionTypeLoadException"/>.</param>
        /// <returns>A <see cref="StringBuilder"/> containing the message.</returns>
        public static StringBuilder BuildLoaderExceptionsMessage(this IReadOnlyDictionary<Assembly, ReflectionTypeLoadException> loadExceptions)
        {
			_logger.LogError(">>> Entering BuildLoaderExceptionsMessage(loadExceptions={LoadExceptions})", loadExceptions);
            return
                loadExceptions.Aggregate(
                    new StringBuilder("Unable to get all types for some assemblies:").AppendLine(),
                    (builder, exceptionPair) =>
                    {
                        var assembly = exceptionPair.Key;
                        var loadException = exceptionPair.Value;
					_logger.LogWarning("<<< Exiting BuildLoaderExceptionsMessage(loadExceptions={LoadExceptions})", loadExceptions);
                        return BuildLoaderExceptionMessage(builder, assembly, loadException).AppendLine();
                    });
        }

        private static StringBuilder BuildLoaderExceptionMessage(StringBuilder builder, Assembly assembly, ReflectionTypeLoadException loadException)
        {
			_logger.LogError(">>> Entering BuildLoaderExceptionMessage(builder={Builder},assembly={Assembly},loadException={LoadException})", builder, assembly, loadException);
            var foundTypes = loadException.Types.Count(x => x != null);
            var allTypes = loadException.Types.Length;
			_logger.LogError("BuildLoaderExceptionMessage(builder={Builder},assembly={Assembly},loadException={LoadException}): foundTypes → {FoundTypes}", builder, assembly, loadException, foundTypes);
            var loaderMessages = loadException.LoaderExceptions.Select(e => e.Message).Distinct();
			_logger.LogError("BuildLoaderExceptionMessage(builder={Builder},assembly={Assembly},loadException={LoadException}): loaderMessages → {LoaderMessages}", builder, assembly, loadException, loaderMessages);

			_logger.LogWarning("<<< Exiting BuildLoaderExceptionMessage(builder={Builder},assembly={Assembly},loadException={LoadException})", builder, assembly, loadException);
            return loaderMessages.Aggregate(
                builder.AppendLine($"- Found {foundTypes} of {allTypes} types from {assembly.FullName}"),
                (theBuilder, message) => theBuilder.AppendLine($"    - {message}"));
        }

        private static IEnumerable<Type> SafeGetTypes(this IEnumerable<Assembly> assemblies, Dictionary<Assembly, ReflectionTypeLoadException> loadExceptions, Dictionary<Assembly, Exception> otherExceptions)
        {
            return assemblies.SelectMany(assembly =>
            {
                var (types, exception) = assembly.GetTypesAndException();
                if (exception is ReflectionTypeLoadException loadException)
                {
                    loadExceptions.Add(assembly, loadException);
                }
                else if (exception is not null)
                {
                    otherExceptions.Add(assembly, exception);
                }

                return types;
            });
        }

        private static (IEnumerable<Type> Types, Exception Exception) GetTypesAndException(this Assembly assembly)
        {
			_logger.LogError(">>> Entering GetTypesAndException(assembly={Assembly})", assembly);
            Exception exception = null;
            IEnumerable<Type> types;
			_logger.LogError("GetTypesAndException(assembly={Assembly}): exception → {Exception}", assembly, exception);
            try
            {
                types = assembly.GetTypes();
				_logger.LogError("GetTypesAndException(assembly={Assembly}): types → {Types}", assembly, types);
            }
            catch (ReflectionTypeLoadException ex)
            {
				_logger.LogError("Exception in GetTypesAndException(assembly={Assembly}): {Arg1}", assembly, ex.Message);
                exception = ex;

                // Ensure that DNN obtains all types that were loaded, ignoring the failure(s)
                types = ex.Types.Where(x => x != null);
            }
            catch (Exception ex)
            {
                exception = ex;
                types = Enumerable.Empty<Type>();
            }

			_logger.LogWarning("<<< Exiting GetTypesAndException(assembly={Assembly})", assembly);
            return (types.OrderBy(type => type.FullName ?? type.Name), exception);
        }
    }
}
