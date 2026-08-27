// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using ReSharp.Extensions;
using UniSharper;

namespace UniSharperEditor.Data.Metadata
{
    internal struct ExcelWorkbookFileMetadata : IEquatable<ExcelWorkbookFileMetadata>
    {
        public long FileLength { get; set; }

        public long LastWriteTimestamp { get; set; }

        public bool Equals(ExcelWorkbookFileMetadata other) => FileLength == other.FileLength && LastWriteTimestamp == other.LastWriteTimestamp;

        public override bool Equals(object obj) => obj is ExcelWorkbookFileMetadata other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(FileLength, LastWriteTimestamp);

        public static ExcelWorkbookFileMetadata Create(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return new ExcelWorkbookFileMetadata
            {
                FileLength = fileInfo.Length,
                LastWriteTimestamp = GetExcelLastWriteTimestamp(filePath, fileInfo)
            };
        }

        private static long GetExcelLastWriteTimestamp(string filePath, FileInfo fileInfo)
        {
            var extension = Path.GetExtension(filePath);
            if (extension == FileExtensions.ExcelXFile)
            {
                var modifiedTime = GetXlsxModifiedTime(filePath);
                if (modifiedTime != null)
                    return modifiedTime.Value.ToUnixTimestamp();
            }
            
            return fileInfo.LastWriteTimeUtc.ToUnixTimestamp();
        }

        private static DateTime? GetXlsxModifiedTime(string filePath)
        {
            using var zip = ZipFile.OpenRead(filePath);
            var entry = zip.GetEntry("docProps/core.xml");
            if (entry == null) return null;

            using var stream = entry.Open();
            var doc = XDocument.Load(stream);
            var ns = XNamespace.Get("http://purl.org/dc/terms/");

            var modified = doc.Descendants(ns + "modified").FirstOrDefault()?.Value;
            return modified != null ? DateTime.Parse(modified).ToUniversalTime() : null;
        }
    }
}