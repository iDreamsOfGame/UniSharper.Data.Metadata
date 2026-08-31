// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using ReSharp.Extensions;
using UniSharper;

namespace UniSharperEditor.Data.Metadata
{
    internal struct ExcelFileMetadata : IEquatable<ExcelFileMetadata>
    {
        public long FileLength { get; set; }

        public long LastWriteTimestamp { get; set; }

        public bool Equals(ExcelFileMetadata other) => FileLength == other.FileLength && LastWriteTimestamp == other.LastWriteTimestamp;

        public override bool Equals(object obj) => obj is ExcelFileMetadata other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(FileLength, LastWriteTimestamp);

        public static ExcelFileMetadata Create(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return new ExcelFileMetadata
            {
                FileLength = fileInfo.Length,
                LastWriteTimestamp = GetExcelLastWriteTimestamp(filePath, fileInfo)
            };
        }

        private static long GetExcelLastWriteTimestamp(string filePath, FileInfo fileInfo)
        {
            var extension = Path.GetExtension(filePath);
            if (extension == FileExtensions.ExcelWorkbookFile)
            {
                var modifiedTime = GetXlsxModifiedTime(filePath);
                if (modifiedTime != null)
                    return modifiedTime.Value.ToUnixTimestamp();
            }
            
            return fileInfo.LastWriteTimeUtc.ToUnixTimestamp();
        }

        private static DateTime? GetXlsxModifiedTime(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                const string targetEntryName = "docProps/core.xml";
                
                using var zip = ZipFile.OpenRead(filePath);
                var entry = zip.GetEntry(targetEntryName);
                if (entry == null) 
                    return null;

                using var stream = entry.Open();
                using var reader = XmlReader.Create(stream, new XmlReaderSettings 
                { 
                    IgnoreWhitespace = true, 
                    IgnoreComments = true 
                });
                
                const string dcTermsNs = "http://purl.org/dc/terms/";
                const string modifiedName = "modified";

                var dcTermsNsSpan = dcTermsNs.AsSpan();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element 
                        && reader.LocalName.AsSpan().SequenceEqual(modifiedName.AsSpan()) 
                        && reader.NamespaceURI.AsSpan().SequenceEqual(dcTermsNsSpan))
                    {
                        var value = reader.ReadElementContentAsString();
                    
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                                return dt.ToUniversalTime();
                        }
                        
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        
            return null;
        }
    }
}