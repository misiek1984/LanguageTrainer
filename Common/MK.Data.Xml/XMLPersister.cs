using System.Xml.Serialization;
using System.IO;

using MK.Utilities;

namespace MK.Data.Xml
{
    public class XMLPersister<T> : IPersister<T>
    {
        public string Path { get; private set;  }

        public XMLPersister()
        {}

        public XMLPersister(string path)
        {
            Init(path);
        }

        public void Init(string path)
        {
            path.PathExists();

            Path = path;
        }

        public T Load()
        {
            return ImportFromFile(System.IO.Path.Combine(Path, typeof(T).Name));
        }
        public void Save(T data)
        {
            data.NotNull("data");

            ExportToFile(System.IO.Path.Combine(Path, typeof(T).Name), data);
        }

        public T Deserialize(string data)
        {
            data.NotNullAndEmpty("data");

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        public string Serialize(T data)
        {
            data.NotNull("data");

            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, data);

                stream.Position = 0;
                using(var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public T ImportFromFile(string path)
        {
            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(T));
                using (FileStream stream = File.OpenRead(path))
                {
                    return (T)serializer.Deserialize(stream);
                }
            }

            return default(T);
        }
        public void ExportToFile(string path, T data)
        {
            data.NotNull("data");

            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, data);
            }
        }
    }
 }
