// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;

    public abstract class ControllerBase<TEntity, TContract, TSelf> : ServiceLocator<TContract, TSelf>
        where TSelf : ServiceLocator<TContract, TSelf>, new()
        where TEntity : class
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected readonly IDataContext DataContext;

        /// <summary>Initializes a new instance of the <see cref="ControllerBase{TEntity, TContract, TSelf}"/> class.</summary>
        protected ControllerBase()
        {
		Logger.LogDebug(">>> Entering ControllerBase()");
		Logger.LogDebug("<<< Exiting ControllerBase()");
        }

        /// <summary>Initializes a new instance of the <see cref="ControllerBase{TEntity, TContract, TSelf}"/> class.</summary>
        /// <param name="dataContext">The data context.</param>
        protected ControllerBase(IDataContext dataContext)
        {
			Logger.LogDebug(">>> Entering ControllerBase(dataContext={DataContext})", dataContext);
            // Argument Contract
            Requires.NotNull("dataContext", dataContext);

            this.DataContext = dataContext;
			Logger.LogDebug("<<< Exiting ControllerBase(dataContext={DataContext})", dataContext);
			Logger.LogDebug("ControllerBase(dataContext={DataContext}): DataContext → {DataContext1}", dataContext, DataContext);
			Logger.LogDebug("ControllerBase(dataContext={DataContext}): this.DataContext → {Arg1}", dataContext, this.DataContext);
        }

        public void Add(TEntity entity)
        {
			Logger.LogDebug(">>> Entering Add(entity={Entity})", entity);
            // Argument Contract
            Requires.NotNull(entity);

            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();

				Logger.LogDebug("Add(entity={Entity}): rep → {Rep}", entity, rep);
                rep.Insert(entity);
            }
			Logger.LogDebug("<<< Exiting Add(entity={Entity})", entity);
        }

        public void Delete(TEntity entity)
        {
			Logger.LogDebug(">>> Entering Delete(entity={Entity})", entity);
            // Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof(TEntity), string.Empty);
            Requires.PropertyNotNull(entity, primaryKey);
			Logger.LogDebug("Delete(entity={Entity}): primaryKey → {PrimaryKey}", entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();

				Logger.LogDebug("Delete(entity={Entity}): rep → {Rep}", entity, rep);
                rep.Delete(entity);
            }
			Logger.LogDebug("<<< Exiting Delete(entity={Entity})", entity);
        }

        public IEnumerable<TEntity> Find(string sqlCondition, params object[] args)
        {
			Logger.LogDebug(">>> Entering Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            IEnumerable<TEntity> entities;
            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();

				Logger.LogDebug("Find(sqlCondition={SqlCondition},args={Args}): rep → {Rep}", sqlCondition, args, rep);
                entities = rep.Find(sqlCondition, args);
				Logger.LogDebug("Find(sqlCondition={SqlCondition},args={Args}): entities → {Entities}", sqlCondition, args, entities);
            }

			Logger.LogDebug("<<< Exiting Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            return entities;
        }

        public IEnumerable<TEntity> Get()
        {
			Logger.LogDebug(">>> Entering Get()");
            IEnumerable<TEntity> entities;
            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();

				Logger.LogDebug("Get(): rep → {Rep}", rep);
                entities = rep.Get();
				Logger.LogDebug("Get(): entities → {Entities}", entities);
            }

			Logger.LogDebug("<<< Exiting Get()");
            return entities;
        }

        public IEnumerable<TEntity> Get<TScope>(TScope scope)
        {
            IEnumerable<TEntity> contentTypes;
            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();

                contentTypes = rep.Get(scope);
            }

            return contentTypes;
        }

        public void Update(TEntity entity)
        {
			Logger.LogDebug(">>> Entering Update(entity={Entity})", entity);
            // Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof(TEntity), string.Empty);
            Requires.PropertyNotNull(entity, primaryKey);
			Logger.LogDebug("Update(entity={Entity}): primaryKey → {PrimaryKey}", entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

				Logger.LogDebug("Update(entity={Entity}): rep → {Rep}", entity, rep);
                rep.Update(entity);
            }
			Logger.LogDebug("<<< Exiting Update(entity={Entity})", entity);
        }
    }
}
