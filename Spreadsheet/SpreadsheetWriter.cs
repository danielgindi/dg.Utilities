using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Globalization;
using System.Drawing;

namespace dg.Utilities.Spreadsheet
{
    /// <summary>
    /// Written by Daniel Cohen Gindi (danielgindi@gmail.com)
    /// </summary>
    public partial class SpreadsheetWriter
    {
        #region Private Variables

        private System.IO.Stream _FileOutput = null;
        private Encoding _FileEncoding = Encoding.UTF8;
        private StringBuilder _OutputBuilder = null;
        private StringBuilder _OutputTempBuilder = null;
        private bool _IsXml = true;
        private bool _WriteBOM = true;
        private List<string> Columns = new List<string>();
        private List<ExcelSheetStyle> Styles = new List<ExcelSheetStyle>();
        private bool _StartWorksheet = false;
        private bool _EndWorksheet = false;
        private bool _EndRow = false;
        private int _FirstRowPos = 0;
        //private long _FirstRowPos = 0;
        private bool _InsertAtBegin = false;

        #endregion

        #region Constructors

        public SpreadsheetWriter(bool IsXml, System.IO.Stream FileOutput, Encoding FileEncoding)
        {
            _IsXml = IsXml;
            _FileOutput = FileOutput;
            _FileEncoding = FileEncoding ?? Encoding.UTF8;
            if (_FileOutput == null)
            {
                _OutputBuilder = new StringBuilder();
            }
        }
        public SpreadsheetWriter(bool IsXml, System.IO.Stream FileOutput)
            : this(IsXml, FileOutput, Encoding.UTF8)
        {

        }
        public SpreadsheetWriter(bool IsXml)
            : this(IsXml, null, null)
        {
        }

        #endregion

        #region Public Properties

        public bool IsXml { get { return _IsXml; } }

        public string FileExtension
        {
            get
            {
                if (IsXml) return "Xml";
                else return "csv";
            }
        }

        public string FileContentType
        {
            get
            {
                if (IsXml) return "text/Xml";
                else return "application/vnd.ms-excel";
            }
        }

        public bool WriteBOM
        {
            get { return _WriteBOM; }
            set { _WriteBOM = value; }
        }

        #endregion

        #region Helpers

