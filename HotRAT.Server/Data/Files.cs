using iNKORE.UI.WPF.Modern.Common.IconKeys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Data
{
    public class File
    {
        public FontIconData Icon {  get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string CreatTime { get; set; }
        public string ChangeTime { get; set; }
        public long Size { get; set; }
    }
}