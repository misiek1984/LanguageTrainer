using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Data;
using System.Xml.Serialization;
using LanguageTrainer.Resources;
using MK.Dropbox;
using MK.Live;
using MK.UI.WPF.VM;
using MK.Utilities;
using MK.Logging;
using MK.Settings;
using MK.UI.WPF;
using MK.Data;

using LanguageTrainer.Entities;
using LanguageTrainer.Paging;
using LanguageTrainer.Common;
using LanguageTrainer.Interfaces;

namespace LanguageTrainer.VM
{
    public class MainWindowVM : ViewModelBase, IPagingClient
    {
        #region Properties

        [SettingsProperty]
        public SearchPanelVM SearchPanel { get; set; }

        [SettingsProperty]
        public TranslateVM TranslatePanel { get; set; }

        [SettingsProperty]
        public TestConfigurationVM TrainingConfiguration { get; set; }

        public ObservableCollection<ExpressionVM> Expressions { get; set; }

        private ObservableCollection<string> _availableCategories;
        public ObservableCollection<string> AvailableCategories
        {
            get
            {
                if (_availableCategories == null)
                {
                    _availableCategories = new ObservableCollection<string>();
                    DataAccess.GetCategories().ForEach(c => _availableCategories.Add(c));
                }

                return _availableCategories;
            }
        }

        private Visibility _expressionsVisibility = Visibility.Visible;
        public Visibility ExpressionsVisibility
        {
            get { return _expressionsVisibility; }
            set
            {
                if (value != _expressionsVisibility)
                {
                    _expressionsVisibility = value;
                    Notify("ExpressionsVisibility");
                }
            }
        }

        private int? _totalNumberOfExpressions;
        public int TotalNumberOfExpressions
        {
            get
            {
                if (_totalNumberOfExpressions == null)
                    _totalNumberOfExpressions =
                        DataAccess.GetNumberOfExpressions(GetPageParameters);

                return _totalNumberOfExpressions.Value;
            }
        }

