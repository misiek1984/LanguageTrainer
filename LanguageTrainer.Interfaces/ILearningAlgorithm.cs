using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public interface ILearningAlgorithm
    {
        void Mark(bool answer, ExpressionEntity entity, TestConfiguration config);
    }
}
