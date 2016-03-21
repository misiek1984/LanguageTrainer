using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageTrainer.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime Current { get; }
    }
}
