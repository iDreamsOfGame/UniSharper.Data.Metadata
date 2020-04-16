// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the
// project root for license information.

using ReSharp.Data.iBoxDB;
using ReSharp.Patterns;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UniSharper.Data.Metadata
{
    /// <summary>
    /// The MetadataManager is a convenience class for managing metadata and entities. This class
    /// cannot be inherited. Implements the <see cref="MetadataManager"/>
    /// </summary>
    /// <seealso cref="MetadataManager"/>
    public sealed class MetadataManager : Singleton<MetadataManager>
    {
        #region Fields

        private readonly Dictionary<Type, byte[]> entityDBRawDataMap;
        private iBoxDBContext configDBContext;

        #endregion Fields

        #region Constructors

        [Preserve]
        private MetadataManager()
        {
            entityDBRawDataMap = new Dictionary<Type, byte[]>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets all entities of the entity type.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <returns>The list entities found in database.</returns>
        public T[] GetAllEntities<T>() where T : MetadataEntity, new()
        {
            Type entityType = typeof(T);
            List<T> list = null;

            CreateDBContextForEntity<T>(entityType, (dbContext) =>
            {
                list = dbContext.SelectAll<T>(entityType.Name);
            });

            return list?.ToArray();
        }

        public T[] GetEntities<T>(string key, object value) where T : MetadataEntity, new()
        {
            Type entityType = typeof(T);
            List<T> list = null;

            CreateDBContextForEntity<T>(entityType, (dbContext) =>
            {
                list = dbContext.Select<T>(entityType.Name, key, value);
            });

            return list?.ToArray();
        }

        public T[] GetEntities<T>(Dictionary<string, object> arguments, QueryLogicalOperator logicalOperator = QueryLogicalOperator.None) where T : MetadataEntity, new()
        {
            Type entityType = typeof(T);
            List<T> list = null;

            CreateDBContextForEntity<T>(entityType, (dbContext) =>
            {
                list = dbContext.Select<T>(entityType.Name, arguments, logicalOperator);
            });

            return list?.ToArray();
        }

        /// <summary>
        /// Gets the entity by the value of primary key.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="key">The value of primary key.</param>
        /// <returns>The entity found in database.</returns>
        public T GetEntity<T>(object key) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            T result = default;

            CreateDBContextForEntity<T>(entityType, (dbContext) =>
            {
                result = dbContext.SelectKey<T>(entityType.Name, key);
            });

            return result;
        }

        /// <summary>
        /// Initializes the specified configuration database data.
        /// </summary>
        /// <param name="configDBData">The configuration database data.</param>
        public void Initialize(byte[] configDBData)
        {
            if (configDBContext != null) return;
            configDBContext = new iBoxDBContext(configDBData);
            configDBContext.EnsureTable<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, MetadataEntityDBConfig.TablePrimaryKey);
            configDBContext.Open();
        }

        /// <summary>
        /// Cache the raw data of entity database.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="rawData">The raw data of entity database.</param>
        public void LoadEntityDatabase<T>(byte[] rawData) where T : MetadataEntity
        {
            var entityType = typeof(T);
            LoadEntityDatabase(entityType, rawData);
        }

        /// <summary>
        /// Cache the raw data of entity database.
        /// </summary>
        /// <param name="entityType">The type of entity.</param>
        /// <param name="rawData">The raw data of entity databse.</param>
        public void LoadEntityDatabase(Type entityType, byte[] rawData)
        {
            entityDBRawDataMap.AddUnique(entityType, rawData);
        }

        public T[] QueryEntities<T>(string query, params object[] arguments) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            List<T> list = null;

            CreateDBContextForEntity<T>(entityType, (dbContext) =>
            {
                list = dbContext.Select<T>(entityType.Name, query, arguments);
            });

            return list?.ToArray();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (configDBContext != null)
                {
                    configDBContext.Dispose();
                    configDBContext = null;
                }
            }

            base.Dispose(disposing);
        }

        private void CreateDBContextForEntity<T>(Type entityType, Action<iBoxDBContext> handler) where T : MetadataEntity, new()
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (GetDBRawDataForEntity(entityType, out byte[] dbRawData))
            {
                using (iBoxDBContext dbContext = new iBoxDBContext(dbRawData))
                {
                    string tableName = entityType.Name;
                    string primaryKeyName = GetMetadataEntityDBPrimaryKey<T>();
                    dbContext.EnsureTable<T>(tableName, primaryKeyName);
                    dbContext.Open();

                    handler.Invoke(dbContext);
                }
            }
        }

        private bool GetDBRawDataForEntity(Type entityType, out byte[] rawData)
        {
            if (!entityDBRawDataMap.ContainsKey(entityType))
            {
                throw new Exception($"No database raw data fro the type {entityType.FullName}, you should load the database raw data for this type by invoking method 'LoadEntityDatabase'!");
            }
            else
            {
                rawData = entityDBRawDataMap[entityType];
                return true;
            }
        }

        private MetadataEntityDBConfig GetMetadataEntityDBConfig<T>() where T : MetadataEntity
        {
            if (configDBContext == null)
            {
                throw new Exception("Method 'Initialize' should be invoke first!");
            }

            var entityName = typeof(T).Name;
            return configDBContext.SelectKey<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, entityName);
        }

        private string GetMetadataEntityDBPrimaryKey<T>() where T : MetadataEntity
        {
            var config = GetMetadataEntityDBConfig<T>();
            var key = config?.PrimaryKey;
            return key;
        }

        #endregion Methods
    }
}