using System.Collections.Generic;

using MK.Utilities;

namespace MK.Data.Xml
{
    public class XMLDictPersister<K, V> : IDictPersister<K, V>
    {
        #region Internal Definitions

        public class Pair<PK, PV> 
        {
            public PK Key { get; set; }
            public PV Value { get; set; }
        }

        public class Wrapper<WK, WV> 
        {
            public List<Pair<WK, WV>> Collection { get; set; }
        }

        #endregion

        #region Fields & Properties

        private XMLPersister<Wrapper<K, V>> _persister = new XMLPersister<Wrapper<K, V>>();

        public string Path { get; private set; }

        #endregion

        #region Public Methods

        public void Init(string path)
        {
            path.PathExists();

            Path = path;
            _persister.Init(path);
        }

        public IDictionary<K, V> Load()
        {
            return ImportFromFile(System.IO.Path.Combine(Path, "DictionaryKey" + typeof(K).Name + "Value" +typeof(V).Name));
        }
        public void Save(IDictionary<K, V> data)
        {
            data.NotNull("data");

            ExportToFile(System.IO.Path.Combine(Path, "DictionaryKey" + typeof(K).Name + "Value" +typeof(V).Name), data);
        }

        public IDictionary<K, V> Deserialize(string data)
        {
            data.NotNullAndEmpty("data");

            var wrapper = _persister.Deserialize(data);

            return ExtracFromWrapper(wrapper);
        }
        public string Serialize(IDictionary<K, V> data)
        {
            var wrapper = PrepareWrapper(data);

            return _persister.Serialize(wrapper);
        }

        public IDictionary<K, V> ImportFromFile(string path)
        {
            var wrapper = _persister.ImportFromFile(path);

            return ExtracFromWrapper(wrapper);
        }
        public void ExportToFile(string path, IDictionary<K, V> data)
        {
            var wrapper = PrepareWrapper(data);

            _persister.ExportToFile(path, wrapper);
        }

        #endregion

        #region Private Methods

        private static Wrapper<K, V> PrepareWrapper(IEnumerable<KeyValuePair<K, V>> data)
        {
            data.NotNull("data");

            var wrapper = new Wrapper<K, V>();
            wrapper.Collection = new List<Pair<K, V>>();

            foreach (var p in data)
                wrapper.Collection.Add(new Pair<K, V> { Key = p.Key, Value = p.Value });

            return wrapper;
        }

        private static IDictionary<K, V> ExtracFromWrapper(Wrapper<K, V> wrapper)
        {
            if (wrapper.Collection == null)
                return new Dictionary<K, V>();

            var res = new Dictionary<K, V>();
            wrapper.Collection.ForEach(p => res.Add(p.Key, p.Value));
            return res;
        }

        #endregion
    }
}