        private string PrepareString(string Value)
        {
            if (IsXml)
            {
                Value = Value.Replace(@"&", @"&amp;");
                Value = Value.Replace(@"<", @"&lt;");
                Value = Value.Replace(@">", @"&gt;");
                Value = Value.Replace(@"""", @"&quot;");
                Value = Value.Replace(@"'", @"&apos;");
                Value = Value.Replace("\r", @"&#xD;");
                Value = Value.Replace("\n", @"&#xA;");
            }
            else
            {
                Value = Value.Replace('\n', ' ');
                Value = Value.Replace('\r', ' ');
                Value = Value.Replace(@"""", @"""""");
            }
            return Value;
        }
        
        public override string ToString()
        {
            if (_OutputBuilder != null) return _OutputBuilder.ToString();
            else return _FileOutput.ToString();
        }

        #endregion

        #region DataTable automation

        public static SpreadsheetWriter FromDataTable(DataTable Table)
        {
            return FromDataTable(Table, true, true);
        }

        public static SpreadsheetWriter FromDataTable(DataTable Table, bool Xml, bool FormatEmptyCells)
        {
            if (Table == null) return null;

            SpreadsheetWriter ex = new SpreadsheetWriter(Xml);

            int iStyleBold = ex.AddStyle(new ExcelSheetStyle { Font = new Font { Bold = true } });
            int iStyleInt = ex.AddStyle(new ExcelSheetStyle { NumberFormat = NumberFormat.GeneralNumber });
            int iStyleFloat = ex.AddStyle(new ExcelSheetStyle { NumberFormat = NumberFormat.Scientific });

            int iCols = Table.Columns.Count;

            ex.BeginFile();
            ex.NewWorksheet("Exported");

            for (int iCol = 0; iCol < iCols; iCol++)
            {
                ex.AddColumn();
            }

            ex.BeginRow(iStyleBold);
            for (int iCol = 0; iCol < iCols; iCol++)
            {
                ex.SetCell(Table.Columns[iCol].ColumnName);
            }

            for (int iRow = 0; iRow < Table.Rows.Count; iRow++)
            {
                ex.BeginRow();
                for (int iCol = 0; iCol < iCols; iCol++)
                {

                    if (Table.Rows[iRow][iCol] is int)
                    {
                        ex.SetCell((int)Table.Rows[iRow][iCol], iStyleInt);
                    }
                    else if (Table.Rows[iRow][iCol] is Int64)
                    {
                        ex.SetCell((Int64)Table.Rows[iRow][iCol], iStyleInt);
                    }
                    else if (Table.Rows[iRow][iCol] is float)
                    {
                        ex.SetCell((float)Table.Rows[iRow][iCol], iStyleFloat);
                    }
                    else if (Table.Rows[iRow][iCol] is double)
                    {
                        ex.SetCell((double)Table.Rows[iRow][iCol], iStyleFloat);
                    }
                    else if (Table.Rows[iRow][iCol] is decimal)
                    {
                        ex.SetCell((decimal)Table.Rows[iRow][iCol], iStyleFloat);
                    }
                    else ex.SetCell(Table.Rows[iRow][iCol].ToString());
                }
            }

            ex.EndFile();
            return ex;
        }

        #endregion

        #region Private writing stuff

        private void Write(string Output)
        {
            if (_FileOutput == null)
            {
                if (_InsertAtBegin) _OutputTempBuilder.Append(Output);
                else _OutputBuilder.Append(Output);
            }
            else
            {
                byte[] bytes = _FileEncoding.GetBytes(Output);
                _FileOutput.Write(bytes, 0, bytes.Length);
            }
        }

        private void SeekToFirstRow()
        {
            if (_FileOutput == null)
            {
                SeekToEnd();
                _OutputTempBuilder = new StringBuilder();
            }
            else
            {
                // this replaces, no good
                //_FileOutput.Seek(_FirstRowPos, System.IO.SeekOrigin.Begin);
            }
            _InsertAtBegin = true;
        }

        private void SeekToEnd()
        {
            if (!_InsertAtBegin) return;
            if (_FileOutput == null)
            {
                if (_OutputTempBuilder != null)
                {
                    _OutputBuilder.Insert(_FirstRowPos, _OutputTempBuilder.ToString());
                    _OutputTempBuilder = null;
                }
            }
            else
            {
                //_FileOutput.Seek(0, System.IO.SeekOrigin.End);
            }
            _InsertAtBegin = false;
        }

        private void KeepRecordOfFirstRowPosition()
        {
            SeekToEnd();
            if (_FileOutput == null)
            {
                _FirstRowPos = _OutputBuilder.Length;
            }
            else
            {
                //_FirstRowPos = _FileOutput.Position;
            }
        }

        private void BeginWorksheet()
        {
            if (IsXml && _StartWorksheet)
            {
                Write(string.Format("  <Table ss:ExpandedColumnCount=\"{0}\">\n", Columns.Count));
                foreach (string Column in Columns)
                {
                    if (Column.Length == 0)
                    {
                        Write("   <Column ss:AutoFitWidth=\"1\"/>\n");
                    }
                    else
                    {
                        Write(string.Format("   <Column ss:Width=\"{0}\"/>\n", Column));
                    }
                }
                _StartWorksheet = false;
            }
        }

        private void EndWorksheet()
        {
            if (_EndWorksheet)
            {
                EndRow();
                if (IsXml)
                {
                    if (!_StartWorksheet) Write("  </Table>\n");
                    Write(" </Worksheet>\n");
                    _StartWorksheet = false;
                }
                else
                { //
                }
                _EndWorksheet = false;
            }
            Columns.Clear();
        }

        private void EndRow()
        {
            if (!_EndRow) return;
            if (IsXml)
            {
                Write("   </Row>\n");
            }
            else
            {
                Write("\n");
            }
            if (_InsertAtBegin)
            {
                SeekToEnd();
            }
            _EndRow = false;
        }

        #endregion

        #region Document Lifespan (public)
        
        public void BeginFile()
        {
            if (_FileOutput != null && _WriteBOM)
            {
                _FileOutput.Write(_FileEncoding.GetPreamble(), 0, _FileEncoding.GetPreamble().Length);
            }
            if (IsXml)
            {
                Write("<?Xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                Write("<?mso-application progid=\"Excel.Sheet\"?>\n");
                Write("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\n");
                Write(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"\n");
                Write(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\n");
                Write(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">\n");

                Write(" <Styles>\n");
                Write("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">\n");
                Write("   <Alignment ss:Vertical=\"Bottom\"/>\n");
                Write("  </Style>\n");

                for (int i = 0; i < Styles.Count; i++)
                {
                    WriteStyle(i);
                }
                Write(" </Styles>\n");
            }
            else
            {
            }
            KeepRecordOfFirstRowPosition();
        }

        public void EndFile()
        {
            EndWorksheet();
            if (IsXml)
            {
                Write("</Workbook>\n");
            }
        }
        
        public void NewWorksheet(string WorksheetName)
        {
            bool bAddEmptyRow = _EndWorksheet;

            EndWorksheet();
            _EndWorksheet = true;
            if (IsXml)
            {
                Write(string.Format(" <Worksheet ss:Name=\"{0}\">\n", PrepareString(WorksheetName)));
                _StartWorksheet = true;
            }
            else
            {
                if (bAddEmptyRow) BeginRow();
                if (WorksheetName != null)
                {
                    BeginRow();
                    SetCell(WorksheetName);
                    BeginRow();
                }
            }
            KeepRecordOfFirstRowPosition();
        }

        public void BeginRow()
        {
            BeginRow(-1, 0f);
        }
        public void BeginRow(bool InsertAtBegin)
        {
            BeginRow(InsertAtBegin, -1, 0f);
        }
        public void BeginRow(int StyleIndex)
        {
            BeginRow(StyleIndex, 0f);
        }
        public void BeginRow(bool InsertAtBegin, int StyleIndex)
        {
            BeginRow(InsertAtBegin, StyleIndex, 0f);
        }
        public void BeginRow(bool InsertAtBegin, int StyleIndex, float Height)
        {
            if (InsertAtBegin)
            {
                EndRow();
            }
            if (!_EndWorksheet)
            {
                NewWorksheet("New Sheet");
            }
            _InsertAtBegin = InsertAtBegin;
            if (_InsertAtBegin)
            {
                SeekToFirstRow();
            }
            BeginRow(StyleIndex, Height);
        }
        public void BeginRow(int StyleIndex, float Height)
        {
            if (!_EndWorksheet) NewWorksheet("New Sheet");

            if (IsXml)
            {
                BeginWorksheet();
                EndRow();

                Write("   <Row");
                if (StyleIndex >= 0)
                {
                    Write(string.Format(" ss:StyleID=\"s{0}\"", StyleIndex + 21));
                }
                if (Height != 0)
                {
                    Write(string.Format(" ss:Height=\"{0:0.##}\"", Height));
                }
                Write(">\n");
            }
            else
            {
                EndRow();
            }

            _EndRow = true;
        }

        #endregion

        #region Syling

        private void WriteStyle(int StyleIndex)
        {
            Write(string.Format(@"<ss:Style ss:ID=""s{0}"">", StyleIndex + 21));

            ExcelSheetStyle style = Styles[StyleIndex];

            if (style.Alignment != null)
            {
                Write(@"<ss:Alignment"); // Opening tag

                string Horizontal = null;
                switch (style.Alignment.Horizontal)
                {
                    default:
                    case HorizontalAlignment.Automatic: // Default
                        break;
                    case HorizontalAlignment.Left:
                        Horizontal = @"Left";
                        break;
                    case HorizontalAlignment.Center:
                        Horizontal = @"Center";
                        break;
                    case HorizontalAlignment.Right:
                        Horizontal = @"Right";
                        break;
                    case HorizontalAlignment.Fill:
                        Horizontal = @"Fill";
                        break;
                    case HorizontalAlignment.Justify:
                        Horizontal = @"Justify";
                        break;
                    case HorizontalAlignment.CenterAcrossSelection:
                        Horizontal = @"CenterAcrossSelection";
                        break;
                    case HorizontalAlignment.Distributed:
                        Horizontal = @"Distributed";
                        break;
                    case HorizontalAlignment.JustifyDistributed:
                        Horizontal = @"JustifyDistributed";
                        break;
                }
                if (Horizontal != null)
                {
                    Write(string.Format(@" ss:Horizontal=""{0}""", Horizontal));
                }

