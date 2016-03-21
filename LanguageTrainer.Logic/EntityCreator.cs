using System.IO;
using System.Linq;
using System.Xml.Serialization;

using MK.Utilities;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic
{
    public class EntityCreator : IEntityCreator
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILanguagesProvider _languagesProvider;
        private XmlSerializer _serializer = new XmlSerializer(typeof(ExpressionEntity));

        public EntityCreator(IDateTimeProvider dateTimeProvider, ILanguagesProvider languagesProvider)
        {
            dateTimeProvider.NotNull("dateTimeProvider");
            languagesProvider.NotNull("languagesProvider");

            _dateTimeProvider = dateTimeProvider;
            _languagesProvider = languagesProvider;
        }

        #region IEntityCreator Members

        public ExpressionEntity CopyExpression(ExpressionEntity ex)
        {
            ex.NotNull("ex");

            var ms = new MemoryStream();
            _serializer.Serialize(ms, ex);
            ms.Position = 0;

            var copy = (ExpressionEntity)_serializer.Deserialize(ms);
            copy.Id = 0;
            copy.UpdateDate = null;
            copy.CreationDate = _dateTimeProvider.Current;
            copy.Translations.ForEach(t =>
            {
                t.BadAnswers = 0;
                t.GoodAnswers = 0;
                t.WasLastAnswerGood = true;

                t.SpellingBadAnswers = 0;
                t.SpellingGoodAnswers = 0;
                t.SpellingWasLastAnswerGood = true;

                t.ReverseBadAnswers = 0;
                t.ReverseGoodAnswers = 0;
                t.ReverseWasLastAnswerGood = true;

                t.ReverseSpellingBadAnswers = 0;
                t.ReverseSpellingGoodAnswers = 0;
                t.ReverseSpellingWasLastAnswerGood = true;

                t.RecentlyUsed = null;
                t.Defined = _dateTimeProvider.Current;
            });
            copy.MakeDirty();

            return copy;
        }
        public ExpressionEntity CreateExpression(bool allDefaultLanguages)
        {
            var ex = new ExpressionEntity();

            if (allDefaultLanguages)
            {
                foreach (var li in _languagesProvider.Languages)
                    ex.Translations.Add(new TranslationEntity { Language = li.Id });
            }

            ex.CreationDate = _dateTimeProvider.Current;
            ex.MakeDirty();

            return ex;
        }

        public ExpressionEntity CreateExpression(int languages)
        {
            var ex = CreateExpression(false);

            foreach (var li in _languagesProvider.Languages.Where(li => (li.Id & languages) != 0))
                ex.Translations.Add(CreateTranslation(li.Id));

            return ex;
        }

        public TranslationEntity CreateTranslation(int language)
        {
            var t = new TranslationEntity { Language = language };
            t.MakeDirty();

            return t;
        }

        #endregion
    }
}
