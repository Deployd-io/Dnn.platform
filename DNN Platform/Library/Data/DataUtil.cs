// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    using DotNetNuke.Collections;
    using DotNetNuke.ComponentModel.DataAnnotations;

    internal static class DataUtil
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        internal static string GenerateExecuteStoredProcedureSql(string procedureName, params object[] args)
        {
			Logger.LogDebug(">>> Entering GenerateExecuteStoredProcedureSql(procedureName={ProcedureName},args={Args})", procedureName, args);
            var sb = new StringBuilder(";Exec ");
            sb.Append(procedureName);
			Logger.LogDebug("GenerateExecuteStoredProcedureSql(procedureName={ProcedureName},args={Args}): sb → {Sb}", procedureName, args, sb);
            for (int i = 0; i < args.Length; i++)
            {
                var parameterFormat = " @{0}";
                string parameterName = null;
				Logger.LogDebug("GenerateExecuteStoredProcedureSql(procedureName={ProcedureName},args={Args}): parameterFormat → {ParameterFormat}", procedureName, args, parameterFormat);
				Logger.LogDebug("GenerateExecuteStoredProcedureSql(procedureName={ProcedureName},args={Args}): parameterName → {ParameterName}", procedureName, args, parameterName);
                if (args[i] is IDataParameter param)
                {
                    // intentionally adding an extra @ before parameter name, so PetaPoco won't try to match it to a passed-in arg
                    parameterFormat = " @{1}=@{0}";
                    if (param.Direction == ParameterDirection.Output)
                    { // Add output for output params
                        parameterFormat += " output";
                    }

                    parameterName = param.ParameterName;
                }

                sb.AppendFormat(CultureInfo.InvariantCulture, parameterFormat, i, parameterName);
                if (i < args.Length - 1)
                {
                    sb.Append(',');
                }
            }

			Logger.LogDebug("<<< Exiting GenerateExecuteStoredProcedureSql(procedureName={ProcedureName},args={Args})", procedureName, args);
            return sb.ToString();
        }

        internal static TAttribute GetAttribute<TAttribute>(Type type)
        {
			Logger.LogDebug(">>> Entering GetAttribute(type={Type})", type);
            TAttribute attribute = default(TAttribute);

			Logger.LogDebug("GetAttribute(type={Type}): attribute → {Attribute}", type, attribute);
            object[] tableNameAttributes = type.GetCustomAttributes(typeof(TAttribute), true);
            if (tableNameAttributes.Length > 0)
            {
                attribute = (TAttribute)tableNameAttributes[0];
            }

            return attribute;
        }

        internal static bool GetAutoIncrement(Type type, bool defaultValue)
        {
			Logger.LogDebug(">>> Entering GetAutoIncrement(type={Type},defaultValue={DefaultValue})", type, defaultValue);
            var autoIncrement = defaultValue;

			Logger.LogDebug("GetAutoIncrement(type={Type},defaultValue={DefaultValue}): autoIncrement → {AutoIncrement}", type, defaultValue, autoIncrement);
            var primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(type);
            if (primaryKeyAttribute != null)
			Logger.LogDebug("GetAutoIncrement(type={Type},defaultValue={DefaultValue}): primaryKeyAttribute → {PrimaryKeyAttribute}", type, defaultValue, primaryKeyAttribute);
            {
                autoIncrement = primaryKeyAttribute.AutoIncrement;
            }

			Logger.LogDebug("<<< Exiting GetAutoIncrement(type={Type},defaultValue={DefaultValue})", type, defaultValue);
            return autoIncrement;
        }

        internal static string GetColumnName(Type type, string propertyName)
        {
			Logger.LogDebug(">>> Entering GetColumnName(type={Type},propertyName={PropertyName})", type, propertyName);
			Logger.LogDebug("<<< Exiting GetColumnName(type={Type},propertyName={PropertyName})", type, propertyName);
            return GetColumnName(type.GetProperty(propertyName), propertyName);
        }

        internal static string GetColumnName(PropertyInfo propertyInfo, string defaultName)
        {
            var columnName = defaultName;

            object[] columnNameAttributes = propertyInfo.GetCustomAttributes(typeof(ColumnNameAttribute), true);
            if (columnNameAttributes.Length > 0)
            {
                var columnNameAttribute = (ColumnNameAttribute)columnNameAttributes[0];
                columnName = columnNameAttribute.ColumnName;
            }

			Logger.LogDebug("<<< Exiting GetColumnName(propertyInfo={PropertyInfo},defaultName={DefaultName})", propertyInfo, defaultName);
            return columnName;
        }

        internal static bool GetIsCacheable(Type type)
        {
			Logger.LogDebug(">>> Entering GetIsCacheable(type={Type})", type);
			Logger.LogDebug("<<< Exiting GetIsCacheable(type={Type})", type);
            return GetAttribute<CacheableAttribute>(type) != null;
        }

        internal static TProperty GetPrimaryKey<TEntity, TProperty>(TEntity item)
        {
			Logger.LogDebug(">>> Entering GetPrimaryKey(item={Item})", item);
            Type modelType = typeof(TEntity);

			Logger.LogDebug("GetPrimaryKey(item={Item}): modelType → {ModelType}", item, modelType);
            // Get the primary key
            var primaryKeyName = GetPrimaryKeyProperty(modelType, string.Empty);

			Logger.LogDebug("GetPrimaryKey(item={Item}): primaryKeyName → {PrimaryKeyName}", item, primaryKeyName);
            return GetPropertyValue<TEntity, TProperty>(item, primaryKeyName);
        }

        internal static string GetPrimaryKeyColumn(Type type, string defaultName)
        {
			Logger.LogDebug(">>> Entering GetPrimaryKeyColumn(type={Type},defaultName={DefaultName})", type, defaultName);
            var primaryKeyName = defaultName;

			Logger.LogDebug("GetPrimaryKeyColumn(type={Type},defaultName={DefaultName}): primaryKeyName → {PrimaryKeyName}", type, defaultName, primaryKeyName);
            var primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(type);
            if (primaryKeyAttribute != null)
			Logger.LogDebug("GetPrimaryKeyColumn(type={Type},defaultName={DefaultName}): primaryKeyAttribute → {PrimaryKeyAttribute}", type, defaultName, primaryKeyAttribute);
            {
                primaryKeyName = primaryKeyAttribute.ColumnName;
            }

			Logger.LogDebug("<<< Exiting GetPrimaryKeyColumn(type={Type},defaultName={DefaultName})", type, defaultName);
            return primaryKeyName;
        }

        internal static string GetPrimaryKeyProperty(Type type, string defaultName)
        {
			Logger.LogDebug(">>> Entering GetPrimaryKeyProperty(type={Type},defaultName={DefaultName})", type, defaultName);
            var primaryKeyName = defaultName;

			Logger.LogDebug("GetPrimaryKeyProperty(type={Type},defaultName={DefaultName}): primaryKeyName → {PrimaryKeyName}", type, defaultName, primaryKeyName);
            var primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(type);
            if (primaryKeyAttribute != null)
			Logger.LogDebug("GetPrimaryKeyProperty(type={Type},defaultName={DefaultName}): primaryKeyAttribute → {PrimaryKeyAttribute}", type, defaultName, primaryKeyAttribute);
            {
                primaryKeyName = primaryKeyAttribute.PropertyName;
            }

			Logger.LogDebug("<<< Exiting GetPrimaryKeyProperty(type={Type},defaultName={DefaultName})", type, defaultName);
            return primaryKeyName;
        }

        internal static TProperty GetPropertyValue<TEntity, TProperty>(TEntity item, string propertyName)
        {
			Logger.LogDebug(">>> Entering GetPropertyValue(item={Item},propertyName={PropertyName})", item, propertyName);
            var modelType = typeof(TEntity);
            var property = modelType.GetProperty(propertyName);
			Logger.LogDebug("GetPropertyValue(item={Item},propertyName={PropertyName}): modelType → {ModelType}", item, propertyName, modelType);
			Logger.LogDebug("GetPropertyValue(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);

            return (TProperty)property.GetValue(item, null);
        }

        internal static string GetTableName(Type type)
        {
			Logger.LogDebug(">>> Entering GetTableName(type={Type})", type);
			Logger.LogDebug("<<< Exiting GetTableName(type={Type})", type);
            return GetTableName(type, string.Empty);
        }

        internal static string GetTableName(Type type, string defaultName)
        {
            var tableName = defaultName;

            var tableNameAttribute = GetAttribute<TableNameAttribute>(type);
            if (tableNameAttribute != null)
            {
                tableName = tableNameAttribute.TableName;
            }

			Logger.LogDebug("<<< Exiting GetTableName(type={Type},defaultName={DefaultName})", type, defaultName);
            return tableName;
        }

        internal static string ReplaceTokens(string sql)
        {
			Logger.LogDebug(">>> Entering ReplaceTokens(sql={Sql})", sql);
            var isSqlCe = DataProvider.Instance().Settings.GetValueOrDefault("isSqlCe", false);
			Logger.LogDebug("ReplaceTokens(sql={Sql}): isSqlCe → {IsSqlCe}", sql, isSqlCe);
			Logger.LogDebug("<<< Exiting ReplaceTokens(sql={Sql})", sql);
            return sql.Replace("{databaseOwner}", isSqlCe ? string.Empty : DataProvider.Instance().DatabaseOwner)
                        .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }
    }
}
