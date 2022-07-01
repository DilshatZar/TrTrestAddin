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
using System.Configuration;
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
    public class TRGR_ChangeConfigSettings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            RevitApplication app = uiapp.Application;
            Document doc = uidoc.Document;

            FilteredElementCollector areas = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType();
            Dictionary<string, List<Room>> apartments = new Dictionary<string, List<Room>>();

            TRGR_GeneralConfigSettings config = new TRGR_GeneralConfigSettings(doc);
            config.ShowDialog();
            return Result.Succeeded;
        }
    }
}
