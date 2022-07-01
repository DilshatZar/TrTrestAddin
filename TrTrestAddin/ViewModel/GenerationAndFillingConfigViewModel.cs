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

        public ParameterModel roomTagType { get; set; }
        public ParameterModel roomTagTypeId { get; set; }
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

            roomTagType = new ParameterModel("Марка помещения", "BasicRoomTag", settings["BasicRoomTag"].Value);
            roomTagTypeId = new ParameterModel("ID марки помещения", "BasicRoomTagId", settings["BasicRoomTag"].Value);
            entryDoorType = new ParameterModel("Типоразмер входной двери", "EntryDoorType", settings["EntryDoorType"].Value);
            entryDoorTypeId = new ParameterModel("ID типоразмера входной двери", "EntryDoorTypeId", settings["EntryDoorType"].Value);
            deleteMop = new ParameterModel("Удаление МОП'ов", "DeleteMOP", settings["DeleteMOP"].Value);
        }

        public void ApplyChanges(
            string roomTagTypeName, string _roomTagTypeId,
            string entryDoorTypeName, string _entryDoorTypeId,
            string isDeleteMOP
            )
        {
            if (!roomTagTypeName.Equals(roomTagType.Value))
            {
                roomTagType.Value = roomTagTypeName;
                roomTagTypeId.Value = _roomTagTypeId;
                settings[roomTagType.Name].Value = roomTagTypeName;
                settings[roomTagTypeId.Name].Value = _roomTagTypeId;
            }
            if (!entryDoorTypeName.Equals(entryDoorType.Value))
            {
                entryDoorType.Value = entryDoorTypeName;
                entryDoorTypeId.Value = _entryDoorTypeId;
                settings[entryDoorType.Name].Value = entryDoorTypeName;
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
