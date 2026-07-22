// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;

    using Microsoft.Extensions.DependencyInjection;

    public abstract class RepositoryBase<T> : IRepository<T>
        where T : class
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RepositoryBase{T}"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        protected RepositoryBase()
            : this(null)
        {
		Logger.LogDebug(">>> Entering RepositoryBase()");
		Logger.LogDebug("<<< Exiting RepositoryBase()");
		Logger.LogDebug(">>> Entering RepositoryBase(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("<<< Exiting RepositoryBase(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="RepositoryBase{T}"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        protected RepositoryBase(IHostSettings hostSettings)
        {
            this.HostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.InitializeInternal();
        }

        protected IHostSettings HostSettings { get; }

        protected CacheItemArgs CacheArgs { get; private set; }

        protected string Scope { get; private set; }

        protected bool IsCacheable { get; private set; }

        protected bool IsScoped { get; private set; }

        /// <inheritdoc />
        public void Delete(T item)
        {
			Logger.LogDebug(">>> Entering Delete(item={Item})", item);
            this.DeleteInternal(item);
            this.ClearCache(item);
			Logger.LogDebug("<<< Exiting Delete(item={Item})", item);
        }

        /// <inheritdoc />
        public abstract void Delete(string sqlCondition, params object[] args);

        /// <inheritdoc />
        public abstract IEnumerable<T> Find(string sqlCondition, params object[] args);

        /// <inheritdoc />
        public abstract IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args);

        /// <inheritdoc />
        public IEnumerable<T> Get()
        {
			Logger.LogDebug(">>> Entering Get()");
			Logger.LogDebug("<<< Exiting Get()");
            return this.IsCacheable && !this.IsScoped
                ? DataCache.GetCachedData<IEnumerable<T>>(this.HostSettings, this.CacheArgs, _ => this.GetInternal())
                : this.GetInternal();
        }

        /// <inheritdoc />
        public IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            this.CheckIfScoped();

            if (this.IsCacheable)
            {
                this.CacheArgs.CacheKey = string.Format(CultureInfo.InvariantCulture, this.CacheArgs.CacheKey, scopeValue);
            }

            return this.IsCacheable
                ? DataCache.GetCachedData<IEnumerable<T>>(this.HostSettings, this.CacheArgs, _ => this.GetByScopeInternal(scopeValue))
                : this.GetByScopeInternal(scopeValue);
        }

        /// <inheritdoc />
        public T GetById<TProperty>(TProperty id)
        {
			Logger.LogDebug(">>> Entering GetById(id={Id})", id);
			Logger.LogDebug("<<< Exiting GetById(id={Id})", id);
            return this.IsCacheable && !this.IsScoped
                        ? this.Get().SingleOrDefault(t => this.CompareTo(this.GetPrimaryKey<TProperty>(t), id) == 0)
                        : this.GetByIdInternal(id);
        }

        /// <inheritdoc />
        public T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            this.CheckIfScoped();

            return this.Get(scopeValue).SingleOrDefault(t => this.CompareTo(this.GetPrimaryKey<TProperty>(t), id) == 0);
        }

        /// <inheritdoc />
        public IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetPage(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
			Logger.LogDebug("<<< Exiting GetPage(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            return this.IsCacheable && !this.IsScoped
                ? this.Get().InPagesOf(pageSize).GetPage(pageIndex)
                : this.GetPageInternal(pageIndex, pageSize);
        }

        /// <inheritdoc />
        public IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            this.CheckIfScoped();

            return this.IsCacheable
                ? this.Get(scopeValue).InPagesOf(pageSize).GetPage(pageIndex)
                : this.GetPageByScopeInternal(scopeValue, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public void Insert(T item)
        {
			Logger.LogDebug(">>> Entering Insert(item={Item})", item);
            this.InsertInternal(item);
            this.ClearCache(item);
			Logger.LogDebug("<<< Exiting Insert(item={Item})", item);
        }

        /// <inheritdoc />
        public void Update(T item)
        {
			Logger.LogDebug(">>> Entering Update(item={Item})", item);
            this.UpdateInternal(item);
            this.ClearCache(item);
			Logger.LogDebug("<<< Exiting Update(item={Item})", item);
        }

        /// <inheritdoc />
        public abstract void Update(string sqlCondition, params object[] args);

        public void Initialize(string cacheKey, int cacheTimeOut = 20, CacheItemPriority cachePriority = CacheItemPriority.Default, string scope = "")
        {
            this.Scope = scope;
            this.IsScoped = !string.IsNullOrEmpty(this.Scope);
            this.IsCacheable = !string.IsNullOrEmpty(cacheKey);
            if (this.IsCacheable)
            {
                if (this.IsScoped)
                {
                    cacheKey += $"_{this.Scope}_{{0}}";
                }

                this.CacheArgs = new CacheItemArgs(cacheKey, cacheTimeOut, cachePriority);
            }
            else
            {
                this.CacheArgs = null;
            }
        }

        protected int CompareTo<TProperty>(TProperty first, TProperty second)
        {
			Logger.LogDebug(">>> Entering CompareTo(first={First},second={Second})", first, second);
            Requires.IsTypeOf<IComparable>("first", first);
            Requires.IsTypeOf<IComparable>("second", second);

            var firstComparable = first as IComparable;
            var secondComparable = second as IComparable;

            // ReSharper disable PossibleNullReferenceException
            return firstComparable.CompareTo(secondComparable);

            // ReSharper restore PossibleNullReferenceException
        }

        protected TProperty GetPropertyValue<TProperty>(T item, string propertyName)
        {
			Logger.LogDebug(">>> Entering GetPropertyValue(item={Item},propertyName={PropertyName})", item, propertyName);
            return DataUtil.GetPropertyValue<T, TProperty>(item, propertyName);
        }

        protected TProperty GetPrimaryKey<TProperty>(T item)
        {
			Logger.LogDebug(">>> Entering GetPrimaryKey(item={Item})", item);
            return DataUtil.GetPrimaryKey<T, TProperty>(item);
        }

        protected TProperty GetScopeValue<TProperty>(T item)
        {
			Logger.LogDebug(">>> Entering GetScopeValue(item={Item})", item);
            return DataUtil.GetPropertyValue<T, TProperty>(item, this.Scope);
        }

        protected abstract void DeleteInternal(T item);

        protected abstract IEnumerable<T> GetInternal();

        protected abstract IPagedList<T> GetPageInternal(int pageIndex, int pageSize);

        protected abstract IEnumerable<T> GetByScopeInternal(object propertyValue);

        protected abstract IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize);

        protected abstract T GetByIdInternal(object id);

        protected abstract void InsertInternal(T item);

        protected abstract void UpdateInternal(T item);

        private void CheckIfScoped()
        {
			Logger.LogDebug(">>> Entering CheckIfScoped()");
            if (!this.IsScoped)
            {
                throw new NotSupportedException("This method requires the model to be cacheable and have a cache scope defined");
            }
			Logger.LogDebug("<<< Exiting CheckIfScoped()");
        }

        private void ClearCache(T item)
        {
			Logger.LogDebug(">>> Entering ClearCache(item={Item})", item);
            if (this.IsCacheable)
            {
                DataCache.RemoveCache(this.IsScoped
                                          ? string.Format(CultureInfo.InvariantCulture, this.CacheArgs.CacheKey, this.GetScopeValue<object>(item))
                                          : this.CacheArgs.CacheKey);
            }
			Logger.LogDebug("<<< Exiting ClearCache(item={Item})", item);
        }

        private void InitializeInternal()
        {
			Logger.LogDebug(">>> Entering InitializeInternal()");
            var type = typeof(T);
            this.Scope = string.Empty;
			Logger.LogDebug("InitializeInternal(): type → {Type}", type);
			Logger.LogDebug("InitializeInternal(): this.Scope → {Arg0}", this.Scope);
			Logger.LogDebug("InitializeInternal(): this.IsScoped → {Arg0}", this.IsScoped);
			Logger.LogDebug("InitializeInternal(): this.CacheArgs → {Arg0}", this.CacheArgs);
            this.IsCacheable = false;
            this.IsScoped = false;
            this.CacheArgs = null;
			Logger.LogDebug("InitializeInternal(): this.IsCacheable → {Arg0}", this.IsCacheable);

            var scopeAttribute = DataUtil.GetAttribute<ScopeAttribute>(type);
            if (scopeAttribute != null)
			Logger.LogDebug("InitializeInternal(): scopeAttribute → {ScopeAttribute}", scopeAttribute);
            {
                this.Scope = scopeAttribute.Scope;
            }

            this.IsScoped = !string.IsNullOrEmpty(this.Scope);

            var cacheableAttribute = DataUtil.GetAttribute<CacheableAttribute>(type);
            if (cacheableAttribute != null)
			Logger.LogDebug("InitializeInternal(): cacheableAttribute → {CacheableAttribute}", cacheableAttribute);
            {
                this.IsCacheable = true;
                var cacheKey = !string.IsNullOrEmpty(cacheableAttribute.CacheKey)
                                ? cacheableAttribute.CacheKey
                                : $"OR_{type.Name}";
                var cachePriority = cacheableAttribute.CachePriority;
                var cacheTimeOut = cacheableAttribute.CacheTimeOut;

                if (this.IsScoped)
                {
                    cacheKey += $"_{this.Scope}_{{0}}";
                }

                this.CacheArgs = new CacheItemArgs(cacheKey, cacheTimeOut, cachePriority);
            }
			Logger.LogDebug("<<< Exiting InitializeInternal()");
        }
    }
}
