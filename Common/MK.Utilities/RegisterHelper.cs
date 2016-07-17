using System;
using System.Text;

using Microsoft.Win32;

namespace MK.Utilities
{
    public static class RegisterHelper
    {
        public static bool AddOpenWithToContextMenu(string commandName, string path, int noOfArguments = 1)
        {
            if (!Security.IsAdmin())
                return false;

            var key = String.Format("*\\Shell\\{0}\\command", commandName);

            using (var rk = Registry.ClassesRoot.CreateSubKey(key))
            {
                var sb = new StringBuilder();
                sb.Append("\"" + path + "\"");
                for (var i = 1; i <= noOfArguments; ++i)
                    sb.AppendFormat(" \"%{0}\"", i);

                rk.SetValue(String.Empty, sb.ToString());
            }

            return true;
        }

        public static bool DeleteOpenWithFromContextMenu(string commandName)
        {
            if (!Security.IsAdmin())
                return false;

            var key = String.Format("*\\Shell\\{0}\\command", commandName);
            Registry.ClassesRoot.DeleteSubKey(key);

            key = String.Format("*\\Shell\\{0}", commandName);
            Registry.ClassesRoot.DeleteSubKey(key);

            return true;
        }

        public static void SetRegKeyMultivalue(string keyName, string valueName, string[] value)
        {
            Registry.SetValue(keyName, valueName, value, RegistryValueKind.MultiString);
        }
    }
}
