using System;
using System.Collections.Generic;

using MK.Utilities;

namespace MK.Data.Xml
{
    public class XMLEnumerablePersister<T> : IEnumerablePersister<T>
    {
        #region Internal Definitions

        public class Wrapper<WT> 
        {
            public List<WT> Collection { get; set; }
        }

        #endregion

        #region Fields & Properties

        private XMLPersister<Wrapper<T>> _persister = new XMLPersister<Wrapper<T>>();

        public string Path { get; private set; }

        #endregion

        #region Public Methods

        public void Init(string dir)
        {
            dir.PathExists();

            Path = dir;
            _persister.Init(dir);
        }

        public IEnumerable<T> Load()
        {
            return ImportFromFile(System.IO.Path.Combine(Path, "EnumerationOf" + typeof(T).Name));
        }
        public void Save(IEnumerable<T> data)
        {
            data.NotNull("data");

            ExportToFile(System.IO.Path.Combine(Path, "EnumerationOf" + typeof(T).Name), data);
        }

        public IEnumerable<T> Deserialize(string data)
        {
            Wrapper<T> wrapper = _persister.Deserialize(data);

            if (wrapper.Collection == null)
                return new List<T>();

            return wrapper.Collection;
        }

        public string Serialize(IEnumerable<T> data)
        {
            data.NotNull("data");

            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Collection = new List<T>();

            foreach (var e in data)
                wrapper.Collection.Add(e);

            return _persister.Serialize( wrapper);
        }

        public IEnumerable<T> ImportFromFile(string path)
        {
            Wrapper<T> wrapper = _persister.ImportFromFile(path);

            if (wrapper.Collection == null)
                return new List<T>();

            return wrapper.Collection;
        }
        public void ExportToFile(string path, IEnumerable<T> data)
        {
            data.NotNull("data");

            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Collection = new List<T>();

            foreach (var e in data)
                wrapper.Collection.Add(e);

            _persister.ExportToFile(path, wrapper);
        }

        #endregion
    }
}
