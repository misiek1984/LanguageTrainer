using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

using MK.Utilities;

using LanguageTrainer.Paging;

namespace LanguageTrainer.Entities
{
    public class MainEntity : BaseEntity
    {
        #region Properties

        public List<ExpressionEntity> Expressions { get; set; }

        #endregion

        #region Constructor

        public MainEntity()
        {
            Expressions = new List<ExpressionEntity>();
        }

        #endregion
    }
}

