using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Policy;

namespace MK.Utilities
{
    public class ReferencedAssembliesLoader
    {
        #region Types

        [Serializable]
        private class InternalLoader : MarshalByRefObject
        {
            public IEnumerable<ReferencedAssemblyInfo> GetReferencedAssemblies(string programPath)
            {
                var dict = new Dictionary<string, ReferencedAssemblyInfo>();
                FindReferencedAssemblies(programPath, dict);

                List<ReferencedAssemblyInfo> res = new List<ReferencedAssemblyInfo>();

                foreach (var a in dict.Values)
                    res.Add(a);

                return res;
            }
        }

        #endregion

        #region Methods

        public static IEnumerable<ReferencedAssemblyInfo> GetReferencedAssemblies(IEnumerable<string> paths, bool loadInSeperateDomain = true)
        {
            var res = new List<ReferencedAssemblyInfo>();
            foreach (var path in paths)
                res.AddRange(GetReferencedAssemblies(path, loadInSeperateDomain));

            return res.Distinct(new ReferencedAssemblyInfo());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="programPath"></param>
        /// <returns>The First element in the tuple is an assembly path.
        /// The second is a full name of an assembly,
        /// The third is a level on which assembly was loaded.</returns>
        public static IEnumerable<ReferencedAssemblyInfo> GetReferencedAssemblies(string programPath, bool loadInSeperateDomain = true)
        {
            if (loadInSeperateDomain)
            {
                AppDomainSetup info = new AppDomainSetup();
                info.ApplicationBase = Path.GetDirectoryName(programPath);
                Evidence ev = AppDomain.CurrentDomain.Evidence;
                AppDomain domain = AppDomain.CreateDomain("ReferencedAssembliesLoaderDomain", ev, info);

                try
                {
                    InternalLoader internalLoader = (InternalLoader)(domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(InternalLoader).FullName));

                    return internalLoader.GetReferencedAssemblies(programPath);
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
            else
            {
                return new InternalLoader().GetReferencedAssemblies(programPath);
            }
        }

        private static void FindReferencedAssemblies(string programPath, Dictionary<string, ReferencedAssemblyInfo> assemblies)
        {
            string baseDir = Path.GetDirectoryName(programPath);
            int order = 0;

            Queue<ReferencedAssemblyInfo> queue = new Queue<ReferencedAssemblyInfo>();

            Assembly assembly = Assembly.LoadFrom(programPath);
            queue.Enqueue(new ReferencedAssemblyInfo()
                    { 
                        Name = assembly.GetName().Name,
                        FullName = assembly.FullName,
                        Level = 0
                    });

            while (queue.Count > 0)
            {
                ReferencedAssemblyInfo info = queue.Dequeue();
                if (!assemblies.ContainsKey(info.FullName))
                {
                    assembly = LoadAssembly(baseDir, info);
                    if (assembly != null)
                    {
                        info.Order = order++;
                        info.Path = assembly.Location;
                        assemblies.Add(info.FullName, info);

                        foreach (var reference in assembly.GetReferencedAssemblies())
                        {
                            queue.Enqueue(new ReferencedAssemblyInfo()
                            {
                                ReferencedByFullName = info.FullName,
                                Name = reference.Name,
                                FullName = reference.FullName,
                                Level = info.Level + 1,
                            });
                        }
                    }
                }
            }
        }

        private static Assembly LoadAssembly(string baseDir, ReferencedAssemblyInfo info)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(info.FullName);
            }
            catch
            {
                try
                {
                    assembly = Assembly.LoadFrom(Path.Combine(baseDir, info.Name + ".dll"));
                }
                catch
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(Path.Combine(baseDir, info.Name + ".exe"));
                    }
                    catch
                    {
                    }
                }
            }
            return assembly;
        }
    
        #endregion
    }

    [Serializable]
    public class ReferencedAssemblyInfo : IEqualityComparer<ReferencedAssemblyInfo>
    {
        public string ReferencedByFullName { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public int Level { get; set; }
        public int Order { get; set; }

        public override string ToString()
        {
            return FullName;
        }

        public bool Equals(ReferencedAssemblyInfo x, ReferencedAssemblyInfo y)
        {
            if (x == null || y == null)
                return false;

            return x.FullName == y.FullName;
        }

        public int GetHashCode(ReferencedAssemblyInfo obj)
        {
            return base.GetHashCode();
        }
    }
}
