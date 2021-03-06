﻿// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using ReSharp.Data.iBoxDB;
using ReSharp.Patterns;
using System;
using System.Collections.Generic;
using System.IO;
using ReSharp.Security.Cryptography;
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

        /// <summary>
        /// Key length of encryption.
        /// </summary>
        public const int EncryptionKeyLength = 16;

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
            var entityType = typeof(T);
            List<T> results = null;
            CreateDBContextForEntity<T>(entityType, context => results = context.SelectAll<T>(entityType.Name));
            return results?.ToArray();
        }

        public T[] GetEntities<T>(string key, object value) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            List<T> results = null;
            CreateDBContextForEntity<T>(entityType, context => results = context.Select<T>(entityType.Name, key, value));
            return results?.ToArray();
        }

        public T[] GetEntities<T>(Dictionary<string, object> arguments, QueryLogicalOperator logicalOperator = QueryLogicalOperator.None) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            List<T> results = null;
            CreateDBContextForEntity<T>(entityType, context => results = context.Select<T>(entityType.Name, arguments, logicalOperator));
            return results?.ToArray();
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
            var result = default(T);
            CreateDBContextForEntity<T>(entityType, context => result = context.SelectKey<T>(entityType.Name, key));
            return result;
        }

        /// <summary>
        /// Initializes the specified configuration database data.
        /// </summary>
        /// <param name="configDBData">The configuration database data.</param>
        public void Initialize(byte[] configDBData)
        {
            if (configDBContext != null) return;
            var rawData = DecryptDatabaseFile(configDBData);
            configDBContext = new iBoxDBContext(rawData);
            configDBContext.EnsureTable<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, MetadataEntityDBConfig.TablePrimaryKey);
            configDBContext.Open();
        }

        /// <summary>
        /// Cache the raw data of entity database.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="rawData">The raw data of entity database.</param>
        public void LoadEntityDatabase<T>(byte[] rawData) where T : MetadataEntity => LoadEntityDatabase(typeof(T), rawData);

        /// <summary>
        /// Cache the raw data of entity database.
        /// </summary>
        /// <param name="entityType">The type of entity.</param>
        /// <param name="rawData">The raw data of entity database.</param>
        public void LoadEntityDatabase(Type entityType, byte[] rawData) => entityDBRawDataMap.AddUnique(entityType, DecryptDatabaseFile(rawData));

        public T[] QueryEntities<T>(string query, params object[] arguments) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            List<T> results = null;
            CreateDBContextForEntity<T>(entityType, context => results = context.Select<T>(entityType.Name, query, arguments));
            return results?.ToArray();
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

            if (!GetDBRawDataForEntity(entityType, out var dbRawData))
            {
                handler?.Invoke(null);
                return;
            }

            using (var dbContext = new iBoxDBContext(dbRawData))
            {
                var tableName = entityType.Name;
                var primaryKeyName = GetMetadataEntityDBPrimaryKey<T>();
                dbContext.EnsureTable<T>(tableName, primaryKeyName);
                dbContext.Open();
                handler?.Invoke(dbContext);
            }
        }

        private byte[] DecryptDatabaseFile(byte[] fileData)
        {
            using (var reader = new BinaryReader(new MemoryStream(fileData)))
            {
                var dataEncryptionFlagRawData = reader.ReadBytes(1);
                var dataEncryptionFlag = BitConverter.ToBoolean(dataEncryptionFlagRawData, 0);

                if (dataEncryptionFlag)
                {
                    var key = reader.ReadBytes(EncryptionKeyLength);
                    var cipherData = reader.ReadBytes(fileData.Length - dataEncryptionFlagRawData.Length - EncryptionKeyLength);
                    return CryptoUtility.AesDecrypt(cipherData, key);
                }
                else
                {
                    return reader.ReadBytes(fileData.Length - dataEncryptionFlagRawData.Length);
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
            return config?.PrimaryKey;
        }

        #endregion Methods
    }
}