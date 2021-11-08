using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicToMap.Models
{
    public struct Settings
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int WidthInMaps { get; set; }
        public int HeightInMaps { get; set; }
        public bool StaircaseSelected { get; set; }
        public bool HighQualitySelected { get; set; }
        public bool DitheringChecked { get; set; }
        public string ImagePath { get; set; }
        //public string CurrentDirectory { get; set; }
        public string DestinationDirectory { get; set; }
        public string Name { get; set; }
    }
}
