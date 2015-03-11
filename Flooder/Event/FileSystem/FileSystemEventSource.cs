using System;
using System.Collections.Generic;
using Flooder.Event.FileSystem.Payloads;

namespace Flooder.Event.FileSystem
{
    public class FileSystemEventSource : IEventSource
    {
        public FileSystemEventSource(IEnumerable<FileSystemEventSourceDetail> details)
        {
            Details = details;
        }

        public IEnumerable<FileSystemEventSourceDetail> Details { get; set; }

    }

    public class FileSystemEventSourceDetail
    {
        public FileSystemEventSourceDetail(string tag, string path, string file, string payload)
        {
            Tag     = tag;
            Path    = path;
            File    = file;
            Payload = Type.GetType(payload) ?? typeof(DefaultPayload);
        }

        public string Tag { get; private set; }
        public string Path { get; private set; }
        public string File { get; private set; }
        public Type Payload { get; private set; }
    }
}
