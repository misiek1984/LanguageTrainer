using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public interface IEntityCreator
    {
        ExpressionEntity CopyExpression(ExpressionEntity ex);
        ExpressionEntity CreateExpression(bool allDefaultLanguages);
        ExpressionEntity CreateExpression(int languages);
        TranslationEntity CreateTranslation(int language);
    }
}
