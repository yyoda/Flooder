using System;
using System.Collections.Generic;
using Flooder.Plugins;

namespace Flooder.Events.FileLoad
{
    public class FileLoadDataSource : IDataSource
    {
        public FileLoadDataSource()
        {
            Options = new FileLoadDataSourceOption[0];
        }

        public FileLoadDataSource(IEnumerable<FileLoadDataSourceOption> options)
        {
            Options = options;
        }

        public IEnumerable<FileLoadDataSourceOption> Options { get; set; }
    }

    public class FileLoadDataSourceOption
    {
        public FileLoadDataSourceOption(string tag, string path, string file, string listener, string parser, int interval)
        {
            Tag      = tag;
            Path     = path;
            File     = file;
            Listener = Type.GetType(listener) ?? typeof (CsvFileLoadListener);
            Parser   = Type.GetType(parser)  ?? typeof (CsvParser);
            Interval = interval;
        }

        public string Tag { get; private set; }
        public string Path { get; private set; }
        public string File { get; private set; }
        public Type Listener { get; private set; }
        public Type Parser { get; private set; }
        public int Interval { get; private set; }
    }
}
