using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageTrainer.Paging
{
    public interface IPagingClient
    {
        int NumberOfItems { get; }
    }
}
