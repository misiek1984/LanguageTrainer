using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace MK.UI.WPF
{
    public class DispatcherService : IDispatcherService
    {
        public Dispatcher Dispatcher { get; private set; }

        public DispatcherService(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }
    }
}
