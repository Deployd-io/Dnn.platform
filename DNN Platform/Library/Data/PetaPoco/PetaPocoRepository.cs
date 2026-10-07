// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using global::PetaPoco;

    using Microsoft.Extensions.DependencyInjection;

    public class PetaPocoRepository<T> : RepositoryBase<T>
        where T : class
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PetaPocoRepository));
        private readonly Database database;
        private readonly IMapper mapper;

        /// <summary>Initializes a new instance of the <see cref="PetaPocoRepository{T}"/> class.</summary>
        /// <param name="database">The database.</param>
        /// <param name="mapper">The mapper.</param>
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoRepository(Database database, IMapper mapper)
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), database, mapper)
        {
		Logger.LogError(">>> Entering PetaPocoRepository(database={Database},mapper={Mapper})", database, mapper);
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoRepository{T}"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="database">The database.</param>
        /// <param name="mapper">The mapper.</param>
        public PetaPocoRepository(IHostSettings hostSettings, Database database, IMapper mapper)
		Logger.LogWarning("<<< Exiting PetaPocoRepository(database={Database},mapper={Mapper})", database, mapper);
            : base(hostSettings)
#pragma warning restore CS3001
        {
			Logger.LogDebug("PetaPocoRepository(hostSettings={HostSettings},database={Database},mapper={Mapper}): database → {Database1}", hostSettings, database, mapper, database);
			Logger.LogDebug("PetaPocoRepository(hostSettings={HostSettings},database={Database},mapper={Mapper}): mapper → {Mapper1}", hostSettings, database, mapper, mapper);
            Requires.NotNull("database", database);

            this.database = database;
            this.mapper = mapper;
			Logger.LogDebug("PetaPocoRepository(hostSettings={HostSettings},database={Database},mapper={Mapper}): this.database → {Arg3}", hostSettings, database, mapper, this.database);

            PetaPocoMapper.SetMapper<T>(mapper);
			Logger.LogDebug("PetaPocoRepository(hostSettings={HostSettings},database={Database},mapper={Mapper}): this.mapper → {Arg3}", hostSettings, database, mapper, this.mapper);
        }

        /// <inheritdoc />
        public override void Delete(string sqlCondition, params object[] args)
        {
			Logger.LogError(">>> Entering Delete(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            this.database.Delete<T>(DataUtil.ReplaceTokens(sqlCondition), args);
			Logger.LogWarning("<<< Exiting Delete(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
        }

        /// <inheritdoc />
        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
			Logger.LogError(">>> Entering Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			Logger.LogWarning("<<< Exiting Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
			Logger.LogWarning("<<< Exiting Find(pageIndex={PageIndex},pageSize={PageSize},sqlCondition={SqlCondition},args={Args})", pageIndex, pageSize, sqlCondition, args);
            return this.database.Fetch<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc />
        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            // Make sure that the sql Condition contains an ORDER BY Clause
            if (!sqlCondition.ToUpperInvariant().Contains("ORDER BY"))
            {
                sqlCondition = $"{sqlCondition} ORDER BY {this.mapper.GetTableInfo(typeof(T)).PrimaryKey}";
            }

            Page<T> petaPocoPage = this.database.Page<T>(pageIndex + 1, pageSize, DataUtil.ReplaceTokens(sqlCondition), args);

            return new PagedList<T>(petaPocoPage.Items, (int)petaPocoPage.TotalItems, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public override void Update(string sqlCondition, params object[] args)
        {
			Logger.LogError(">>> Entering Update(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            this.database.Update<T>(DataUtil.ReplaceTokens(sqlCondition), args);
			Logger.LogWarning("<<< Exiting Update(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
        }

        /// <inheritdoc />
        protected override void DeleteInternal(T item)
        {
			Logger.LogError(">>> Entering DeleteInternal(item={Item})", item);
            this.database.Delete(item);
			Logger.LogWarning("<<< Exiting DeleteInternal(item={Item})", item);
        }

        /// <inheritdoc />
        protected override IEnumerable<T> GetInternal()
        {
			Logger.LogError(">>> Entering GetInternal()");
            Logger.LogWarning("<<< Exiting GetInternal()");
            return this.database.Fetch<T>(string.Empty);
        }

        /// <inheritdoc />
        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering GetPageInternal(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            Logger.LogWarning("<<< Exiting GetPageInternal(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            return this.Find(pageIndex, pageSize, string.Empty);
        }

        /// <inheritdoc />
        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
			Logger.LogError(">>> Entering GetByScopeInternal(propertyValue={PropertyValue})", propertyValue);
            Logger.LogWarning("<<< Exiting GetByScopeInternal(propertyValue={PropertyValue})", propertyValue);
            return this.database.Fetch<T>(this.GetScopeSql(), propertyValue);
        }

        /// <inheritdoc />
        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering GetPageByScopeInternal(propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize})", propertyValue, pageIndex, pageSize);
            Logger.LogWarning("<<< Exiting GetPageByScopeInternal(propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize})", propertyValue, pageIndex, pageSize);
            return this.Find(pageIndex, pageSize, this.GetScopeSql(), propertyValue);
        }

        /// <inheritdoc />
        protected override T GetByIdInternal(object id)
        {
			Logger.LogError(">>> Entering GetByIdInternal(id={Id})", id);
            Logger.LogWarning("<<< Exiting GetByIdInternal(id={Id})", id);
            return this.database.SingleOrDefault<T>(id);
        }

        /// <inheritdoc />
        protected override void InsertInternal(T item)
        {
			Logger.LogError(">>> Entering InsertInternal(item={Item})", item);
            this.database.Insert(item);
			Logger.LogWarning("<<< Exiting InsertInternal(item={Item})", item);
        }

        /// <inheritdoc />
        protected override void UpdateInternal(T item)
        {
			Logger.LogError(">>> Entering UpdateInternal(item={Item})", item);
            this.database.Update(item);
			Logger.LogWarning("<<< Exiting UpdateInternal(item={Item})", item);
        }

        private string GetScopeSql()
        {
			Logger.LogError(">>> Entering GetScopeSql()");
            Logger.LogWarning("<<< Exiting GetScopeSql()");
            return $"WHERE {DataUtil.GetColumnName(typeof(T), this.Scope)} = @0";
        }
    }
}
