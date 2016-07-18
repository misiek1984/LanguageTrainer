using System;
using System.Linq;
using System.ServiceProcess;

namespace MK.Utilities
{
    public class Validators
    {
        public static bool ServiceExists(string serviceName)
        {
            return ServiceController.GetServices().Where(s => s.ServiceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }
    }
}
