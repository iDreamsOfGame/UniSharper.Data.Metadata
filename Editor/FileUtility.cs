// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.IO;
using System.Linq;

namespace UniSharperEditor.Data.Metadata
{
    internal static class FileUtility
    {
        public static string[] GetExcelFilePathCollection(string excelFilesFolderPath)
        {
            var paths = Directory.GetFiles(excelFilesFolderPath, "*.xls", SearchOption.AllDirectories);
            var xlsxFilePaths = Directory.GetFiles(excelFilesFolderPath, "*.xlsx", SearchOption.AllDirectories);
            paths = paths.Union(xlsxFilePaths).ToArray();
            return (from path in paths let fileName = Path.GetFileName(path) where !fileName.StartsWith("~$") select path).ToArray();
        }
    }
}