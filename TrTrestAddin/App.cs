#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrTrestAddin.Commands;
#endregion

namespace TrTrestAddin
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            string tabName = "������ �����";
            a.CreateRibbonTab(tabName);
            #region Aparmentography
            RibbonPanel apartmnetographyPanel = a.CreateRibbonPanel(tabName, "��������������");

            PushButtonData genRooms = new PushButtonData("RoomsGen", "��������� �\n����������", Assembly.GetExecutingAssembly().Location, typeof(TRGR_RoomsGenerating).FullName);
            genRooms.ToolTip = "��������� � ����������� ����� ���������. " +
                "���������� ��������� ��� ������ �������� � ������������ ������ �������� ��� ������� ��������� � ������� �����.";
            genRooms.LongDescription = "������������ ����� ���� ���������, ���: " +
                "\"����� �������\", \"�������\", \"������\", \"�����\", \"�����\", \"�����������\", \"�.�.\". " +
                "��� ����������� ���� ������������ ����, �����, � ����� ����������� � ��������� ���������� ��������� PlumbingFixtures (����������). " +
                "��������� �������� �������������� �� ������� �����, ������� �������� ����� ���������. " +
                "������������ ��������� \"ADSK_����� ��������\" � \"ADSK_����\".";
            PushButton genRoomsBtn = apartmnetographyPanel.AddItem(genRooms) as PushButton;
            Image genRoomsImg = Properties.Resources.AR_RoomsGeneration;
            genRoomsBtn.LargeImage = ConvertToBitmap(genRoomsImg, new Size(32, 32));
            genRoomsBtn.Image = ConvertToBitmap(genRoomsImg, new Size(16, 16));

            PushButtonData calcAreas = new PushButtonData("CalculateAreas", "��������������", Assembly.GetExecutingAssembly().Location, typeof(TRGR_Apartmentography).FullName);
            calcAreas.ToolTip = "���������� ������� ��������� ��� ������ ��������.";
            calcAreas.LongDescription = "���������� ���������� � ������� �� ���� ������ � � ������������ ����������� " +
                "\"ADSK_����� ��������\", \"���\", � \"RM_����������� �������\". " +
                "������������ �������� ��������� \"RM_����������� �������\" �� �������, ��� ������� ������� ��������� ���� � ������������� ID ��������� � ���������������� �������������. " +
                "������������ ��������� ����� ������������ ��������� �� ����� ����������. " +
                "��������� ���������� �������������� ����� ��� �������� ������� ��������� � ����� ��������������. ���������� ������� � �����������";
            PushButton calcAreasBtn = apartmnetographyPanel.AddItem(calcAreas) as PushButton;
            Image calcAreasImg = Properties.Resources.AR_Apartmentography;
            calcAreasBtn.LargeImage = ConvertToBitmap(calcAreasImg, new Size(32, 32));
            calcAreasBtn.Image = ConvertToBitmap(calcAreasImg, new Size(16, 16));

            PushButtonData chgParametersBtnData = new PushButtonData("ChangeConfigSettings", "��������\n���������", Assembly.GetExecutingAssembly().Location, typeof(TRGR_ChangeConfigSettings).FullName);
            chgParametersBtnData.ToolTip = "��������� ���������� ��� ��������������.";
            chgParametersBtnData.LongDescription = "������������ ��� �������������� ����� ����������, ���: \"���������� ����� ����� �������\", ����������� ��� ���������� �������� ����������; " +
                "\"����������� ������� ������\"; \"����������� ������� �������\"; \"����������� ������� �����/������� ���������\"; \"�������� ������� ������\"; \"�������� ������� �����\". " +
                "��������� ��������� ���� ��� �������������� ���������� \"��������������\" � \"��������� � ���������� ���������\".";
            PushButton chgParametersBtn = apartmnetographyPanel.AddItem(chgParametersBtnData) as PushButton;
            Image chgParametersImg = Properties.Resources.settings_32;
            chgParametersBtn.LargeImage = ConvertToBitmap(chgParametersImg, new Size(32, 32));
            chgParametersBtn.Image = ConvertToBitmap(chgParametersImg, new Size(16, 16));
            #endregion
            return Result.Succeeded;
        }
        public BitmapImage ConvertToBitmap(Image img, Size size)
        {
            img = (Image)(new Bitmap(img, size));
            using (MemoryStream memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
