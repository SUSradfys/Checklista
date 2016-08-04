using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Checklist
{
    class QueryContents
    {
        public static string FindInFiles(string searchTerm)
        {
            // Modify this path as necessary.
            string startFolder = @"\\Mtdb001\va_data$\Filedata\ProgramData\Vision\VD\";
            string fileExtension = ".dps";

            // Take a snapshot of the file system.
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            // This method assumes that the application has discovery permissions
            // for all folders under the specified path.
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            // Search the contents of each file.
            // A regular expression created with the RegEx class
            // could be used instead of the Contains method.
            // queryMatchingFiles is an IEnumerable<string>.
            var queryMatchingFiles =
                from file in fileList
                where file.Extension == fileExtension
                let fileText = GetFileText(file.FullName)
                where fileText.Contains(searchTerm)
                select file.FullName;

            // Execute the query.
            foreach (string filename in queryMatchingFiles)
            {
                return filename.Replace(startFolder,"").Replace(fileExtension,"");
            }

            return "Okänd";

        }

        // Read the contents of the file.
        static string GetFileText(string name)
        {
            string fileContents = String.Empty;

            // If the file has been deleted since we took 
            // the snapshot, ignore it and return the empty string.
            if (System.IO.File.Exists(name))
            {
                fileContents = System.IO.File.ReadAllText(name);
            }
            return fileContents;
        }
    }
}
