// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Utilities.Mocks
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
	using Microsoft.Extensions.Logging;

    /// <summary>A mock <see cref="ICBO"/> implementation that implements <see cref="GetCachedObject{T}"/> by always calling the callback function (i.e. never caching).</summary>
    public class MockCBO : ICBO
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public System.Collections.Generic.List<TObject> FillCollection<TObject>(IDataReader dr)
            where TObject : new()
        {
			_logger.LogDebug(">>> Entering FillCollection(dr={Dr})", dr);
			_logger.LogDebug("<<< Exiting FillCollection(dr={Dr})", dr);
			_logger.LogWarning("Throwing NotImplementedException from FillCollection(dr={Dr})", dr);
            throw new NotImplementedException();
        }

        public TObject FillObject<TObject>(IDataReader dr)
            where TObject : new()
        {
			_logger.LogDebug(">>> Entering FillObject(dr={Dr})", dr);
			_logger.LogDebug("<<< Exiting FillObject(dr={Dr})", dr);
			_logger.LogWarning("Throwing NotImplementedException from FillObject(dr={Dr})", dr);
            throw new NotImplementedException();
        }

        public TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
			_logger.LogDebug(">>> Entering GetCachedObject(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired},saveInDictionary={SaveInDictionary})", cacheItemArgs, cacheItemExpired, saveInDictionary);
			_logger.LogDebug("<<< Exiting GetCachedObject(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired},saveInDictionary={SaveInDictionary})", cacheItemArgs, cacheItemExpired, saveInDictionary);
            return (TObject)cacheItemExpired(cacheItemArgs);
        }
    }
}
