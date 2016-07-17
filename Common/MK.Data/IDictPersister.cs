using System.Collections.Generic;

namespace MK.Data
{
    public interface IDictPersister<K, V>
    {
        IDictionary<K, V> Load();
        void Save(IDictionary<K, V> data);

        IDictionary<K, V> Deserialize(string data);
        string Serialize(IDictionary<K, V> data);
        
        IDictionary<K, V> ImportFromFile(string path);
        void ExportToFile(string path, IDictionary<K, V> data);
    }
}
