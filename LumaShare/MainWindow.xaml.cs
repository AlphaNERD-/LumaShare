using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LumaShare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool allowConfigUpdate = true;
        private string configFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "config.txt");

        private List<Profile> profiles = new List<Profile>();
        public List<Profile> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                profiles = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Profiles)));
            }
        }

        private Profile selectedProfile;
        public Profile SelectedProfile
        {
            get
            {
                return selectedProfile;
            }
            set
            {
                selectedProfile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProfile)));
                UpdateConfig();
                UpdateSharepic();
            }
        }

        private string topImagePath;
        public string TopImagePath
        {
            get
            {
                return topImagePath;
            }
            set
            {
                topImagePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopImagePath)));
                UpdateSharepic();
            }
        }

        private string bottomImagePath;
        public string BottomImagePath
        {
            get
            {
                return bottomImagePath;
            }
            set
            {
                bottomImagePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BottomImagePath)));
                UpdateSharepic();
            }
        }

        private RenderTargetBitmap sharepic;
        public RenderTargetBitmap Sharepic
        {
            get
            {
                return sharepic;
            }
            set
            {
                sharepic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sharepic)));
            }
        }

        private Dictionary<string, BackgroundSettingEnum> backgroundSettings;
        public Dictionary<string, BackgroundSettingEnum> BackgroundSettings
        {
            get
            {
                return backgroundSettings;
            }
            set
            {
                backgroundSettings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundSettings)));
            }
        }

        private KeyValuePair<string, BackgroundSettingEnum> selectedBackgroundSettings;
        public KeyValuePair<string, BackgroundSettingEnum> SelectedBackgroundSettings
        {
            get
            {
                return selectedBackgroundSettings;
            }
            set
            {
                selectedBackgroundSettings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedBackgroundSettings)));

                UpdateConfig();

                if (value.Value == BackgroundSettingEnum.TopScreenBlurred & !(TopImagePath == "" | TopImagePath == null))
                    UpdateSharepic();
                else if (value.Value == BackgroundSettingEnum.BottomScreenBlurred & !(BottomImagePath == "" | BottomImagePath == null))
                    UpdateSharepic();
                else if (value.Value == BackgroundSettingEnum.Transparent)
                    UpdateSharepic();
            }
        }

        private string border;
        public string Border
        {
            get
            {
                return border;
            }
            set
            {
                border = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Border)));

                UpdateConfig();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            allowConfigUpdate = false;

            this.DataContext = this;

            BackgroundSettings = new Dictionary<string, BackgroundSettingEnum>();
            BackgroundSettings.Add("Transparent", BackgroundSettingEnum.Transparent);
            BackgroundSettings.Add("Top Screen (blurred)", BackgroundSettingEnum.TopScreenBlurred);
            BackgroundSettings.Add("Bottom Screen (blurred)", BackgroundSettingEnum.BottomScreenBlurred);
            SelectedBackgroundSettings = BackgroundSettings.ElementAt(0);

            string profileDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "profiles");

            if (!Directory.Exists(profileDirectory))
                Directory.CreateDirectory(profileDirectory);

            string[] xmlFiles = Directory.GetFiles(profileDirectory, "*.xml");

            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    Profile p = new Profile(xmlFile);
                    Profiles.Add(p);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("An error has occured while loading the profile \"" + xmlFile + "\":\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (Profiles.Count() == 0)
            {
                System.Windows.MessageBox.Show("No profiles found. Please download and add profiles to LumaShare.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }

            LoadConfig();
        }

        private void UpdateSharepic()
        {
            if (SelectedProfile != null)
            {
                try
                {
                    Sharepic = LumaSharepicRenderer.RenderSharepic(topImagePath, bottomImagePath, selectedProfile, SelectedBackgroundSettings.Value,  Convert.ToInt32(Border));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("An error has occured while generating the sharepic:\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void btnPathSelectTop_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Top Screen Screenshots|*_top.bmp|All supported image files|*.png;*.bmp;*.jpg";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TopImagePath = openFileDialog.FileName;

                CheckForSecondImageFile(openFileDialog.FileName);
            }
        }

        private void btnPathSelectBottom_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bottom Screen Screenshots|*_bot.bmp|All supported image files|*.png;*.bmp;*.jpg";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BottomImagePath = openFileDialog.FileName;

                CheckForSecondImageFile(openFileDialog.FileName);
            }
        }

        private void CheckForSecondImageFile(string fileName)
        {
            if (fileName.Contains("_bot") & (TopImagePath == null || TopImagePath == ""))
            {
                string tempTopImage = fileName.Replace("_bot", "_top");

                if (File.Exists(tempTopImage))
                    TopImagePath = tempTopImage;
            }
            else if (fileName.Contains("_top") & (BottomImagePath == null || BottomImagePath == ""))
            {
                string tempBottomImage = fileName.Replace("_top", "_bot");

                if (File.Exists(tempBottomImage))
                    BottomImagePath = tempBottomImage;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            TopImagePath = "";
            BottomImagePath = "";
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void tbxBorder_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void tbxBorder_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void btnRefreshBorder_Click(object sender, RoutedEventArgs e)
        {
            UpdateSharepic();
        }

        private void btnSavePicture_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG file|*.png";
            sfd.DefaultExt = "png";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BitmapFrame frame = BitmapFrame.Create(Sharepic);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);

                using (var stream = File.Create(sfd.FileName))
                {
                    encoder.Save(stream);
                }
            }
        }

        private void LoadConfig()
        {
            if (File.Exists(configFileName))
            {
                string[] config = File.ReadAllLines(configFileName);

                SelectedProfile = Profiles.Find(x => x.ProfileName == config[0].Split("=")[1]);
                SelectedBackgroundSettings = BackgroundSettings.SingleOrDefault(x => x.Value.ToString() == config[1].Split("=")[1]);
                Border = config[2].Split("=")[1];

                allowConfigUpdate = true;
            }
            else
            {
                SelectedProfile = Profiles.ElementAt(0);
                SelectedBackgroundSettings = BackgroundSettings.ElementAt(0);
                Border = "0";

                allowConfigUpdate = true;
                UpdateConfig();
            }
        }

        private void UpdateConfig()
        {
            if (allowConfigUpdate)
            {
                string[] config = new string[] { "SelectedProfile=" + SelectedProfile.ProfileName, "SelectedBackgroundSettings=" + SelectedBackgroundSettings.Value.ToString(), "Border=" + Border };

                File.WriteAllLines(configFileName, config);
            }
        }
    }
}
