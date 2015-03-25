using System;
using System.Collections.Generic;
using Flooder.Event.FileSystem.Parser;

namespace Flooder.Event.FileSystem
{
    public class FileSystemDataSource : IDataSource
    {
        public FileSystemDataSource()
        {
            Options = new FileSystemDataSourceOption[0];
        }

        public FileSystemDataSource(IEnumerable<FileSystemDataSourceOption> options)
        {
            Options = options;
        }

        public IEnumerable<FileSystemDataSourceOption> Options { get; set; }
    }

    public class FileSystemDataSourceOption
    {
        public FileSystemDataSourceOption(string tag, string path, string file, string listner, string parser)
        {
            Tag     = tag;
            Path    = path;
            File    = file;
            Listner = Type.GetType(listner) ?? typeof (DefaultEventListener);
            Parser  = Type.GetType(parser) ?? typeof (DefaultParser);
        }

        public string Tag { get; private set; }
        public string Path { get; private set; }
        public string File { get; private set; }
        public Type Listner { get; private set; } 
        public Type Parser { get; private set; }
    }
}
