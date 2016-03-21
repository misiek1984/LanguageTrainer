using System.Collections.Generic;

using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public interface IExpressionsChooser
    {
        IList<ExpressionEntity> SelectExpressions(TestConfiguration config);
    }
}
