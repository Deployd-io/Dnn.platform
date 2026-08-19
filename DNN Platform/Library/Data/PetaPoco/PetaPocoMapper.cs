// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using global::PetaPoco;

    public class PetaPocoMapper : IMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PetaPocoMapper));
        private static IMapper defaultMapper;
        private static ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
        private readonly string tablePrefix;

        /// <summary>Initializes a new instance of the <see cref="PetaPocoMapper"/> class.</summary>
        /// <param name="tablePrefix">The table prefix.</param>
        public PetaPocoMapper(string tablePrefix)
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper(tablePrefix={TablePrefix})", tablePrefix);
            this.tablePrefix = tablePrefix;
            defaultMapper = new StandardMapper();
			Logger.LogDebug("PetaPocoMapper(tablePrefix={TablePrefix}): defaultMapper → {DefaultMapper}", tablePrefix, defaultMapper);
			Logger.LogDebug("PetaPocoMapper(tablePrefix={TablePrefix}): this.tablePrefix → {Arg1}", tablePrefix, this.tablePrefix);
			Logger.LogDebug("<<< Exiting PetaPocoMapper(tablePrefix={TablePrefix})", tablePrefix);
        }

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static void SetMapper<T>(IMapper mapper)
#pragma warning restore CS3001
        {
			Logger.LogDebug(">>> Entering SetMapper(mapper={Mapper})", mapper);
            @lock.EnterWriteLock();
            try
            {
                if (Mappers.GetMapper(typeof(T), defaultMapper) is StandardMapper)
                {
                    Mappers.Register(typeof(T), mapper);
                }
            }
            finally
            {
                @lock.ExitWriteLock();
            }
			Logger.LogDebug("<<< Exiting SetMapper(mapper={Mapper})", mapper);
        }

        /// <inheritdoc />
#pragma warning disable CS3002 // Return type is not CLS-compliant
        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
#pragma warning restore CS3002
        {
			Logger.LogDebug(">>> Entering GetColumnInfo(pocoProperty={PocoProperty})", pocoProperty);
            bool includeColumn = true;

			Logger.LogDebug("GetColumnInfo(pocoProperty={PocoProperty}): includeColumn → {IncludeColumn}", pocoProperty, includeColumn);
            // Check if the class has the DeclareColumnsAttribute
            bool declareColumns = pocoProperty.DeclaringType != null
                            && pocoProperty.DeclaringType.GetCustomAttributes(typeof(DeclareColumnsAttribute), true).Length > 0;

            if (declareColumns)
            {
                if (pocoProperty.GetCustomAttributes(typeof(IncludeColumnAttribute), true).Length == 0)
                {
                    includeColumn = false;
                }
            }
            else
            {
                if (pocoProperty.GetCustomAttributes(typeof(IgnoreColumnAttribute), true).Length > 0)
                {
                    includeColumn = false;
                }
            }

            ColumnInfo ci = null;
			Logger.LogDebug("GetColumnInfo(pocoProperty={PocoProperty}): ci → {Ci}", pocoProperty, ci);
            if (includeColumn)
            {
                ci = ColumnInfo.FromProperty(pocoProperty);
                ci.ColumnName = DataUtil.GetColumnName(pocoProperty, ci.ColumnName);

                ci.ResultColumn = pocoProperty.GetCustomAttributes(typeof(ReadOnlyColumnAttribute), true).Length > 0;
            }

			Logger.LogDebug("<<< Exiting GetColumnInfo(pocoProperty={PocoProperty})", pocoProperty);
            return ci;
        }

        /// <inheritdoc />
#pragma warning disable CS3002 // Return type is not CLS-compliant
        public TableInfo GetTableInfo(Type pocoType)
#pragma warning restore CS3002
        {
			Logger.LogDebug(">>> Entering GetTableInfo(pocoType={PocoType})", pocoType);
            TableInfo ti = TableInfo.FromPoco(pocoType);

			Logger.LogDebug("GetTableInfo(pocoType={PocoType}): ti → {Ti}", pocoType, ti);
            // Table Name
            ti.TableName = DataUtil.GetTableName(pocoType, ti.TableName + "s");

            ti.TableName = this.tablePrefix + ti.TableName;
			Logger.LogDebug("GetTableInfo(pocoType={PocoType}): ti.TableName → {Arg1}", pocoType, ti.TableName);

            // Primary Key
            ti.PrimaryKey = DataUtil.GetPrimaryKeyColumn(pocoType, ti.PrimaryKey);

            ti.AutoIncrement = DataUtil.GetAutoIncrement(pocoType, true);

			Logger.LogDebug("<<< Exiting GetTableInfo(pocoType={PocoType})", pocoType);
            return ti;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public Func<object, object> GetFromDbConverter(PropertyInfo pi, Type sourceType)
        {
            return null;
        }

        /// <inheritdoc />
        public Func<object, object> GetToDbConverter(PropertyInfo sourceProperty)
        {
            return null;
        }
    }
}
