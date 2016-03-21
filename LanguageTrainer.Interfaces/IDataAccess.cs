using System;
using System.Collections.Generic;

using LanguageTrainer.Entities;


namespace LanguageTrainer.Interfaces
{
    public class GetPageParameters
    {
        public int Index { get; set; }
        public int PageSize { get; set; }
        public bool ShowEmpty { get; set; }
        public bool OnlySelected { get; set; }
        public int Lang { get; set; }
        public bool MatchWholeWord { get; set; }
        public string TextToFind { get; set; }
        public string Category { get; set; }

        public GetPageParameters()
        {
            ShowEmpty = true;
            Lang = -1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GetPageParameters)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Index;
                hashCode = (hashCode * 397) ^ PageSize;
                hashCode = (hashCode * 397) ^ ShowEmpty.GetHashCode();
                hashCode = (hashCode * 397) ^ OnlySelected.GetHashCode();
                hashCode = (hashCode * 397) ^ Lang;
                hashCode = (hashCode * 397) ^ MatchWholeWord.GetHashCode();
                hashCode = (hashCode * 397) ^ (TextToFind != null ? TextToFind.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Category != null ? Category.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(GetPageParameters other)
        {
            return Index == other.Index && PageSize == other.PageSize && ShowEmpty.Equals(other.ShowEmpty) && OnlySelected.Equals(other.OnlySelected) && Lang == other.Lang && MatchWholeWord.Equals(other.MatchWholeWord) && string.Equals(TextToFind, other.TextToFind) && string.Equals(Category, other.Category);
        }

    }

    public interface IDataAccess
    {
        int NumberOfExpressions { get; }

        int GetNumberOfExpressions(GetPageParameters parameters);


        void Save(ExpressionEntity ex);

        void Save(IEnumerable<ExpressionEntity> expressions);

        void Remove(ExpressionEntity ex);


        bool IsExpressionUnique(ExpressionEntity expressionToCheck);

        bool IsTranslationUnique(TranslationEntity expressionToCheck);

        IEnumerable<ExpressionEntity> GetPage(GetPageParameters parameters);

        IEnumerable<ExpressionEntity> GetPage(int index, int pageSize, bool showEmpty = true, bool onlySelected = false, int lang = -1);

        IEnumerable<ExpressionEntity> GetAllExpressions(DateTime? since = null);

        IList<string> GetCategories();

        IList<ExpressionEntity> Find(bool matchWholeWord, string textToFind, string category, int lang = -1);

        IDictionary<int, int> CountExpressionsByLanguage();

        IDictionary<int, int> CountSelectedByLanguage();

        void Configure(string option, object value);
    }

}
