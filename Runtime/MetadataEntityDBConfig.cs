﻿// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEngine.Scripting;

namespace UniSharper.Data.Metadata
{
    /// <summary>
    /// MetadataEntityDBConfig contains the database configuration parameters of metadata entity.
    /// </summary>
    public class MetadataEntityDBConfig
    {
        /// <summary>
        /// The database local address.
        /// </summary>
        public const long DatabaseLocalAddress = 1L;

        /// <summary>
        /// The table name.
        /// </summary>
        public const string TableName = nameof(MetadataEntityDBConfig);

        /// <summary>
        /// The table primary key.
        /// </summary>
        public const string TablePrimaryKey = "EntityName";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataEntityDBConfig"/> class.
        /// </summary>
        [Preserve]
        public MetadataEntityDBConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataEntityDBConfig"/> class.
        /// </summary>
        /// <param name="entityName">The name of entity.</param>
        /// <param name="primaryKey">The primary key.</param>
        [Preserve]
        public MetadataEntityDBConfig(string entityName, string primaryKey)
        {
            EntityName = entityName;
            PrimaryKey = primaryKey;
        }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        /// <value>The name of the entity.</value>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the primary key.
        /// </summary>
        /// <value>The name of the primary key.</value>
        public string PrimaryKey { get; set; }
    }
}