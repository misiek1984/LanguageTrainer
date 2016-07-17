using System;
using System.Windows.Threading;

namespace MK.UI.WPF
{
    public interface IDispatcherService
    {
        Dispatcher Dispatcher { get; }
    }
}