                if (style.Alignment.Indent > 0) // 0 is default
                {
                    Write(string.Format(@" ss:Indent=""{0}""", style.Alignment.Indent));
                }

                string ReadingOrder = null;
                switch (style.Alignment.ReadingOrder)
                {
                    default:
                    case HorizontalReadingOrder.Context: // Default
                        break;
                    case HorizontalReadingOrder.RightToLeft:
                        ReadingOrder = @"RightToLeft";
                        break;
                    case HorizontalReadingOrder.LeftToRight:
                        ReadingOrder = @"LeftToRight";
                        break;
                }
                if (ReadingOrder != null)
                {
                    Write(string.Format(@" ss:ReadingOrder=""{0}""", ReadingOrder));
                }

                if (style.Alignment.Rotate != 0.0) // 0 is default
                {
                    Write(string.Format(CultureInfo.InvariantCulture, @" ss:Rotate=""{0}""", style.Alignment.Rotate));
                }

                if (style.Alignment.ShrinkToFit) // FALSE is default
                {
                    Write(@" ss:ShrinkToFit=""1""");
                }

                string Vertical = null;
                switch (style.Alignment.Vertical)
                {
                    default:
                    case VerticalAlignment.Automatic: // Default
                        break;
                    case VerticalAlignment.Top:
                        Vertical = @"Top";
                        break;
                    case VerticalAlignment.Bottom:
                        Vertical = @"Bottom";
                        break;
                    case VerticalAlignment.Center:
                        Vertical = @"Center";
                        break;
                    case VerticalAlignment.Justify:
                        Vertical = @"Justify";
                        break;
                    case VerticalAlignment.Distributed:
                        Vertical = @"Distributed";
                        break;
                    case VerticalAlignment.JustifyDistributed:
                        Vertical = @"JustifyDistributed";
                        break;
                }
                if (Vertical != null)
                {
                    Write(string.Format(@" ss:Vertical=""{0}""", Vertical));
                }

