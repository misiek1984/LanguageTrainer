using System;
namespace LanguageTrainer.Entities
{
    public enum TypeOfExpressions
    {
        All,
        New,
        AllWithoutNew,
        WithLastWrongAnswer
    }

    public class TestConfiguration
    {
        public int Language { get; set; }

        public string Category { get; set; }

        public int NumberOfExpressionsInTest { get; set; }


        public TypeOfExpressions TypeOfExpressions { get; set; }


        public bool OnlySelected { get; set; }

        public bool RecentlyUsed { get; set; }

        public bool CheckTheSpelling { get; set; }

        public bool Reverse { get; set; }

        public DateTime? CreatedFromDate { get; set; }

        public DateTime? CreateToDate { get; set; }
    }
}
