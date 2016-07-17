using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MK.UI.WPF.VM
{
    public class SimpleInputVM : ViewModelBase
    {
        private string _input;

        public string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                Notify(() => Input);
            }
        }
    }
}
