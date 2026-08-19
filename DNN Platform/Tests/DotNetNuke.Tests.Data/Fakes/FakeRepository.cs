// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;

    using DotNetNuke.Collections;
    using DotNetNuke.Data;
	using Microsoft.Extensions.Logging;

    public class FakeRepository<T> : RepositoryBase<T>
        where T : class
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public override void Delete(string sqlCondition, params object[] args)
        {
			_logger.LogDebug(">>> Entering Delete(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			_logger.LogWarning("Throwing NotImplementedException from Delete(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			_logger.LogDebug("<<< Exiting Delete(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            throw new System.NotImplementedException();
        }

        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
			_logger.LogDebug(">>> Entering Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			_logger.LogWarning("Throwing NotImplementedException from Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			_logger.LogDebug("<<< Exiting Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            throw new System.NotImplementedException();
        }

        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
			_logger.LogDebug(">>> Entering Find(pageIndex={PageIndex},pageSize={PageSize},sqlCondition={SqlCondition},args={Args})", pageIndex, pageSize, sqlCondition, args);
			_logger.LogWarning("Throwing NotImplementedException from Find(pageIndex={PageIndex},pageSize={PageSize},sqlCondition={SqlCondition},args={Args})", pageIndex, pageSize, sqlCondition, args);
			_logger.LogDebug("<<< Exiting Find(pageIndex={PageIndex},pageSize={PageSize},sqlCondition={SqlCondition},args={Args})", pageIndex, pageSize, sqlCondition, args);
            throw new System.NotImplementedException();
        }

        public override void Update(string sqlCondition, params object[] args)
        {
			_logger.LogDebug(">>> Entering Update(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			_logger.LogWarning("Throwing NotImplementedException from Update(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			_logger.LogDebug("<<< Exiting Update(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            throw new System.NotImplementedException();
        }

        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
			_logger.LogDebug(">>> Entering GetPageByScopeInternal(propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize})", propertyValue, pageIndex, pageSize);
			_logger.LogWarning("Throwing NotImplementedException from GetPageByScopeInternal(propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize})", propertyValue, pageIndex, pageSize);
			_logger.LogDebug("<<< Exiting GetPageByScopeInternal(propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize})", propertyValue, pageIndex, pageSize);
            throw new System.NotImplementedException();
        }

        protected override T GetByIdInternal(object id)
        {
			_logger.LogDebug(">>> Entering GetByIdInternal(id={Id})", id);
			_logger.LogWarning("Throwing NotImplementedException from GetByIdInternal(id={Id})", id);
			_logger.LogDebug("<<< Exiting GetByIdInternal(id={Id})", id);
            throw new System.NotImplementedException();
        }

        protected override void InsertInternal(T item)
        {
			_logger.LogDebug(">>> Entering InsertInternal(item={Item})", item);
			_logger.LogWarning("Throwing NotImplementedException from InsertInternal(item={Item})", item);
			_logger.LogDebug("<<< Exiting InsertInternal(item={Item})", item);
            throw new System.NotImplementedException();
        }

        protected override void DeleteInternal(T item)
        {
			_logger.LogDebug(">>> Entering DeleteInternal(item={Item})", item);
			_logger.LogWarning("Throwing NotImplementedException from DeleteInternal(item={Item})", item);
			_logger.LogDebug("<<< Exiting DeleteInternal(item={Item})", item);
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<T> GetInternal()
        {
			_logger.LogDebug(">>> Entering GetInternal()");
			_logger.LogWarning("Throwing NotImplementedException from GetInternal()");
			_logger.LogDebug("<<< Exiting GetInternal()");
            throw new System.NotImplementedException();
        }

        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
			_logger.LogDebug(">>> Entering GetPageInternal(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
			_logger.LogWarning("Throwing NotImplementedException from GetPageInternal(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
			_logger.LogDebug("<<< Exiting GetPageInternal(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
			_logger.LogDebug(">>> Entering GetByScopeInternal(propertyValue={PropertyValue})", propertyValue);
			_logger.LogWarning("Throwing NotImplementedException from GetByScopeInternal(propertyValue={PropertyValue})", propertyValue);
			_logger.LogDebug("<<< Exiting GetByScopeInternal(propertyValue={PropertyValue})", propertyValue);
            throw new System.NotImplementedException();
        }

        protected override void UpdateInternal(T item)
        {
			_logger.LogDebug(">>> Entering UpdateInternal(item={Item})", item);
			_logger.LogWarning("Throwing NotImplementedException from UpdateInternal(item={Item})", item);
			_logger.LogDebug("<<< Exiting UpdateInternal(item={Item})", item);
            throw new System.NotImplementedException();
        }
    }
}
