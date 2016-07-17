using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using MK.Utilities;

namespace MK.UI.WPF.Controls
{
    public partial class FileFolderBrowser : UserControl
    {
        #region Dependency Properties

        public static DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(FileFolderBrowser),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
               {
                   d.SetValue(PathsProperty, FileFolderBrowser.PathToPaths((string)e.NewValue));
               }));

        public string Path
        {
            get { return GetValue(PathProperty) as string; }
            set { SetValue(PathProperty, value); }
        }

        public static DependencyProperty PathsProperty = DependencyProperty.Register("Paths", typeof(string[]), typeof(FileFolderBrowser),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
               {
                   var path = FileFolderBrowser.PathsToPath((string[])e.NewValue);
                   var currentPath = d.GetValue(PathProperty) as string;

                   if (path != currentPath)
                       d.SetValue(PathProperty, path);
               }));

        public string[] Paths
        {
            get { return GetValue(PathsProperty) as string[]; }
            set { SetValue(PathsProperty, value); }
        }

        public static DependencyProperty MultiselectProperty = DependencyProperty.Register("Multiselect", typeof(bool), typeof(FileFolderBrowser),
           new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool Multiselect
        {
            get { return (bool)GetValue(MultiselectProperty); }
            set { SetValue(MultiselectProperty, value); }
        }

        public static DependencyProperty FolderModeProperty = DependencyProperty.Register("FolderMode", typeof(bool), typeof(FileFolderBrowser),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool FolderMode
        {
            get { return (bool)GetValue(FolderModeProperty); }
            set { SetValue(FolderModeProperty, value); }
        }

        public static DependencyProperty SaveFileModeProperty = DependencyProperty.Register("SaveFileModeMode", typeof(bool), typeof(FileFolderBrowser),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool SaveFileMode
        {
            get { return (bool)GetValue(SaveFileModeProperty); }
            set { SetValue(SaveFileModeProperty, value); }
        }

        #endregion

        #region Constructor

        public FileFolderBrowser()
        {
            InitializeComponent();

            var service = ServiceProvider.GetService<IWindowService>();
            if (service == null)
                throw new Exception("IWindowService service has not been registered!");
        }

        #endregion

        #region Methods

        public static string PathsToPath(string[] paths)
        {
            var sb = new StringBuilder();

            if (paths != null && paths.Length > 0)
            {
                foreach (var f in paths)
                {
                    sb.Append(f);
                    sb.Append(";");
                }

                sb.Length -= 1;
            }

            return sb.ToString();
        }

        public static string[] PathToPaths(string path)
        {
            if (path != null)
                return path.Split(';');
            else
                return new string[] { };
        }

        #endregion

        #region Events

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            if (FolderMode)
            {
                string folderPath = ServiceProvider.GetService<IWindowService>().ShowFolderBrowser(Path);

                if (folderPath != null)
                    Path = folderPath;
            }
            else
            {
                if (SaveFileMode)
                {
                    string filePath = ServiceProvider.GetService<IWindowService>().ShowSaveFileBrowser(Path);

                    if (filePath != null)
                        Path = filePath;
                }
                else
                {
                    string[] filesPaths = ServiceProvider.GetService<IWindowService>().ShowOpenFileBrowser(Path, Multiselect);

                    if (filesPaths != null)
                    {
                        Path = PathsToPath(filesPaths);
                        Paths = filesPaths;
                    }
                }
            }
        }

        private void GoTo_Click(object sender, RoutedEventArgs e)
        {
            string path = null;

            if (FolderMode)
                path = Path;
            else if (!Multiselect)
                path = System.IO.Path.GetDirectoryName(Path);
            else if (Multiselect && Paths.Length > 0)
                path = System.IO.Path.GetDirectoryName(Paths[0]);

            if (System.IO.Directory.Exists(path))
            {
                System.Diagnostics.Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = path;
                prc.Start();
            }
        }

        #endregion
    }
}
