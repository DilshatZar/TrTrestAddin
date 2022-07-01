using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TrTrestAddin.Windows
{
    /// <summary>
    /// Interaction logic for TRGR_ConfigSettingsWindow.xaml
    /// </summary>
    public partial class TRGR_ConfigSettingsWindow : Window
    {
        Configuration libConfig;
        AppSettingsSection section;
        KeyValueConfigurationCollection settings;
        Dictionary<string, string> configurations = new Dictionary<string, string>();
        public TRGR_ConfigSettingsWindow()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            section = libConfig.GetSection("appSettings") as AppSettingsSection;
            settings = libConfig.AppSettings.Settings;

            InitializeComponent();

            configurations.Add("Числа после запятой", "RoundingNumber");
            configurations.Add("Коэффициент площади лоджии", "LoggiaAreaCoef");
            configurations.Add("Коэффициент площади балкона", "BalconyAreaCoef");
            configurations.Add("Коэффициент площади обычных помещений", "DefaultAreaCoef");
            configurations.Add("Смещение комнаты сверху", "RoomUpperOffset");
            configurations.Add("Смещение комнаты снизу", "RoomLowerOffset");

            cmbBox.ItemsSource = configurations.Keys;

            cmbBox.SelectedIndex = 0;
            try
            {
                txtBox.Text = settings[configurations[cmbBox.SelectedItem.ToString()]].Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void acceptChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                settings[configurations[cmbBox.SelectedItem.ToString()]].Value = txtBox.Text;
                libConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения параметра.\n" + ex.Message, "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void cmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                txtBox.Text = settings[configurations[cmbBox.SelectedItem.ToString()]].Value;
                txtBox.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(cmbBox.SelectedItem.ToString(), ex.Message);
            }
        }

        public string GetParameterValue(string name)
        {
            return settings[name].Value;
        }
        public void SetParameterValue(string name, string value)
        {
            settings[name].Value = value;
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }
    }
}
