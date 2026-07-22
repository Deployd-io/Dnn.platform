// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Utilities.Mocks
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;

    /// <summary>A mock <see cref="ICBO"/> implementation that implements <see cref="GetCachedObject{T}"/> by always calling the callback function (i.e. never caching).</summary>
    public class MockCBO : ICBO
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public System.Collections.Generic.List<TObject> FillCollection<TObject>(IDataReader dr)
            where TObject : new()
        {
			Logger.LogDebug(">>> Entering FillCollection(dr={Dr})", dr);
            throw new NotImplementedException();
        }

        public TObject FillObject<TObject>(IDataReader dr)
            where TObject : new()
        {
			Logger.LogDebug(">>> Entering FillObject(dr={Dr})", dr);
            throw new NotImplementedException();
        }

        public TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary)
        {
			Logger.LogDebug(">>> Entering GetCachedObject(cacheItemArgs={CacheItemArgs},cacheItemExpired={CacheItemExpired},saveInDictionary={SaveInDictionary})", cacheItemArgs, cacheItemExpired, saveInDictionary);
            return (TObject)cacheItemExpired(cacheItemArgs);
        }
    }
}
