using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

//using Raven.Imports.Newtonsoft.Json;

namespace LanguageTrainer.Entities
{
    public class TranslationEntity : BaseEntity
    {
        [XmlIgnore]
        //[JsonIgnore]
        public int Id { get; set; }

        public int Language { get; set; }

        public bool IsSelected
        {
            get { return GetData<bool>("IsSelected"); }
            set { SetData("IsSelected", value); }
        }

        public string Translation
        {
            get { return GetData<string>("Translation"); }
            set { SetData("Translation",value); }
        }

        public string Translation2
        {
            get { return GetData<string>("Translation2"); }
            set { SetData("Translation2", value); }
        }

        public int GoodAnswers
        {
            get { return GetData<int>("GoodAnswers"); }
            set { SetData("GoodAnswers", value); }
        }
        public int BadAnswers
        {
            get { return GetData<int>("BadAnswers"); }
            set { SetData("BadAnswers", value); }
        }
        public bool WasLastAnswerGood
        {
            get { return GetData<bool>("WasLastAnswerGood"); }
            set { SetData("WasLastAnswerGood", value); }
        }
        //[JsonIgnore]
        [NotMapped]
        public int MeaningTotalAnswers
        {
            get { return GoodAnswers + BadAnswers; }
        }

        public int ReverseGoodAnswers
        {
            get { return GetData<int>("ReverseGoodAnswers"); }
            set { SetData("ReverseGoodAnswers", value); }
        }
        public int ReverseBadAnswers
        {
            get { return GetData<int>("ReverseBadAnswers"); }
            set { SetData("ReverseBadAnswers", value); }
        }
        public bool ReverseWasLastAnswerGood
        {
            get { return GetData<bool>("ReverseWasLastAnswerGood"); }
            set { SetData("ReverseWasLastAnswerGood", value); }
        }
        //[JsonIgnore]
        [NotMapped]
        public int ReverseMeaningTotalAnswers
        {
            get { return ReverseGoodAnswers + ReverseBadAnswers; }
        }

        public int SpellingGoodAnswers
        {
            get { return GetData<int>("SpellingGoodAnswers"); }
            set { SetData("SpellingGoodAnswers", value); }
        }
        public int SpellingBadAnswers
        {
            get { return GetData<int>("SpellingBadAnswers"); }
            set { SetData("SpellingBadAnswers", value); }
        }
        public bool SpellingWasLastAnswerGood
        {
            get { return GetData<bool>("SpellingWasLastAnswerGood"); }
            set { SetData("SpellingWasLastAnswerGood", value); }
        }
        //[JsonIgnore]
        [NotMapped]
        public int SpellingTotalAnswers
        {
            get { return SpellingGoodAnswers + SpellingBadAnswers; }
        }

        public int ReverseSpellingGoodAnswers
        {
            get { return GetData<int>("ReverseSpellingGoodAnswers"); }
            set { SetData("ReverseSpellingGoodAnswers", value); }
        }
        public int ReverseSpellingBadAnswers
        {
            get { return GetData<int>("ReverseSpellingBadAnswers"); }
            set { SetData("ReverseSpellingBadAnswers", value); }
        }
        public bool ReverseSpellingWasLastAnswerGood
        {
            get { return GetData<bool>("ReverseSpellingWasLastAnswerGood"); }
            set { SetData("ReverseSpellingWasLastAnswerGood", value); }
        }
        //[JsonIgnore]
        [NotMapped]
        public int ReverseSpellingTotalAnswers
        {
            get { return ReverseSpellingGoodAnswers + ReverseSpellingBadAnswers; }
        }

        public DateTime? Defined { get; set; }

        public DateTime? RecentlyUsed { get; set; }

        //[JsonIgnore]
        [NotMapped]
        public int TotalGoodAnswers
        {
            get { return GoodAnswers + SpellingGoodAnswers + ReverseGoodAnswers + ReverseSpellingGoodAnswers; }
        }
        //[JsonIgnore]
        [NotMapped]
        public int TotalBadAnswers
        {
            get { return BadAnswers + SpellingBadAnswers + ReverseBadAnswers + ReverseSpellingBadAnswers; }
        }
        //[JsonIgnore]
        [NotMapped]
        public int TotalAnswers
        {
            get { return TotalBadAnswers + TotalGoodAnswers; }
        }

        public TranslationEntity()
        {
            GoodAnswers = 0;
            BadAnswers = 0;
            WasLastAnswerGood = true;

            SpellingBadAnswers = 0;
            SpellingGoodAnswers = 0;
            SpellingWasLastAnswerGood = true;

            ReverseGoodAnswers = 0;
            ReverseBadAnswers = 0;
            ReverseWasLastAnswerGood = true;

            ReverseSpellingBadAnswers = 0;
            ReverseSpellingGoodAnswers = 0;
            ReverseSpellingWasLastAnswerGood = true;

            Translation = String.Empty;
            Translation2 = String.Empty;
        }
    }
}
