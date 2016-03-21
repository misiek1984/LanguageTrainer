using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageTrainer.Entities
{
     public class MergerConfiguration
    {
        public bool ImportStatisticsForNew { get; set; }

        public bool ImportStatisticsForOld { get; set; }

        public bool ImportCreationDate { get; set; }

        public bool ImportDefinedDate { get; set; }

        public bool ImportRecentlyUsedDate { get; set; }
    }
}
