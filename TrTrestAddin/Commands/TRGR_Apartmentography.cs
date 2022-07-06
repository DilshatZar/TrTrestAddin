#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using RevitApplication = Autodesk.Revit.ApplicationServices.Application;

using TrTrestAddin.Windows;
using TrTrestAddin.Model;
#endregion

namespace TrTrestAddin.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TRGR_Apartmentography : IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        RevitApplication app;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;

            FilteredElementCollector areas = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType();
            Dictionary<string, List<Room>> apartments = new Dictionary<string, List<Room>>();

            IList<IList<object>> mistakes = new List<IList<object>>();

            AllConfigParameters config = new AllConfigParameters();
            int roundNum = 2;
            double loggieAreaCoef = 0.5;
            double balconyAreaCoef = 0.3;
            double defaultAreaCoef = 1.0;
            string fullTagTypeFamily = config.GetStringValue("FullTagTypeFamily");
            string fullTagType = config.GetStringValue("FullTagType");
            int fullTagTypeId = config.GetIntegerValue("FullTagTypeId");
            string areaTagTypeFamily = config.GetStringValue("AreaTagTypeFamily");
            string areaTagType = config.GetStringValue("AreaTagType");
            int areaTagTypeId = config.GetIntegerValue("AreaTagTypeId");
            if (config.WrongElementTypeParameters(new List<Tuple<string, string, int>> 
                {
                    new Tuple<string, string, int> ( fullTagTypeFamily, fullTagType, fullTagTypeId ),
                    new Tuple<string, string, int> ( areaTagTypeFamily, areaTagType, areaTagTypeId )
                }, 
                doc))
            {
                MessageBox.Show("Указаны неправильные марки для помещений.", "Ошибка считывания параметров.");
                return Result.Failed;
            }
            try
            {
                roundNum = config.GetIntegerValue("RoundingNumber");
                loggieAreaCoef = config.GetDoubleValue("LoggiaAreaCoef");
                balconyAreaCoef = config.GetDoubleValue("BalconyAreaCoef");
                defaultAreaCoef = config.GetDoubleValue("DefaultAreaCoef");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка считывания параметров.");
            }

            foreach (Room room in areas)
            {
                if (room.LookupParameter("ADSK_Номер квартиры").AsString() != null)
                {
                    if (!apartments.ContainsKey(room.LookupParameter("ADSK_Номер квартиры").AsString()))
                    {
                        apartments.Add(room.LookupParameter("ADSK_Номер квартиры").AsString(), new List<Room>());
                    }
                    apartments[room.LookupParameter("ADSK_Номер квартиры").AsString()].Add(room);
                }
            }

            using (Transaction t = new Transaction(doc, "Квартирография"))
            {
                t.Start();
                foreach (string num in apartments.Keys)
                {
                    Room biggestRoom = apartments[num][0]; // Потенциально самая большая комната

                    int numberOfLivingRooms = 0;                    // AP_Количество комнат
                    double apartmentAreaLivingRooms = 0;            // APs_Жилая площадь
                    double apartmaneAreaWithoutSummerRooms = 0;     // APs_Общая площадь
                    double apartmentAreaGeneral = 0;                // APs_Общая площадь с учетом лет. пом.
                    double apartmentAreaGeneralWithoutCoef = 0;     // APs_Общая площадь с учетом лет. пом. без кф
                    // RM_Коэффициент площади
                    // RMs_Площадь
                    // RMs_Площадь с кф
                    foreach (Room room in apartments[num])
                    {
                        double biggestArea = Math.Round(UnitUtils.ConvertFromInternalUnits(biggestRoom.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble(), UnitTypeId.SquareMeters), roundNum);
                        double areaOfRoom = Math.Round(UnitUtils.ConvertFromInternalUnits(room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble(), UnitTypeId.SquareMeters), roundNum);

                        if (biggestArea < areaOfRoom) // Нахождение самого большшого помещения
                        {
                            biggestArea = areaOfRoom;
                            biggestRoom = room;
                        }

                        try
                        {
                            room.LookupParameter("RMs_Площадь").Set(UnitUtils.ConvertToInternalUnits(Math.Round(areaOfRoom, roundNum), UnitTypeId.SquareMeters));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        try
                        {
                            int roomType = RoomType.Type[room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()];

                            double coefficent = room.LookupParameter("RM_Коэффициент площади").AsDouble();

                            if (roomType != 3 && roomType != 4)
                            {
                                apartmaneAreaWithoutSummerRooms += areaOfRoom;
                                apartmentAreaGeneral += areaOfRoom;
                                if (coefficent != defaultAreaCoef)
                                {
                                    mistakes.Add(new List<object> { room.Id.IntegerValue, coefficent, defaultAreaCoef });
                                    coefficent = defaultAreaCoef;
                                    room.LookupParameter("RM_Коэффициент площади").Set(defaultAreaCoef);
                                }
                            }
                            if (roomType == 3 || roomType == 4)
                            {
                                if (roomType == 3 && coefficent != loggieAreaCoef)
                                {
                                    mistakes.Add(new List<object> { room.Id.IntegerValue, coefficent, loggieAreaCoef });
                                    coefficent = loggieAreaCoef;
                                    room.LookupParameter("RM_Коэффициент площади").Set(loggieAreaCoef);
                                }
                                else if (roomType == 4 && coefficent != balconyAreaCoef)
                                {
                                    mistakes.Add(new List<object> { room.Id.IntegerValue, coefficent, balconyAreaCoef });
                                    coefficent = balconyAreaCoef;
                                    room.LookupParameter("RM_Коэффициент площади").Set(balconyAreaCoef);
                                }
                                apartmentAreaGeneral += Math.Round(areaOfRoom * coefficent, roundNum);
                            }
                            if (roomType == 1)
                            {
                                numberOfLivingRooms++;
                                apartmentAreaLivingRooms += areaOfRoom;
                            }
                            room.LookupParameter("RMs_Площадь с кф").Set(UnitUtils.ConvertToInternalUnits(Math.Round(areaOfRoom * coefficent, roundNum), UnitTypeId.SquareMeters));
                            apartmentAreaGeneralWithoutCoef += areaOfRoom;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    apartmaneAreaWithoutSummerRooms = UnitUtils.ConvertToInternalUnits(Math.Round(apartmaneAreaWithoutSummerRooms, roundNum), UnitTypeId.SquareMeters);
                    apartmentAreaLivingRooms = UnitUtils.ConvertToInternalUnits(Math.Round(apartmentAreaLivingRooms, roundNum), UnitTypeId.SquareMeters);
                    apartmentAreaGeneral = UnitUtils.ConvertToInternalUnits(Math.Round(apartmentAreaGeneral, roundNum), UnitTypeId.SquareMeters);
                    apartmentAreaGeneralWithoutCoef = UnitUtils.ConvertToInternalUnits(Math.Round(apartmentAreaGeneralWithoutCoef, roundNum), UnitTypeId.SquareMeters);
                    foreach (Room room in apartments[num])
                    {
                        try
                        {
                            room.LookupParameter("AP_Количество комнат").Set(numberOfLivingRooms);
                            room.LookupParameter("APs_Общая площадь").Set(apartmaneAreaWithoutSummerRooms);
                            room.LookupParameter("APs_Жилая площадь").Set(apartmentAreaLivingRooms);
                            room.LookupParameter("APs_Общая площадь с учетом лет. пом.").Set(apartmentAreaGeneral);
                            room.LookupParameter("APs_Общая площадь с учетом лет. пом. без кф").Set(apartmentAreaGeneralWithoutCoef);
                            CreateRoomTag(room, doc, areaTagTypeId, "bottom", "right");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    CreateRoomTag(biggestRoom, doc, fullTagTypeId, "top", "right", 1, 1);
                }
                t.Commit();
            }
            if (mistakes.Count > 0)
            {
                TRGR_RoomIdListWindow roomIdWin = new TRGR_RoomIdListWindow();
                for (int i = 0; i < mistakes.Count; i++)
                {
                    roomIdWin.AddNewLine((int)mistakes[i][0], (double)mistakes[i][1], (double)mistakes[i][2]);
                }
                roomIdWin.Show();
            }
            return Result.Succeeded;
        }

        private static void CreateRoomTag(Room room, Document doc, int tagType,
            string vert = "bottom", string horiz = "right",
            double padx = 0.0, double pady = 0.0)
        {
            List<RoomTag> tags = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .WhereElementIsNotElementType()
                .Cast<RoomTag>()
                .Where(t
                 => t.Room.Id.IntegerValue == room.Id.IntegerValue
                 && t.GetTypeId().IntegerValue == tagType)
                .ToList(); // Отслеживаем существование тега указанного типа в комнате
            if (tags.Count == 0) // При отсутствии такового тега, создаем
            {
                double x = 0;
                double y = 0;
                SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
                IList<IList<BoundarySegment>> boundarySegmentsList = room.GetBoundarySegments(options);
                foreach (IList<BoundarySegment> boundarySegments in boundarySegmentsList) // Определяем угол, в котором должен находитья создаваемый тег
                {
                    y = Math.Round(boundarySegments[0].GetCurve().GetEndPoint(0).Y, 8);
                    x = Math.Round(boundarySegments[0].GetCurve().GetEndPoint(0).X, 8);
                    foreach (BoundarySegment segment in boundarySegments)
                    {
                        XYZ startXYZ = segment.GetCurve().GetEndPoint(0);
                        XYZ endXYZ = segment.GetCurve().GetEndPoint(1);
                        if (vert == "bottom" && Math.Round(startXYZ.Y, 8) == Math.Round(endXYZ.Y, 8))
                        {
                            if (y >= Math.Round(startXYZ.Y, 8))
                            {
                                y = Math.Round(startXYZ.Y, 8);
                                if (horiz == "right")
                                {
                                    x = Math.Round(startXYZ.X, 8) > Math.Round(endXYZ.X, 8) ? Math.Round(startXYZ.X, 8) : Math.Round(endXYZ.X, 8);
                                }
                                else if (horiz == "left")
                                {
                                    x = Math.Round(startXYZ.X, 8) < Math.Round(endXYZ.X, 8) ? Math.Round(startXYZ.X, 8) : Math.Round(endXYZ.X, 8);
                                }
                            }
                        }
                        else if (vert == "top" && Math.Round(startXYZ.Y, 8) == Math.Round(endXYZ.Y, 8))
                        {
                            if (y <= Math.Round(startXYZ.Y, 8))
                            {
                                y = Math.Round(startXYZ.Y, 8);
                                if (horiz == "right")
                                {
                                    x = Math.Round(startXYZ.X, 8) > Math.Round(endXYZ.X, 8) ? Math.Round(startXYZ.X, 8) : Math.Round(endXYZ.X, 8);
                                }
                                else if (horiz == "left")
                                {
                                    x = Math.Round(startXYZ.X, 8) < Math.Round(endXYZ.X, 8) ? Math.Round(startXYZ.X, 8) : Math.Round(endXYZ.X, 8);
                                }
                            }
                        }
                    }
                }


                RoomTag tag = doc.Create.NewRoomTag(new LinkElementId(room.Id), new UV(x, y), null); // Создание тега
                tag.ChangeTypeId(new ElementId(tagType)); // Смена типа тега

                double a = 0;
                double b = 0;
                BoundingBoxXYZ size = tag.get_BoundingBox(tag.View);
                if (horiz == "right")
                {
                    a = size.Min.X - size.Max.X - padx;
                }
                else if (horiz == "left")
                {
                    a = size.Max.X - size.Min.X + padx;
                }
                if (vert == "top")
                {
                    b = size.Min.Y - size.Max.Y - pady;
                }
                else if (vert == "bottom")
                {
                    b = size.Max.Y - size.Min.Y + pady;
                }
                tag.Location.Move(new XYZ(a / 2.0, b / 2.0, 0)); // Смещение тега от угла в зависимости от размера и местоположения
            }
        }
    }
    public class RoomType
    {
        ///<summary>
        ///Перечисление всех возможных типов помещений с указанием цифровых эквивалентов. 
        ///1 - жилые помещения, 2 - нежилые помещения, 3 - лоджия, 4 - балкон
        ///</summary>
        public static Dictionary<string, int> Type { get; set; } = new Dictionary<string, int>
        {
            {"Жилая комната", 1 },
            {"Гостиная", 1 },
            {"Спальня", 1 },
            {"Ванная", 2 },
            {"Уборная", 2 },
            {"Постирочная", 2 },
            {"С.У.", 2 },
            {"Коридор", 2 },
            {"Прихожая", 2 },
            {"Кухня", 2 },
            {"Лоджия", 3 },
            {"Балкон", 4 },
        };
    }
}
