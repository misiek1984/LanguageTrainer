using System.Windows.Data;

namespace MK.UI.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class IsBitSetAndConverter: BaseIsBitSetConverter
    {
        public IsBitSetAndConverter() : base(true)
        {
        }
    }
}
