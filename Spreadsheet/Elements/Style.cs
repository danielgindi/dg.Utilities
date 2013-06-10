using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities.Spreadsheet
{
    public class ExcelSheetStyle
    {
        public string NumberFormat;
        public Alignment Alignment = new Alignment();
        public List<Border> Borders;
        public Interior Interior = new Interior();
        public Font Font = new Font();
    }
}