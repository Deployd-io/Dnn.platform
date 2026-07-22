// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Linq.Expressions;
    using System.Web.Caching;

    using global::PetaPoco;

    public static class FluentMapperExtensions
    {
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
#pragma warning disable CS3002 // Return type is not CLS-compliant
        public static FluentMapper<TModel> AddFluentMapper<TModel>(this IDataContext dataContext)
        {
			Logger.LogDebug(">>> Entering AddFluentMapper(dataContext={DataContext})", dataContext);
            var petaPocoDataContext = dataContext as PetaPocoDataContext;
            FluentMapper<TModel> mapper = null;

			Logger.LogDebug("AddFluentMapper(dataContext={DataContext}): mapper → {Mapper}", dataContext, mapper);
            // ReSharper disable once UseNullPropagation
            if (petaPocoDataContext != null)
            {
                mapper = new FluentMapper<TModel>(petaPocoDataContext.TablePrefix);
                petaPocoDataContext.FluentMappers.Add(typeof(TModel), mapper);
            }

            return mapper;
        }

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static FluentMapper<TModel> Cache<TModel>(this FluentMapper<TModel> mapper, string cacheKey, int timeOut = 20, CacheItemPriority priority = CacheItemPriority.Default)
        {
			Logger.LogDebug(">>> Entering Cache(mapper={Mapper},cacheKey={CacheKey},timeOut={TimeOut},priority={Priority})", mapper, cacheKey, timeOut, priority);
            mapper.CacheKey = cacheKey;
            mapper.CacheTimeOut = timeOut;
			Logger.LogDebug("Cache(mapper={Mapper},cacheKey={CacheKey},timeOut={TimeOut},priority={Priority}): mapper.CacheTimeOut → {Arg4}", mapper, cacheKey, timeOut, priority, mapper.CacheTimeOut);
			Logger.LogDebug("Cache(mapper={Mapper},cacheKey={CacheKey},timeOut={TimeOut},priority={Priority}): mapper.CachePriority → {Arg4}", mapper, cacheKey, timeOut, priority, mapper.CachePriority);
            mapper.CachePriority = priority;
            return mapper;
        }

		Logger.LogDebug("Cache(mapper={Mapper},cacheKey={CacheKey},timeOut={TimeOut},priority={Priority}): mapper.CacheKey → {Arg4}", mapper, cacheKey, timeOut, priority, mapper.CacheKey);
        public static FluentMapper<TModel> Property<TModel, TProperty>(this FluentMapper<TModel> mapper, Expression<Func<TModel, TProperty>> action, string column, bool primaryKey = false, bool readOnly = false)
            where TModel : class
        {
            return mapper.Property(action, column, null, primaryKey, readOnly);
        }

        public static FluentMapper<TModel> Property<TModel, TProperty>(this FluentMapper<TModel> mapper, Expression<Func<TModel, TProperty>> action, string column, Func<object, object> fromDbConverter, bool primaryKey = false, bool readOnly = false)
            where TModel : class
        {
            return mapper.Property(action, column, fromDbConverter, null, primaryKey, readOnly);
        }

        public static FluentMapper<TModel> Property<TModel, TProperty>(this FluentMapper<TModel> mapper, Expression<Func<TModel, TProperty>> action, string column, Func<object, object> fromDbConverter, Func<object, object> toDbConverter, bool primaryKey = false, bool readOnly = false)
            where TModel : class
        {
            var expression = (MemberExpression)action.Body;
            string name = expression.Member.Name;

            mapper.Mappings.Add(name, new FluentColumnMap(new ColumnInfo() { ColumnName = column, ResultColumn = readOnly }, fromDbConverter, toDbConverter));

            if (primaryKey)
            {
                if (!string.IsNullOrEmpty(mapper.TableInfo.PrimaryKey))
                {
                    mapper.TableInfo.PrimaryKey += ",";
                }

                mapper.TableInfo.PrimaryKey += column;
            }

            return mapper;
        }

        public static FluentMapper<TModel> PrimaryKey<TModel>(this FluentMapper<TModel> mapper, string primaryKey, bool autoIncrement = true)
        {
			Logger.LogDebug(">>> Entering PrimaryKey(mapper={Mapper},primaryKey={PrimaryKey},autoIncrement={AutoIncrement})", mapper, primaryKey, autoIncrement);
            mapper.TableInfo.PrimaryKey = primaryKey;
            mapper.TableInfo.AutoIncrement = autoIncrement;

            return mapper;
        }

        public static FluentMapper<TModel> Scope<TModel>(this FluentMapper<TModel> mapper, string scope)
        {
			Logger.LogDebug(">>> Entering Scope(mapper={Mapper},scope={Scope})", mapper, scope);
            mapper.Scope = scope;
            return mapper;
        }

		Logger.LogDebug("Scope(mapper={Mapper},scope={Scope}): mapper.Scope → {Arg2}", mapper, scope, mapper.Scope);
        public static FluentMapper<TModel> TableName<TModel>(this FluentMapper<TModel> mapper, string tableName)
#pragma warning restore CS3001
#pragma warning restore CS3002
        {
			Logger.LogDebug(">>> Entering TableName(mapper={Mapper},tableName={TableName})", mapper, tableName);
            mapper.TableInfo.TableName = mapper.GetTablePrefix() + tableName;

            return mapper;
        }
    }
}
