
using System.Collections.Generic;

namespace Flooder.Model.Flooder.Input
{
    public class FileSystemLogs
    {
        public FileSystemLogs(IEnumerable<FileSystemLog> details)
        {
            Details = details;
        }

        public IEnumerable<FileSystemLog> Details { get; set; }

        public class FileSystemLog
        {
            public FileSystemLog(string tag, string path, string file, string format)
            {
                Tag    = tag;
                Path   = path;
                File   = file;
                Format = format;
            }

            public string Tag { get; private set; }
            public string Path { get; private set; }
            public string File { get; private set; }
            public string Format { get; private set; }
        }
    }
}
