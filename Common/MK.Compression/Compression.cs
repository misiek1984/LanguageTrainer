using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

using MK.Utilities;

namespace MK.Compression
{
    public static class Compression
    {
        public static byte[] CompressDirectory(string directory)
        {
            directory.PathExists();
            using (var ms = new MemoryStream())
            {
                using (var file = ZipFile.Create(ms))
                {
                    file.BeginUpdate();

                    var stack = new Stack<string>();
                    stack.Push(directory);
                    while (stack.Count > 0)
                    {
                        var p = stack.Pop();
                        if (Directory.Exists(p))
                        {
                            foreach (var d in Directory.GetDirectories(p))
                            {
                                stack.Push(d);
                                file.AddDirectory(d);
                            }

                            foreach (var f in Directory.GetFiles(p))
                                file.Add(f);
                        }
                    }

                    file.CommitUpdate();
                }

                return ms.ToArray();
            }
        }

        public static byte[] CompressFiles(IEnumerable<string> files)
        {
            files.NotNull("files");

            using (var ms = new MemoryStream())
            {
                using (var file = ZipFile.Create(ms))
                {
                    file.BeginUpdate();
                    foreach (var f in files)
                        file.Add(f);
                    file.CommitUpdate();
                }

                return ms.ToArray();
            }
        }

        public static byte[] CompressProgram(string programPath, Predicate<string> tester = null)
        {
            return CompressAssembliesWithReferences(new[] { programPath }, tester);
        }

        public static byte[] CompressAssembliesWithReferences(IEnumerable<string> assemblies, Predicate<string> tester = null)
        {
            assemblies.NotNull("assemblies");

            var enumerable = assemblies as string[] ?? assemblies.ToArray();
            foreach(var path in enumerable)
                path.PathExists();
            
            using (var ms = new MemoryStream())
            {
                using (var zipStream = new ZipOutputStream(ms))
                {
                    zipStream.SetLevel(9);

                    foreach (var path in enumerable)
                    {
                        foreach (var a in ReferencedAssembliesLoader.GetReferencedAssemblies(path))
                            if (tester == null || tester(a.FullName))
                                AddToStream(a.Path, true, zipStream);

                        string configPath = Path.ChangeExtension(path, ".exe.config");
                        if (File.Exists(configPath))
                            AddToStream(configPath, true, zipStream);
                    }
                }

                return ms.ToArray();
            }
        }

        public static void Decompress(byte[] data, string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            using (var ms = new MemoryStream(data))
            {
                using (var s = new ZipInputStream(ms))
                {
                    ZipEntry entry;
                    while ((entry = s.GetNextEntry()) != null)
                    {
                        var path = Path.Combine(outputFolder, entry.Name);

                        if (entry.IsDirectory)
                            Directory.CreateDirectory(path);
                        else
                        {
                            CreateMissingDirectories(path);
                            using (var streamWriter = File.Create(path))
                            {
                                var size = 2048;
                                var bytes = new byte[size];
                                while (true)
                                {
                                    size = s.Read(bytes, 0, bytes.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(bytes, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CreateMissingDirectories(string path)
        {
            var dir = Path.GetDirectoryName(path);

            if (Directory.Exists(dir))
                return;

            CreateMissingDirectories(dir);
            Directory.CreateDirectory(dir);
        }

        private static void AddToStream(string path, bool flattenHiararchy, ZipOutputStream zipStream)
        {
            var fi = new FileInfo(path);
            var newEntry = new ZipEntry(flattenHiararchy ? Path.GetFileName(path) : ZipEntry.CleanName(path)) { Size = fi.Length };

            zipStream.PutNextEntry(newEntry);

            var buffer = new byte[4096];
            using (var streamReader = File.OpenRead(path))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }

            zipStream.CloseEntry();
        }
    }
}
