using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parcer.jsonModels
{
    class Lot
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string AddCharacteristics { get; set; }
        public string Measure { get; set; }
        public double Quantity { get; set; }
        public double Currency { get; set; }
        public double StepSide { get; set; }

    }
}
