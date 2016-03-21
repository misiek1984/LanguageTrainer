using MK.UI.WPF;

namespace LanguageTrainer.VM
{
    public class LanguageVM : ViewModelBase
    {
        public int Lang { get; set; }

        public LanguageVM(ViewModelBase chooser)
            : base(chooser)
        {
        }
    }
}
