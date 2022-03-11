using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web;
using System.Linq;
using System.Diagnostics;

namespace Helpful_Tools_for_Microsoft_AX
{
    /// <summary>
    /// Interaction logic for LabelTranslatorControl.
    /// </summary>
    public partial class LabelTranslatorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelTranslatorControl"/> class.
        /// </summary>
        public LabelTranslatorControl()
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
        private string _content;
        string selectedFolder = "";
        string lang = "";
        private const string key = "023cc3f9e19849eb907c1a630151ee28";

        private void OpenFileButton(ComboBox path, TextBox content)
        {
            var ofd = new OpenFileDialog();
            string filepath = "";
            ofd.Filter = "Текстовый файл (*.txt) | *.txt";

            if ((bool)!ofd.ShowDialog()) return;
            filepath = path.Text = Path.GetFullPath(ofd.FileName);

            ShowFile(filepath, content);
        }


        private string ShowFilePath(OpenFileDialog ofd) => Path.GetFullPath(ofd.FileName);

        private void RecentFiles(object sender, RoutedEventArgs e)
        {
            var recentDirs = sender as ComboBox;
            recentDirs.Items.Add(Properties.Settings.Default.RecentDirRu);
            recentDirs.Items.Add(Properties.Settings.Default.RecentDirEn);
        }

        private void recentFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var chosenrecentDir = sender as ComboBox;
            TextBox content;

            if (chosenrecentDir.Name == "ShowFilePathRu")
            {
                content = ShowFileRu;
            }
            else
            {
                content = ShowFileEn;
            }
            selectedFolder = chosenrecentDir.SelectedItem as string;

            ShowFile(selectedFolder, content);
        }

        private void ShowFile(string filepath, TextBox content)
        {
            _content = File.ReadAllText(filepath);
            content.Text = _content;

            if (content.Name == "ShowFileRu")
            {
                Properties.Settings.Default.RecentDirRu = Convert.ToString(filepath);
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.RecentDirEn = Convert.ToString(filepath);
                Properties.Settings.Default.Save();
            }

        }

        public async void Translator()
        {
            Window mainWindow = Window.GetWindow(this);
            mainWindow.IsEnabled = false;

            try
            {
                var russianWords = new List<string>();
                var textWithoutRu = Regex.Replace(_content, "[а-яА-ЯёЁ ]+", m => {
                    if (m.Value == " ") return " ";
                    russianWords.Add(m.Value);
                    return $"{{{russianWords.Count - 1}}}";
                });

                var russianList = new List<string>();
                var translated = new List<string>();
                var result = new List<string>();
                var currentLength = 0;
                const int maxLengthAllowed = 1000;
                string[] fileEn = File.ReadAllLines(ShowFilePathEn.Text);

                foreach (var word in russianWords)
                {
                    if (word.Length + currentLength < maxLengthAllowed)
                    {
                        currentLength += word.Length;
                        russianList.Add(word);

                    }
                    else
                    {
                        translated.AddRange(await TranslateRussianList(russianList));
                        russianList = new List<string>();
                        russianList.Add(word);
                        currentLength = 0;
                    }
                }

                if (translated.Count < russianWords.Count)
                {
                    translated.AddRange(await TranslateRussianList(russianList));
                }

                foreach (string lineEn in fileEn)
                {
                    result.Add(lineEn);
                }
                string tmp = string.Format(textWithoutRu, translated.ToArray());
                result.Add(tmp);

                ShowFileMod.Text = string.Join("\n", result.ToArray());
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            mainWindow.IsEnabled = true;
        }


        private void OpenFileButtonRu_Click(object sender, RoutedEventArgs e) => OpenFileButton(ShowFilePathRu, ShowFileRu);

        private void OpenFileButtonEn_Click(object sender, RoutedEventArgs e) => OpenFileButton(ShowFilePathEn, ShowFileEn);

        private void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowNewLabels.Text == "" || ShowNewLabels.Text == null)
            {
                MessageBox.Show("Ошибка: Поле новых меток пустое. Сначала выберите меточный файл для экспорта, " +
                    "потом количество новых меток, " +
                    "только после этого нажимайте на кнопку." + '\n' + '\n' +
                    "Я не дам вам все испортить. Не плачьте.",
                    "Э, нет, так дело не пойдет",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Translator();
            }
        }


        private void FileAdditionalButton_Click(object sender, RoutedEventArgs e)
        {
            string[] fileRu = File.ReadAllLines(ShowFilePathRu.Text);

            int lineEnum = 0;
            int thisLineNum = 0;
            int newLabelsAmount = Int32.Parse(NewLabelsAmount.Text);

            try
            {
                var result = new List<string>();

                foreach (string lineRu in fileRu)
                {
                    if (lineRu.Contains("="))
                    {
                        lineEnum += 1;
                    }
                }

                foreach (string lineRu in fileRu)
                {
                    if (lineRu.Contains("="))
                    {
                        thisLineNum += 1;
                    }

                    if (thisLineNum < (lineEnum - (newLabelsAmount - 1)))
                    {
                        continue;
                    }
                    else
                    {
                        if (lineRu.Contains("="))
                        {
                            result.Add(lineRu);
                        }

                        if (lineRu.Contains(";"))
                        {
                            result.Add(lineRu);
                        }
                    }
                }
                string resultMod = _content = string.Join("\n", result);
                ShowNewLabels.Text = _content;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Текстовый файл (*.txt) | *.txt";

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, ShowFileMod.Text);
            }
        }

        private void ChooseLang_Loaded(object sender, RoutedEventArgs e)
        {
            string[] langs = { "ar", "cs",
                "da", "de", "de-AT", "de-CH", "en-AU",
                "en-CA", "en", "en-HK", "en-IE", "en-IN",
                "en-MY", "en-NZ", "en-SG", "en-US", "en-ZA",
                "es", "es-MX", "et", "fi", "fr", "fr-BE",
                "fr-CA", "fr-CH", "hu", "is", "it", "it-CH",
                "ja", "lt", "lv", "nb-NO", "nl", "nl-BE",
                "pl", "pt-BR", "ro", "ru", "sv", "th", "tr", "zh-Hans"
            };

            var choose = sender as ComboBox;
            choose.ItemsSource = langs;
            choose.SelectedIndex = Array.IndexOf(langs, "ru");
            lang = langs[choose.SelectedIndex];
        }

        private void ChooseLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLang = sender as ComboBox;
            lang = selectedLang.SelectedItem as string;
        }

        private async Task<List<string>> TranslateRussianList(List<string> words)
        {

            var uri = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=ru&to=" + lang;

            object[] body = words.Select(t => new { Text = t }).ToArray();

            var RequestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(RequestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", "northeurope");

                var response = await client.SendAsync(request);
                var ResponseBody = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(ResponseBody);

                var TranslatedWords = result.Select(t => t["translations"][0]["text"]);

                return TranslatedWords.ToList();
            }



        }
    }
}