                if (style.Alignment.VerticalText) // FALSE is default
                {
                    Write(@" ss:VerticalText=""1""");
                }

                if (style.Alignment.WrapText) // FALSE is default
                {
                    Write(@" ss:WrapText=""1""");
                }

                Write("/>\n"); // Closing tag
            }

            if (style.NumberFormat != null && style.NumberFormat.Length > 0)
            {
                Write(string.Format(@"<ss:NumberFormat ss:Format=""{0}""/>", style.NumberFormat));
            }

            if (style.Borders != null && style.Borders.Count > 0)
            {
                Write(@"<ss:Borders>"); // Opening tag

                foreach (Border border in style.Borders)
                {
                    Write(@"<ss:Border"); // Opening tag

                    string Position = null;
                    switch (border.Position)
                    {
                        default:
                        case BorderPosition.Left:
                            Position = @"Left";
                            break;
                        case BorderPosition.Top:
                            Position = @"Top";
                            break;
                        case BorderPosition.Right:
                            Position = @"Right";
                            break;
                        case BorderPosition.Bottom:
                            Position = @"Bottom";
                            break;
                        case BorderPosition.DiagonalLeft:
                            Position = @"DiagonalLeft";
                            break;
                        case BorderPosition.DiagonalRight:
                            Position = @"DiagonalRight";
                            break;
                    }
                    Write(string.Format(@" ss:Position=""{0}""", Position)); // Required

                    if (!border.Color.IsTransparentOrEmpty())
                    {
                        Write(string.Format(@" ss:Color=""{0}""", border.Color.Css()));
                    }

                    string LineStyle = null;
                    switch (border.LineStyle)
                    {
                        default:
                        case BorderLineStyle.None: // Default
                            break;
                        case BorderLineStyle.Continuous:
                            LineStyle = @"Continuous";
                            break;
                        case BorderLineStyle.Dash:
                            LineStyle = @"Dash";
                            break;
                        case BorderLineStyle.Dot:
                            LineStyle = @"Dot";
                            break;
                        case BorderLineStyle.DashDot:
                            LineStyle = @"DashDot";
                            break;
                        case BorderLineStyle.DashDotDot:
                            LineStyle = @"DashDotDot";
                            break;
                        case BorderLineStyle.SlantDashDot:
                            LineStyle = @"SlantDashDot";
                            break;
                        case BorderLineStyle.Double:
                            LineStyle = @"Double";
                            break;
                    }
                    if (LineStyle != null)
                    {
                        Write(string.Format(@" ss:LineStyle=""{0}""", LineStyle));
                    }

                    if (border.Weight > 0.0) // 0 is default
                    {
                        Write(string.Format(CultureInfo.InvariantCulture, @" ss:Weight=""{0}""", border.Weight));
                    }

                    Write("/>\n"); // Closing tag
                }

                Write("</ss:Borders>\n"); // Closing tag
            }

