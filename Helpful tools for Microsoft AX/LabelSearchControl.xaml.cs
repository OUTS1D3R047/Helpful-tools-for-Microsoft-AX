using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsForms = System.Windows.Forms;
using System.Linq;

namespace Helpful_Tools_for_Microsoft_AX
{
    /// <summary>
    /// Interaction logic for LabelSearchControl.
    /// </summary>

    class labelsGrid
    {
        public labelsGrid(string labelFile, string lang, string labelID, string label, string desc)
        {
            this.labelFile = labelFile;
            this.lang = lang;
            this.labelID = labelID;
            this.label = label;
            this.desc = desc;
        }
        public string labelFile { get; set; }
        public string lang { get; set; }
        public string labelID { get; set; }
        public string label { get; set; }
        public string desc { get; set; }
    }

    public partial class LabelSearchControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelSearchControl"/> class.
        /// </summary>
        public LabelSearchControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]

        string selectedFolder = "";
        string lang = "";
        List<labelsGrid> result;

        public void parser()
        {
            Task.Delay(1000);

            Window mainWindow = Window.GetWindow(this);
            mainWindow.IsEnabled = false;

            result = new List<labelsGrid>(4);
            List<string> fileList = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(selectedFolder);
            string searchText = keyWord.Text;

            try
            {
                foreach (var file in dir.GetFiles("*.txt", SearchOption.AllDirectories))
                {
                    if (!file.FullName.Contains("\\" + lang + "\\")) continue;
                    fileList.Add(file.FullName);

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            foreach (var file in fileList)
            {

                string firstPart = null;
                string tmp = "";

                StreamReader f = new StreamReader(file);
                string labelID;
                string label;
                string line;
                string fileName = Path.GetFileName(file);

                while ((line = f.ReadLine()) != null)
                {
                    if (line.Contains('=') && line.IndexOf(';') != 1)
                    {
                        tmp = line;
                        if (tmp.Contains(searchText))
                        {
                            if (firstPart != null)
                            {
                                try
                                {
                                    labelfunc(firstPart, fileName, out label, out labelID);
                                    result.Add(new labelsGrid(fileName, lang, labelID, label, ""));
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Ошибка: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                firstPart = null;
                            }
                            firstPart = line;
                            continue;
                        }
                    }

                    if (line.Contains(';') && line.IndexOf(';') == 1)
                    {
                        if (tmp == firstPart)
                        {
                            int k = line.IndexOf(";");
                            string desc = line.Substring(k + 1);
                            try
                            {
                                labelfunc(tmp, fileName, out label, out labelID);
                                result.Add(new labelsGrid(fileName, lang, labelID, label, desc));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Ошибка: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                            tmp = "";
                            firstPart = null;
                        }
                    }
                }
                if (firstPart != null)
                {
                    labelfunc(firstPart, fileName, out label, out labelID);
                    result.Add(new labelsGrid(fileName, lang, labelID, label, ""));
                    firstPart = null;
                }

            }

            if (textFilter.IsChecked == true) { textFilter_Checked(); }
            if (exactFilter.IsChecked == true) { exactFilter_Checked(); }

            mainWindow.IsEnabled = true;
        }
        private void labelfunc(string firstPart, string fileName, out string label, out string labelID)
        {
            var fNCut = Regex.Replace(fileName, "." + lang + ".label.txt", "");

            int k = firstPart.IndexOf('=');
            string labelName = firstPart.Substring(0, k);
            label = firstPart.Substring(k + 1);
            if (firstPart.Contains("@")) labelID = labelName;
            else labelID = "@" + fNCut + ':' + labelName;
        }

        private void chooseLang_Loaded(object sender, RoutedEventArgs e)
        {
            string[] langs = { "ar", "ar-AE", "ar-BH",
                "ar-EG", "ar-KW", "ar-OM", "ar-QA", "cs",
                "da", "de", "de-AT", "de-CH", "en-AU",
                "en-CA", "en-GB", "en-HK", "en-IE", "en-IN",
                "en-MY", "en-NZ", "en-SG", "en-US", "en-ZA",
                "es", "es-MX", "et", "fi", "fr", "fr-BE",
                "fr-CA", "fr-CH", "hu", "is", "it", "it-CH",
                "ja", "lt", "lv", "nb-NO", "nl", "nl-BE",
                "pl", "pt-BR", "ru", "sv", "th", "tr", "zh-Hans"
            };

            var choose = sender as ComboBox;
            choose.ItemsSource = langs;
            choose.SelectedIndex = Array.IndexOf(langs, "ru");
            lang = langs[choose.SelectedIndex];
        }

        private void SearchPathButton_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new WindowsForms.FolderBrowserDialog();
            WindowsForms.DialogResult result = fbd.ShowDialog();
            selectedFolder = fbd.SelectedPath;
            MessageBox.Show(String.Format("Выбрана папка \"{0}\"\t", selectedFolder));
            Properties.Settings.Default.RecentDirRu = Convert.ToString(selectedFolder);
            Properties.Settings.Default.Save();

        }

        private void recentFiles_Loaded(object sender, RoutedEventArgs e)
        {
            var recentDirs = sender as ComboBox;
            recentDirs.Items.Add(Properties.Settings.Default.RecentDirRu);

        }

        private void recentFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosenrecentDir = sender as ComboBox;
            selectedFolder = chosenrecentDir.SelectedItem as string;
        }

        private void chooseLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLang = sender as ComboBox;
            lang = selectedLang.SelectedItem as string;
        }

        private void textFilter_Checked()
        {

            labelsGrid.ItemsSource = result;
        }

        private void phraseFilter_Checked()
        {

            string searchText = keyWord.Text;
            List<labelsGrid> modResult = new List<labelsGrid>(4);
            foreach (var item in result)
            {
                if (item.label.ToLower().Contains(searchText.ToLower())
                    && item.label.IndexOf(searchText[0]) - 1 == item.label.IndexOf(' ')
                    && item.label.IndexOf(searchText.Substring(searchText.Length - 1)) - 1 == item.label.IndexOf(' '))
                {
                    modResult.Add(item);
                }
            }
            labelsGrid.ItemsSource = modResult;
        }

        private void exactFilter_Checked()
        {
            List<labelsGrid> modResult = new List<labelsGrid>(4);
            foreach (var item in result)
            {
                if (keyWord.Text.ToLower() == item.label.ToLower()) modResult.Add(item);
            }
            labelsGrid.ItemsSource = modResult;
        }

        private void keyWord_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) searchButton_Click(sender, e);
        }
        private void grid_Loaded(object sender, RoutedEventArgs e) { }
        private void searchButton_Click(object sender, RoutedEventArgs e) => parser();
    }
}