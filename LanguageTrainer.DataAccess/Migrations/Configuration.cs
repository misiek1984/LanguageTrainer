namespace LanguageTrainer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<LanguageTrainer.DataAccess.ExpressionsDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(LanguageTrainer.DataAccess.ExpressionsDataContext context)
        {
        }
    }
}
