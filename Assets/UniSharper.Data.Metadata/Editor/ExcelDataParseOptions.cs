// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata
{
    /// <summary>
    /// The information struct of Excel data parse options.
    /// </summary>
    public readonly struct ExcelDataParseOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelDataParseOptions"/> struct.
        /// </summary>
        /// <param name="entityPropertyCommentRowIndex">The row index of entity property comment.</param>
        /// <param name="entityPropertyTypeRowIndex">The row index of entity property type.</param>
        /// <param name="entityPropertyNameRowIndex">The row index of entity property name.</param>
        /// <param name="entityDataStartingRowIndex">The row index of entity data starting.</param>
        /// <param name="arrayElementSeparator">The array element separator.</param>
        public ExcelDataParseOptions(int entityPropertyCommentRowIndex,
            int entityPropertyTypeRowIndex,
            int entityPropertyNameRowIndex,
            int entityDataStartingRowIndex,
            char arrayElementSeparator)
        {
            EntityPropertyCommentRowIndex = entityPropertyCommentRowIndex;
            EntityPropertyTypeRowIndex = entityPropertyTypeRowIndex;
            EntityPropertyNameRowIndex = entityPropertyNameRowIndex;
            EntityDataStartingRowIndex = entityDataStartingRowIndex;
            ArrayElementSeparator = arrayElementSeparator;
        }

        /// <summary>
        /// Gets the row index of entity property comment.
        /// </summary>
        public int EntityPropertyCommentRowIndex { get; }

        /// <summary>
        /// Gets the row index of entity property type.
        /// </summary>
        public int EntityPropertyTypeRowIndex { get; }

        /// <summary>
        /// Gets the row index of entity property name.
        /// </summary>
        public int EntityPropertyNameRowIndex { get; }

        /// <summary>
        /// Gets the starting row index of entity data.
        /// </summary>
        public int EntityDataStartingRowIndex { get; }

        /// <summary>
        /// Gets the array element separator.
        /// </summary>
        public char ArrayElementSeparator { get; }
    }
}