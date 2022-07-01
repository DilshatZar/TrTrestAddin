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
#endregion

namespace TrTrestAddin.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TRGR_RoomsGenerating : IExternalCommand
    {
        static UIApplication uiapp;
        static UIDocument uidoc;
        static RevitApplication app;
        static Document doc;
        Phase phase;

        TRGR_ConfigSettingsWindow config;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;
            phase = doc.Phases.get_Item(doc.Phases.Size - 1);

            config = new TRGR_ConfigSettingsWindow();

            // Взятие параметров конфигурации

            double loggieAreaCoef = 0.5;
            double balconyAreaCoef = 0.3;
            double defaultAreaCoef = 1.0;
            double upperOffset = 3300;
            double lowerOffset = 0;
            try
            {
                loggieAreaCoef = double.Parse(config.GetParameterValue("LoggiaAreaCoef").Replace(".", ","));
                balconyAreaCoef = double.Parse(config.GetParameterValue("BalconyAreaCoef").Replace(".", ","));
                defaultAreaCoef = double.Parse(config.GetParameterValue("DefaultAreaCoef").Replace(".", ","));
                upperOffset = double.Parse(config.GetParameterValue("RoomUpperOffset").Replace(".", ","));
                lowerOffset = double.Parse(config.GetParameterValue("RoomLowerOffset").Replace(".", ","));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Использование значений по умолчанию для параметров: \n" +
                    "\"Коэффициент площади лоджии\": 0,5 \n" +
                    "\"Коэффициент площади балкона\": 0,3 \n" +
                    "\"Коэффициент площади обычных помещений\": 1,0" +
                    "\"Смещение комнаты сверху\": 3300" +
                    "\"Смещение комнаты снизу\": 0",
                    "Ошибка считывания параметров.");
                config.SetParameterValue("LoggiaAreaCoef", "0,5");
                config.SetParameterValue("BalconyAreaCoef", "0,3");
                config.SetParameterValue("DefaultAreaCoef", "0,1");
                config.SetParameterValue("RoomUpperOffset", "3300");
                config.SetParameterValue("RoomLowerOffset", "0");
            }

            IList<Room> roomsToRemove = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(room =>
                    room.LookupParameter("ADSK_Этаж").AsString() == null || room.LookupParameter("ADSK_Этаж").AsString().Equals("")
                    || room.LookupParameter("ADSK_Номер квартиры").AsString() == null || room.LookupParameter("ADSK_Номер квартиры").AsString().Equals("")
                    || room.LookupParameter("ADSK_Тип помещения").AsInteger() == 0 
                    || UnitUtils.ConvertFromInternalUnits(room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).AsDouble(), UnitTypeId.Millimeters) != upperOffset
                    || UnitUtils.ConvertFromInternalUnits(room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble(), UnitTypeId.Millimeters) != lowerOffset
                    || CoefsNotRight(room, loggieAreaCoef, balconyAreaCoef, defaultAreaCoef))
                .ToList();
            if (roomsToRemove.Count > 0)
            {
                using (Transaction t = new Transaction(doc, "Удаление существующих незаполненных помещений"))
                { // Добавить проверку на наличие заполненых комнат
                    t.Start();
                    doc.Delete(roomsToRemove.Select(room => room.Id).ToList());
                    t.Commit();
                }
            }

            List<Room> oldRooms = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .ToList();
            List<Room> genRooms = new List<Room>();
            List<Room> newRooms = new List<Room>();
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Генерация помещений");

                Level level = doc.ActiveView.GenLevel;

                foreach (ElementId roomId in doc.Create.NewRooms2(level))
                {
                    Room genRoom = doc.GetElement(roomId) as Room;
                    genRooms.Add(genRoom);
                    newRooms.Add(genRoom);
                    genRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(UnitUtils.ConvertToInternalUnits(upperOffset, UnitTypeId.Millimeters));
                    genRoom.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(UnitUtils.ConvertToInternalUnits(lowerOffset, UnitTypeId.Millimeters));
                }

                //foreach (Room genRoom in genRooms)
                //{
                //    if (RoomsAreSame(oldRooms, genRoom, doc.ActiveView))
                //    {
                //        doc.Delete(genRoom.Id);
                //    }
                //    else
                //    {
                //        newRooms.Add(genRoom);
                //    }
                //}
                t.Commit();

                t.Start("Смена тега помещений");
                List<RoomTag> roomtags = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsNotElementType()
                    .Cast<RoomTag>()
                    .ToList();
                foreach (RoomTag roomTag in roomtags)
                {
                    if (roomTag.GetTypeId().IntegerValue != 159750 && roomTag.GetTypeId().IntegerValue != 159393)
                    {
                        roomTag.ChangeTypeId(new ElementId(159738)); // !!Назначить ID из конфига!!
                    }
                }
                t.Commit();
            }

            List<Room> smallRooms = newRooms.Where(room => UnitUtils.ConvertFromInternalUnits(room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble(), UnitTypeId.SquareMeters) <= 1).ToList();
            newRooms = newRooms.Where(room => UnitUtils.ConvertFromInternalUnits(room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble(), UnitTypeId.SquareMeters) > 1).ToList();
            if (smallRooms.Count > 0)
            {
                using (Transaction t = new Transaction(doc, "Удаление малых помещений"))
                {
                    t.Start();
                    doc.Delete(smallRooms.Select(room => room.Id).ToList());
                    t.Commit();
                }
            }

            FilteredElementCollector plumbingFixtures = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PlumbingFixtures).WhereElementIsNotElementType();

            List<FamilyInstance> kitchenPlumbs = new List<FamilyInstance>();
            List<FamilyInstance> toiletBowls = new List<FamilyInstance>();
            List<FamilyInstance> sinks = new List<FamilyInstance>();
            List<FamilyInstance> washers = new List<FamilyInstance>();
            List<FamilyInstance> baths = new List<FamilyInstance>();

            foreach (FamilyInstance fixture in plumbingFixtures)
            {
                string fixtureName = fixture.Symbol.FamilyName;
                if (fixtureName.Contains("Варочная_панель"))
                {
                    kitchenPlumbs.Add(fixture);
                }
                else if (fixtureName.Contains("Унитаз"))
                {
                    toiletBowls.Add(fixture);
                }
                else if (fixtureName.Contains("Умывальник"))
                {
                    sinks.Add(fixture);
                }
                else if (fixtureName.Contains("Стиральная_машина"))
                {
                    washers.Add(fixture);
                }
                else if (fixtureName.Contains("Ванная") || fixtureName.Contains("ДушевойПоддон"))
                {
                    baths.Add(fixture);
                }
            }

            FilteredElementCollector windows = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Windows).WhereElementIsNotElementType();
            FilteredElementCollector doors = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType();

            using (Transaction t = new Transaction(doc, "Определение помещений"))
            {
                t.Start();
                foreach (Room newRoom in newRooms)
                {
                    List<string> roomFixtures = new List<string>();
                    foreach (FamilyInstance fixture in kitchenPlumbs)
                    {
                        XYZ located = (fixture.Location as LocationPoint).Point;
                        if (newRoom.IsPointInRoom(located))
                        {
                            roomFixtures.Add("Кухня");
                        }
                    }
                    foreach (FamilyInstance fixture in toiletBowls)
                    {
                        if (newRoom.IsPointInRoom((fixture.Location as LocationPoint).Point))
                        {
                            roomFixtures.Add("Унитаз");
                        }
                    }
                    foreach (FamilyInstance fixture in sinks)
                    {
                        if (newRoom.IsPointInRoom((fixture.Location as LocationPoint).Point))
                        {
                            roomFixtures.Add("Умывальник");
                        }
                    }
                    foreach (FamilyInstance fixture in washers)
                    {
                        if (newRoom.IsPointInRoom((fixture.Location as LocationPoint).Point))
                        {
                            roomFixtures.Add("Стиральная машина");
                        }
                    }
                    foreach (FamilyInstance fixture in baths)
                    {
                        if (newRoom.IsPointInRoom((fixture.Location as LocationPoint).Point))
                        {
                            roomFixtures.Add("Ванна");
                        }
                    }

                    if (roomFixtures.Contains("Унитаз") && roomFixtures.Contains("Ванна"))
                    {
                        newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("С.У.");
                        newRoom.LookupParameter("ADSK_Тип помещения").Set(2);
                    }
                    else if (!roomFixtures.Contains("Ванна") && roomFixtures.Contains("Умывальник") && roomFixtures.Contains("Унитаз"))
                    {
                        newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Уборная");
                        newRoom.LookupParameter("ADSK_Тип помещения").Set(2);
                    }
                    else if (!roomFixtures.Contains("Унитаз") && roomFixtures.Contains("Ванна"))
                    {
                        newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Вання");
                        newRoom.LookupParameter("ADSK_Тип помещения").Set(2);
                    }
                    else if (!roomFixtures.Contains("Ванна") && !roomFixtures.Contains("Умывальник") && !roomFixtures.Contains("Унитаз") && roomFixtures.Contains("Стиральная машина"))
                    {
                        newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Постирочная");
                        newRoom.LookupParameter("ADSK_Тип помещения").Set(2);
                    }
                    else if (roomFixtures.Contains("Кухня"))
                    {
                        newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Кухня");
                        newRoom.LookupParameter("ADSK_Тип помещения").Set(2);
                    }

                    List<int> roomWindowsIds = new List<int>();
                    foreach (FamilyInstance window in windows)
                    {
                        Room windowToRoom = window.get_ToRoom(phase);
                        if (windowToRoom != null)
                        {
                            roomWindowsIds.Add(windowToRoom.Id.IntegerValue);
                        }
                    }
                    foreach (int elementId in roomWindowsIds.Distinct())
                    {
                        if (newRoom.Id.IntegerValue == elementId && newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString().Contains("Помещение"))
                        {
                            newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Жилая комната");
                            newRoom.LookupParameter("ADSK_Тип помещения").Set(1);
                        }
                    }
                    if (newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString() == "Помещение")
                    {
                        newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Коридор");
                        newRoom.LookupParameter("ADSK_Тип помещения").Set(2);
                    }
                    newRoom.LookupParameter("ADSK_Коэффициент площади").Set(defaultAreaCoef);
                }
                foreach (FamilyInstance door in doors)
                {
                    Room doorFromRoom = door.get_FromRoom(phase);
                    if (doorFromRoom != null && door.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString().Contains("Балконная"))
                    {
                        doorFromRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Лоджия");
                        doorFromRoom.LookupParameter("ADSK_Тип помещения").Set(3);
                        doorFromRoom.LookupParameter("ADSK_Коэффициент площади").Set(loggieAreaCoef);
                    }
                }
                t.Commit();
            }

            List<FamilyInstance> entryDoors = new FilteredElementCollector(doc, doc.ActiveView.Id) // Находим входные двери квартиры
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(door => door.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString().Contains("Дверь.Квартирная")) // Изменить подход поиска входных дверей на виде, используя конфиг
                .ToList();
            FilteredElementCollector allRooms = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType();

            using (Transaction t = new Transaction(doc, "Нумерация комнат квартир")) // Основная транзакция
            {
                t.Start();
                foreach (FamilyInstance entryDoor in entryDoors) // Проходим по каждой входной двери
                {
                    List<Room> apartmentRooms = GetApartmentRooms(entryDoor.get_FromRoom(phase), allRooms, null, entryDoor); // Эта функция отвечает за нахождение всех комнат в картире
                    Level lvl = doc.GetElement(entryDoor.LevelId) as Level; // Часть, просто отвечающая за взятие номера квартиры, у нас по форме L01_001 с указанием уровня и номера квартиры
                    string doorNumber = entryDoor.LookupParameter("ADSK_Номер квартиры").AsString();
                    string apartmentNumber = null;
                    if (!doorNumber.Contains("L") && !doorNumber.Contains("_"))
                    {
                        apartmentNumber = $"L{lvl.Name.Replace("Этаж ", "")}_{LeadingZeros(entryDoor.LookupParameter("ADSK_Номер квартиры").AsString())}";
                    }
                    else
                    {
                        apartmentNumber = doorNumber;
                    }
                    foreach (Room room in apartmentRooms)
                    {
                        try
                        {
                            room.LookupParameter("ADSK_Номер квартиры").Set(apartmentNumber);
                            room.LookupParameter("ADSK_Этаж").Set($"L{room.Level.Name.Replace("Этаж ", "")}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка");
                        }
                    }
                    entryDoor.LookupParameter("ADSK_Номер квартиры").Set(apartmentNumber);
                }
                t.Commit();
            }

            List<Room> MOP = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .Where(room => room.LookupParameter("ADSK_Номер квартиры").AsString() == null)
                .ToList();
            if (MOP.Count > 0)
            {
                using (Transaction t = new Transaction(doc, "Удаление МОП-ов"))
                {
                    t.Start();
                    doc.Delete(MOP.Select(room => room.Id).ToList());
                    t.Commit();
                }
            }

            return Result.Succeeded;
        }
        private List<Room> GetApartmentRooms(Room currentRoom, FilteredElementCollector allRooms, List<Room> apartmentRooms = null, FamilyInstance entryDoor = null)
        {
            if (apartmentRooms == null)
            {
                apartmentRooms = new List<Room>();
            }
            apartmentRooms.Add(currentRoom);
            List<int> roomsIds = apartmentRooms.Select(room => room.Id.IntegerValue).ToList();

            List<FamilyInstance> allDoorsOfRoom = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(door =>
                door.get_FromRoom(phase).Id.IntegerValue == currentRoom.Id.IntegerValue || door.get_ToRoom(phase).Id.IntegerValue == currentRoom.Id.IntegerValue)
                .ToList();
            if (entryDoor != null)
            {
                allDoorsOfRoom = allDoorsOfRoom.Where(door => door.Id.IntegerValue != entryDoor.Id.IntegerValue).ToList();
            }

            if (allDoorsOfRoom.Count > 0)
            {
                foreach (FamilyInstance door in allDoorsOfRoom)
                {
                    if (door.get_FromRoom(phase).Id.IntegerValue != currentRoom.Id.IntegerValue)
                    {
                        if (!roomsIds.Contains(door.get_FromRoom(phase).Id.IntegerValue))
                        {
                            apartmentRooms = GetApartmentRooms(door.get_FromRoom(phase), allRooms, apartmentRooms, door);
                        }
                    }
                    else
                    {
                        if (!roomsIds.Contains(door.get_ToRoom(phase).Id.IntegerValue))
                        {
                            apartmentRooms = GetApartmentRooms(door.get_ToRoom(phase), allRooms, apartmentRooms, door);
                        }
                    }
                }
            }
            Solid currentRoomSolid = GetSolidOfRoom(currentRoom);
            List<Room> adjoiningRooms = allRooms.Cast<Room>()
                .Where(room => SolidsAreToching(currentRoomSolid, GetSolidOfRoom(room)) && !roomsIds.Contains(room.Id.IntegerValue))
                .ToList();
            if (adjoiningRooms.Count > 0)
            {
                foreach (Room room in adjoiningRooms)
                {
                    apartmentRooms = GetApartmentRooms(room, allRooms, apartmentRooms);
                }
            }

            return apartmentRooms;
        }
        private static bool SolidsAreToching(Solid solid1, Solid solid2) // Подобрать иной подход, избежать использования солидов
        {
            Solid interSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
            Solid unionSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Union);

            double sumArea = Math.Round(Math.Abs(solid1.SurfaceArea + solid2.SurfaceArea), 5);
            double sumFaces = Math.Abs(solid1.Faces.Size + solid2.Faces.Size);
            double unionArea = Math.Round(Math.Abs(unionSolid.SurfaceArea), 5);
            double unionFaces = Math.Abs(unionSolid.Faces.Size);

            if (sumArea > unionArea && sumFaces > unionFaces && interSolid.Volume < 0.00001)
            {
                return true;
            }
            return false;
        }
        private static string LeadingZeros(string str)
        {
            if (str.Length == 1) { return "00" + str; }
            if (str.Length == 2) { return "0" + str; }
            if (str.Length == 3) { return str; }
            return null;
        }
        private static Solid GetSolidOfRoom(Room room)
        {
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);
            Solid roomSolid = results.GetGeometry();

            return roomSolid;
        }
        private static bool RoomsAreSame(Room room1, Room room2, View view)
        {
            BoundingBoxXYZ bBox1 = room1.get_BoundingBox(view);
            BoundingBoxXYZ bBox2 = room2.get_BoundingBox(view);

            return bBox1.Min.IsAlmostEqualTo(bBox2.Min) && bBox1.Max.IsAlmostEqualTo(bBox2.Max);
        }
        private static bool RoomsAreSame(ICollection<Room> rooms1, Room room2, View view)
        {
            BoundingBoxXYZ bBox2 = room2.get_BoundingBox(view);
            foreach (Room room1 in rooms1)
            {
                BoundingBoxXYZ bBox1 = room1.get_BoundingBox(view);
                if (bBox1.Min.IsAlmostEqualTo(bBox2.Min) && bBox1.Max.IsAlmostEqualTo(bBox2.Max))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CoefsNotRight(Room room, double loggieCoef, double balconyCoef, double defaultCoef)
        {
            double roomCoef = room.LookupParameter("ADSK_Коэффициент площади").AsDouble();
            if (loggieCoef == roomCoef)
            {
                return false;
            }
            if (balconyCoef == roomCoef)
            {
                return false;
            }
            if (defaultCoef == roomCoef)
            {
                return false;
            }
            return true;
        }
    }
}