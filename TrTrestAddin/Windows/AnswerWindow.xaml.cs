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

namespace TrTrestAddin.Windows
{
    /// <summary>
    /// Interaction logic for AnswerWindow.xaml
    /// </summary>
    public partial class AnswerWindow : Window
    {
        public AnswerWindow()
        {
            InitializeComponent();
            Show();
        }
        public AnswerWindow(int number)
        {
            InitializeComponent();
            answerTextBlock.Text = number.ToString() + '\n';
            Show();
        }
        public AnswerWindow(double number, bool round = false)
        {
            InitializeComponent();
            if (round)
            {
                number = Math.Round(number);
            }
            answerTextBlock.Text = number.ToString() + '\n';
            Show();
        }
        public AnswerWindow(string text)
        {
            InitializeComponent();
            answerTextBlock.Text = text + '\n';
            Show();
        }

        public void WriteLine(string str = "") => answerTextBlock.Text += str + '\n';
        public void WriteLine(string[] str)
        {
            foreach (string str2 in str)
            {
                Write(str2 + ' ');
            }
        }
        public void WriteLine(bool val) => answerTextBlock.Text += val.ToString() + '\n';
        public void Write(string str = " ") => answerTextBlock.Text += str;
        public void Devide() => answerTextBlock.Text += "-------------------------------------\n";
    }
}