            if (style.Interior != null)
            {
                Write(@"<ss:Interior"); // Opening tag

                if (!style.Interior.Color.IsTransparentOrEmpty())
                {
                    Write(string.Format(@" ss:Color=""{0}""", style.Interior.Color.Css()));
                }

                string Pattern = null;
                switch (style.Interior.Pattern)
                {
                    default:
                    case InteriorPattern.None: // Default
                        break;
                    case InteriorPattern.Solid:
                        Pattern = @"Solid";
                        break;
                    case InteriorPattern.Gray75:
                        Pattern = @"Gray75";
                        break;
                    case InteriorPattern.Gray50:
                        Pattern = @"Gray50";
                        break;
                    case InteriorPattern.Gray25:
                        Pattern = @"Gray25";
                        break;
                    case InteriorPattern.Gray125:
                        Pattern = @"Gray125";
                        break;
                    case InteriorPattern.Gray0625:
                        Pattern = @"Gray0625";
                        break;
                    case InteriorPattern.HorzStripe:
                        Pattern = @"HorzStripe";
                        break;
                    case InteriorPattern.VertStripe:
                        Pattern = @"VertStripe";
                        break;
                    case InteriorPattern.ReverseDiagStripe:
                        Pattern = @"ReverseDiagStripe";
                        break;
                    case InteriorPattern.DiagCross:
                        Pattern = @"DiagCross";
                        break;
                    case InteriorPattern.ThickDiagCross:
                        Pattern = @"ThickDiagCross";
                        break;
                    case InteriorPattern.ThinHorzStripe:
                        Pattern = @"ThinHorzStripe";
                        break;
                    case InteriorPattern.ThinVertStripe:
                        Pattern = @"ThinVertStripe";
                        break;
                    case InteriorPattern.ThinReverseDiagStripe:
                        Pattern = @"ThinReverseDiagStripe";
                        break;
                    case InteriorPattern.ThinDiagStripe:
                        Pattern = @"ThinDiagStripe";
                        break;
                    case InteriorPattern.ThinHorzCross:
                        Pattern = @"ThinHorzCross";
                        break;
                    case InteriorPattern.ThinDiagCross:
                        Pattern = @"ThinDiagCross";
                        break;
                }
                if (Pattern != null)
                {
                    Write(string.Format(@" ss:Pattern=""{0}""", Pattern));
                }

                if (!style.Interior.PatternColor.IsTransparentOrEmpty())
                {
                    Write(string.Format(@" ss:PatternColor=""{0}""", style.Interior.PatternColor.Css()));
                }

                Write("/>\n"); // Closing tag
            }

