using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Net;
using System.ServiceProcess;
using System.IO;
using System.Security.AccessControl;


namespace MK.Utilities
{
    public class Utils
    {
        private static bool CheckPrivileges(int port)
        {
            try
            {
                using (var httpListener = new HttpListener())
                {
                    httpListener.Prefixes.Add("http://+:" + port + "/");
                    httpListener.Start();
                }

                return true;
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode != 5)
                {
                    throw;
                }
            }

            return false;
        }
        /// <summary>
        /// Na podstawie klasy NonAdminHttp z Raven DB
        /// </summary>
        /// <param name="port"></param>
        public static void GrantHttpPrivilegesIfNeccessary(int port)
        {
            if (!CheckPrivileges(port))
            {
                string args;
                string cmd;

                //http://msdn.microsoft.com/en-us/library/ms733768.aspx
                if (Environment.OSVersion.Version.Major > 5)
                {
                    cmd = "netsh";
                    args = string.Format("http add urlacl url=http://+:{0}/ user=\"{1}\"", port, WindowsIdentity.GetCurrent().Name);
                }
                else
                {
                    cmd = "httpcfg";
                    args = string.Format("set urlacl /u http://+:{0}/ /a D:(A;;GX;;;\"{1}\")", port, WindowsIdentity.GetCurrent().User);
                }

                var psi = new ProcessStartInfo();
                psi.Verb = "runas";
                psi.Arguments = args;
                psi.FileName = cmd;

                Process process = Process.Start(psi);
                process.WaitForExit();
            }
        }


        public static void GrantFullAccessForCurrentUser(string logPath)
        {
            var ac = File.GetAccessControl(logPath);
            ac.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(logPath, ac);
        }


        public static bool TryStopService(string serviceName, string machineName = "localhost", bool ignoreExceptions = true)
        {
            try
            {
                if (Validators.ServiceExists(serviceName))
                {
                    using (var ctr = new ServiceController(serviceName, machineName))
                    {
                        if (ctr.Status == ServiceControllerStatus.Stopped)
                            return true;

                        if (ctr.Status == ServiceControllerStatus.Running)
                        {
                            ctr.Stop();
                            return true;
                        }
                    }
                }
            }
            catch
            {
                if (!ignoreExceptions)
                    throw;
            }

            return false;
        }
        public static bool TryStartService(string serviceName, string machineName = "localhost", bool ignoreExceptions = true)
        {
            try
            {
                if (Validators.ServiceExists(serviceName))
                {
                    using (var ctr = new ServiceController(serviceName, machineName))
                    {
                        if (ctr.Status == ServiceControllerStatus.Running)
                            return true;
                        
                        if (ctr.Status == ServiceControllerStatus.Stopped)
                        {
                            ctr.Start();
                            return true;
                        }
                    }
                }
            }
            catch
            {
                if (!ignoreExceptions)
                    throw;
            }

            return false;
        }
        public static void WaitForServiceStatus(ServiceControllerStatus status, string serviceName, string machineName = "localhost", bool ignoreExceptions = true)
        {
            try
            {
                if (Validators.ServiceExists(serviceName))
                {
                    using (var ctr = new ServiceController(serviceName, machineName))
                    {
                        ctr.WaitForStatus(status);
                    }
                }
            }
            catch
            {
                if (!ignoreExceptions)
                    throw;
            }
        }
        public static ServiceControllerStatus? GetServiceStatus(string serviceName, string machineName = "localhost", bool ignoreExceptions = true)
        {
            try
            {
                if (Validators.ServiceExists(serviceName))
                {
                    using (var ctr = new ServiceController(serviceName, machineName))
                    {
                        return ctr.Status;
                    }
                }
            }
            catch
            {
                if (!ignoreExceptions)
                    throw;
            }

            return null; 
        }


        public static byte[] ReadAllBytes(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        public class ProcessorUtilizationInfo
        {
            public TimeSpan LastProcessorTime { get; set; }
            public DateTime LastCheck { get; set; }
            public double Value { get; set; }
            public Process Process { get; set; }
        }
        public static ProcessorUtilizationInfo GetProcessorUtilization(ProcessorUtilizationInfo info)
        {
            var now = DateTime.Now;
            var elapsed = now - info.LastCheck;
            var processorTime = (info.Process.TotalProcessorTime - info.LastProcessorTime);

            info.LastProcessorTime = info.Process.TotalProcessorTime;
            info.LastCheck = now;
            info.Value = processorTime.TotalMilliseconds / elapsed.TotalMilliseconds / Environment.ProcessorCount;

            return info;
        }
    }
}
