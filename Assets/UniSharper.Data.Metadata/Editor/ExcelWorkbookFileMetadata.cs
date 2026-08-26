// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata
{
    internal struct ExcelWorkbookFileMetadata : IEquatable<ExcelWorkbookFileMetadata>
    {
        public long FileLength { get; set; }

        public long LastWriteTimestamp { get; set; }

        public bool Equals(ExcelWorkbookFileMetadata other) => FileLength == other.FileLength && LastWriteTimestamp == other.LastWriteTimestamp;

        public override bool Equals(object obj) => obj is ExcelWorkbookFileMetadata other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(FileLength, LastWriteTimestamp);
    }
}