            if (style.Font != null)
            {
                Write("<ss:Font"); // Opening tag

                if (style.Font.Bold) // FALSE is default
                {
                    Write(@" ss:Bold=""1""");
                }

                if (!style.Font.Color.IsTransparentOrEmpty())
                {
                    Write(string.Format(@" ss:Color=""{0}""", style.Font.Color.Css()));
                }

                if (style.Font.FontName != null && style.Font.FontName.Length > 0)
                {
                    //Write(string.Format(@" ss:FontName=""{0}""", style.Font.FontName));
                    Write(string.Format(@" ss:FontName=""{0}""", @"Arial"));
                }

                if (style.Font.Italic) // FALSE is default
                {
                    Write(@" ss:Italic=""1""");
                }

                if (style.Font.Outline) // FALSE is default
                {
                    Write(@" ss:Outline=""1""");
                }

                if (style.Font.Shadow) // FALSE is default
                {
                    Write(@" ss:Shadow=""1""");
                }

                if (style.Font.Size != 10.0) // 10 is default
                {
                    Write(string.Format(CultureInfo.InvariantCulture, @" ss:Size=""{0}""", style.Font.Size));
                }

                if (style.Font.StrikeThrough) // FALSE is default
                {
                    Write(@" ss:StrikeThrough=""1""");
                }

                string Underline = null;
                switch (style.Font.Underline)
                {
                    default:
                    case FontUnderline.None: // Default
                        break;
                    case FontUnderline.Single:
                        Underline = @"Single";
                        break;
                    case FontUnderline.Double:
                        Underline = @"Double";
                        break;
                    case FontUnderline.SingleAccounting:
                        Underline = @"SingleAccounting";
                        break;
                    case FontUnderline.DoubleAccounting:
                        Underline = @"DoubleAccounting";
                        break;
                }
                if (Underline != null)
                {
                    Write(string.Format(@" ss:Underline=""{0}""", Underline));
                }

                string VerticalAlign = null;
                switch (style.Font.VerticalAlign)
                {
                    default:
                    case FontVerticalAlign.None: // Default
                        break;
                    case FontVerticalAlign.Subscript:
                        VerticalAlign = @"Subscript";
                        break;
                    case FontVerticalAlign.Superscript:
                        VerticalAlign = @"Superscript";
                        break;
                }
                if (VerticalAlign != null)
                {
                    Write(string.Format(@" ss:VerticalAlign=""{0}""", VerticalAlign));
                }

                if (style.Font.CharSet > 0) // 0 is default
                {
                    Write(string.Format(CultureInfo.InvariantCulture, @" ss:CharSet=""{0}""", style.Font.CharSet));
                }

                string Family = null;
                switch (style.Font.Family)
                {
                    default:
                    case FontFamily.Automatic: // Default
                        break;
                    case FontFamily.Decorative:
                        Family = @"Decorative";
                        break;
                    case FontFamily.Modern:
                        Family = @"Modern";
                        break;
                    case FontFamily.Roman:
                        Family = @"Roman";
                        break;
                    case FontFamily.Script:
                        Family = @"Script";
                        break;
                    case FontFamily.Swiss:
                        Family = @"Swiss";
                        break;
                }
                if (Family != null)
                {
                    Write(string.Format(@" ss:Family=""{0}""", Family));
                }

                Write("/>\n"); // Closing tag
            }

