using System.Collections.Generic;

namespace MK.Data
{
    public interface IEnumerablePersister<T>
    {
        IEnumerable<T> Load();
        void Save(IEnumerable<T> data);

        IEnumerable<T> Deserialize(string data);
        string Serialize(IEnumerable<T> data);

        IEnumerable<T> ImportFromFile(string path);
        void ExportToFile(string path, IEnumerable<T> data);
    }
}
