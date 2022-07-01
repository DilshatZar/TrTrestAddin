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
            string tabName = "Третий Трест";
            a.CreateRibbonTab(tabName);
            #region Aparmentography
            RibbonPanel apartmnetographyPanel = a.CreateRibbonPanel(tabName, "Квартирография");

            PushButtonData genRooms = new PushButtonData("RoomsGen", "Генерация\nи заполнение\nпомещений", Assembly.GetExecutingAssembly().Location, typeof(TRGR_RoomsGenerating).FullName);
            genRooms.ToolTip = "Генерация и определение типов помещений. " +
                "Нахождение помещений для каждой квартиры с выставлением номера квартиры для каждого помещения и входной двери.";
            genRooms.LongDescription = "Определяются такие типы помещений, как: " +
                "\"Жилая комната\", \"Коридор\", \"Лоджия\", \"Ванная\", \"Кухня\", \"Постирочная\", \"С.У.\". " +
                "Для определения типа используются окна, двери, а также размещенные в помещении экземпляры категории PlumbingFixtures." +
                "Помещения квартиры обнаруживаются по входной двери, описание типоразмера семейства которого соответствует \"Дверь.Квартирная\". " +
                "Выставляются параметры \"ADSK_Номер квартиры\" и \"ADSK_Этаж\"";
            PushButton genRoomsBtn = apartmnetographyPanel.AddItem(genRooms) as PushButton;
            Image genRoomsImg = Properties.Resources.AR_RoomsGeneration;
            genRoomsBtn.LargeImage = ConvertToBitmap(genRoomsImg, new Size(32, 32));
            genRoomsBtn.Image = ConvertToBitmap(genRoomsImg, new Size(16, 16));

            PushButtonData calcAreas = new PushButtonData("CalculateAreas", "Квартирография", Assembly.GetExecutingAssembly().Location, typeof(TRGR_Apartmentography).FullName);
            calcAreas.ToolTip = "Вычисление площади помещений для каждой квартиры.";
            calcAreas.LongDescription = "Вычисления происходят только у квартир в активном виде и с заполненными параметрами " +
                "\"ADSK_Номер квартиры\", \"ADSK_Тип помещения\", и \"ADSK_Коэффициент площади\". " +
                "Производится проверка параметра \"ADSK_Коэффициент площади\" на наличие ошибок, при наличии таковых, выводится окно с перечислением ID помещений и соответствующими исправлениями. " +
                "Производится изменение тега наименований на более компактный. " +
                "В плане: добавить размещение дополнительных тегов для указания площади помещений и формы Квартирографии.";
            PushButton calcAreasBtn = apartmnetographyPanel.AddItem(calcAreas) as PushButton;
            Image calcAreasImg = Properties.Resources.AR_Apartmentography;
            calcAreasBtn.LargeImage = ConvertToBitmap(calcAreasImg, new Size(32, 32));
            calcAreasBtn.Image = ConvertToBitmap(calcAreasImg, new Size(16, 16));

            PushButtonData chgParametersBtnData = new PushButtonData("ChangeConfigSettings", "Изменить\nпараметры", Assembly.GetExecutingAssembly().Location, typeof(TRGR_ChangeConfigSettings).FullName);
            chgParametersBtnData.ToolTip = "Изменение параметров для квартирографии.";
            chgParametersBtnData.LongDescription = "Используется для редактирования таких параметров, как: \"Количество чисел после запятой\", применяемый для округления значений вычислений; " +
                "\"Коэффициент площади лоджии\"; \"Коэффициент площади балкона\"; \"Коэффициент площади обычных помещений\"; \"Смещение комнаты сверху\"; \"Смещение комнаты снизу\".";
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
