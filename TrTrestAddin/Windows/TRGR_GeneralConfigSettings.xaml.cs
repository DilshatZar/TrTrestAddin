using Autodesk.Revit.DB;
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
using TrTrestAddin.Model;
using TrTrestAddin.ViewModel;
using TrTrestAddin.Windows;

namespace TrTrestAddin.Windows
{
    /// <summary>
    /// Interaction logic for TRGR_GeneralConfigSettings.xaml
    /// </summary>
    public partial class TRGR_GeneralConfigSettings : Window
    {
        private GeneralConfigViewModel configViewModel { get; } = new GeneralConfigViewModel();
        public Document doc;
        public TRGR_GeneralConfigSettings(Document doc)
        {
            InitializeComponent();
            DataContext = configViewModel;
            this.doc = doc;
        }

        private void parametersTable_SelectionChanged(object sender, RoutedEventArgs e)
        {
            configViewModel.ChangeEvent();
        }

        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            configViewModel.ApplyChanges();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetDefaultValues_Click(object sender, RoutedEventArgs e)
        {
            configViewModel.SetDefaultValues();
            parametersTable.Items.Refresh();
        }

        private void GenerationAndFillingConfigSettings_Click(object sender, RoutedEventArgs e)
        {
            TRGR_GenerationAndFillingConfigSettings genSettings = new TRGR_GenerationAndFillingConfigSettings(doc);
            genSettings.ShowDialog();
        }

        private void ApartmentographyConfigSettings_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
