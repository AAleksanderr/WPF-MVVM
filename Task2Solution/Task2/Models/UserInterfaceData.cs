using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Task2.Models
{
    public class UserInterfaceData
    {
        public string Name { get; set; }
        public int AverageSteps { get; set; }
        public int MaxSteps { get; set; }
        public int MinSteps { get; set; }
        public SolidColorBrush ItemColor { get; set; }
        public PointCollection Points { get; set; }
    }
}
