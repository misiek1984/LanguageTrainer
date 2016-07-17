namespace MK.Data
{
    public interface IPersister<T>
    {
        T Load();
        
        void Save(T data);

        T Deserialize(string data);
        string Serialize(T data);

        T ImportFromFile(string path);
        void ExportToFile(string path, T data);
    }
}
