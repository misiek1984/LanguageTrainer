using System;
using System.Linq;
using System.Collections.ObjectModel;

using MK.Utilities;
using MK.UI.WPF;

using LanguageTrainer.Entities;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class ExpressionVM : TypedVM<ExpressionEntity>
    {
        #region Properties

        public override string Name
        {
            get
            {
                return Entity.Expression;
            }
            set
            {}
        }

        public string Category
        {
            get
            {
                return Entity.Category;
            }
            set
            {
                if (Entity.Category != value)
                {
                    Entity.Category = value;
                    Notify("Category");
                }
            }
        }

        public string Expression
        {
            get
            {
                return Entity.Expression;
            }
            set
            {
                if (Entity.Expression != value)
                {
                    Entity.Expression = value;
                    Notify("Expression");
                }
            }
        }

        public TranslationVM this[int lang]
        {
            get
            {
                return (from t in Translations where t.Language == lang select t).FirstOrDefault();
            }
        }

        public ObservableCollection<TranslationVM> Translations { get; set; }



        public ILanguagesProvider LanguagesProvider { get; set; }
        public IEntityCreator EntityCreator { get; set; }
        public IDataAccess DataAccess { get; set; }

        #endregion

        #region Commands

        public CustomCommand RemoveExpression
        {
            get
            {
                return new CustomCommand(() => ExecuteRemoveExpression(), () => true);
            }
        }
        private void ExecuteRemoveExpression()
        {
            if (Parent is MainWindowVM)
                ((MainWindowVM)Parent).RemoveExpression(this);
        }

        public CustomCommand ManageLanguages
        {
            get
            {
                return new CustomCommand(() => ExecuteManageLanguages(), () => true);
            }
        }

        private void ExecuteManageLanguages()
        {
            var selected = Translations.Aggregate(0, (current, t) => current | t.Language);

            var vm = ServiceProvider.Inject(new LanguageChooserVM(null));
            vm.SelectedLanguages = selected;
            vm.Name = Resources.Lbl.ManageLanguages;

            if (WindowService.ShowDialog(vm, canResize: true, allowSettings: false, saveSizeAndPosition: true, showOkCancelButtons:true, extraSettingsKey: "ManageLanguages") == true)
            {
                for (var i = 0; i < Entity.Translations.Count; ++i)
                    if ((Entity.Translations[i].Language & vm.SelectedLanguages) == 0)
                    {
                        Entity.Translations[i].Deleted = true;
                        Entity.Translations[i].MakeDirty();
                    }

                foreach (var li in LanguagesProvider.Languages)
                    if ((li.Id & vm.SelectedLanguages) != 0 && Entity.Translations.All(t => t.Language != li.Id))
                        Entity.Translations.Add(EntityCreator.CreateTranslation(li.Id));

                try
                {
                    DataAccess.Save(Entity);
                }
                catch (Exception ex)
                {
                    WindowService.ShowError(ex);
                }

                var parent = (MainWindowVM) Parent;
                if (Entity.Translations.Count(t => (t.Language & parent.Languages.SelectedLanguages) != 0) == 0)
                {
                    parent.HideExpression(this);
                }
                else if (!parent.ShowEmptyTranslations &&
                         Entity.Translations.Where(t => (t.Language & parent.Languages.SelectedLanguages) != 0)
                               .All(t => String.IsNullOrEmpty(t.Translation)))
                {
                    parent.HideExpression(this);
                }
                else
                {
                    PopulateTranslations();
                }
            }
        }

        #endregion

        #region Constructor

        public ExpressionVM(ViewModelBase parent, ExpressionEntity expression)
            : base(parent)
        {
            expression.NotNull("expression");
            Entity = expression;

            Translations = new ObservableCollection<TranslationVM>();
            PopulateTranslations();
        }

        #endregion

        #region Methods

        public override void Validate()
        {
            if (Parent is MainWindowVM)
            {
                if (!DataAccess.IsExpressionUnique(Entity))
                    AddError("Expression", Resources.Msg.NotUnique);
                else
                    ClearError("Expression");

                foreach (var t in Translations)
                    t.Validate();
            }
        }

        private void PopulateTranslations()
        {
            Translations.Clear();
            foreach (var te in Entity.Translations.Where(t => !t.Deleted).OrderBy(t => t.Language))
                Translations.Add(ServiceProvider.Inject(new TranslationVM(this, te)));
        }

        #endregion
    }
}

