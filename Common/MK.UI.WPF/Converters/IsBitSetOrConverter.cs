using System.Windows.Data;

namespace MK.UI.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class IsBitSetOrConverter: BaseIsBitSetConverter
    {
        public IsBitSetOrConverter() : base(false)
        {
        }
    }
}
