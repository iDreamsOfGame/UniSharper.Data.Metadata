// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using ReSharp.Extensions;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal static class ExcelWorkbookFileHashMap
    {
        private const string FileName = ".ExcelWorkbookFileHashMap";

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

        public static Dictionary<string, ExcelWorkbookFileMetadata> Load()
        {
            var filePath = FilePath;
            if (!File.Exists(filePath))
                return new Dictionary<string, ExcelWorkbookFileMetadata>();

            try
            {
                var content = File.ReadAllText(filePath);
                return JsonMapper.ToObject<Dictionary<string, ExcelWorkbookFileMetadata>>(content);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return new Dictionary<string, ExcelWorkbookFileMetadata>();
            }
        }

        public static void Save(Dictionary<string, ExcelWorkbookFileMetadata> hashMap)
        {
            try
            {
                var content = JsonMapper.ToJson(hashMap);
                File.WriteAllText(FilePath, content);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
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
                Debug.LogError(e.ToString());
            }
        }
    }
}