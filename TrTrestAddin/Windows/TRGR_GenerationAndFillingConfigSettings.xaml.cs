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
        private List<Family> roomTagFamilies;
        private List<RoomTagType> roomTagTypes;
        private List<Family> doorFamilies;
        private List<FamilySymbol> doorTypes;
        private Document doc;
        public TRGR_GenerationAndFillingConfigSettings(Document doc)
        {
            InitializeComponent();
            this.doc = doc;

            roomTagFamilies = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategoryId.IntegerValue == (int)BuiltInCategory.OST_RoomTags)
                .ToList();
            roomTagTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .WhereElementIsElementType()
                .Cast<RoomTagType>()
                .ToList();

            doorFamilies = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategoryId.IntegerValue == (int)BuiltInCategory.OST_Doors)
                .ToList();
            doorTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();

            roomTagsFamilyCmbBox.ItemsSource = roomTagFamilies.Select(f => f.Name);
            roomTagsFamilyCmbBox.SelectedItem = configViewModel.roomTagFamily.Value;

            entryDoorsFamilyCmbBox.ItemsSource = doorFamilies.Select(f => f.Name);
            entryDoorsFamilyCmbBox.SelectedItem = configViewModel.entryDoorFamily.Value;

            deleteMOPCheckBox.IsChecked = bool.Parse(configViewModel.deleteMop.Value);
        }
        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            string roomTagFamily = roomTagsFamilyCmbBox.SelectedItem.ToString();
            string roomTagType = roomTagsCmbBox.SelectedItem.ToString();
            string roomTagTypeId = "";
            string entryDoorFamily = entryDoorsFamilyCmbBox.SelectedItem.ToString();
            string entryDoorType = entryDoorsCmbBox.SelectedItem.ToString();
            string entryDoorTypeId = "";
            if (roomTagTypes.Where(tag => tag.Name.Equals(roomTagType) && tag.FamilyName.Equals(roomTagFamily)).Count() == 1)
            {
                roomTagTypeId = roomTagTypes.Where(tag => tag.Name.Equals(roomTagType) && tag.FamilyName.Equals(roomTagFamily))
                    .Select(tag => tag.Id.IntegerValue.ToString()).First();
            }
            if (doorTypes.Where(door => door.Name.Equals(entryDoorType) && door.FamilyName.Equals(entryDoorFamily)).Count() == 1)
            {
                entryDoorTypeId = doorTypes.Where(door => door.Name.Equals(entryDoorType) && door.FamilyName.Equals(entryDoorFamily))
                    .Select(door => door.Id.IntegerValue.ToString()).First();
            }
            configViewModel.ApplyChanges(roomTagFamily, roomTagType, roomTagTypeId, entryDoorFamily, entryDoorType, entryDoorTypeId, deleteMOPCheckBox.IsChecked.ToString());
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void roomTagsFamilyCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string roomTagFamily = roomTagsFamilyCmbBox.SelectedItem.ToString();
            roomTagsCmbBox.ItemsSource = roomTagTypes.Where(t => t.FamilyName.Equals(roomTagFamily)).Select(t => t.Name);
            if (roomTagFamily.Equals(configViewModel.roomTagFamily.Value))
            {
                roomTagsCmbBox.SelectedItem = configViewModel.roomTagType.Value;
            }
            else
            {
                roomTagsCmbBox.SelectedIndex = 0;
            }
        }

        private void entryDoorsFamilyCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string entryDoorFamily = entryDoorsFamilyCmbBox.SelectedItem.ToString();
            entryDoorsCmbBox.ItemsSource = doorTypes.Where(d => d.FamilyName.Equals(entryDoorFamily)).Select(d => d.Name);
            if (entryDoorFamily.Equals(configViewModel.entryDoorFamily.Value))
            {
                entryDoorsCmbBox.SelectedItem = configViewModel.entryDoorType.Value;
            }
            else
            {
                entryDoorsCmbBox.SelectedIndex = 0;
            }
        }
    }
}
