using System;
using System.Collections.Generic;
using Flooder.Event.FileSystem.Payloads;

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
            public FileSystemLog(string tag, string path, string file, string payload)
            {
                Tag      = tag;
                Path     = path;
                File     = file;
                Payload  = Type.GetType(payload) ?? typeof(DefaultPayload);
            }

            public string Tag { get; private set; }
            public string Path { get; private set; }
            public string File { get; private set; }
            public Type Payload { get; private set; }
        }
    }
}