            Write("</ss:Style>\n");
        }

        public int AddStyle(ExcelSheetStyle Style)
        {
            Styles.Add(Style);
            return Styles.Count - 1;
        }

        #endregion

        #region Cells

        public void AddColumn()
        {
            AddColumn(0f);
        }
        public void AddColumn(float Width)
        {
            string strCol;
            if (Width != 0.0)
                strCol = Width.ToString(@"0.##");
            else
                strCol = "";
            Columns.Add(strCol);
        }

        public void SetCell(string Data, int StyleIndex = -1, bool FormatFromStyle = false, int MergeAcross = 0, int MergeDown = 0)
        {
            if (IsXml)
            {
                string merge = (MergeAcross == 0 && MergeDown == 0) ? @"" :
                    (
                        (MergeAcross != 0 && MergeDown != 0) ?
                        string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}"" ss:MergeDown=""{0}""", MergeAcross, MergeDown) :
                        (
                            (MergeAcross != 0) ?
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}""", MergeAcross) :
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeDown=""{0}""", MergeDown)
                        )
                    );
                if (StyleIndex != -1)
                {
                    if (FormatFromStyle)
                    {
                        ExcelSheetStyle style = Styles[StyleIndex];
                        if (style.NumberFormat != null && (
                            style.NumberFormat == NumberFormat.Scientific ||
                            style.NumberFormat == NumberFormat.Fixed ||
                            style.NumberFormat == NumberFormat.Standard ||
                            style.NumberFormat == NumberFormat.Number0 ||
                            style.NumberFormat == NumberFormat.Number0_00))
                        {
                            Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"Number\">{2}</Data></Cell>\n", StyleIndex + 21, merge, PrepareString(Data)));
                        }
                        else
                        {
                            Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"String\">{2}</Data></Cell>\n", StyleIndex + 21, merge, PrepareString(Data)));
                        }
                    }
                    else
                    {
                        Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"String\">{2}</Data></Cell>\n", StyleIndex + 21, merge, PrepareString(Data)));
                    }
                }
                else
                {
                    Write(string.Format("    <Cell{0}><Data ss:Type=\"String\">{1}</Data></Cell>\n", merge, PrepareString(Data)));
                }
            }
            else
            {
                Write(string.Format(@"""{0}"",", PrepareString(Data)));
            }
        }

        public void SetCell(int Data, int StyleIndex = -1, int MergeAcross = 0, int MergeDown = 0)
        {
            if (IsXml)
            {
                string merge = (MergeAcross == 0 && MergeDown == 0) ? @"" :
                    (
                        (MergeAcross != 0 && MergeDown != 0) ?
                        string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}"" ss:MergeDown=""{0}""", MergeAcross, MergeDown) :
                        (
                            (MergeAcross != 0) ?
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}""", MergeAcross) :
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeDown=""{0}""", MergeDown)
                        )
                    );
                if (StyleIndex != -1)
                {
                    Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"Number\">{2}</Data></Cell>\n", StyleIndex + 21, merge, Data));
                }
                else
                {
                    Write(string.Format("    <Cell{0}><Data ss:Type=\"Number\">{1}</Data></Cell>\n", merge, Data));
                }
            }
            else
            {
                Write(string.Format("{0},", Data));
            }
        }

        public void SetCell(Int64 Data, int StyleIndex = -1, int MergeAcross = 0, int MergeDown = 0)
        {
            if (IsXml)
            {
                string merge = (MergeAcross == 0 && MergeDown == 0) ? @"" :
                    (
                        (MergeAcross != 0 && MergeDown != 0) ?
                        string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}"" ss:MergeDown=""{0}""", MergeAcross, MergeDown) :
                        (
                            (MergeAcross != 0) ?
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}""", MergeAcross) :
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeDown=""{0}""", MergeDown)
                        )
                    );
                if (StyleIndex != -1)
                {
                    Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"Number\">{2}</Data></Cell>\n", StyleIndex + 21, merge, Data));
                }
                else
                {
                    Write(string.Format("    <Cell{0}><Data ss:Type=\"Number\">{1}</Data></Cell>\n", merge, Data));
                }
            }
            else
            {
                Write(string.Format("{0},", Data));
            }
        }
                
        public void SetCell(double Data, int StyleIndex = -1, int MergeAcross = 0, int MergeDown = 0)
        {
            if (IsXml)
            {
                string merge = (MergeAcross == 0 && MergeDown == 0) ? @"" :
                    (
                        (MergeAcross != 0 && MergeDown != 0) ?
                        string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}"" ss:MergeDown=""{0}""", MergeAcross, MergeDown) :
                        (
                            (MergeAcross != 0) ?
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}""", MergeAcross) :
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeDown=""{0}""", MergeDown)
                        )
                    );
                if (StyleIndex != -1)
                {
                    Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"Number\">{2}</Data></Cell>\n", StyleIndex + 21, merge, Data));
                }
                else
                {
                    Write(string.Format("    <Cell{0}><Data ss:Type=\"Number\">{1}</Data></Cell>\n", merge, Data));
                }
            }
            else
            {
                Write(string.Format("{0},", Data));
            }
        }

        public void SetCell(decimal Data, int StyleIndex = -1, int MergeAcross = 0, int MergeDown = 0)
        {
            if (IsXml)
            {
                string merge = (MergeAcross == 0 && MergeDown == 0) ? @"" :
                    (
                        (MergeAcross != 0 && MergeDown != 0) ?
                        string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}"" ss:MergeDown=""{0}""", MergeAcross, MergeDown) :
                        (
                            (MergeAcross != 0) ?
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeAcross=""{0}""", MergeAcross) :
                            string.Format(CultureInfo.InvariantCulture, @" ss:MergeDown=""{0}""", MergeDown)
                        )
                    );
                if (StyleIndex != -1)
                {
                    Write(string.Format("    <Cell ss:StyleID=\"s{0}\"{1}><Data ss:Type=\"Number\">{2}</Data></Cell>\n", StyleIndex + 21, merge, Data));
                }
                else
                {
                    Write(string.Format("    <Cell{0}><Data ss:Type=\"Number\">{1}</Data></Cell>\n", merge, Data));
                }
            }
            else
            {
                Write(string.Format("{0},", Data));
            }
        }

        #endregion
    }
}
