using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
    /// Interaction logic for TRGR_ApartmentographyConfigSettings.xaml
    /// </summary>
    public partial class TRGR_ApartmentographyConfigSettings : Window
    {
        private ApartmentographyConfigViewModel configViewModel { get; } = new ApartmentographyConfigViewModel();
        private List<Family> allTagsFamilies;
        private List<RoomTagType> allRoomTags;
        private Document doc;

        public TRGR_ApartmentographyConfigSettings(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
            allTagsFamilies = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategoryId.IntegerValue == (int)BuiltInCategory.OST_RoomTags)
                .ToList();

            allRoomTags = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .WhereElementIsElementType()
                .Cast<RoomTagType>()
                .ToList();

            FullTagFamilyCmbBox.ItemsSource = allTagsFamilies.Select(tag => tag.Name);
            FullTagFamilyCmbBox.SelectedItem = configViewModel.fullTagTypeFamily.Value;

            AreaTagFamilyCmbBox.ItemsSource = allTagsFamilies.Select(tag => tag.Name);
            AreaTagFamilyCmbBox.SelectedItem = configViewModel.areaTagTypeFamily.Value;
        }
        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            string FullTagFamilyName = FullTagFamilyCmbBox.SelectedItem.ToString();
            string FullTagTypeName = FullTagCmbBox.SelectedItem.ToString();
            string AreaTagFamilyName = AreaTagFamilyCmbBox.SelectedItem.ToString();
            string AreaTagTypeName = AreaTagCmbBox.SelectedItem.ToString();
            string fullTagTypeId = "";
            string areaTagTypeId = "";
            if (allRoomTags.Where(tag => tag.Name.Equals(FullTagTypeName) && tag.FamilyName.Equals(FullTagFamilyName)).Count() == 1)
            {
                fullTagTypeId = allRoomTags.Where(tag => tag.Name.Equals(FullTagTypeName) && tag.FamilyName.Equals(FullTagFamilyName))
                    .Select(tag => tag.Id.IntegerValue.ToString()).First();
            }
            if (allRoomTags.Where(tag => tag.Name.Equals(AreaTagTypeName) && tag.FamilyName.Equals(AreaTagFamilyName)).Count() == 1)
            {
                areaTagTypeId = allRoomTags.Where(tag => tag.Name.Equals(AreaTagTypeName) && tag.FamilyName.Equals(AreaTagFamilyName))
                    .Select(door => door.Id.IntegerValue.ToString()).First();
            }
            configViewModel.ApplyChanges(FullTagFamilyName, FullTagTypeName, fullTagTypeId, AreaTagFamilyName, AreaTagTypeName, areaTagTypeId);
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FullTagFamilyCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FullTagCmbBox.ItemsSource = allRoomTags.Where(t => t.FamilyName.Equals(FullTagFamilyCmbBox.SelectedItem.ToString())).Select(t => t.Name);
            if (FullTagFamilyCmbBox.SelectedItem.ToString().Equals(configViewModel.fullTagTypeFamily.Value)) 
            { 
                FullTagCmbBox.SelectedItem = configViewModel.fullTagType.Value; 
            }
            else 
            { 
                FullTagCmbBox.SelectedIndex = 0; 
            }
        }

        private void AreaTagFamilyCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AreaTagCmbBox.ItemsSource = allRoomTags.Where(t => t.FamilyName.Equals(AreaTagFamilyCmbBox.SelectedItem.ToString())).Select(t => t.Name);
            if (AreaTagFamilyCmbBox.SelectedItem.ToString().Equals(configViewModel.areaTagTypeFamily.Value))
            {
                AreaTagCmbBox.SelectedItem = configViewModel.areaTagType.Value;
            }
            else
            {
                AreaTagCmbBox.SelectedIndex = 0;
            }
        }
    }
}
