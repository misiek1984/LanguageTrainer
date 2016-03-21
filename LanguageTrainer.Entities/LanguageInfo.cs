using System.Xml.Serialization;

namespace LanguageTrainer.Entities
{
    public class LanguageInfo : BaseEntity
    {
        public int Id { get; set; }

        public bool IsMain { get; set; }
        
        public string Culture { get; set; }

        public string BingTranslatorCode { get; set; }

        public string IconSource { get; set; }

        public string GoogleTranslatorCode { get; set; }

        public bool UseTranslation2 { get; set; }
    }
}
