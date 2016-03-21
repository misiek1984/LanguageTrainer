using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LanguageTrainer.Entities;

namespace LanguageTrainer.Interfaces
{
    public interface IMerger
    {
        int Merge(MainEntity toMerge, MergerConfiguration conf);
    }
}
