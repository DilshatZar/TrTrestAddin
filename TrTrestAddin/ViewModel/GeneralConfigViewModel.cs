using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TrTrestAddin.Model;

namespace TrTrestAddin.ViewModel
{
    internal class GeneralConfigViewModel
    {
        Configuration libConfig;
        AppSettingsSection section;
        KeyValueConfigurationCollection settings;

        public ObservableCollection<ParameterModel> Parameters { get; set; } = new ObservableCollection<ParameterModel>();
        public ParameterModel SelectedParameter { get; set; }
        public List<ParameterModel> ChangedParameters { get; set; }

        private static List<string> parameterNames = new List<string> { 
            "RoundingNumber",
            "DefaultAreaCoef",
            "LoggiaAreaCoef",
            "BalconyAreaCoef",
            "RoomUpperOffset",
            "RoomLowerOffset"};
        private static List<string> parameterDescriptions = new List<string> {
            "Числа после запятой",
            "Коэффициент площади помещений",
            "Коэффициент площади лоджии",
            "Коэффициент площади балкона",
            "Смещение сверху",
            "Смещение снизу"};
        private static Dictionary<string, string> DefaultValues = new Dictionary<string, string> {
            {"RoundingNumber",  "2"},
            {"DefaultAreaCoef", "1,0"},
            {"LoggiaAreaCoef",  "0,5"},
            {"BalconyAreaCoef", "0,3"},
            {"RoomUpperOffset", "3300"},
            {"RoomLowerOffset", "0"}
        };
        public GeneralConfigViewModel()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            section = libConfig.GetSection("appSettings") as AppSettingsSection;
            settings = libConfig.AppSettings.Settings;

            for (int i = 0; i < parameterNames.Count; i++)
            {
                Parameters.Add(new ParameterModel(parameterDescriptions[i], parameterNames[i], settings[parameterNames[i]].Value));
            }

            ChangedParameters = new List<ParameterModel> {};
        }

        public void ChangeEvent()
        {
            if (SelectedParameter != null 
                && !ChangedParameters.Contains(SelectedParameter))
            {
                ChangedParameters.Add(SelectedParameter);
            }
        }
        public void ApplyChanges()
        {
            if (ChangedParameters.Count > 0)
            {
                foreach (ParameterModel parameter in ChangedParameters)
                {
                    if (settings[parameter.Name].Value != parameter.Value)
                    {
                        settings[parameter.Name].Value = parameter.Value;
                    }
                }
                libConfig.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
            }
        }
        public void SetDefaultValues()
        {
            foreach (ParameterModel parameter in Parameters)
            {
                if (parameter.Value != DefaultValues[parameter.Name])
                {
                    ChangedParameters.Add(parameter);
                    parameter.Value = DefaultValues[parameter.Name];
                }
            }
        }
    }
}