        private ExpressionVM _selectedItem;
        public ExpressionVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != _selectedItem)
                {
                    _selectedItem = value;
                    Notify(() => SelectedItem);
                    //PagingHelper.RefreshPagingProperties();
                }
            }
        }

        public string NumberOfExpressionsPerLanguage
        {
            get
            {
                var res = DataAccess.CountExpressionsByLanguage();
                var selected = DataAccess.CountSelectedByLanguage();

                var sb = new StringBuilder();

                foreach (var li in LanguagesProvider.Languages)
                {
                    sb.Append(li.Name);
                    sb.Append(" ");
                    sb.Append(selected.ContainsKey(li.Id) ? selected[li.Id] : 0);
                    sb.Append("/");
                    sb.Append(res.ContainsKey(li.Id) ? res[li.Id] : 0);
                    sb.Append(", ");
                }

                if (sb.Length > 0)
                    sb.Length -= 2;

                return sb.ToString();
            }
        }

        public string SpecialSymbols
        {
            get { return SpecialSymbolsProvider.SpecialSymbols; }
        }

        public PagingHelper PagingHelper { get; set; }

        public override string Name { get { return Resources.Lbl.LanguageTrainer; } }

        private GetPageParameters GetPageParameters
        {
            get
            {
                return new GetPageParameters
                {
                    Index = PagingHelper.CurrentPageIndex - 1,
                    PageSize = PagingConstans.MaxPageSize,
                    ShowEmpty = ShowEmptyTranslations,
                    OnlySelected = OnlySelected,
                    Lang = Languages.SelectedLanguages,
                    MatchWholeWord = SearchPanel.MatchWholeWord,
                    TextToFind = SearchPanel.TextToFind,
                    Category = SearchPanel.SelectedCategory == Resources.Lbl.All ? String.Empty : SearchPanel.SelectedCategory
                };
            }
        }

        private List<ExpressionEntity> CopiedExpressions { get; set; }



        public IDataAccess DataAccess { get; set; }

        public IDateTimeProvider DateTimeProvider { get; set; }

        public ISettingsProvider SettingsProvider { get; set; }

        public IDropboxProvider DropboxProvider { get; set; }

        public ILiveProvider LiveProvider { get; set; }

        public ILanguagesProvider LanguagesProvider { get; set; }

        public IEntityCreator EntityCreator { get; set; }

        public ISpecialSymbolsProvider SpecialSymbolsProvider { get; set; }

        public IPersister<MainEntity> Persister { get; set; }

        public IMerger Merger { get; set; }

        #endregion
        #region Import/Export Properties

        public DateTime ExportSinceDate { get; set; }

        [SettingsProperty]
        public bool ExportSince { get; set; }

        [SettingsProperty]
        public bool ImportStatisticsForNew { get; set; }

        [SettingsProperty]
        public bool ImportStatisticsForOld { get; set; }

        [SettingsProperty]
        public bool ImportCreationDate { get; set; }

        [SettingsProperty]
        public bool ImportDefinedDate { get; set; }

        [SettingsProperty]
        public bool ImportRecentlyUsedDate { get; set; }

        [SettingsProperty]
        public bool SyncWithDropboxEnabled { get; set; }

        private bool _SyncWithLiveEnabled;
        [SettingsProperty]
        public bool SyncWithLiveEnabled
        {
            get { return _SyncWithLiveEnabled; }
            set
            {
                if (value != _SyncWithLiveEnabled)
                {
                    _SyncWithLiveEnabled = value;
                    Notify(() => SyncWithLiveEnabled);
                }
            }
        }

        public bool SyncLocally
        {
            get { return !SyncWithDropboxEnabled && !SyncWithLiveEnabled; }
        }

        #endregion
        #region Configuration Properties

        [SettingsProperty]
        public LanguageChooserVM Languages { get; set; }

        private bool _showEmptyTranslations = true;
        [SettingsProperty]
        public bool ShowEmptyTranslations
        {
            get { return _showEmptyTranslations; }
            set
            {
                if (value != _showEmptyTranslations)
                {
                    _showEmptyTranslations = value;
                    Notify("ShowEmptyTranslations");
                }
            }
        }

        private bool _showDetails;
        [SettingsProperty]
        public bool ShowDetails
        {
            get { return _showDetails; }
            set
            {
                if (value != _showDetails)
                {
                    _showDetails = value;
                    Notify("ShowDetails");
                }
            }
        }

        private bool _onlySelected;
        [SettingsProperty]
        public bool OnlySelected
        {
            get { return _onlySelected; }
            set
            {
                if (_onlySelected != value)
                {
                    _onlySelected = value;
                    Notify(() => OnlySelected);

                    RebuildList(true);
                }
            }
        }

        #endregion
        #region Expanders Properties

        private bool _isTrainingExpanderExpanded;
        [SettingsProperty]
        public bool IsTrainingExpanderExpanded
        {
            get { return _isTrainingExpanderExpanded; }
            set
            {
                if (value != _isTrainingExpanderExpanded)
                {
                    _isTrainingExpanderExpanded = value;
                    Notify("IsTrainingExpanderExpanded");
                }
            }
        }

        private bool _isTranslationExpanderExpanded;
        [SettingsProperty]
        public bool IsTranslationExpanderExpanded
        {
            get { return _isTranslationExpanderExpanded; }
            set
            {
                if (value != _isTranslationExpanderExpanded)
                {
                    _isTranslationExpanderExpanded = value;
                    Notify("IsTranslationExpanderExpanded");
                }
            }
        }

        private bool _isConfigurationExpanderExpanded;
        [SettingsProperty]
        public bool IsConfigurationExpanderExpanded
        {
            get { return _isConfigurationExpanderExpanded; }
            set
            {
                if (value != _isConfigurationExpanderExpanded)
                {
                    _isConfigurationExpanderExpanded = value;
                    Notify("IsConfigurationExpanderExpanded");
                }
            }
        }

        private bool _isImportExportExpanderExpanded;
        [SettingsProperty]
        public bool IsImportExportExpanderExpanded
        {
            get { return _isImportExportExpanderExpanded; }
            set
            {
                if (value != _isImportExportExpanderExpanded)
                {
                    _isImportExportExpanderExpanded = value;
                    Notify("IsImportExportExpanderExpanded");
                }
            }
        }

        #endregion

        #region Commands

        private CustomCommand _addExpressionCommand;
        public CustomCommand AddExpressionCommand
        {
            get
            {
                if (_addExpressionCommand == null)
                    _addExpressionCommand = new CustomCommand(ExecuteAddExpression, () => true);

                return _addExpressionCommand;
            }
        }
        private void ExecuteAddExpression()
        {
            var vm = new LanguageChooserVM(null) { Name = Resources.Lbl.ManageLanguages };
            ServiceProvider.Inject(vm);

            if (WindowService
                               .ShowDialog(vm, canResize: true, allowSettings: false, saveSizeAndPosition: true,
                                           extraSettingsKey: "ManageLanguages", showOkCancelButtons: true) == true)
            {
                ExpressionEntity ex = EntityCreator.CreateExpression(vm.SelectedLanguages);
                Expressions.Add(ServiceProvider.Inject(new ExpressionVM(this, ex)));

                DataAccess.Save(ex);

                RefreshStatistics();
            }
        }

        private CustomCommand _goToApplicationFolder;
        public CustomCommand GoToApplicationFolder
        {
            get
            {
                if (_goToApplicationFolder == null)
                    _goToApplicationFolder = new CustomCommand(
                        () => WindowService.ShowDirectory(ApplicationPaths.ApplicationFolder),
                        () => true);

                return _goToApplicationFolder;
            }
        }

        private CustomCommand _validateExpressions;
        public CustomCommand ValidateExpressions
        {
            get
            {
                if (_validateExpressions == null)
                    _validateExpressions = new CustomCommand(
                        () =>
                        {
                            //To be sure that the current state of entities will be validate. 
                            //Without this operation the state in database can be different than the state on the UI.
                            Save();
                            WindowService.DoBackgroundTask(() => Validate());
                        },
                        () => true);

                return _validateExpressions;
            }
        }


        public CustomCommand _CopyCommand = null;
        public CustomCommand CopyCommand
        {
            get
            {
                if (_CopyCommand == null)
                    _CopyCommand = new CustomCommand(
                        () => Copy(),
                        () => true);

                return _CopyCommand;
            }
        }
        private void Copy()
        {
            CopiedExpressions.Clear();

            foreach (var ex in Expressions)
                if (ex.IsSelected)
                {
                    var copy = EntityCreator.CopyExpression(ex.Entity);
                    CopiedExpressions.Add(copy);
                }
        }

        public CustomCommand _PasteCommand = null;
        public CustomCommand PasteCommand
        {
            get
            {
                if (_PasteCommand == null)
                    _PasteCommand = new CustomCommand(
                        () => Paste(),
                        () => true);

                return _PasteCommand;
            }
        }
        private void Paste()
        {
            if (CopiedExpressions.Count > 0)
            {
                foreach (var ex in CopiedExpressions)
                {
                    Expressions.Add(ServiceProvider.Inject(new ExpressionVM(this, ex)));
                    DataAccess.Save(ex);
                    RefreshStatistics();
                }

                CopiedExpressions.Clear();
            }
        }

        public CustomCommand _SaveCommand = null;
        public CustomCommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                    _SaveCommand = new CustomCommand(
                        () => SaveAll(),
                        () => true);

                return _SaveCommand;
            }
        }
        private void SaveAll()
        {
            SettingsProvider.ExtractSettings(this);
            SettingsProvider.Save();
            Save();
        }

        #endregion

        #region Constructor

        public MainWindowVM()
        {
            Log.StartTiming();

            CopiedExpressions = new List<ExpressionEntity>();

            SearchPanel = ServiceProvider.Inject(new SearchPanelVM(this));
            TranslatePanel = ServiceProvider.Inject(new TranslateVM());
            TrainingConfiguration = ServiceProvider.Inject(new TestConfigurationVM(this));
            Languages = ServiceProvider.Inject(new LanguageChooserVM(this));

            Expressions = new ObservableCollection<ExpressionVM>();

            PagingHelper = new PagingHelper(this);
            PagingHelper.PageChanged += PagingHelper_PageChanged;

            Log.StopTiming();
        }

        #endregion

        #region Methods

        public void RemoveExpression(ExpressionVM ex)
        {
            Expressions.Remove(ex);

            DataAccess.Remove(ex.Entity);

            RefreshStatistics();
        }

        public void HideExpression(ExpressionVM ex)
        {
            Expressions.Remove(ex);
        }

        public void RebuildList(bool refreshStatistics = false, bool resetPageIndex = false)
        {
            Log.StartTiming();

            Expressions.Clear();

            if (resetPageIndex)
            {
                PagingHelper.PageChanged -= PagingHelper_PageChanged;
                PagingHelper.FirstPage();
                PagingHelper.PageChanged += PagingHelper_PageChanged;
            }

            var srv = DataAccess;
            var res = srv.GetPage(GetPageParameters);

            foreach (var entity in res)
                Expressions.Add(ServiceProvider.Inject(new ExpressionVM(this, entity)));

            if (refreshStatistics)
                RefreshStatistics();

            Log.StopTiming();
        }

        public void Save()
        {
            var list = new List<ExpressionEntity>();

            foreach (var ex in Expressions)
                list.Add(ex.Entity);

            DataAccess.Save(list);
        }

        public override void Validate()
        {
            foreach (ExpressionVM ex in Expressions)
                ex.Validate();
        }

        public override bool BeforeShow()
        {
            ExportSinceDate = DateTimeProvider.Current;

            RebuildList(true);

            //Posorotwanie grida po tej kolumnie zapewnii, ¿e nowe wêz³y bêd¹ (przynajmniej na pocz¹tku)
            //dodawane na pocz¹tku grida.
            GridViewSort.ApplySort(CollectionViewSource.GetDefaultView(Expressions), "Expression");

            WindowService.HideProgress();
            WindowService.MakeWindowVisible(this);
            return true;
        }
        public override void BeforeClose()
        {
            Save();
        }

        protected override void InternalOnPropertyChanged(ViewModelBase vm, string property)
        {
            if (vm is ExpressionVM && property == "Category")
            {
                var newCategory = ((ExpressionVM)vm).Category;
                if (!AvailableCategories.Any(c => c.Equals(newCategory)))
                {
                    AvailableCategories.Add(newCategory);
                    TrainingConfiguration.AvailableCategories.Add(newCategory);
                    SearchPanel.AvailableCategories.Add(newCategory);
                }
            }
            else if (vm is TranslationVM && property == "Translation")
            {
                Save();//To be sure that a new translation will be taken into account
                Notify(() => NumberOfExpressionsPerLanguage);
            }
            else if (vm is TranslationVM && property == "IsSelected")
            {
                Save();//To be sure that a new translation will be taken into account
                Notify(() => NumberOfExpressionsPerLanguage);
            }
            else if (vm is LanguageVM && property == "IsSelected" || property == "ShowEmptyTranslations")
            {
                Save();
                RebuildList(true);

                foreach (var ex in Expressions)
                    foreach (var t in ex.Translations)
                        t.Notify("TranslationVisibility");
            }
        }

        private void RefreshStatistics()
        {
            Notify(() => NumberOfExpressionsPerLanguage);
            _totalNumberOfExpressions = null;
            Notify(() => TotalNumberOfExpressions);
            PagingHelper.RefreshPagingProperties();
        }

        #endregion

        #region Import & Export

        #region Fields & Properties

        private const string ImportExportGroupName = "ImportExport";

        public override string CancelCommandGroupName
        {
            get { return ImportExportGroupName; }
        }

        #endregion

        #region Commands

        private AsyncCustomCommand _importExpressionsCommand;
        public AsyncCustomCommand ImportExpressionsCommand
        {
            get
            {
                if (_importExpressionsCommand == null)
                    _importExpressionsCommand = new AsyncCustomCommand(ExecuteImportExpressions, () => !CheckIfOperationInProgress(ImportExportGroupName), autoCanExecute: true, parent: this, groupName: "ImportExport");

                return _importExpressionsCommand;
            }
        }

        private AsyncCustomCommand _exportExpressionsCommand;
        public AsyncCustomCommand ExportExpressionsCommand
        {
            get
            {
                if (_exportExpressionsCommand == null)
                    _exportExpressionsCommand = new AsyncCustomCommand(ExecuteExportExpressions, () => !CheckIfOperationInProgress(ImportExportGroupName), autoCanExecute: true, parent: this, groupName: "ImportExport");

                return _exportExpressionsCommand;
            }
        }

        #endregion

        #region Methods

        private async Task ExecuteImportExpressions()
        {
            Log.StartTiming();

            try
            {
                if (SyncWithDropboxEnabled && SyncWithLiveEnabled)
                {
                    WindowService.ShowMessage(Msg.CannotSyncFromTwoSourcesAtTheSameTime);
                    return;
                }

                var cts = GetTokenSource(ImportExportGroupName);

                MainEntity entity = null;
                if (SyncWithDropboxEnabled)
                {
                    entity = await DownloadDromDropbox(cts.Token);
                }
                else if (SyncWithLiveEnabled)
                {
                    entity = await DownloadFromLive(cts.Token);
                }
                else
                {
                    using (var o = new OpenFileDialog())
                    {
                        var res = o.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            Log.StartTiming("ImportFromFile");
                            entity = Persister.ImportFromFile(o.FileName);
                            Log.StopTiming("ImportFromFile");
                        }
                    }
                }

                if (entity != null)
                {
                    Merge(entity);
                    RefreshAfterMerge();
                }
                else
                {
                    WindowService.ShowMessage(Msg.ImportFailed);
                }
            }
            catch (Exception ex)
            {
                WindowService.ShowError(ex);
            }
            finally
            {
                RefreshCommands();
            }

            Log.StopTiming();
        }

        private async Task ExecuteExportExpressions()
        {
            Log.StartTiming();

            RefreshCommands();
            try
            {
                if (!SyncLocally)
                {
                    if (SyncWithDropboxEnabled && SyncWithLiveEnabled)
                    {
                        WindowService.ShowMessage(Msg.CannotSyncFromTwoSourcesAtTheSameTime);
                        return;
                    }

                    var entity = new MainEntity();
                    entity.Expressions.AddRange(DataAccess.GetAllExpressions());

                    var cts = GetTokenSource(ImportExportGroupName);
                    var ct = cts.Token;

                    if (SyncWithDropboxEnabled)
                    {
                        if (!await UploadToDropbox(entity, ct))
                            WindowService.ShowMessage(Msg.DropboxExportFailed);
                    }
                    if (SyncWithLiveEnabled)
                    {
                        if (!await UploadToLive(entity, ct))
                            WindowService.ShowMessage(Msg.LiveExportFailed);
                    }
                }
                else
                {
                    using (var o = new SaveFileDialog())
                    {
                        var res = o.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            var since = ExportSince && ExportSinceDate != DateTime.MinValue
                                            ? ExportSinceDate
                                            : (DateTime?)null;

                            var entity = new MainEntity();
                            entity.Expressions.AddRange(
                                DataAccess.GetAllExpressions(since));

                            Log.StartTiming("ExportToFile");
                            Persister.ExportToFile(o.FileName, entity);
                            Log.StartTiming("ExportToFile");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowService.ShowError(ex);
            }
            finally
            {
                RefreshCommands();
            }

            Log.StopTiming();
        }

        private void Merge(MainEntity source, bool inbackground = true)
        {
            Log.StartTiming("Merge");

            var conf = new MergerConfiguration
            {
                ImportCreationDate = ImportCreationDate,
                ImportRecentlyUsedDate = ImportRecentlyUsedDate,
                ImportDefinedDate = ImportDefinedDate,
                ImportStatisticsForNew = ImportStatisticsForNew,
                ImportStatisticsForOld = ImportStatisticsForOld
            };
            if (inbackground)
            {
                WindowService.DoBackgroundTask(
                    () => Merger.Merge(source, conf));
            }
            else
            {
                Merger.Merge(source, conf);
            }
            Log.StopTiming("Merge");
        }

        private void RefreshAfterMerge()
        {
            DataAccess.Configure("WaitForNonStaleData", true);
            RebuildList(true);
            DataAccess.Configure("WaitForNonStaleData", false);

            _availableCategories = null;
            Notify(() => AvailableCategories);

            TrainingConfiguration.RefreshCategories();
            SearchPanel.RefreshCategories();
        }

        #endregion

        #region Dropbox

        private void TryAuthoriseDropbox(IDropboxProvider dropbox)
        {
            if (dropbox.IsAuthorised)
                return;

            WindowService.ShowDialog(new WebLoginVM(this, dropbox.SingInUrl, dropbox.LoginCallback, dropbox.IsFinalUrl) { Name = Lbl.DropboxLogin });

            if (!dropbox.IsAuthorised)
                WindowService.ShowError(String.Format(Msg.CannotAuthorise));
        }

        private async Task<MainEntity> DownloadDromDropbox(CancellationToken ct)
        {
            var dropbox = DropboxProvider;
            MainEntity res = null;

            Log.StartTiming("DownloadDromDropbox");
            try
            {
                TryAuthoriseDropbox(dropbox);

                if (!dropbox.IsAuthorised)
                    return null;

                var serializer = new XmlSerializer(typeof(MainEntity));
                using (var stream = await dropbox.GetFile("/MainEntity.xml", ct))
                {
                    var entity = (MainEntity)serializer.Deserialize(stream);
                    res = entity;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
            Log.StopTiming("DownloadDromDropbox");

            return res;
        }

        private async Task<bool> UploadToDropbox(MainEntity entity, CancellationToken ct)
        {
            var dropbox = DropboxProvider;
            var res = false;

            Log.StartTiming("UploadToDropbox");
            try
            {
                TryAuthoriseDropbox(dropbox);

                if (!dropbox.IsAuthorised)
                    return res;

                var serializer = new XmlSerializer(typeof(MainEntity));
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, entity);
                    stream.Position = 0;
                    res = await dropbox.Upload("/", "MainEntity.xml", stream, ct);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
            Log.StopTiming("UploadToDropbox");

            return res;
        }

        #endregion
        #region Live

        private void TryAuthoriseLive(ILiveProvider live)
        {
            if (live.IsAuthorised)
                return;

            WindowService.ShowDialog(new WebLoginVM(this, live.SingInUrl, live.LoginCallback) { Name = Lbl.LiveLogin }, saveSizeAndPosition: true, canResize: true);

            if (!live.IsAuthorised)
            {
                if (!String.IsNullOrEmpty(live.ErrorCode))
                    WindowService.ShowError(String.Format(Msg.CannotAuthorise2, live.ErrorCode));
                else if (!String.IsNullOrEmpty(live.ErrorDescription))
                    WindowService.ShowError(String.Format(Msg.CannotAuthorise2, live.ErrorDescription));
                if (!String.IsNullOrEmpty(live.ErrorCode))
                    WindowService.ShowError(String.Format(Msg.CannotAuthorise));
            }
        }

        private async Task<MainEntity> DownloadFromLive(CancellationToken ct)
        {
            var liveProvider = LiveProvider;
            MainEntity res = null;

            Log.StartTiming("DownloadFromLive");
            try
            {
                TryAuthoriseLive(liveProvider);

                if (!liveProvider.IsAuthorised)
                    return null;

                var serializer = new XmlSerializer(typeof(MainEntity));
                using (var stream = await liveProvider.GetFileByName("me/skydrive/files", "MainEntity.xml", ct))
                {
                    var entity = (MainEntity)serializer.Deserialize(stream);
                    res = entity;
                }
                Log.StopTiming("DownloadFromLive");
                return res;
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
            Log.StopTiming("DownloadFromLive");

            return res;
        }

        private async Task<bool> UploadToLive(MainEntity entity, CancellationToken ct)
        {
            var liveProvider = LiveProvider;
            var res = false;

            Log.StartTiming("UploadToLive");
            try
            {
                TryAuthoriseLive(liveProvider);

                if (!liveProvider.IsAuthorised)
                    return res;

                var serializer = new XmlSerializer(typeof(MainEntity));
                using (var stream = new MemoryStream())
                {
                    serializer.Serialize(stream, entity);
                    stream.Position = 0;
                    res = await liveProvider.Upload("", "MainEntity.xml", stream, ct) != null;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
            Log.StopTiming("UploadToLive");

            return res;
        }

        #endregion

        #endregion

        #region Event Handlers

        private void PagingHelper_PageChanged(object sender, EventArgs e)
        {
            Save();
            RebuildList();
        }

        #endregion

        #region IPagingClient Members

        public int NumberOfItems
        {
            get { return TotalNumberOfExpressions; }
        }

        #endregion
    }
}

