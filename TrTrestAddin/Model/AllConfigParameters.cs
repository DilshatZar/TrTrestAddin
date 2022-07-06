using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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

        private Dictionary<string, string> Parameters { get; set; }
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
            {"DeleteMOP", "True"},
            {"FullTagType", ""},
            {"FullTagTypeId", ""},
            {"AreaTagType", ""},
            {"AreaTagTypeId", ""}
        };
        public AllConfigParameters()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            section = libConfig.GetSection("appSettings") as AppSettingsSection;
            settings = libConfig.AppSettings.Settings;

            Parameters = new Dictionary<string, string>();
            foreach (string key in settings.AllKeys)
            {
                if (!key.Equals("ClientSettingsProvider.ServiceUri"))
                {
                    Parameters.Add(key, settings[key].Value);
                }
            }
        }

        public void SetValue(string parameterName, string value)
        {
            Parameters[parameterName] = value;
            settings[parameterName].Value = value;
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }

        public void SetDefaultValues(string key)
        {
            Parameters[key] = _defaultParametersValues[key];
            settings[key].Value = _defaultParametersValues[key];
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }
        public int GetIntegerValue(string name)
        {
            return int.Parse(Parameters[name].Equals("") ? "0" : Parameters[name]);
        }
        public double GetDoubleValue(string name)
        {
            return double.Parse(Parameters[name].Replace(".", ","));
        }
        public string GetStringValue(string name)
        {
            return Parameters[name];
        }
        public bool GetBoolValue(string name)
        {
            return bool.Parse(Parameters[name]);
        }
        public bool WrongElementTypeParameters(List<Tuple<string, string, int>> configElements, Document doc) // item1 - Family Name, item2 - FamilySymbol Name, item3 - FamilySymbol ID
        {
            List<FamilySymbol> docElements = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).WhereElementIsElementType().Cast<FamilySymbol>().ToList();
            foreach (Tuple<string, string, int> element in configElements)
            {
                List<FamilySymbol> elementTypes = docElements.Where(el => el.FamilyName.Equals(element.Item1) && el.Name.Equals(element.Item2)).ToList();
                if (elementTypes.Count == 0) { return true; }
                if (elementTypes.Count == 1)
                {
                    int tag = elementTypes.First().Id.IntegerValue;
                    if (tag != element.Item3) { return true; }
                }
                if (elementTypes.Count > 1) { return true; }
            }
            return false;
        }
    }
}
