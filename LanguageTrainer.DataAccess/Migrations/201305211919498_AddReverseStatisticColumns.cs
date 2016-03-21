namespace LanguageTrainer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReverseStatisticColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("TranslationEntities", "ReverseGoodAnswers", c => c.Int(false, defaultValue: 0));
            AddColumn("TranslationEntities", "ReverseBadAnswers", c => c.Int(false, defaultValue: 0));
            AddColumn("TranslationEntities", "ReverseWasLastAnswerGood", c => c.Boolean(false, defaultValue: true));
            AddColumn("TranslationEntities", "ReverseSpellingGoodAnswers", c => c.Int(false, defaultValue: 0));
            AddColumn("TranslationEntities", "ReverseSpellingBadAnswers", c => c.Int(false, defaultValue: 0));
            AddColumn("TranslationEntities", "ReverseSpellingWasLastAnswerGood", c => c.Boolean(false, defaultValue: true));
        }

        public override void Down()
        {
        }
    }
}
