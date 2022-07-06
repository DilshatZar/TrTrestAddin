using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TrTrestAddin.Model;

namespace TrTrestAddin.ViewModel
{
    internal class ApartmentographyConfigViewModel
    {
        Configuration libConfig;
        AppSettingsSection section;
        KeyValueConfigurationCollection settings;

        public ParameterModel fullTagTypeFamily { get; set; }
        public ParameterModel fullTagType { get; set; }
        public ParameterModel fullTagTypeId { get; set; }
        public ParameterModel areaTagTypeFamily { get; set; }
        public ParameterModel areaTagType { get; set; }
        public ParameterModel areaTagTypeId { get; set; }
        
        public ApartmentographyConfigViewModel()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            section = libConfig.GetSection("appSettings") as AppSettingsSection;
            settings = libConfig.AppSettings.Settings;

            fullTagTypeFamily = new ParameterModel("Семейство полной марки квартиры", "FullTagTypeFamily", settings["FullTagTypeFamily"].Value);
            fullTagType = new ParameterModel("Полная марка квартиры", "FullTagType", settings["FullTagType"].Value);
            fullTagTypeId = new ParameterModel("ID полной марки квартиры", "FullTagTypeId", settings["FullTagTypeId"].Value);
            areaTagTypeFamily = new ParameterModel("Семейсвто марки площади помещения", "AreaTagTypeFamily", settings["AreaTagTypeFamily"].Value);
            areaTagType = new ParameterModel("Марка площади помещения", "AreaTagType", settings["AreaTagType"].Value);
            areaTagTypeId = new ParameterModel("ID марки площади помещения", "AreaTagTypeId", settings["AreaTagTypeId"].Value);
        }

        public void ApplyChanges(
            string _fullTagTypeFamily, string _fullTagType, string _fullTagTypeId,
            string _areaTagTypeFamily, string _areaTagType, string _areaTagTypeId)
        {
            if (!_fullTagType.Equals(fullTagType.Value) || !_fullTagTypeFamily.Equals(fullTagTypeFamily.Value))
            {
                fullTagTypeFamily.Value = _fullTagTypeFamily;
                fullTagType.Value = _fullTagType;
                fullTagTypeId.Value = _fullTagTypeId;
                settings[fullTagTypeFamily.Name].Value = _fullTagTypeFamily;
                settings[fullTagType.Name].Value = _fullTagType;
                settings[fullTagTypeId.Name].Value = _fullTagTypeId;
            }
            if (!_areaTagType.Equals(areaTagType.Value) || !_areaTagTypeFamily.Equals(areaTagTypeFamily.Value))
            {
                areaTagTypeFamily.Value = _areaTagTypeFamily;
                areaTagType.Value = _areaTagType;
                areaTagTypeId.Value = _areaTagTypeId;
                settings[areaTagTypeFamily.Name].Value = _areaTagTypeFamily;
                settings[areaTagType.Name].Value = _areaTagType;
                settings[areaTagTypeId.Name].Value = _areaTagTypeId;
            }
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }
    }
}
