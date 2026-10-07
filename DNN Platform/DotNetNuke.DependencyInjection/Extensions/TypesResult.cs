// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

/// <summary>The result of a request to get types from assemblies.</summary>
public class TypesResult
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <summary>Initializes a new instance of the <see cref="TypesResult"/> class.</summary>
    /// <param name="types">The types.</param>
    /// <param name="loadExceptions">The <see cref="ReflectionTypeLoadException"/> instances that were occurred while loading the assemblies.</param>
    /// <param name="otherExceptions">The other exceptions that occurred while loading the assemblies.</param>
    public TypesResult(IReadOnlyCollection<Type> types, IReadOnlyDictionary<Assembly, ReflectionTypeLoadException> loadExceptions, IReadOnlyDictionary<Assembly, Exception> otherExceptions)
    {
		_logger.LogError(">>> Entering TypesResult(types={Types},loadExceptions={LoadExceptions},otherExceptions={OtherExceptions})", types, loadExceptions, otherExceptions);
        this.Types = types;
		_logger.LogDebug("TypesResult(types={Types},loadExceptions={LoadExceptions},otherExceptions={OtherExceptions}): this.Types → {Arg3}", types, loadExceptions, otherExceptions, this.Types);
        this.LoadExceptions = loadExceptions;
        this.OtherExceptions = otherExceptions;
    }

    /// <summary>Gets the types.</summary>
    public IReadOnlyCollection<Type> Types { get; }

    /// <summary>Gets the <see cref="ReflectionTypeLoadException"/> instances that were occurred while loading the assemblies.</summary>
    public IReadOnlyDictionary<Assembly, ReflectionTypeLoadException> LoadExceptions { get; }

    /// <summary>Gets any other exceptions that occurred while loading the assemblies.</summary>
    public IReadOnlyDictionary<Assembly, Exception> OtherExceptions { get; }
}
