// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniSharperEditor.Data.Metadata
{
    internal static class ExcelWorkbookFileHashMap
    {
        private const string FileName = ".excel_workbook_file_hash_map";

        private static readonly string FilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(MetadataAssetSettings.MetadataFolderPath, FileName));

        public static Dictionary<string, string> Load()
        {
            if (!File.Exists(FilePath))
                return new Dictionary<string, string>();
            
            using var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryFormatter();
            return reader.Deserialize(stream) as Dictionary<string, string>;
        }

        public static void Save(Dictionary<string, string> hashMap)
        {
            using var stream = File.Open(FilePath, FileMode.OpenOrCreate);
            var writer = new BinaryFormatter();
            writer.Serialize(stream, hashMap);
        }
    }
}