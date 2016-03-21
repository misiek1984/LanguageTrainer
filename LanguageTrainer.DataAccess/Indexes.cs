//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Raven.Client.Indexes;

//using LanguageTrainer.Entities;

//namespace LanguageTrainer.DataAccess
//{
//    public class TranslationsCounter : AbstractIndexCreationTask<ExpressionEntity, TranslationsCounter.ReduceResult>
//    {
//        public class ReduceResult
//        {
//            public int Lang { get; set; }
//            public int Count { get; set; }
//        }

//        public TranslationsCounter()
//        {
//            Map = docs => from doc in docs
//                           from t in doc.Translations
//                           select new { Lang = t.Language, Count = String.IsNullOrEmpty(t.Translation) ? 0 : 1 };

//            Reduce = results => from t in results
//                                group t by t.Lang
//                                into g
//                                select new { Lang = g.Key, Count = g.Sum(x => x.Count) };
//        }
//    }

//    public class SelectedTranslationsCounter : AbstractIndexCreationTask<ExpressionEntity, SelectedTranslationsCounter.ReduceResult>
//    {
//        public class ReduceResult
//        {
//            public int Lang { get; set; }
//            public int Count { get; set; }
//        }

//        public SelectedTranslationsCounter()
//        {
//            Map = docs => from doc in docs
//                          from t in doc.Translations
//                          select new { Lang = t.Language, Count = t.IsSelected ? 1 : 0 };

//            Reduce = results => from t in results
//                                group t by t.Lang
//                                    into g
//                                    select new { Lang = g.Key, Count = g.Sum(x => x.Count) };
//        }
//    }
//}
