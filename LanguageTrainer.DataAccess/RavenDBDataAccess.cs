//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Raven.Client.Indexes;
//using Raven.Client.Linq;
//using Raven.Client;

//using MK.Data.RavenDB;
//using MK.Utilities;

//using LanguageTrainer.Entities;
//using LanguageTrainer.Interfaces;

//namespace LanguageTrainer.DataAccess
//{
//    public class RavenDBDataAccess : DBAccess, IDataAccess
//    {
//        #region Fields

//        private readonly IDateTimeProvider _dateTimeProvider;

//        #endregion

//        #region Constructor

//        public RavenDBDataAccess(IDateTimeProvider dateTimeProvider)
//        {
//            dateTimeProvider.NotNull("dateTimeProvider");
//            _dateTimeProvider = dateTimeProvider;
//        }

//        #endregion

//        #region Overriders

//        public override void InitializeIndexes()
//        {
//            IndexCreation.CreateIndexes(typeof(RavenDBDataAccess).Assembly, Store);
//        }
//        public override void InitializeStore()
//        {
//            //Store.Conventions.CustomizeJsonSerializer = jsonSerializer =>
//            //{
//            //    jsonSerializer.Converters.Remove(jsonSerializer.Converters.Where(c =>
//            //    c.GetType() == typeof(Raven.Abstractions.Json.JsonEnumConverter)).First());
//            //};
//            //Store.Conventions.QueryEnumsAsIntegers = true;
//        }

//        #endregion

//        #region Data modification operations

//        public void Save(ExpressionEntity ex)
//        {
//            using (var session = Store.OpenSession())
//            {
//                if (ex.CheckIfIsDirty())
//                {
//                    InnerSave(ex);

//                    session.Store(ex);
//                    session.SaveChanges();
//                }
//            }
//        }

//        public void Save(IEnumerable<ExpressionEntity> expressions)
//        {
//            using (var session = Store.OpenSession())
//            {
//                foreach (ExpressionEntity ex in expressions)
//                {
//                    if (ex.CheckIfIsDirty())
//                    {
//                        InnerSave(ex);

//                        session.Store(ex);
//                    }
//                }

//                session.SaveChanges();
//            }
//        }

//        public void Remove(ExpressionEntity ex)
//        {
//            using (var session = Store.OpenSession())
//            {
//                //session.Delete<ExpressionEntity>(session.Load<ExpressionEntity>(ex.Id));
//                session.Advanced.DocumentStore.DatabaseCommands.Delete("expressionentities/" + ex.Id, null);
//                session.SaveChanges();
//            }
//        }

//        private void InnerSave(ExpressionEntity ex)
//        {
//            for (var i = 0; i < ex.Translations.Count; ++i)
//                if (ex.Translations[i].Deleted)
//                    ex.Translations.Remove(ex.Translations[i]);

//            ex.UpdateDate = _dateTimeProvider.Current;
//            ex.SetClean();
//        }

//        #endregion

//        #region Queries

//        public int NumberOfExpressions
//        {
//            get
//            {
//                using (var session = Store.OpenSession())
//                {
//                    return (from ex in session.Query<ExpressionEntity>().Customize(TryWaitForNonStaleData)
//                            select ex).Count();
//                }
//            }
//        }

//        public int GetNumberOfExpressions(GetPageParameters parameters)
//        {
//            return NumberOfExpressions;
//        }

//        public bool IsExpressionUnique(ExpressionEntity expressionToCheck)
//        {
//            expressionToCheck.NotNull("expressionToCheck");

//            if (String.IsNullOrEmpty(expressionToCheck.Expression))
//                return true;

//            using (var session = Store.OpenSession())
//            {
//                var res = (from ex in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                           where ex.Expression == expressionToCheck.Expression
//                           select ex).Count();

//                return res == 1;
//            }
//        }
//        public bool IsTranslationUnique(TranslationEntity translationToCheck)
//        {
//            translationToCheck.NotNull("translationToCheck");

//            if (String.IsNullOrEmpty(translationToCheck.Translation))
//                return true;

//            using (var session = Store.OpenSession())
//            {
//                var res = (from ex in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                           where
//                            ex.Translations.Any(
//                                t => t.Language == translationToCheck.Language && t.Translation == translationToCheck.Translation)
//                           select ex).Count();

//                return res == 1;
//            }
//        }

//        public IEnumerable<ExpressionEntity> GetPage(GetPageParameters parameters)
//        {
//            return GetPage(parameters.Index, parameters.PageSize, parameters.ShowEmpty, parameters.OnlySelected,
//                           parameters.Lang);
//        }

//        public IEnumerable<ExpressionEntity> GetPage(int index, int pageSize, bool showEmpty = true, bool onlySelected = false, int lang = -1)
//        {
//            if (index < 0)
//                index = 0;

//            if (pageSize < 0)
//                pageSize = 20;
            
//            using (var session = Store.OpenSession())
//            {
//                var res = (from ex in session.Query<ExpressionEntity>().Customize(TryWaitForNonStaleData)
//                           //where ex.Translations.Any(t => (!onlySelected || t.IsSelected)/* && (lang < 0 || t.Language == lang)*/)
//                           orderby ex.Expression
//                           select ex).Skip(index * pageSize).Take(pageSize).ToList();

//                foreach (var ex in res)
//                    ex.SetClean();

//                return res;
//            }
//        }

//        public IEnumerable<ExpressionEntity> GetAllExpressions(DateTime? since = null)
//        {
//            if (since == null)
//                throw new NotSupportedException("'since' parameter is not supported by RavenDB mode");

