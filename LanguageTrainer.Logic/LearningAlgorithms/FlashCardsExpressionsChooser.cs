using System.Collections.Generic;
using MK.Utilities;
using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic.LearningAlgorithms
{
    public class FlashCardsExpressionsChooser : IExpressionsChooser
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDataAccess _dataAccess;

        public FlashCardsExpressionsChooser(IDateTimeProvider dateTimeProvider, IDataAccess dataAccess)
        {
            dateTimeProvider.NotNull("dataTimeProvider");
            dataAccess.NotNull("dataAccess");

            _dateTimeProvider = dateTimeProvider;
            _dataAccess = dataAccess;
        }

        public IList<ExpressionEntity> SelectExpressions(TestConfiguration config)
        {
            return new List<ExpressionEntity>();
        }
    }
}
