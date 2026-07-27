// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Fakes
{
    using System.Collections.Generic;

    using DotNetNuke.Collections;
    using DotNetNuke.Data;

    public class FakeRepository<T> : RepositoryBase<T>
        where T : class
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public override void Delete(string sqlCondition, params object[] args)
        {
			Logger.LogDebug(">>> Entering Delete(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            throw new System.NotImplementedException();
        }

        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
			Logger.LogDebug(">>> Entering Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            throw new System.NotImplementedException();
        }

        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
			Logger.LogDebug(">>> Entering Find(pageIndex={PageIndex},pageSize={PageSize},sqlCondition={SqlCondition},args={Args})", pageIndex, pageSize, sqlCondition, args);
            throw new System.NotImplementedException();
        }

        public override void Update(string sqlCondition, params object[] args)
        {
			Logger.LogDebug(">>> Entering Update(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            throw new System.NotImplementedException();
        }

        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetPageByScopeInternal(propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize})", propertyValue, pageIndex, pageSize);
            throw new System.NotImplementedException();
        }

        protected override T GetByIdInternal(object id)
        {
			Logger.LogDebug(">>> Entering GetByIdInternal(id={Id})", id);
            throw new System.NotImplementedException();
        }

        protected override void InsertInternal(T item)
        {
			Logger.LogDebug(">>> Entering InsertInternal(item={Item})", item);
            throw new System.NotImplementedException();
        }

        protected override void DeleteInternal(T item)
        {
			Logger.LogDebug(">>> Entering DeleteInternal(item={Item})", item);
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<T> GetInternal()
        {
			Logger.LogDebug(">>> Entering GetInternal()");
            throw new System.NotImplementedException();
        }

        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetPageInternal(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
			Logger.LogDebug(">>> Entering GetByScopeInternal(propertyValue={PropertyValue})", propertyValue);
            throw new System.NotImplementedException();
        }

        protected override void UpdateInternal(T item)
        {
			Logger.LogDebug(">>> Entering UpdateInternal(item={Item})", item);
            throw new System.NotImplementedException();
        }
    }
}
