// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using ReSharp.Data.IBoxDB;
using ReSharp.Extensions;
using ReSharp.Patterns;
using ReSharp.Security.Cryptography;
using UniSharper.Data.Metadata.Providers;
using UnityEngine;
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
        /// <summary>
        /// The crypto provider to encrypt/decrypt database data.
        /// </summary>
        public IDatabaseCryptoProvider CryptoProvider { get; set; } = new AesCbcCryptoProvider();
        
        /// <summary>
        /// The compression provider to compress/decompress database data.
        /// </summary>
        public IDatabaseCompressionProvider CompressionProvider { get; set; } = new DeflateCompressionProvider();

        /// <summary>
        /// Gets the real table name.
        /// </summary>
        /// <param name="tableName">The input table name. </param>
        /// <returns>The real table name. </returns>
        public static string GetRealTableName(string tableName) => tableName.Length > 32 ? Md5CryptoUtility.ComputeHashToHexString(tableName) : tableName;

        private readonly Dictionary<Type, byte[]> entityDBRawDataMap;

        private IBoxDBAdapter configDBAdapter;

        [Preserve]
        private MetadataManager()
        {
            entityDBRawDataMap = new Dictionary<Type, byte[]>();
        }

        /// <summary>
        /// Gets all entities of the entity type.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <returns>The list entities found in database.</returns>
        public T[] GetAllEntities<T>() where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            var tableName = GetRealTableName(entityType.Name);
            List<T> results = null;
            CreateDataDBAdapterForEntity<T>(entityType, context => results = context?.GetAll<T>(tableName));
            return results?.ToArray();
        }

        public T[] GetEntities<T>(string key, object value) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            var tableName = GetRealTableName(entityType.Name);
            List<T> results = null;
            CreateDataDBAdapterForEntity<T>(entityType, context => results = context?.Get<T>(tableName, key, value));
            return results?.ToArray();
        }

        public T[] GetEntities<T>(Dictionary<string, object> arguments, QueryLogicalOperator logicalOperator = QueryLogicalOperator.None) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            var tableName = GetRealTableName(entityType.Name);
            List<T> results = null;
            CreateDataDBAdapterForEntity<T>(entityType, context => results = context?.Get<T>(tableName, arguments, logicalOperator));
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
            var tableName = GetRealTableName(entityType.Name);
            var result = default(T);
            CreateDataDBAdapterForEntity<T>(entityType, context => result = context?.Get<T>(tableName, key));
            return result;
        }

        /// <summary>
        /// Initializes the specified configuration database data.
        /// </summary>
        /// <param name="configDBData">The configuration database data.</param>
        public void Initialize(byte[] configDBData)
        {
            if (configDBAdapter != null) 
                return;
            
            var rawData = GetDatabaseData(configDBData);
            configDBAdapter = new IBoxDBAdapter(rawData);
            configDBAdapter.EnsureTable<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, MetadataEntityDBConfig.TablePrimaryKey);
            configDBAdapter.Open();
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
        public void LoadEntityDatabase(Type entityType, byte[] rawData)
        {
            entityDBRawDataMap.AddUnique(entityType, GetDatabaseData(rawData));
        }

        public T[] QueryEntities<T>(string query, params object[] arguments) where T : MetadataEntity, new()
        {
            var entityType = typeof(T);
            var tableName = GetRealTableName(entityType.Name);
            List<T> results = null;
            CreateDataDBAdapterForEntity<T>(entityType, context => results = context?.Get<T>(tableName, query, arguments));
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
                if (configDBAdapter != null)
                {
                    configDBAdapter.Dispose();
                    configDBAdapter = null;
                }
            }

            base.Dispose(disposing);
        }

        private void CreateDataDBAdapterForEntity<T>(Type entityType, Action<IBoxDBAdapter> handler) where T : MetadataEntity, new()
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

            using var dataDBAdapter = new IBoxDBAdapter(dbRawData);
            var tableName = GetRealTableName(entityType.Name);
            var primaryKeyName = GetMetadataEntityDBPrimaryKey(tableName);
            dataDBAdapter.EnsureTable<T>(tableName, primaryKeyName);
            dataDBAdapter.Open();
            handler?.Invoke(dataDBAdapter);
        }

        private byte[] GetDatabaseData(byte[] fileData)
        {
            try
            {
                using var reader = new BinaryReader(new MemoryStream(fileData));
                var encryptionFlag = reader.ReadByte();
                
                if (encryptionFlag > 0)
                {
                    // Need to decrypt data.
                    var key = reader.ReadBytes(CryptoProvider.EncryptionKeyLength);
                    var compressionFlagRawData = reader.ReadBytes(1);
                    var compressionFlag = BitConverter.ToBoolean(compressionFlagRawData, 0);
                    var cipherData = reader.ReadBytes(fileData.Length - sizeof(byte) - CryptoProvider.EncryptionKeyLength - compressionFlagRawData.Length);
                    var content = CryptoProvider.Decrypt(cipherData, key);
                    return compressionFlag ? CompressionProvider.Decompress(content) : content;
                }
                else
                {
                    // No need to decrypt data.
                    var compressionFlagRawData = reader.ReadBytes(1);
                    var compressionFlag = BitConverter.ToBoolean(compressionFlagRawData, 0);
                    var content = reader.ReadBytes(fileData.Length - sizeof(byte) - compressionFlagRawData.Length);
                    return compressionFlag ? CompressionProvider.Decompress(content) : content;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        private bool GetDBRawDataForEntity(Type entityType, out byte[] rawData)
        {
            if (!entityDBRawDataMap.TryGetValue(entityType, out var value))
            {
                Debug.LogError($"No database raw data for the type {entityType.FullName}, you should load the database raw data for this type by invoking method 'LoadEntityDatabase'!");
                rawData = null;
                return false;
            }
            
            rawData = value;
            return true;
        }

        private MetadataEntityDBConfig GetMetadataEntityDBConfig(string entityName)
        {
            if (configDBAdapter == null)
            {
                throw new Exception("Method 'Initialize' should be invoke first!");
            }
            
            return configDBAdapter.Get<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, entityName);
        }

        private string GetMetadataEntityDBPrimaryKey(string entityName)
        {
            var config = GetMetadataEntityDBConfig(entityName);
            return config?.PrimaryKey;
        }
    }
}