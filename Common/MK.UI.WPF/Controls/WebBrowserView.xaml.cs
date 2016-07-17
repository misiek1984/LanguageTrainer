using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MK.UI.WPF.Controls
{
    /// <summary>
    /// Interaction logic for WebBrowserView.xaml
    /// </summary>
    public partial class WebBrowserView : UserControl
    {
        public static DependencyProperty NavigateTo =
            DependencyProperty.RegisterAttached(
                "NavigateTo",
                typeof (string),
                typeof (WebBrowserView),
                new UIPropertyMetadata(null, NavigateToPropertyChanged));

        public static string GetNavigateTo(DependencyObject obj)
        {
            return (string) obj.GetValue(NavigateTo);
        }

        public static void SetNavigateTo(DependencyObject obj, string value)
        {
            obj.SetValue(NavigateTo, value);
        }

        public static void NavigateToPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var webBrowser = obj as WebBrowser;
            webBrowser.Navigate(e.NewValue as string);
        }

        public static readonly DependencyProperty NavigatedCommand =
            DependencyProperty.RegisterAttached(
                "NavigatedCommand",
                typeof (ICommand),
                typeof (WebBrowserView),
                new UIPropertyMetadata(null));

        public static ICommand GetNavigatedCommand(DependencyObject obj)
        {
            return (ICommand) obj.GetValue(NavigatedCommand);
        }

        public static void SetNavigatedCommand(DependencyObject obj, string value)
        {
            obj.SetValue(NavigatedCommand, value);
        }

        public WebBrowserView()
        {
            InitializeComponent();
        }

        private void WebBrowser_OnNavigated(object sender, NavigationEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            var cmd = GetNavigatedCommand(element);

            if (cmd != null && cmd.CanExecute(webBrowser.Source))
                cmd.Execute(webBrowser.Source);
        }
    }
}
