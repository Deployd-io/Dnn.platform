// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;

    using Dnn.ExportImport.Dto;
    using Dnn.ExportImport.Interfaces;
    using LiteDB;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class ExportImportRepository : IExportImportRepository
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private LiteDatabase liteDb;

        /// <summary>Initializes a new instance of the <see cref="ExportImportRepository"/> class.</summary>
        /// <param name="dbFileName">The LiteDB connection string.</param>
        public ExportImportRepository(string dbFileName)
        {
			_logger.LogDebug(">>> Entering ExportImportRepository(dbFileName={DbFileName})", dbFileName);
            this.liteDb = new LiteDatabase(new ConnectionString(dbFileName) { Upgrade = true });
            this.liteDb.Mapper.EmptyStringToNull = false;
			_logger.LogDebug("ExportImportRepository(dbFileName={DbFileName}): liteDb → {LiteDb}", dbFileName, liteDb);
            this.liteDb.Mapper.TrimWhitespace = false;
			_logger.LogDebug("<<< Exiting ExportImportRepository(dbFileName={DbFileName})", dbFileName);
        }

        /// <summary>Finalizes an instance of the <see cref="ExportImportRepository"/> class.</summary>
        ~ExportImportRepository()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			_logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			_logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <inheritdoc />
        public T AddSingleItem<T>(T item)
            where T : class
        {
            var collection = this.DbCollection<T>();
            collection.Insert(item);
            return item;
        }

        /// <inheritdoc />
        public T UpdateSingleItem<T>(T item)
            where T : class
        {
            var collection = this.DbCollection<T>();
            collection.Update(item);
            return item;
        }

        /// <inheritdoc />
        public T GetSingleItem<T>()
            where T : class
        {
			_logger.LogDebug(">>> Entering GetSingleItem()");
            var collection = this.DbCollection<T>();
            var first = collection.Min();
			_logger.LogDebug("GetSingleItem(): first → {First}", first);
			_logger.LogDebug("<<< Exiting GetSingleItem()");
			_logger.LogDebug("GetSingleItem(): collection → {Collection}", collection);
            return collection.FindById(first);
        }

        /// <inheritdoc />
        public T CreateItem<T>(T item, int? referenceId)
            where T : BasicExportImportDto
        {
            if (item == null)
            {
                return null;
            }

            var collection = this.DbCollection<T>();
			_logger.LogDebug("CreateItem(item={Item},referenceId={ReferenceId}): collection → {Collection}", item, referenceId, collection);
            if (referenceId != null)
            {
                item.ReferenceId = referenceId;
				_logger.LogDebug("CreateItem(item={Item},referenceId={ReferenceId}): item.ReferenceId → {Arg2}", item, referenceId, item.ReferenceId);
            }

            item.Id = collection.Insert(item);
			_logger.LogDebug("<<< Exiting CreateItem(item={Item},referenceId={ReferenceId})", item, referenceId);
            return item;
        }

        /// <inheritdoc />
        public void CreateItems<T>(IEnumerable<T> items, int? referenceId = null)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering CreateItems(items={Items},referenceId={ReferenceId})", items, referenceId);
            if (items == null)
            {
                return;
            }

            var allItems = items as List<T> ?? items.ToList();
			_logger.LogDebug("CreateItems(items={Items},referenceId={ReferenceId}): allItems → {AllItems}", items, referenceId, allItems);
            if (allItems.Count == 0)
            {
                return;
            }

            var collection = this.DbCollection<T>();
			_logger.LogDebug("CreateItems(items={Items},referenceId={ReferenceId}): collection → {Collection}", items, referenceId, collection);
            if (referenceId != null)
            {
                allItems.ForEach(x => { x.ReferenceId = referenceId; });
            }

            collection.Insert(allItems);
			_logger.LogDebug("<<< Exiting CreateItems(items={Items},referenceId={ReferenceId})", items, referenceId);
        }

        /// <inheritdoc />
        public T GetItem<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering GetItem(predicate={Predicate})", predicate);
			_logger.LogDebug("<<< Exiting GetItem(predicate={Predicate})", predicate);
            return this.InternalGetItems(predicate).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<T> GetItems<T>(
            Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null,
            bool asc = true,
            int? skip = null,
            int? max = null)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering GetItems(predicate={Predicate},orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max})", predicate, orderKeySelector, asc, skip, max);
			_logger.LogDebug("<<< Exiting GetItems(predicate={Predicate},orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max})", predicate, orderKeySelector, asc, skip, max);
			_logger.LogDebug("<<< Exiting GetItems(idList={IdList})", idList);
            return this.InternalGetItems(predicate, orderKeySelector, asc, skip, max);
        }

        /// <inheritdoc />
        public int GetCount<T>()
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering GetCount()");
            var collection = this.DbCollection<T>();
			_logger.LogDebug("<<< Exiting GetCount()");
			_logger.LogDebug("GetCount(): collection → {Collection}", collection);
            return collection?.Count() ?? 0;
        }

        /// <inheritdoc />
        public int GetCount<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
			_logger.LogDebug("GetCount(predicate={Predicate}): collection → {Collection}", predicate, collection);
            return collection?.Count(predicate) ?? 0;
        }

        /// <inheritdoc />
        public void RebuildIndex<T>(Expression<Func<T, object>> predicate, bool unique = false)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering RebuildIndex(predicate={Predicate},unique={Unique})", predicate, unique);
            var collection = this.DbCollection<T>();
            collection.EnsureIndex(predicate, unique);
			_logger.LogDebug("<<< Exiting RebuildIndex(predicate={Predicate},unique={Unique})", predicate, unique);
			_logger.LogDebug("RebuildIndex(predicate={Predicate},unique={Unique}): collection → {Collection}", predicate, unique, collection);
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering GetAllItems(orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max})", orderKeySelector, asc, skip, max);
			_logger.LogDebug("<<< Exiting GetAllItems(orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max})", orderKeySelector, asc, skip, max);
            return this.InternalGetItems(null, orderKeySelector, asc, skip, max);
        }

        /// <inheritdoc />
        public T GetItem<T>(int id)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
			_logger.LogDebug("GetItem(id={Id}): collection → {Collection}", id, collection);
            return collection.FindById(id);
        }

        /// <inheritdoc />
        public IEnumerable<T> GetItems<T>(IEnumerable<int> idList)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => idList.Contains(p.Id);
            return this.InternalGetItems(predicate);
        }

        /// <inheritdoc />
        public IEnumerable<T> GetRelatedItems<T>(int referenceId)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering GetRelatedItems(referenceId={ReferenceId})", referenceId);
            Expression<Func<T, bool>> predicate = p => p.ReferenceId == referenceId;
			_logger.LogDebug("<<< Exiting GetRelatedItems(referenceId={ReferenceId})", referenceId);
            return this.InternalGetItems(predicate);
        }

        /// <inheritdoc />
        public IEnumerable<T> FindItems<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering FindItems(predicate={Predicate})", predicate);
            var collection = this.DbCollection<T>();
			_logger.LogDebug("<<< Exiting FindItems(predicate={Predicate})", predicate);
			_logger.LogDebug("FindItems(predicate={Predicate}): collection → {Collection}", predicate, collection);
            return collection.Find(predicate);
        }

        /// <inheritdoc />
        public void UpdateItem<T>(T item)
            where T : BasicExportImportDto
        {
            if (item == null)
            {
                return;
            }

            var collection = this.DbCollection<T>();
			_logger.LogDebug("UpdateItem(item={Item}): collection → {Collection}", item, collection);
            if (collection.FindById(item.Id) == null)
            {
				_logger.LogWarning("Throwing KeyNotFoundException from UpdateItem(item={Item})", item);
                throw new KeyNotFoundException();
            }

            collection.Update(item);
			_logger.LogDebug("<<< Exiting UpdateItem(item={Item})", item);
        }

        /// <inheritdoc />
        public void UpdateItems<T>(IEnumerable<T> items)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering UpdateItems(items={Items})", items);
            var allItems = items as T[] ?? items.ToArray();
            if (allItems.Length == 0)
			_logger.LogDebug("UpdateItems(items={Items}): allItems → {AllItems}", items, allItems);
            {
                return;
            }

            var collection = this.DbCollection<T>();
			_logger.LogDebug("UpdateItems(items={Items}): collection → {Collection}", items, collection);
            collection.Update(allItems);
			_logger.LogDebug("<<< Exiting UpdateItems(items={Items})", items);
        }

        /// <inheritdoc />
        public bool DeleteItem<T>(int id)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering DeleteItem(id={Id})", id);
            var collection = this.DbCollection<T>();
            var item = collection.FindById(id);
			_logger.LogDebug("DeleteItem(id={Id}): item → {Item}", id, item);
			_logger.LogDebug("DeleteItem(id={Id}): collection → {Collection}", id, collection);
            if (item == null)
            {
				_logger.LogWarning("Throwing KeyNotFoundException from DeleteItem(id={Id})", id);
                throw new KeyNotFoundException();
            }

			_logger.LogDebug("<<< Exiting DeleteItem(id={Id})", id);
            return collection.Delete(id);
        }

        /// <inheritdoc />
        public void DeleteItems<T>(Expression<Func<T, bool>> deleteExpression)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering DeleteItems(deleteExpression={DeleteExpression})", deleteExpression);
            var collection = this.DbCollection<T>();
            if (deleteExpression != null)
			_logger.LogDebug("DeleteItems(deleteExpression={DeleteExpression}): collection → {Collection}", deleteExpression, collection);
            {
                collection.DeleteMany(deleteExpression);
            }
			_logger.LogDebug("<<< Exiting DeleteItems(deleteExpression={DeleteExpression})", deleteExpression);
        }

        /// <inheritdoc />
        public void CleanUpLocal(string collectionName)
        {
			_logger.LogDebug(">>> Entering CleanUpLocal(collectionName={CollectionName})", collectionName);
            if (!this.liteDb.CollectionExists(collectionName))
            {
                return;
            }

            var collection = this.liteDb.GetCollection<BsonDocument>(collectionName);
			_logger.LogDebug("CleanUpLocal(collectionName={CollectionName}): collection → {Collection}", collectionName, collection);
            var documentsToUpdate = collection.Find(Query.All()).ToList();
			_logger.LogDebug("CleanUpLocal(collectionName={CollectionName}): documentsToUpdate → {DocumentsToUpdate}", collectionName, documentsToUpdate);
            documentsToUpdate.ForEach(x =>
            {
                x["LocalId"] = null;
            });
            collection.Update(documentsToUpdate);
			_logger.LogDebug("<<< Exiting CleanUpLocal(collectionName={CollectionName})", collectionName);
        }

        protected virtual void Dispose(bool disposing)
        {
			_logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
                var temp = Interlocked.Exchange(ref this.liteDb, null);
                temp?.Dispose();
            }
			_logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }

        private IEnumerable<T> InternalGetItems<T>(
            Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null,
            bool asc = true,
            int? skip = null,
            int? max = null)
            where T : BasicExportImportDto
        {
			_logger.LogDebug(">>> Entering InternalGetItems(predicate={Predicate},orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max})", predicate, orderKeySelector, asc, skip, max);
            var collection = this.DbCollection<T>();
			_logger.LogDebug("InternalGetItems(predicate={Predicate},orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max}): collection → {Collection}", predicate, orderKeySelector, asc, skip, max, collection);

            var result = predicate != null
                ? collection.Find(predicate, skip ?? 0, max ?? int.MaxValue)
                : collection.Find(Query.All(), skip ?? 0, max ?? int.MaxValue);
				_logger.LogDebug("InternalGetItems(predicate={Predicate},orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max}): result → {Result}", predicate, orderKeySelector, asc, skip, max, result);

            if (orderKeySelector != null)
            {
                result = asc ? result.OrderBy(orderKeySelector) : result.OrderByDescending(orderKeySelector);
            }

			_logger.LogDebug("<<< Exiting InternalGetItems(predicate={Predicate},orderKeySelector={OrderKeySelector},asc={Asc},skip={Skip},max={Max})", predicate, orderKeySelector, asc, skip, max);
            return result.AsEnumerable();
        }

        private ILiteCollection<T> DbCollection<T>()
        {
			_logger.LogDebug(">>> Entering DbCollection()");
			_logger.LogDebug("<<< Exiting DbCollection()");
            return this.liteDb.GetCollection<T>(typeof(T).Name);
        }
    }
}
