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
    internal class GenerationAndFillingConfigViewModel
    {
        Configuration libConfig;
        AppSettingsSection section;
        KeyValueConfigurationCollection settings;

        public ParameterModel roomTagFamily { get; set; }
        public ParameterModel roomTagType { get; set; }
        public ParameterModel roomTagTypeId { get; set; }
        public ParameterModel entryDoorFamily { get; set; }
        public ParameterModel entryDoorType { get; set; }
        public ParameterModel entryDoorTypeId { get; set; }
        public ParameterModel deleteMop { get; set; }
        public GenerationAndFillingConfigViewModel()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            section = libConfig.GetSection("appSettings") as AppSettingsSection;
            settings = libConfig.AppSettings.Settings;

            roomTagFamily = new ParameterModel("Семейсто марки помещения", "BasicRoomTagFamily", settings["BasicRoomTagFamily"].Value);
            roomTagType = new ParameterModel("Марка помещения", "BasicRoomTag", settings["BasicRoomTag"].Value);
            roomTagTypeId = new ParameterModel("ID марки помещения", "BasicRoomTagId", settings["BasicRoomTag"].Value);
            entryDoorFamily = new ParameterModel("Семейство типоразмера входной двери", "EntryDoorTypeFamily", settings["EntryDoorTypeFamily"].Value);
            entryDoorType = new ParameterModel("Типоразмер входной двери", "EntryDoorType", settings["EntryDoorType"].Value);
            entryDoorTypeId = new ParameterModel("ID типоразмера входной двери", "EntryDoorTypeId", settings["EntryDoorType"].Value);
            deleteMop = new ParameterModel("Удаление МОП'ов", "DeleteMOP", settings["DeleteMOP"].Value);
        }

        public void ApplyChanges(
            string _roomTagFamily, string _roomTagType, string _roomTagTypeId,
            string _entryDoorFamily, string _entryDoorType, string _entryDoorTypeId,
            string isDeleteMOP
            )
        {
            if (!_roomTagType.Equals(roomTagType.Value) || !_roomTagFamily.Equals(roomTagFamily.Value))
            {
                roomTagFamily.Value = _roomTagFamily;
                roomTagType.Value = _roomTagType;
                roomTagTypeId.Value = _roomTagTypeId;
                settings[roomTagFamily.Name].Value = _roomTagFamily;
                settings[roomTagType.Name].Value = _roomTagType;
                settings[roomTagTypeId.Name].Value = _roomTagTypeId;
            }
            if (!_entryDoorType.Equals(entryDoorType.Value) || !_entryDoorFamily.Equals(entryDoorFamily.Value))
            {
                entryDoorFamily.Value = _entryDoorFamily;
                entryDoorType.Value = _entryDoorType;
                entryDoorTypeId.Value = _entryDoorTypeId;
                settings[entryDoorFamily.Name].Value = _entryDoorFamily;
                settings[entryDoorType.Name].Value = _entryDoorType;
                settings[entryDoorTypeId.Name].Value = _entryDoorTypeId;
            }
            if (!isDeleteMOP.Equals(deleteMop.Value))
            {
                deleteMop.Value = isDeleteMOP;
                settings[deleteMop.Name].Value = isDeleteMOP;
            }
            libConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(libConfig.AppSettings.SectionInformation.Name);
        }
    }
}
