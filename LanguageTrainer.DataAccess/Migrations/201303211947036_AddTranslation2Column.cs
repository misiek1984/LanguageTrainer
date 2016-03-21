namespace LanguageTrainer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddTranslation2Column : DbMigration
    {
        public override void Up()
        {
            AddColumn("TranslationEntities", "Translation2", c => c.String(true, maxLength: 4000, storeType: "nvarchar"));
        }

        public override void Down()
        {
        }
    }
}
