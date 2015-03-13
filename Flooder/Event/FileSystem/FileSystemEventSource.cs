using System;
using System.Collections.Generic;
using Flooder.Event.FileSystem.Parser;

namespace Flooder.Event.FileSystem
{
    public class FileSystemEventSource : IEventSource
    {
        public FileSystemEventSource()
        {
            Details = new[] { new FileSystemEventSourceDetail() };
        }

        public FileSystemEventSource(IEnumerable<FileSystemEventSourceDetail> details)
        {
            Details = details;
        }

        public IEnumerable<FileSystemEventSourceDetail> Details { get; set; }
    }

    public class FileSystemEventSourceDetail
    {
        public FileSystemEventSourceDetail()
        {
        }

        public FileSystemEventSourceDetail(string tag, string path, string file, string parser)
        {
            Tag    = tag;
            Path   = path;
            File   = file;
            Parser = Type.GetType(parser) ?? typeof(DefaultParser);
        }

        public string Tag { get; private set; }
        public string Path { get; private set; }
        public string File { get; private set; }
        public Type Parser { get; private set; }
    }
}
