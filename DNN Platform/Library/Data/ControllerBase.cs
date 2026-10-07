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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ControllerBase));
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected readonly IDataContext DataContext;

        /// <summary>Initializes a new instance of the <see cref="ControllerBase{TEntity, TContract, TSelf}"/> class.</summary>
        protected ControllerBase()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ControllerBase{TEntity, TContract, TSelf}"/> class.</summary>
        /// <param name="dataContext">The data context.</param>
        protected ControllerBase(IDataContext dataContext)
        {
			Logger.LogError(">>> Entering ControllerBase(dataContext={DataContext})", dataContext);
            // Argument Contract
            Requires.NotNull("dataContext", dataContext);

			Logger.LogWarning("<<< Exiting ControllerBase(dataContext={DataContext})", dataContext);
            this.DataContext = dataContext;
			Logger.LogDebug("ControllerBase(dataContext={DataContext}): DataContext → {DataContext1}", dataContext, DataContext);
        }

        public void Add(TEntity entity)
        {
			Logger.LogError(">>> Entering Add(entity={Entity})", entity);
            // Argument Contract
            Requires.NotNull(entity);

            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();
				Logger.LogError("Add(entity={Entity}): rep → {Rep}", entity, rep);

                rep.Insert(entity);
            }
			Logger.LogWarning("<<< Exiting Add(entity={Entity})", entity);
        }

        public void Delete(TEntity entity)
        {
			Logger.LogError(">>> Entering Delete(entity={Entity})", entity);
            // Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof(TEntity), string.Empty);
			Logger.LogError("Delete(entity={Entity}): primaryKey → {PrimaryKey}", entity, primaryKey);
            Requires.PropertyNotNull(entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();
				Logger.LogError("Delete(entity={Entity}): rep → {Rep}", entity, rep);

                rep.Delete(entity);
            }
			Logger.LogWarning("<<< Exiting Delete(entity={Entity})", entity);
        }

        public IEnumerable<TEntity> Find(string sqlCondition, params object[] args)
        {
			Logger.LogError(">>> Entering Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            IEnumerable<TEntity> entities;
            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();
				Logger.LogError("Find(sqlCondition={SqlCondition},args={Args}): rep → {Rep}", sqlCondition, args, rep);

                entities = rep.Find(sqlCondition, args);
				Logger.LogError("Find(sqlCondition={SqlCondition},args={Args}): entities → {Entities}", sqlCondition, args, entities);
				Logger.LogDebug("Find(sqlCondition={SqlCondition},args={Args}): entities → {Entities}", sqlCondition, args, entities);
            }

			Logger.LogWarning("<<< Exiting Find(sqlCondition={SqlCondition},args={Args})", sqlCondition, args);
            return entities;
        }

        public IEnumerable<TEntity> Get()
        {
			Logger.LogError(">>> Entering Get()");
            IEnumerable<TEntity> entities;
            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();
				Logger.LogError("Get(): rep → {Rep}", rep);

                entities = rep.Get();
				Logger.LogError("Get(): entities → {Entities}", entities);
				Logger.LogDebug("Get(): entities → {Entities}", entities);
            }

			Logger.LogWarning("<<< Exiting Get()");
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
			Logger.LogError(">>> Entering Update(entity={Entity})", entity);
            // Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof(TEntity), string.Empty);
			Logger.LogError("Update(entity={Entity}): primaryKey → {PrimaryKey}", entity, primaryKey);
            Requires.PropertyNotNull(entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (this.DataContext)
            {
                var rep = this.DataContext.GetRepository<TEntity>();
				Logger.LogError("Update(entity={Entity}): rep → {Rep}", entity, rep);

                rep.Update(entity);
            }
			Logger.LogWarning("<<< Exiting Update(entity={Entity})", entity);
        }
    }
}
