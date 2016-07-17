using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Policy;

namespace MK.Utilities
{
    public class AssemblyTester
    {
        #region Types

        [Serializable]
        private class InternalLoader : MarshalByRefObject
        {
            public bool IsAssemblyIntalled(string fullName, bool onlyInGAC)
            {
                try
                {
                    var asm = Assembly.Load(fullName);

                    if (onlyInGAC)
                        return asm.GlobalAssemblyCache;

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region Methods

        public static bool IsAssemblyIntalled(string fullName, bool onlyInGAC)
        {
            AppDomain domain = AppDomain.CreateDomain("AssemblyTesterDomain");

            try
            {
                InternalLoader internalLoader = (InternalLoader)(domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(InternalLoader).FullName));
                return internalLoader.IsAssemblyIntalled(fullName, onlyInGAC);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        private static void FindReferencedAssemblies(string baseDir, Dictionary<string, Assembly> assemblies, AssemblyName assemblyToSearch)
        {
            string name = assemblyToSearch.FullName;

            if (!name.Contains("mscorlib") &&
                !name.Contains("System") &&
                !assemblies.ContainsKey(name))
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(name);
                }
                catch
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(Path.Combine(baseDir, assemblyToSearch.Name + ".dll"));
                    }
                    catch
                    {
                        try
                        {
                            assembly = Assembly.LoadFrom(Path.Combine(baseDir, assemblyToSearch.Name + ".exe"));
                        }
                        catch
                        {
                        }
                    }
                }

                if (assembly != null)
                {
                    assemblies.Add(assemblyToSearch.FullName, assembly);

                    var references = assembly.GetReferencedAssemblies();

                    foreach (var reference in references)
                        FindReferencedAssemblies(baseDir, assemblies, reference);
                }
            }
        }
    
        #endregion
    }
}
