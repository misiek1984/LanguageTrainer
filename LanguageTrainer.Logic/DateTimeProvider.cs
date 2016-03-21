using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LanguageTrainer.Interfaces;

namespace LanguageTrainer.Logic
{
    public class DateTimeProvider : IDateTimeProvider
    {
        #region IDateTimeProvider Members

        public DateTime Current
        {
            get { return DateTime.Now; }
        }

        #endregion
    }
}
