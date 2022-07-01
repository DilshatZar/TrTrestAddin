using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TrTrestAddin.Windows
{
    /// <summary>
    /// Interaction logic for TRGR_RoomIdList.xaml
    /// </summary>
    public partial class TRGR_RoomIdListWindow : Window
    {
        public TRGR_RoomIdListWindow()
        {
            InitializeComponent();
        }
        public void AddNewLine(int id, double prevValue, double newValue)
        {
            roomIdsList.Items.Add(new ListViewItem { Content = new RoomValsViewModel(id, prevValue, newValue) });
        }

        private void roomIdsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems.OfType<ListViewItem>())
            {
                RoomValsViewModel roomValsViewModel = (RoomValsViewModel)item.Content;
                txtBlock.Text = $"ID {roomValsViewModel.roomId} скопирован в буфер обмена.";
                Clipboard.SetText(roomValsViewModel.roomId.ToString());
            }
        }
    }
    public class RoomValsViewModel
    {
        public int roomId { get; set; }
        public double roomPrevValue { get; set; }
        public double roomNewValue { get; set; }
        public RoomValsViewModel(int id, double prevValue, double newValue)
        {
            roomId = id;
            roomPrevValue = prevValue;
            roomNewValue = newValue;
        }
    }
}