//            return GetPage(0, NumberOfExpressions);
//        }

//        public IList<string> GetCategories()
//        {
//            using (var session = Store.OpenSession())
//            {
//                var res = (from e in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                           where e.Category != null
//                           orderby e.Category
//                           select e.Category).Distinct();

//                return new List<string>(res);
//            }
//        }

//        public IList<ExpressionEntity> Find(bool matchWholeWord, string textToFind, string category, int lang = -1)
//        {
//            if (textToFind == null)
//                textToFind = String.Empty;

//            if (category == null)
//                category = String.Empty;

//            using (var session = Store.OpenSession())
//            {
//                List<ExpressionEntity> list = null;

//                if (matchWholeWord)
//                {
//                    list = MatchWholeWord(textToFind, category, session);
//                }
//                else
//                {
//                    list = DoNotMatchWholeWord(textToFind, category, session);
//                }

//                foreach (var ex in list)
//                    ex.SetClean();

//                return list;
//            }
//        }

//        private List<ExpressionEntity> DoNotMatchWholeWord(string textToFind, string category, IDocumentSession session)
//        {
//            textToFind = String.Format("*{0}*", textToFind);

//            if (String.IsNullOrEmpty(category))
//            {
//                return DoNotMatchWholeWordWithoutCategory(textToFind, category, session);
//            }
//            else
//            {
//                return DoNotMatchWholeWordWithCategory(textToFind, category, session);
//            }
//        }
//        private List<ExpressionEntity> DoNotMatchWholeWordWithCategory(string textToFind, string category, IDocumentSession session)
//        {
//            var res = from e in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                      where (
//                                e.Expression != null && e.Expression.Contains(textToFind)
//                                ||
//                                e.Translations.Any(t => t.Translation != null && t.Translation.Contains(textToFind))
//                            )
//                            &&
//                            e.Category.Equals(category, StringComparison.InvariantCultureIgnoreCase)
//                      orderby e.Expression
//                      select e;

//            return res.ToList();
//        }
//        private List<ExpressionEntity> DoNotMatchWholeWordWithoutCategory(string textToFind, string category, IDocumentSession session)
//        {
//            var res = from e in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                      where e.Expression != null && e.Expression.Contains(textToFind)
//                            ||
//                            e.Translations.Any(t => t.Translation != null && t.Translation.Contains(textToFind))
//                      orderby e.Expression
//                      select e;

//            return res.ToList();
//        }

//        private List<ExpressionEntity> MatchWholeWord(string textToFind, string category, IDocumentSession session)
//        {
//            if (String.IsNullOrEmpty(category))
//            {
//                return MatchWholeWordWithoutCategory(textToFind, category, session);
//            }
//            else
//            {
//                return MatchWholeWordWithCategory(textToFind, category, session);
//            }
//        }
//        private List<ExpressionEntity> MatchWholeWordWithCategory(string textToFind, string category, IDocumentSession session)
//        {
//            var res = from e in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                      where (
//                                e.Expression != null && e.Expression.Equals(textToFind, StringComparison.InvariantCultureIgnoreCase)
//                                ||
//                                e.Translations.Any(t => t.Translation != null && t.Translation.Equals(textToFind, StringComparison.InvariantCultureIgnoreCase))
//                            )
//                            &&
//                            e.Category.Equals(category, StringComparison.InvariantCultureIgnoreCase)
//                      orderby e.Expression
//                      select e;

//            return res.ToList();
//        }
//        private List<ExpressionEntity> MatchWholeWordWithoutCategory(string textToFind, string category, IDocumentSession session)
//        {
//            var res = from e in session.Query<ExpressionEntity>().Customize(c => TryWaitForNonStaleData(c))
//                      where e.Expression != null && e.Expression.Equals(textToFind, StringComparison.InvariantCultureIgnoreCase)
//                            ||
//                            e.Translations.Any(t => t.Translation != null && t.Translation.Equals(textToFind, StringComparison.InvariantCultureIgnoreCase))
//                      orderby e.Expression
//                      select e;

//            return res.ToList();
//        }

//        public IDictionary<int,int> CountExpressionsByLanguage()
//        {
//            using (var session = Store.OpenSession())
//            {
//                var dict = new Dictionary<int, int>();

//                foreach(var res in session.Query<TranslationsCounter.ReduceResult, TranslationsCounter>().Customize(TryWaitForNonStaleData))
//                {
//                    dict.Add(res.Lang, res.Count);
//                }

//                return dict;
//            }
//        }
//        public IDictionary<int, int> CountSelectedByLanguage()
//        {
//            using (var session = Store.OpenSession())
//            {
//                var dict = new Dictionary<int, int>();

//                foreach (var res in session.Query<SelectedTranslationsCounter.ReduceResult, SelectedTranslationsCounter>().Customize(TryWaitForNonStaleData))
//                {
//                    dict.Add(res.Lang, res.Count);
//                }

//                return dict;
//            }
//        }

//        #endregion

//        #region Configuration

//        public void Configure(string param, object val)
//        {
//            if (param == "WaitForNonStaleData")
//                WaitForNonStaleData = (bool)val;
//        }

//        private bool WaitForNonStaleData { get; set; }
//        private void TryWaitForNonStaleData(IDocumentQueryCustomization c)
//        {
//            if (WaitForNonStaleData)
//                c.WaitForNonStaleResults();
//        }

//        #endregion
//    }
//}
