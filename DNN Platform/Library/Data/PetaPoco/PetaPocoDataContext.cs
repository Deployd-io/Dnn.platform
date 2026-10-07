// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using global::PetaPoco;

    using Microsoft.Extensions.DependencyInjection;

    [CLSCompliant(false)]
    public class PetaPocoDataContext : IDataContext
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PetaPocoDataContext));
        private readonly IHostSettings hostSettings;
        private readonly Database database;
        private readonly IMapper mapper;

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext()
            : this(ConfigurationManager.ConnectionStrings[0].Name, string.Empty)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public PetaPocoDataContext(IHostSettings hostSettings)
            : this(hostSettings, ConfigurationManager.ConnectionStrings[0].Name, string.Empty)
        {
		Logger.LogError(">>> Entering PetaPocoDataContext(hostSettings={HostSettings})", hostSettings);
        Logger.LogWarning("<<< Exiting PetaPocoDataContext(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext(string connectionStringName)
            : this(connectionStringName, string.Empty, new Dictionary<Type, IMapper>())
        {
		Logger.LogError(">>> Entering PetaPocoDataContext(connectionStringName={ConnectionStringName})", connectionStringName);
        Logger.LogWarning("<<< Exiting PetaPocoDataContext(connectionStringName={ConnectionStringName})", connectionStringName);
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        public PetaPocoDataContext(IHostSettings hostSettings, string connectionStringName)
            : this(hostSettings, connectionStringName, string.Empty, new Dictionary<Type, IMapper>())
        {
		Logger.LogError(">>> Entering PetaPocoDataContext(hostSettings={HostSettings},connectionStringName={ConnectionStringName})", hostSettings, connectionStringName);
        Logger.LogWarning("<<< Exiting PetaPocoDataContext(hostSettings={HostSettings},connectionStringName={ConnectionStringName})", hostSettings, connectionStringName);
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext(string connectionStringName, string tablePrefix)
            : this(connectionStringName, tablePrefix, new Dictionary<Type, IMapper>())
        {
		Logger.LogError(">>> Entering PetaPocoDataContext(connectionStringName={ConnectionStringName},tablePrefix={TablePrefix})", connectionStringName, tablePrefix);
        Logger.LogWarning("<<< Exiting PetaPocoDataContext(connectionStringName={ConnectionStringName},tablePrefix={TablePrefix})", connectionStringName, tablePrefix);
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        public PetaPocoDataContext(IHostSettings hostSettings, string connectionStringName, string tablePrefix)
            : this(hostSettings, connectionStringName, tablePrefix, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        /// <param name="mappers">A <see cref="Dictionary{TKey,TValue}"/> with <see cref="FluentMapper{TModel}"/> instances for types.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext(string connectionStringName, string tablePrefix, Dictionary<Type, IMapper> mappers)
            : this(null, connectionStringName, tablePrefix, mappers)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        /// <param name="mappers">A <see cref="Dictionary{TKey,TValue}"/> with <see cref="FluentMapper{TModel}"/> instances for types.</param>
        public PetaPocoDataContext(IHostSettings hostSettings, string connectionStringName, string tablePrefix, Dictionary<Type, IMapper> mappers)
        {
            Requires.NotNullOrEmpty("connectionStringName", connectionStringName);

            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("PetaPocoDataContext(hostSettings={HostSettings},connectionStringName={ConnectionStringName},tablePrefix={TablePrefix},mappers={Mappers}): hostSettings → {HostSettings1}", hostSettings, connectionStringName, tablePrefix, mappers, hostSettings);

            this.database = new Database(connectionStringName);
			Logger.LogDebug("PetaPocoDataContext(hostSettings={HostSettings},connectionStringName={ConnectionStringName},tablePrefix={TablePrefix},mappers={Mappers}): database → {Database}", hostSettings, connectionStringName, tablePrefix, mappers, database);
            this.mapper = new PetaPocoMapper(tablePrefix);
			Logger.LogDebug("PetaPocoDataContext(hostSettings={HostSettings},connectionStringName={ConnectionStringName},tablePrefix={TablePrefix},mappers={Mappers}): mapper → {Mapper}", hostSettings, connectionStringName, tablePrefix, mappers, mapper);
            this.TablePrefix = tablePrefix;
			Logger.LogDebug("PetaPocoDataContext(hostSettings={HostSettings},connectionStringName={ConnectionStringName},tablePrefix={TablePrefix},mappers={Mappers}): this.TablePrefix → {Arg4}", hostSettings, connectionStringName, tablePrefix, mappers, this.TablePrefix);
            this.FluentMappers = mappers;
        }

        public Dictionary<Type, IMapper> FluentMappers { get; private set; }

        public string TablePrefix { get; private set; }

        public bool EnableAutoSelect
        {
            get { return this.database.EnableAutoSelect; }
            set { this.database.EnableAutoSelect = value; }
        }

        /// <inheritdoc />
        public void BeginTransaction()
        {
			Logger.LogError(">>> Entering BeginTransaction()");
            this.database.BeginTransaction();
			Logger.LogWarning("<<< Exiting BeginTransaction()");
        }

        /// <inheritdoc />
        public void Commit()
        {
			Logger.LogError(">>> Entering Commit()");
            this.database.CompleteTransaction();
			Logger.LogWarning("<<< Exiting Commit()");
        }

        /// <inheritdoc />
        public void Execute(CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering Execute(type={Type},sql={Sql},args={Args})", type, sql, args);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
				Logger.LogError("Execute(type={Type},sql={Sql},args={Args}): sql → {Sql1}", type, sql, args, sql);
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("Execute(type,args)={Type},{Args}: database query executed in {Arg2} ms", type, args, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting Execute(type={Type},sql={Sql},args={Args})", type, sql, args);
            this.database.Execute(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public IEnumerable<T> ExecuteQuery<T>(CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteQuery(type={Type},sql={Sql},args={Args})", type, sql, args);
            PetaPocoMapper.SetMapper<T>(this.mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
				Logger.LogError("ExecuteQuery(type={Type},sql={Sql},args={Args}): sql → {Sql1}", type, sql, args, sql);
            }

			Logger.LogWarning("<<< Exiting ExecuteQuery(type={Type},sql={Sql},args={Args})", type, sql, args);
            return this.database.Fetch<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public T ExecuteScalar<T>(CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteScalar(type={Type},sql={Sql},args={Args})", type, sql, args);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
				Logger.LogError("ExecuteScalar(type={Type},sql={Sql},args={Args}): sql → {Sql1}", type, sql, args, sql);
            }

			Logger.LogWarning("<<< Exiting ExecuteScalar(type={Type},sql={Sql},args={Args})", type, sql, args);
            return this.database.ExecuteScalar<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public T ExecuteSingleOrDefault<T>(CommandType type, string sql, params object[] args)
        {
			Logger.LogError(">>> Entering ExecuteSingleOrDefault(type={Type},sql={Sql},args={Args})", type, sql, args);
            PetaPocoMapper.SetMapper<T>(this.mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
				Logger.LogError("ExecuteSingleOrDefault(type={Type},sql={Sql},args={Args}): sql → {Sql1}", type, sql, args, sql);
            }

			Logger.LogWarning("<<< Exiting ExecuteSingleOrDefault(type={Type},sql={Sql},args={Args})", type, sql, args);
            return this.database.SingleOrDefault<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public IRepository<T> GetRepository<T>()
            where T : class
        {
			Logger.LogError(">>> Entering GetRepository()");
            PetaPocoRepository<T> rep = null;

            // Determine whether to use a Fluent Mapper
            if (this.FluentMappers.ContainsKey(typeof(T)))
            {
                if (this.FluentMappers[typeof(T)] is FluentMapper<T> fluentMapper)
                {
                    rep = new PetaPocoRepository<T>(this.hostSettings, this.database, fluentMapper);
					Logger.LogError("GetRepository(): rep → {Rep}", rep);
                    rep.Initialize(fluentMapper.CacheKey, fluentMapper.CacheTimeOut, fluentMapper.CachePriority, fluentMapper.Scope);
                }
            }
            else
            {
                rep = new PetaPocoRepository<T>(this.hostSettings, this.database, this.mapper);
            }

			Logger.LogWarning("<<< Exiting GetRepository()");
            return rep;
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
			Logger.LogError(">>> Entering RollbackTransaction()");
            this.database.AbortTransaction();
			Logger.LogWarning("<<< Exiting RollbackTransaction()");
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
                this.database.Dispose();
            }
			Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }
    }
}
