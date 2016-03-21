using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace LanguageTrainer.Entities
{
    public class ExpressionEntity : BaseEntity
    {
        #region Properties

        [XmlIgnore]
        public int Id { get; set; }

        public string Category
        {
            get { return GetData<string>("Category"); }
            set { SetData("Category", String.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        public string Expression
        {
            get { return GetData<string>("Expression"); }
            set { SetData("Expression", String.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        public DateTime? CreationDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        private List<TranslationEntity> _translations;
        public List<TranslationEntity> Translations
        {
            get { return _translations; }
            set
            {
                UnregisterCollection(_translations);
                _translations = value;
                RegisterCollection(value);
            }
        }

        public TranslationEntity this[int lang]
        {
            get
            {
                return (from te in Translations where te.Language == lang select te).FirstOrDefault();
            }
        }

        #endregion

        #region Constructor

        public ExpressionEntity()
        {
            Translations = new List<TranslationEntity>();
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            var ex = obj as ExpressionEntity;

            if (ex == null)
                return false;

            if (!String.IsNullOrEmpty(Expression) && !String.IsNullOrEmpty(ex.Expression))
            {
                if (!String.Equals(Expression, ex.Expression, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            if(Translations == null && ex.Translations != null || Translations != null && ex.Translations == null)
                return false;

            if(Translations.Count != ex.Translations.Count)
                return false;

            foreach (var te in Translations)
            {
                var res = (from t in ex.Translations
                           where t.Language == te.Language
                           select t).FirstOrDefault();

                if (res == null)
                    return false;

                if (!String.IsNullOrEmpty(te.Translation) && !String.IsNullOrEmpty(res.Translation))
                {
                    if (!String.Equals(te.Translation, res.Translation, StringComparison.InvariantCultureIgnoreCase))
                        return false;
                }

                if (!String.IsNullOrEmpty(te.Translation2) && !String.IsNullOrEmpty(res.Translation2))
                {
                    if (!String.Equals(te.Translation2, res.Translation2, StringComparison.InvariantCultureIgnoreCase))
                        return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return Id + " " + Category + " " + Expression;
        }

        #endregion
    }
}

