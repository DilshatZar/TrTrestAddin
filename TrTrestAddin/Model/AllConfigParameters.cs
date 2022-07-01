using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrTrestAddin.Model
{
    internal class AllConfigParameters
    {
        private Configuration libConfig;
        private AppSettingsSection section;
        private KeyValueConfigurationCollection settings;

        public Dictionary<string, string> GetParameter { get; }
        private static Dictionary<string, string> _defaultParametersValues = new Dictionary<string, string>
        {
            {"RoundingNumber", "2"},
            {"LoggiaAreaCoef", "0,5"},
            {"BalconyAreaCoef", "0,3"},
            {"DefaultAreaCoef", "1,0"},
            {"RoomUpperOffset", "3300"},
            {"RoomLowerOffset", "0"},
            {"BasicRoomTag", ""},
            {"BasicRoomTagId", ""},
            {"EntryDoorType", ""},
            {"EntryDoorTypeId", ""},
            {"DeleteMOP", "True"}
        };
        public AllConfigParameters()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            section = libConfig.GetSection("appSettings") as AppSettingsSection;
            settings = libConfig.AppSettings.Settings;

            foreach (string key in settings.AllKeys)
            {
                if (!key.Equals("ClientSettingsProvider.ServiceUri"))
                {
                    GetParameter.Add(key, settings[key].Value);
                }
            }
        }

        public void SetValue(string parameterName, string value)
        {
            GetParameter[parameterName] = value;
            settings[parameterName].Value = value;
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }

        public void SetDefaultValues()
        {
            foreach (string key in _defaultParametersValues.Keys)
            {
                GetParameter[key] = _defaultParametersValues[key];
                settings[key].Value = _defaultParametersValues[key];
            }
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }
    }
}
