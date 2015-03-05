
using System.Collections.Generic;

namespace Flooder.Core.Settings.In
{
    public class FileSystemSettings
    {
        public FileSystemSettings(IEnumerable<FileSystemSettingsDetail> details)
        {
            Details = details;
        }

        public IEnumerable<FileSystemSettingsDetail> Details { get; set; }

        public class FileSystemSettingsDetail
        {
            public FileSystemSettingsDetail(string tag, string path, string file, string format)
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
