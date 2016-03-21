using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageTrainer.Entities
{
    public class IrregularVerbEntity
    {
        private string _Infinitive;
        public string Infinitive
        {
            get { return _Infinitive; }
            set { _Infinitive = String.IsNullOrEmpty(value) ? value : value.Trim(); }
        }

        private string _FirstForm;
        public string FirstForm
        {
            get { return _FirstForm; }
            set { _FirstForm = String.IsNullOrEmpty(value) ? value : value.Trim(); }
        }

        private string _SecondForm;
        public string SecondForm
        {
            get { return _SecondForm; }
            set { _SecondForm = String.IsNullOrEmpty(value) ? value : value.Trim(); }
        }

        private string _Translation;
        public string Translation
        {
            get { return _Translation; }
            set { _Translation = String.IsNullOrEmpty(value) ? value : value.Trim(); }
        }
    }
}
