using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrTrestAddin.Model
{
    internal class ParameterModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public ParameterModel(string decriptionName, string parameter, string value)
        {
            Description = decriptionName;
            Name = parameter;
            Value = value;
        }
    }
}
