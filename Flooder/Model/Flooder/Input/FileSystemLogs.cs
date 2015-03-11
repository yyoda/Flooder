
using System;
using System.Collections.Generic;
using Flooder.Event.FileSystem;

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
            public FileSystemLog(string tag, string path, string file, string listener)
            {
                Tag      = tag;
                Path     = path;
                File     = file;
                Listener = Type.GetType(listener) ?? typeof(DefaultEventListener);
            }

            public string Tag { get; private set; }
            public string Path { get; private set; }
            public string File { get; private set; }
            public Type Listener { get; private set; }
        }
    }
}
