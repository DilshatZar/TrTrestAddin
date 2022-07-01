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
    public class TRGR_Apartmentography : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            RevitApplication app = uiapp.Application;
            Document doc = uidoc.Document;

            FilteredElementCollector areas = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType();
            Dictionary<string, List<Room>> apartments = new Dictionary<string, List<Room>>();

            IList<IList<object>> mistakes = new List<IList<object>>();
            
            TRGR_ConfigSettingsWindow config = new TRGR_ConfigSettingsWindow();
            int roundNum = 2;
            double loggieAreaCoef = 0.5;
            double balconyAreaCoef = 0.3;
            double defaultAreaCoef = 1.0;
            try
            {
                roundNum = int.Parse(config.GetParameterValue("RoundingNumber"));
                loggieAreaCoef = double.Parse(config.GetParameterValue("LoggiaAreaCoef").Replace(".", ","));
                balconyAreaCoef = double.Parse(config.GetParameterValue("BalconyAreaCoef").Replace(".", ","));
                defaultAreaCoef = double.Parse(config.GetParameterValue("DefaultAreaCoef").Replace(".", ","));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("������������� �������� �� ��������� ��� ����������: \n" +
                    "\"����� ����� �������\": 2\n" +
                    "\"����������� ������� ������\": 0,5\n" +
                    "\"����������� ������� �������\": 0,3\n" +
                    "\"����������� ������� ������� ���������\": 1,0\n", "������ ���������� ����������.");
                config.SetParameterValue("RoundingNumber", "2");
                config.SetParameterValue("LoggiaAreaCoef", "0,5");
                config.SetParameterValue("BalconyAreaCoef", "0,3");
                config.SetParameterValue("DefaultAreaCoef", "1,0");
            }

            foreach (Room room in areas)
            {
                if (room.LookupParameter("ADSK_����� ��������").AsString() != null)
                {
                    if (!apartments.ContainsKey(room.LookupParameter("ADSK_����� ��������").AsString()))
                    {
                        apartments.Add(room.LookupParameter("ADSK_����� ��������").AsString(), new List<Room>());
                    }
                    apartments[room.LookupParameter("ADSK_����� ��������").AsString()].Add(room);
                }
            }

            using (Transaction t = new Transaction(doc, "��������������"))
            {
                t.Start();
                foreach (string num in apartments.Keys)
                {
                    Room biggestRoom = apartments[num][0]; // ������������ ����� ������� �������

                    int numberOfLivingRooms = 0;
                    double apartmentAreaLivingRooms = 0;            // ADSK_������� �������� �����
                    double apartmaneAreaWithoutSummerRooms = 0;     // ADSK_������� ��������
                    double apartmentAreaGeneral = 0;                // ADSK_������� �������� �����
                    double apartmentAreaGeneralWithoutCoef = 0;     // TRGR_������� �������� ��� ��
                    foreach (Room room in apartments[num])
                    {
                        double biggestArea = Math.Round(UnitUtils.ConvertFromInternalUnits(biggestRoom.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble(), UnitTypeId.SquareMeters), roundNum);
                        double areaOfRoom = Math.Round(UnitUtils.ConvertFromInternalUnits(room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble(), UnitTypeId.SquareMeters), roundNum);

                        if (biggestArea < areaOfRoom) // ���������� ������ ��������� ���������
                        {
                            biggestArea = areaOfRoom;
                            biggestRoom = room;
                        }

                        try
                        {
                            room.LookupParameter("TRGR_������� ���������").Set(UnitUtils.ConvertToInternalUnits(Math.Round(areaOfRoom, roundNum), UnitTypeId.SquareMeters));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        try
                        {
                            int roomType = room.LookupParameter("ADSK_��� ���������").AsInteger();

                            double coefficent = room.LookupParameter("ADSK_����������� �������").AsDouble();

                            if (roomType != 3 && roomType != 4)
                            {
                                apartmaneAreaWithoutSummerRooms += areaOfRoom;
                                apartmentAreaGeneral += areaOfRoom;
                                if (coefficent != defaultAreaCoef)
                                {
                                    mistakes.Add(new List<object> { room.Id.IntegerValue, coefficent, defaultAreaCoef });
                                    coefficent = defaultAreaCoef;
                                    room.LookupParameter("ADSK_����������� �������").Set(defaultAreaCoef);
                                }
                            }
                            if (roomType == 3 || roomType == 4)
                            {
                                if (roomType == 3 && coefficent != loggieAreaCoef)
                                {
                                    mistakes.Add(new List<object> { room.Id.IntegerValue, coefficent, loggieAreaCoef });
                                    coefficent = loggieAreaCoef;
                                    room.LookupParameter("ADSK_����������� �������").Set(loggieAreaCoef);
                                }
                                else if (roomType == 4 && coefficent != balconyAreaCoef)
                                {
                                    mistakes.Add(new List<object> { room.Id.IntegerValue, coefficent, balconyAreaCoef });
                                    coefficent = balconyAreaCoef;
                                    room.LookupParameter("ADSK_����������� �������").Set(balconyAreaCoef);
                                }
                                apartmentAreaGeneral += Math.Round(areaOfRoom * coefficent, roundNum);
                            }
                            if (roomType == 1)
                            {
                                numberOfLivingRooms++;
                                apartmentAreaLivingRooms += areaOfRoom;
                            }
                            room.LookupParameter("ADSK_������� � �������������").Set(UnitUtils.ConvertToInternalUnits(Math.Round(areaOfRoom * coefficent, roundNum), UnitTypeId.SquareMeters));
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
                            room.LookupParameter("ADSK_���������� ������").Set(numberOfLivingRooms);
                            room.LookupParameter("ADSK_������� ��������").Set(apartmaneAreaWithoutSummerRooms);
                            room.LookupParameter("ADSK_������� �������� �����").Set(apartmentAreaLivingRooms);
                            room.LookupParameter("ADSK_������� �������� �����").Set(apartmentAreaGeneral);
                            room.LookupParameter("TRGR_������� �������� ��� ��").Set(apartmentAreaGeneralWithoutCoef);
                            CreateRoomTag(room, doc, 159750, "bottom", "right"); // !!�������� ID �� �������!!  �������� ������ ���� � �������� ���� � �������, ��� ���������� �������
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    CreateRoomTag(biggestRoom, doc, 159393, "top", "right", 1, 1); // !!�������� ID �� �������!! �������� �������� ���� � �������� ���� � �������, ��� ���������� �������
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
                .ToList(); // ����������� ������������� ���� ���������� ���� � �������
            if (tags.Count == 0) // ��� ���������� �������� ����, �������
            {
                double x = 0;
                double y = 0;
                SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
                IList<IList<BoundarySegment>> boundarySegmentsList = room.GetBoundarySegments(options);
                foreach (IList<BoundarySegment> boundarySegments in boundarySegmentsList) // ���������� ����, � ������� ������ ��������� ����������� ���
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


                RoomTag tag = doc.Create.NewRoomTag(new LinkElementId(room.Id), new UV(x, y), null); // �������� ����
                tag.ChangeTypeId(new ElementId(tagType)); // ����� ���� ����

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
                tag.Location.Move(new XYZ(a / 2.0, b / 2.0, 0)); // �������� ���� �� ���� � ����������� �� ������� � ��������������
            }
        }
    }
}
