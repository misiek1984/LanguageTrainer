using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using MK.Utilities;
using MK.Settings;

namespace LanguageTrainer.Controls
{
    public class ThemeChooser : ComboBox
    {
        public string CurrentTheme
        {
            get { return String.Format("pack://application:,,,/WPF Themes/{0}.xaml", SelectedItem);  } 
        }

        public ThemeChooser()
        {
            //Items.Add("BureauBlack");
            Items.Add("BureauBlue");
            Items.Add("ExpressionDark");
            Items.Add("ExpressionLight");
            Items.Add("ShinyBlue");
            Items.Add("ShinyRed");
            Items.Add("WhistlerBlue");

            if (!String.IsNullOrEmpty(ServiceProvider.GetService<ISettingsProvider>().GetValue<string>("LastTheme")))
                SelectedItem = ServiceProvider.GetService<ISettingsProvider>()["LastTheme"];
            else
                SelectedItem = "WhistlerBlue";
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; ++i)
                if (Application.Current.Resources.MergedDictionaries[i].Source == new Uri(CurrentTheme))
                    Application.Current.Resources.MergedDictionaries.RemoveAt(i);

            ServiceProvider.GetService<ISettingsProvider>()["LastTheme"] = SelectedItem as string;
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(CurrentTheme) });
        }
    }
}
