using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TrTrestAddin.ViewModel;

namespace TrTrestAddin.Windows
{
    /// <summary>
    /// Interaction logic for TRGR_GenerationAndFillingConfigSettings.xaml
    /// </summary>
    public partial class TRGR_GenerationAndFillingConfigSettings : Window
    {
        private GenerationAndFillingConfigViewModel configViewModel { get; } = new GenerationAndFillingConfigViewModel();
        private List<RoomTagType> roomTagTypes;
        private List<FamilySymbol> doorTypes;
        private Document doc;
        public TRGR_GenerationAndFillingConfigSettings(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
            roomTagTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .WhereElementIsElementType()
                .Cast<RoomTagType>()
                .ToList();
            doorTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();

            roomTagsCmbBox.ItemsSource = roomTagTypes.Select(tag => tag.Name);
            roomTagsCmbBox.SelectedItem = configViewModel.roomTagType.Value;

            entryDoorCmbBox.ItemsSource = doorTypes.Select(tag => tag.Name);
            entryDoorCmbBox.SelectedItem = configViewModel.entryDoorType.Value;

            deleteMOPCheckBox.IsChecked = bool.Parse(configViewModel.deleteMop.Value);
        }
        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            string roomTagTypeId = roomTagTypes.Where(tag => tag.Name.Equals(roomTagsCmbBox.Text))
                .Select(tag => tag.Id.IntegerValue.ToString()).First();
            string entryDoorTypeId = doorTypes.Where(door => door.Name.Equals(entryDoorCmbBox.Text))
                .Select(door => door.Id.IntegerValue.ToString()).First();
            configViewModel.ApplyChanges(roomTagsCmbBox.Text, roomTagTypeId, entryDoorCmbBox.Text, entryDoorTypeId, deleteMOPCheckBox.IsChecked.ToString());
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
