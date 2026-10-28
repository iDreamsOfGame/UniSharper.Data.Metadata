// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using ReSharp.Extensions;

namespace UniSharperEditor.Data.Metadata
{
    internal static class ExcelFilesSnapshot
    {
        private const string FileName = ".excel_files_snapshot";

        private static string FilePath
        {
            get
            {
                var settings = MetadataAssetSettings.Load(true);
                if (!settings)
                    return PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(MetadataAssetSettings.DefaultMetadataFolderPath, FileName));

                var assetPath = Path.Combine(settings.MetadataPersistentStorePath, FileName);
                return EditorPath.GetFullPath(assetPath);
            }
        }

        public static Dictionary<string, ExcelFileMetadata> Load()
        {
            var filePath = FilePath;
            if (!File.Exists(filePath))
                return new Dictionary<string, ExcelFileMetadata>();

            try
            {
                var content = File.ReadAllText(filePath);
                return JsonMapper.ToObject<Dictionary<string, ExcelFileMetadata>>(content);
            }
            catch (Exception e)
            {
                Debugger.LogError(e.ToString());
                return new Dictionary<string, ExcelFileMetadata>();
            }
        }

        public static void Save(Dictionary<string, ExcelFileMetadata> snapshot)
        {
            try
            {
                var content = JsonMapper.ToJson(snapshot);
                File.WriteAllText(FilePath, content);
            }
            catch (Exception e)
            {
                Debugger.LogError(e.ToString());
            }
        }

        public static void Delete()
        {
            var filePath = FilePath;
            if (!File.Exists(filePath))
                return;

            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Debugger.LogError(e.ToString());
            }
        }
    }
}