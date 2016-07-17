using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace MK.Utilities
{
    public static class Encryption
    {
        public static byte[] GenerateIV()
        {
            using (var crypt = new RijndaelManaged())
            {
                crypt.GenerateIV();
                return crypt.IV;
            }
        }

        public static byte[] GenerateKey()
        {
            using (var crypt = new RijndaelManaged())
            {
                crypt.GenerateKey();
                return crypt.Key;
            }
        }

        public static byte[] GenerateIV(string seed)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(seed);
            List<byte> iv = new List<byte>();

            while (iv.Count < 16)
                iv.Add(bytes[iv.Count % bytes.Length]);
            return iv.ToArray();
        }

        public static byte[] GenerateKey(string seed)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(seed);
            List<byte> key = new List<byte>();

            while (key.Count < 32)
                key.Add(bytes[key.Count % bytes.Length]);

            return key.ToArray();
        }
                 
        public static string EncryptObject(byte[] iv, byte[] key, object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var crypt = new RijndaelManaged())
                {
                    crypt.IV = iv;
                    crypt.Key = key;
                    crypt.Padding = PaddingMode.Zeros;

                    using (var cryptoStream = new CryptoStream(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        var f = new BinaryFormatter();
                        f.Serialize(cryptoStream, obj);
                    }
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static T DecryptObject<T>(byte[] iv, byte[] key, string data)
        {
            using (var memoryStream = new MemoryStream(Convert.FromBase64String(data)))
            {
                using (var crypt = new RijndaelManaged())
                {
                    crypt.IV = iv;
                    crypt.Key = key;
                    crypt.Padding = PaddingMode.Zeros;

                    using (var cryptoStream = new CryptoStream(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        var f = new BinaryFormatter();
                        object o = f.Deserialize(cryptoStream);

                        return (T)o;
                    }
                }
            }
        }
    }
}
