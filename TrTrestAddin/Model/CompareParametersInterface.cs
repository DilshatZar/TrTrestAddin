#region Namespaces
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

#endregion

namespace TrTrestAddin.Model
{
    internal class CompareParametersInterface : IComparer<Parameter>
    {
        public int Compare(Parameter x, Parameter y)
        {
            return String.Compare(x.Definition.Name, y.Definition.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
