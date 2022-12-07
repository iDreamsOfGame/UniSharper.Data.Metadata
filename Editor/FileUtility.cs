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
            return paths;
        }
    }
}