using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using System.Globalization;


namespace data_chart
{

    public partial class Form1 : Form
    {
        public string[] month = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };

        public partial class Pair<T1, T2>
        {
            public T1 First { get; set; }
            public T2 Second { get; set; }

            public Pair(T1 first, T2 second)
            {
                First = first;
                Second = second;
            }
        }

        public partial class YearMonthComparer : IComparer<Pair<string,int>>
        {
            public int Compare(Pair<string,int> x, Pair<string,int> y)
            {
                string s1 = x.First.Substring(0, 4);
                string s2 = y.First.Substring(0, 4);
                string s3 = x.First.Substring(5, 2);
                string s4 = y.First.Substring(5, 2);

                int result = string.Compare(s1, s2);

                if (result < 0)
                {
                    return -1;
                }
                else if (result > 0)
                {
                    return 1;
                }
                else
                {
                    int res = string.Compare(s3, s4);
                    if (res < 0) return -1;
                    else if (res > 0) return 1;
                    else return 0;
                }

            }
        }
        DataTable dt = new DataTable("CSV data");
        /*船東那些日期上的出船次數*/
        public Dictionary<string, int> YM = new Dictionary<string, int>();
        public Dictionary<string, int> EVE = new Dictionary<string, int>();
        public Dictionary<string, int> WH = new Dictionary<string, int>();
        public HashSet<string> port = new HashSet<string>();
        /*日期對應港口*/
        public Dictionary<string, string> YM_DateToPort = new Dictionary<string, string>();
        public Dictionary<string, string> EVE_DateToPort = new Dictionary<string, string>();
        public Dictionary<string, string> WH_DateToPort = new Dictionary<string, string>();
        /*港口對應頻率*/
        public Dictionary<string, int> YM_portToFreq = new Dictionary<string, int>();
        public Dictionary<string, int> EVE_portToFreq = new Dictionary<string, int>();
        public Dictionary<string, int> WH_portToFreq = new Dictionary<string, int>();
        /*船舶對應頻率*/
        public Dictionary<string, int> YM_ShipToFreq = new Dictionary<string, int>();
        public Dictionary<string, int> EVE_ShipToFreq = new Dictionary<string, int>();
        public Dictionary<string, int> WH_ShipToFreq = new Dictionary<string, int>();
        /*船舶對應日期*/
        public Dictionary<string, Dictionary<string,int>> YM_ShipToDate = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> EVE_ShipToDate = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> WH_ShipToDate = new Dictionary<string, Dictionary<string, int>>();
        /*港口與貨櫃船次數*/
        public Dictionary<string, int> container = new Dictionary<string, int>();

        /*萬海、長榮、陽明*/
        public List<Series>[] CompanyStock = new List<Series>[3];
        public class ChartForm : Form
        {
            public ChartForm()
            {

            }
            public ChartForm(List<Series> BoolWalk, List<Series> MACD_List)
            {
                DrawChart5(BoolWalk, MACD_List);
            }
            public ChartForm(Series S, Dictionary<string, string> Data)
            {
                DrawChart1(S, Data);
            }
            public ChartForm(Series S, List<Series> list)
            {
                DrawChart2(S, list);
            }
            public ChartForm(Dictionary<string,int> Data)
            {
                DrawChart3(Data);
            }
            public ChartForm(Series K_line, Series Volume, Series K, Series D, List<Series> BoolWalk)
            {
                DrawChart4(K_line, Volume, K, D, BoolWalk);
            }
            public Panel DrawChart1(Series S, Dictionary<string, string> Data)
            {
                Panel panel = new Panel();
                panel.Dock = DockStyle.Fill;
                //panel.Location = new System.Drawing.Point(0, 0);

                S["PixelPointWidth"] = "30"; // 30 是粗细值

                Chart chart1 = new Chart();
                chart1.Parent = this;
                chart1.Size = new System.Drawing.Size(1500, 500);
                chart1.Location = new System.Drawing.Point(0, 200);

                ChartArea chartArea1 = new ChartArea();
                chartArea1.AxisX.Interval = 1;
                chartArea1.AxisX.Title = "港口";
                chartArea1.AxisY.Title = "次數";
                chartArea1.AxisX.LabelAutoFitMaxFontSize = 12;
                chart1.ChartAreas.Add(chartArea1);
                chart1.Series.Add(S);
                chart1.Titles.Add("近三年前十大港口與船隻次數");
                
                DataGridView dataGridView = new DataGridView();
                dataGridView.Location = new System.Drawing.Point(0, 0); // 设置左上角的位置
                dataGridView.Size = new System.Drawing.Size(1500, 200); // 设置宽度和高度
                dataGridView.BackgroundColor = Color.White;
                dataGridView.BorderStyle = BorderStyle.Fixed3D;
                DataTable table = new DataTable();
                table.Columns.Add("年份",typeof(String));
                table.Columns.Add("月份", typeof(String));
                table.Columns.Add("進港港口", typeof(String));
                table.Columns.Add("離港港口", typeof(String));
                dataGridView.DataSource = table;

                foreach (var data in Data)
                {
                    DataRow row = table.NewRow();
                    string[] port = data.Key.Split('-');
                    string[] date = data.Value.Split('/');
                    row["年份"] = date[0];
                    row["月份"] = date[1];
                    row["進港港口"] = port[0];
                    row["離港港口"] = port[1];
                    table.Rows.Add(row);
                }

                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                panel.Controls.Add(chart1);
                panel.Controls.Add(dataGridView);
                //this.Controls.Add(dataGridView);

                return panel;
            }
            public Panel DrawChart2(Series S, List<Series> list)
            {
                Panel panel = new Panel();
                panel.Dock = DockStyle.Fill;

                S["PixelPointWidth"] = "30"; // 30 是粗细值

                Chart chart1 = new Chart();
                chart1.Parent = this;
                chart1.Size = new System.Drawing.Size(1500, 300);
                chart1.Location = new System.Drawing.Point(0, 0);

                ChartArea chartArea1 = new ChartArea();
                chartArea1.AxisX.Interval = 1;
                chartArea1.AxisX.Title = "船舶呼號";
                chartArea1.AxisY.Title = "出航次數";
                chart1.ChartAreas.Add(chartArea1);
                chart1.Series.Add(S);
                chart1.Titles.Add("近三年前十大船舶出航次數");

                Chart chart2 = new Chart();
                chart2.Parent = this;
                chart2.Size = new System.Drawing.Size(1500, 450);
                chart2.Location = new System.Drawing.Point(0, 300);

                ChartArea chartArea2 = new ChartArea();
                chartArea2.AxisX.Interval = 1;
                chartArea2.AxisX.Title = "日期";
                chartArea2.AxisY.Title = "出航次數";
                chart2.ChartAreas.Add(chartArea2);
                chart2.Titles.Add("近三年前十大船舶出航次數");
                chartArea2.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.All;

                Legend legend = new Legend();
                legend.IsDockedInsideChartArea = false; // 确保图例在图表区域外部显示
                chart2.Legends.Add(legend);

                foreach (var series in list)
                {
                    chart2.Series.Add(series);
                }

                panel.Controls.Add(chart1);
                panel.Controls.Add(chart2);

                return panel;
            }

            public Panel DrawChart3(Dictionary<string, int> Data)
            {
                Panel panel = new Panel();
                panel.Dock = DockStyle.Fill;

                Series S = new Series();
                S.ChartType = SeriesChartType.Column;
                S["PixelPointWidth"] = "30"; 

                foreach(var ele in Data)
                {
                    S.Points.AddXY(ele.Key, ele.Value);
                }

                Chart chart1 = new Chart();
                chart1.Parent = this;
                chart1.Size = new System.Drawing.Size(1500, 600);
                chart1.Location = new System.Drawing.Point(0, 0);

                ChartArea chartArea1 = new ChartArea();
                chartArea1.AxisX.Interval = 1;
                chartArea1.AxisX.Title = "港口";
                chartArea1.AxisY.Title = "次數";
                chartArea1.AxisX.LabelAutoFitMaxFontSize = 12;
                chart1.ChartAreas.Add(chartArea1);
                chart1.Series.Add(S);
                chart1.Titles.Add("近三年前台灣十大港口與貨櫃船出港次數");

                panel.Controls.Add(chart1);
                return panel;
            }

            public Panel DrawChart4(Series K_line, Series Volume, Series K, Series D, List<Series> BoolWalk)
            {
                Panel panel = new Panel();
                panel.Dock = DockStyle.Fill;

                Legend legend = new Legend("legend");
                legend.IsDockedInsideChartArea = false;
                Legend legend1 = new Legend("legend1");
                legend1.IsDockedInsideChartArea = false;

                Chart stockChart = new Chart();
                Chart VolumeChart = new Chart();

                stockChart.Titles.Add("K線圖");
                stockChart.Size = new System.Drawing.Size(1550, 350);
                stockChart.Location = new System.Drawing.Point(0, 0);

                VolumeChart.Titles.Add("交易量圖");
                VolumeChart.Size = new System.Drawing.Size(1585, 350);
                VolumeChart.Location = new System.Drawing.Point(-50, 350);

                ChartArea candlestickChartArea = new ChartArea("Stock Price");
                candlestickChartArea.AxisX.Interval = 10;
                candlestickChartArea.AxisY.Title = "股價";
                candlestickChartArea.AxisX.LabelAutoFitMaxFontSize = 12;
                candlestickChartArea.AxisX.MajorGrid.Enabled = false;
                candlestickChartArea.AxisY.MajorGrid.Enabled = false;
                candlestickChartArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";

                ChartArea volumeChartArea = new ChartArea("VolumeChart");
                volumeChartArea.AxisX.Interval = 10;
                volumeChartArea.AxisY.Title = "交易量";
                volumeChartArea.AxisX.LabelAutoFitMaxFontSize = 12;
                volumeChartArea.AxisX.MajorGrid.Enabled = false;
                volumeChartArea.AxisY.MajorGrid.Enabled = false;
                volumeChartArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";

                candlestickChartArea.BackColor = Color.Black;
                volumeChartArea.BackColor = Color.Black;

                stockChart.ChartAreas.Add(candlestickChartArea);
                VolumeChart.ChartAreas.Add(volumeChartArea);

                BoolWalk[0].YAxisType = AxisType.Secondary;
                BoolWalk[1].YAxisType = AxisType.Secondary;
                BoolWalk[2].YAxisType = AxisType.Secondary;

                K.YAxisType = AxisType.Secondary;
                D.YAxisType = AxisType.Secondary;
                

                K.LegendText = "K線";
                D.LegendText = "D線";
                Volume.LegendText = "成交量";
                BoolWalk[0].LegendText = "上軌";
                BoolWalk[1].LegendText = "中軌";
                BoolWalk[2].LegendText = "下軌";

       
                // 将Series添加到Chart控件
                stockChart.Series.Add(K_line);
                foreach(var ele in BoolWalk)
                {
                    
                    stockChart.Series.Add(ele);
                }
                VolumeChart.Series.Add(Volume);
                VolumeChart.Series.Add(K);
                VolumeChart.Series.Add(D);
                VolumeChart.Legends.Add(legend1);
                
                stockChart.Legends.Add(legend);
                
                // 设置X轴为日期时间类型
                candlestickChartArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";
                volumeChartArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";

                stockChart.BackColor = Color.Cornsilk;
                VolumeChart.BackColor = Color.Cornsilk;
                
                panel.Controls.Add(stockChart);
                panel.Controls.Add(VolumeChart);
                panel.BackColor = Color.Cornsilk;
                return panel;
            }
            public Panel DrawChart5(List<Series> BoolWalk, List<Series> MACD_List)
            {
                Panel panel = new Panel();
                panel.Dock = DockStyle.Fill;

                Chart Bool = new Chart();
                Chart MACD_Chart = new Chart();

                ChartArea BoolArea = new ChartArea();
                ChartArea MACDchartArea = new ChartArea("MACDChart");

                Legend legend1 = new Legend("legend1");
                Legend legend = new Legend("legend");
                legend1.IsDockedInsideChartArea = false;
                legend.IsDockedInsideChartArea = false;

                Bool.Titles.Add("布林通道");
                Bool.Size = new System.Drawing.Size(1400, 350);
                Bool.Location = new System.Drawing.Point(0,350);
                Bool.BackColor = Color.Cornsilk;
                Bool.Legends.Clear();
                Bool.Legends.Add(legend);
                BoolArea.AxisX.Interval = 10;
                BoolArea.AxisX.MajorGrid.Enabled = false;
                BoolArea.AxisY.MajorGrid.Enabled = false;
                BoolArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";
                BoolArea.BackColor = Color.Black;

                MACD_Chart.Titles.Add("MACD圖");
                MACD_Chart.Size = new System.Drawing.Size(1450, 350);
                MACD_Chart.Location = new System.Drawing.Point(0, 0);
                MACD_Chart.Legends.Add(legend1);
                MACD_Chart.BackColor = Color.Cornsilk;
                MACD_List[0].YAxisType = AxisType.Secondary;
                MACD_List[1].YAxisType = AxisType.Secondary;
                MACDchartArea.AxisX.Interval = 10;
                MACDchartArea.AxisX.MajorGrid.Enabled = false;
                MACDchartArea.AxisY.MajorGrid.Enabled = false;
                MACDchartArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";
                MACDchartArea.BackColor = Color.Black;
                MACDchartArea.AxisX.LabelStyle.Format = "yyyy-MM-dd";
                

                foreach (var S in MACD_List)
                {
                    MACD_Chart.Series.Add(S);
                }

                
                foreach (var ele in BoolWalk)
                {
                    Bool.Series.Add(ele);
                }

                Bool.ChartAreas.Add(BoolArea);
                MACD_Chart.ChartAreas.Add(MACDchartArea);

                

                panel.Controls.Add(Bool);
                panel.Controls.Add(MACD_Chart);
                panel.BackColor = Color.Cornsilk;
                return panel;
            }
        }


        public Form1()
        {
            
            InitializeComponent();

            dt.Columns.Add("ShipNumber", typeof(String));
            dt.Columns.Add("ShipName", typeof(String));
            dt.Columns.Add("ShipType", typeof(String));
            dt.Columns.Add("ShipOwner", typeof(String));
            dt.Columns.Add("StartPlace", typeof(String));
            dt.Columns.Add("Destination", typeof(String));
            dt.Columns.Add("ArriveTime", typeof(String));
            dt.Columns.Add("LeaveTime", typeof(String));

            dt.Columns["ShipNumber"].MaxLength = 1000;
            dt.Columns["ShipName"].MaxLength = 1000;
            dt.Columns["ShipType"].MaxLength = 1000;
            dt.Columns["ShipOwner"].MaxLength = 1000;
            dt.Columns["StartPlace"].MaxLength = 1000;
            dt.Columns["Destination"].MaxLength = 1000;
            dt.Columns["ArriveTime"].MaxLength = 1000;
            dt.Columns["LeaveTime"].MaxLength = 1000;

            tabControl1.Size = new System.Drawing.Size(1500, 800);

        }

        private void ReadCsvData(string[] csvFiles)
        {
            //讀入政府OpenData的船隻資訊
            using (StreamReader reader = new StreamReader(csvFiles[0]))
            {
                string line;
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(',');
                    string[] name = data[5].Split(' ');

                    if (data[5] != "陽明海運股份有限公司" &&
                        data[5] != "萬海航運股份有限公司" &&
                        data[5] != "長榮海運股份有限公司" &&
                        name[0] != "EVERGREEN" && 
                        name[0] != "WAN") continue;


                    if (data.Length != 17) continue;

                    //ShipNumber = data[0];
                    //ShipName = data[1];
                    //ShipOwner = data[5];
                    //ShipType = data[7];
                    //StartPlace = data[11];
                    //Destination = data[12];
                    //ArriveTime = data[13] + " ; " + data[14];
                    //LeaveTime = data[15] + " ; " + data[16];


                    // insert start place and destination
                    string srcTodest = "";
                    srcTodest += data[11].Length == 0 ? "無" : data[11];
                    srcTodest += '-';
                    srcTodest += data[12].Length == 0 ? "無" : data[12];

                    string date = "";
                    if (data[13].Length > 0)
                        date = data[13].Substring(0, 4) + '/' + data[13].Substring(4, 2);
                    else if (data[15].Length > 0)
                        date = data[15].Substring(0, 4) + '/' + data[15].Substring(4, 2);
                    else continue;

                    port.Add(srcTodest);

                    string shipNum = data[0];
                    switch (data[5])
                    {
                        case "陽明海運股份有限公司":

                            YM_DateToPort[srcTodest] = date;

                            if (YM.ContainsKey(date))// count date coresponde to number of company
                            {
                                YM[date]++;
                            }
                            else{
                                YM[date] = 1;
                            }
                            if (YM_portToFreq.ContainsKey(srcTodest)){// count port coresponde to its frequency
                                ++YM_portToFreq[srcTodest];
                            }
                            else{
                                YM_portToFreq[srcTodest] = 1;
                            }
                            if (YM_ShipToFreq.ContainsKey(data[0]))// count ship number coresponde to its frequency
                            {
                                YM_ShipToFreq[shipNum]++;
                            }
                            else{
                                YM_ShipToFreq[shipNum] = 1;
                            }
                            if (YM_ShipToDate.ContainsKey(shipNum))
                            {
                                if (YM_ShipToDate[shipNum].ContainsKey(date))
                                    YM_ShipToDate[shipNum][date]++;
                                else
                                    YM_ShipToDate[shipNum][date] = 1;
                            }
                            else
                            {
                                YM_ShipToDate[shipNum] = new Dictionary<string, int>();
                                YM_ShipToDate[shipNum][date] = 1;
                            }
                            break;

                        case "長榮海運股份有限公司":

                            EVE_DateToPort[srcTodest] = date;

                            if (EVE.ContainsKey(date))// count date coresponde to number of company
                            {
                                EVE[date]++;
                            }
                            else
                            {
                                EVE[date] = 1;
                            }
                            if (EVE_portToFreq.ContainsKey(srcTodest))// count port coresponde to its frequency
                            {
                                EVE_portToFreq[srcTodest]++;
                            }
                            else
                            {
                                EVE_portToFreq[srcTodest] = 1;
                            }
                            if (EVE_ShipToFreq.ContainsKey(data[0]))// count ship number coresponde to its frequency
                            {
                                EVE_ShipToFreq[shipNum]++;
                            }
                            else
                            {
                                EVE_ShipToFreq[shipNum] = 1;
                            }
                            if (EVE_ShipToDate.ContainsKey(shipNum))
                            {
                                if (EVE_ShipToDate[shipNum].ContainsKey(date))
                                    EVE_ShipToDate[shipNum][date]++;
                                else
                                    EVE_ShipToDate[shipNum][date] = 1;
                            }
                            else
                            {
                                EVE_ShipToDate[shipNum] = new Dictionary<string, int>();
                                EVE_ShipToDate[shipNum][date] = 1;
                            }
                            break;

                        case "萬海航運股份有限公司":

                            WH_DateToPort[srcTodest] = date;
                            
                            if (WH.ContainsKey(date))// count date coresponde to number of company
                            {
                                WH[date]++;
                            }
                            else
                            {
                                WH[date] = 1;
                            }
                            if (WH_portToFreq.ContainsKey(srcTodest))// count port coresponde to its frequency
                            {
                                WH_portToFreq[srcTodest]++;
                            }
                            else
                            {
                                WH_portToFreq[srcTodest] = 1;
                            }
                            if (WH_ShipToFreq.ContainsKey(data[0]))// count ship number coresponde to its frequency
                            {
                                WH_ShipToFreq[shipNum]++;
                            }
                            else
                            {
                                WH_ShipToFreq[shipNum] = 1;
                            }
                            if (WH_ShipToDate.ContainsKey(shipNum))
                            {
                                if (WH_ShipToDate[shipNum].ContainsKey(date))
                                    WH_ShipToDate[shipNum][date]++;
                                else
                                    WH_ShipToDate[shipNum][date] = 1;
                            }
                            else
                            {
                                WH_ShipToDate[shipNum] = new Dictionary<string, int>();
                                WH_ShipToDate[shipNum][date] = 1;
                            }
                            break;
                    }
                    switch (name[0])
                    {
                        case "EVERGREEN":

                            EVE_DateToPort[srcTodest] = date;

                            if (EVE.ContainsKey(date))
                            {
                                EVE[date]++;
                            }
                            else
                            {
                                EVE[date] = 1;
                            }
                            if (EVE_portToFreq.ContainsKey(srcTodest))
                            {
                                EVE_portToFreq[srcTodest]++;
                            }
                            else
                            {
                                EVE_portToFreq[srcTodest] = 1;
                            }
                            if (EVE_ShipToFreq.ContainsKey(shipNum))
                            {
                                EVE_ShipToFreq[shipNum]++;
                            }
                            else
                            {
                                EVE_ShipToFreq[shipNum] = 1;
                            }
                            if (EVE_ShipToDate.ContainsKey(shipNum))
                            {
                                if(EVE_ShipToDate[shipNum].ContainsKey(date))
                                    EVE_ShipToDate[shipNum][date]++;
                                else
                                    EVE_ShipToDate[shipNum][date] = 1;
                            }
                            else
                            {
                                EVE_ShipToDate[shipNum] = new Dictionary<string, int>();
                                EVE_ShipToDate[shipNum][date] = 1;
                            }
                            break;
                        case "WAN":

                            WH_DateToPort[srcTodest] = date;

                            if (WH.ContainsKey(date))
                            {
                                WH[date]++;
                            }
                            else
                            {
                                WH[date] = 1;
                            }
                            if (WH_portToFreq.ContainsKey(srcTodest))
                            {
                                WH_portToFreq[srcTodest]++;
                            }
                            else
                            {
                                WH_portToFreq[srcTodest] = 1;
                            }
                            if (WH_ShipToFreq.ContainsKey(shipNum))
                            {
                                WH_ShipToFreq[shipNum]++;
                            }
                            else
                            {
                                WH_ShipToFreq[shipNum] = 1;
                            }
                            if (WH_ShipToDate.ContainsKey(shipNum))
                            {
                                if (WH_ShipToDate[shipNum].ContainsKey(date))
                                    WH_ShipToDate[shipNum][date]++;
                                else
                                    WH_ShipToDate[shipNum][date] = 1;
                            }
                            else
                            {
                                WH_ShipToDate[shipNum] = new Dictionary<string, int>();
                                WH_ShipToDate[shipNum][date] = 1;
                            }
                            break;

                    }

                    
                    // insert row
                    DataRow row = dt.NewRow();
                    row["ShipNumber"] = data[0];
                    row["ShipName"] = data[1];
                    row["ShipOwner"] = data[5];
                    row["ShipType"] = data[7];
                    row["StartPlace"] = data[11];
                    row["Destination"] = data[12];
                    row["ArriveTime"] = (data[13].Length > 0 && data[14].Length > 0) ?
                        data[13].Substring(0, 4) + '/' + data[13].Substring(4, 2) + '/' +
                        data[13].Substring(6, 2) + '/' + data[14].Substring(0, 2) + ':' + data[14].Substring(2, 2) :
                        "";
                    row["LeaveTime"] = (data[15].Length > 0 && data[16].Length > 0) ?
                        data[15].Substring(0, 4) + '/' + data[15].Substring(4, 2) + '/' +
                        data[15].Substring(6, 2) + '/' + data[16].Substring(0, 2) + ':' + data[16].Substring(2, 2) :
                        "";
                    dt.Rows.Add(row);
                }
            }
            //DrawChart();

            int j = 0;
            for (int i = 2; i < 5; i++)
            {
                using (StreamReader reader = new StreamReader(csvFiles[i]))
                {
                    // 创建一个系列，用于表示股价
                    Series priceSeries = new Series("Stock Price");
                    priceSeries.ChartType = SeriesChartType.Candlestick;

                    // 创建一个Series，表示交易量图的交易量数据
                    Series volumeSeries = new Series("VolumeData");
                    volumeSeries.ChartType = SeriesChartType.Column;

                    // 创建一个Series，表示KD线图的KD值数据
                    Series KSeries = new Series("KData");
                    KSeries.ChartType = SeriesChartType.Line;
                    KSeries.Color = Color.Gold;

                    Series DSeries = new Series("DData");
                    DSeries.ChartType = SeriesChartType.Line;
                    DSeries.Color = Color.SteelBlue;

                    Series DIF = new Series("DIF12-26");
                    DIF.ChartType = SeriesChartType.Line;
                    DIF.Color = Color.Blue;
                    DIF.LegendText = "DIF12-26";

                    Series MACD = new Series("MACD9");
                    MACD.ChartType = SeriesChartType.Line;
                    MACD.Color = Color.Red;
                    MACD.LegendText = "MACD9";

                    Series OSC = new Series("OSC");
                    OSC.ChartType = SeriesChartType.Column;
                    OSC.Color = Color.Green;
                    OSC.LegendText = "OSC";

                    Series booleanUp = new Series("上軌");
                    booleanUp.ChartType = SeriesChartType.Line;
                    booleanUp.Color = Color.Gold;
                    booleanUp.LegendText = "上軌";

                    Series booleanM = new Series("中軌");
                    booleanM.ChartType = SeriesChartType.Line;
                    booleanM.Color = Color.Orange;
                    booleanM.LegendText = "中軌";

                    Series booleanDown = new Series("下軌");
                    booleanDown.ChartType = SeriesChartType.Line;
                    booleanDown.Color = Color.Silver;
                    booleanDown.LegendText = "下軌";

                    string line;
                    line = reader.ReadLine();
                    int index = 0;
                    double candleWidth = 2;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] data = line.Split(',');

                        string[] date = data[0].Split(' ');
                        string[] clearDate = date[0].Split('/');
                        string format_date = clearDate[0] + '-';
                        format_date += clearDate[1].Length == 1 ? '0' + clearDate[1] + '-' : clearDate[1] + '-';
                        format_date += clearDate[2].Length == 1 ? '0' + clearDate[2] : clearDate[2];

                        double open = double.Parse(data[1]);
                        double high = double.Parse(data[2]);
                        double low = double.Parse(data[3]);
                        double close = double.Parse(data[4]);
                        int volume = int.Parse(data[5]);
                        double K = double.Parse(data[6]);
                        double D = double.Parse(data[7]);
                        double dif_12_26 = double.Parse(data[8]);
                        double MACD9 = double.Parse(data[9]);
                        double Osc = double.Parse(data[10]);
                        double up = double.Parse(data[11]);
                        double middle = double.Parse(data[12]);
                        double down = double.Parse(data[13]);

                        double[] arr = new double[4] { high, low, open, close };
                        DateTime Date = DateTime.ParseExact(format_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                        // stock price
                        DataPoint dataPoint = new DataPoint();
                        dataPoint.SetValueXY(Date.ToOADate(), arr[0], arr[1], arr[2], arr[3]);
                        dataPoint.AxisLabel = Date.ToString("yyyy-MM-dd");
                        dataPoint["CandleWidth"] = candleWidth.ToString();
                        priceSeries.Points.Add(dataPoint);

                        if (close < open)
                        {
                            priceSeries.Points[index].Color = Color.Green;
                        }
                        else
                        {
                            priceSeries.Points[index].Color = Color.Red;
                        }

                        // volume
                        DataPoint dataPoint1 = new DataPoint();
                        dataPoint1.SetValueXY(Date.ToOADate(), volume);
                        dataPoint1.AxisLabel = Date.ToString("yyyy-MM-dd");
                        volumeSeries.Points.Add(dataPoint1);
                        if (close < open)
                        {
                            volumeSeries.Points[index].Color = Color.Green;
                        }
                        else
                        {
                            volumeSeries.Points[index].Color = Color.Red;
                        }

                        // k D line
                        DataPoint dataPoint2 = new DataPoint();
                        dataPoint2.SetValueXY(Date.ToOADate(), K);
                        dataPoint2.AxisLabel = Date.ToString("yyyy-MM-dd");
                        KSeries.Points.Add(dataPoint2);

                        DataPoint dataPoint3 = new DataPoint();
                        dataPoint3.SetValueXY(Date.ToOADate(), D);
                        dataPoint3.AxisLabel = Date.ToString("yyyy-MM-dd");
                        DSeries.Points.Add(dataPoint3);

                        // MACD
                        DataPoint dif = new DataPoint();
                        dif.SetValueXY(Date.ToOADate(), dif_12_26);
                        dif.AxisLabel = Date.ToString("yyyy-MM-dd");
                        DIF.Points.Add(dif);

                        DataPoint macd = new DataPoint();
                        macd.SetValueXY(Date.ToOADate(), MACD9);
                        macd.AxisLabel = Date.ToString("yyyy-MM-dd");
                        MACD.Points.Add(macd);

                        DataPoint osc = new DataPoint();
                        osc.SetValueXY(Date.ToOADate(), Osc);
                        osc.AxisLabel = Date.ToString("yyyy-MM-dd");
                        OSC.Points.Add(osc);
                        if (Osc > 0) OSC.Points[index].Color = Color.Red;
                        else OSC.Points[index].Color = Color.Green;

                        // 布林通道
                        DataPoint U = new DataPoint();
                        U.SetValueXY(Date.ToOADate(), up);
                        U.AxisLabel = Date.ToString("yyyy-MM-dd");
                        booleanUp.Points.Add(U);

                        DataPoint M = new DataPoint();
                        M.SetValueXY(Date.ToOADate(), middle);
                        M.AxisLabel = Date.ToString("yyyy-MM-dd");
                        booleanM.Points.Add(M);

                        DataPoint Down = new DataPoint();
                        Down.SetValueXY(Date.ToOADate(), down);
                        Down.AxisLabel = Date.ToString("yyyy-MM-dd");
                        booleanDown.Points.Add(Down);

                        index++;

                    }
                    CompanyStock[j] = new List<Series>();
                    CompanyStock[j].Add(priceSeries);
                    CompanyStock[j].Add(volumeSeries);
                    CompanyStock[j].Add(KSeries);
                    CompanyStock[j].Add(DSeries);
                    CompanyStock[j].Add(DIF);
                    CompanyStock[j].Add(MACD);
                    CompanyStock[j].Add(OSC);
                    CompanyStock[j].Add(booleanUp);
                    CompanyStock[j].Add(booleanM);
                    CompanyStock[j].Add(booleanDown);
                    ++j;
                    
                }
            }

            using (StreamReader reader = new StreamReader(csvFiles[1]))
            {
                string line;
                line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(',');

                    string time = data[0];
                    string shipType = data[4];
                    string portName = data[2];

                    int d1 = int.Parse(data[0].Substring(0, 4));
                    int d2 = int.Parse("2020");

                    if (d1 < d2) continue;

                    if (shipType != "貨櫃船") continue;

                    if (container.ContainsKey(portName))
                    {
                        container[portName]++;
                    }
                    else
                    {
                        container[portName] = 1;
                    }


                }
            }
            

        }
        private void DrawChart()
        {
            Chart chart1 = new Chart();
            chart1.Location = new System.Drawing.Point(0, 80);
            chart1.Titles.Add("X: 月份,Y: 船隻數");
            chart1.Width = 750;
            chart1.Height = 600;
            chart1.Series.Clear();
            Series series1 = new Series();
            series1.LegendText = "陽明";
            series1.ChartType = SeriesChartType.FastLine;
            series1.BorderWidth = 3;
            Series series2 = new Series();
            series2.LegendText = "萬海";
            series2.ChartType = SeriesChartType.FastLine;
            series2.BorderWidth = 3;
            Series series3 = new Series();
            series3.LegendText = "長榮";
            series3.ChartType = SeriesChartType.FastLine;
            series3.BorderWidth = 3;

            int year = 2020;
            int m = 0;
            while(year <= 2023)
            {
                string date = year.ToString() + "/" + month[m];
                if (YM.ContainsKey(date))
                {
                    series1.Points.AddXY(date, YM[date]);
                }
                if (EVE.ContainsKey(date))
                {
                    series2.Points.AddXY(date, EVE[date]);
                }
                if (WH.ContainsKey(date))
                {
                    series3.Points.AddXY(date, WH[date]);
                }

                m++;
                if(m > 11)
                {
                    year++;
                    m = 0;
                }
            }

            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
            chart1.Series.Add(series3);
        }
        private void ReadAirData(string[] csvfiles)
        {
            double candleWidth = 2;
            for (int i = 0; i < csvfiles.Length; i++)
            {
                // 创建一个系列，用于表示股价
                Series priceSeries = new Series("Stock Price");
                priceSeries.ChartType = SeriesChartType.Candlestick;

                // 创建一个Series，表示交易量图的交易量数据
                Series volumeSeries = new Series("VolumeData");
                volumeSeries.ChartType = SeriesChartType.Column;

                // 创建一个Series，表示KD线图的KD值数据
                Series KSeries = new Series("KData");
                KSeries.ChartType = SeriesChartType.Line;
                KSeries.Color = Color.Gold;

                Series DSeries = new Series("DData");
                DSeries.ChartType = SeriesChartType.Line;
                DSeries.Color = Color.SteelBlue;

                Series DIF = new Series("DIF12-26");
                DIF.ChartType = SeriesChartType.Line;
                DIF.Color = Color.Blue;
                DIF.LegendText = "DIF12-26";

                Series MACD = new Series("MACD9");
                MACD.ChartType = SeriesChartType.Line;
                MACD.Color = Color.Red;
                MACD.LegendText = "MACD9";

                Series OSC = new Series("OSC");
                OSC.ChartType = SeriesChartType.Column;
                OSC.Color = Color.Green;
                OSC.LegendText = "OSC";

                Series booleanUp = new Series("上軌");
                booleanUp.ChartType = SeriesChartType.Line;
                booleanUp.Color = Color.Gold;
                booleanUp.LegendText = "上軌";

                Series booleanM = new Series("中軌");
                booleanM.ChartType = SeriesChartType.Line;
                booleanM.Color = Color.Orange;
                booleanM.LegendText = "中軌";

                Series booleanDown = new Series("下軌");
                booleanDown.ChartType = SeriesChartType.Line;
                booleanDown.Color = Color.Silver;
                booleanDown.LegendText = "下軌";

                using (StreamReader reader = new StreamReader(csvfiles[i]))
                {
                    string line = reader.ReadLine();
                    line = reader.ReadLine();
                    int index = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] data = line.Split(',');
                        string[] date = data[0].Split(' ');
                        string[] clearDate = date[0].Split(' ');
                        clearDate = date[0].Split('/');
                        string format_date = clearDate[0] + '-';
                        format_date += clearDate[1].Length == 1 ? '0' + clearDate[1] + '-' : clearDate[1] + '-';
                        format_date += clearDate[2].Length == 1 ? '0' + clearDate[2] : clearDate[2];

                        double open = double.Parse(data[1]);
                        double high = double.Parse(data[2]);
                        double low = double.Parse(data[3]);
                        double close = double.Parse(data[4]);
                        int volume = int.Parse(data[5]);
                        double K = double.Parse(data[6]);
                        double D = double.Parse(data[7]);
                        double dif_12_26 = double.Parse(data[8]);
                        double MACD9 = double.Parse(data[9]);
                        double Osc = double.Parse(data[10]);
                        double up = double.Parse(data[11]);
                        double middle = double.Parse(data[12]);
                        double down = double.Parse(data[13]);

                        double[] arr = new double[4] { high, low, open, close };
                        DateTime Date = DateTime.ParseExact(format_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                        // stock price
                        DataPoint dataPoint = new DataPoint();
                        dataPoint.SetValueXY(Date.ToOADate(), arr[0], arr[1], arr[2], arr[3]);
                        dataPoint.AxisLabel = Date.ToString("yyyy-MM-dd");
                        dataPoint["CandleWidth"] = candleWidth.ToString();
                        priceSeries.Points.Add(dataPoint);

                        if (close < open)
                        {
                            priceSeries.Points[index].Color = Color.Green;
                        }
                        else
                        {
                            priceSeries.Points[index].Color = Color.Red;
                        }

                        // volume
                        DataPoint dataPoint1 = new DataPoint();
                        dataPoint1.SetValueXY(Date.ToOADate(), volume);
                        dataPoint1.AxisLabel = Date.ToString("yyyy-MM-dd");
                        volumeSeries.Points.Add(dataPoint1);
                        if (close < open)
                        {
                            volumeSeries.Points[index].Color = Color.Green;
                        }
                        else
                        {
                            volumeSeries.Points[index].Color = Color.Red;
                        }

                        // k D line
                        DataPoint dataPoint2 = new DataPoint();
                        dataPoint2.SetValueXY(Date.ToOADate(), K);
                        dataPoint2.AxisLabel = Date.ToString("yyyy-MM-dd");
                        KSeries.Points.Add(dataPoint2);

                        DataPoint dataPoint3 = new DataPoint();
                        dataPoint3.SetValueXY(Date.ToOADate(), D);
                        dataPoint3.AxisLabel = Date.ToString("yyyy-MM-dd");
                        DSeries.Points.Add(dataPoint3);

                        // MACD
                        DataPoint dif = new DataPoint();
                        dif.SetValueXY(Date.ToOADate(), dif_12_26);
                        dif.AxisLabel = Date.ToString("yyyy-MM-dd");
                        DIF.Points.Add(dif);

                        DataPoint macd = new DataPoint();
                        macd.SetValueXY(Date.ToOADate(), MACD9);
                        macd.AxisLabel = Date.ToString("yyyy-MM-dd");
                        MACD.Points.Add(macd);

                        DataPoint osc = new DataPoint();
                        osc.SetValueXY(Date.ToOADate(), Osc);
                        osc.AxisLabel = Date.ToString("yyyy-MM-dd");
                        OSC.Points.Add(osc);
                        if (Osc > 0) OSC.Points[index].Color = Color.Red;
                        else OSC.Points[index].Color = Color.Green;

                        // 布林通道
                        DataPoint U = new DataPoint();
                        U.SetValueXY(Date.ToOADate(), up);
                        U.AxisLabel = Date.ToString("yyyy-MM-dd");
                        booleanUp.Points.Add(U);

                        DataPoint M = new DataPoint();
                        M.SetValueXY(Date.ToOADate(), middle);
                        M.AxisLabel = Date.ToString("yyyy-MM-dd");
                        booleanM.Points.Add(M);

                        DataPoint Down = new DataPoint();
                        Down.SetValueXY(Date.ToOADate(), down);
                        Down.AxisLabel = Date.ToString("yyyy-MM-dd");
                        booleanDown.Points.Add(Down);

                        index++;
                    }
                    CompanyStock[i] = new List<Series>();
                    CompanyStock[i].Add(priceSeries);
                    CompanyStock[i].Add(volumeSeries);
                    CompanyStock[i].Add(KSeries);
                    CompanyStock[i].Add(DSeries);
                    CompanyStock[i].Add(DIF);
                    CompanyStock[i].Add(MACD);
                    CompanyStock[i].Add(OSC);
                    CompanyStock[i].Add(booleanUp);
                    CompanyStock[i].Add(booleanM);
                    CompanyStock[i].Add(booleanDown);

                }
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            string folderPath = @"C:\Users\張肇恩\航港大數據資料夾";
            string folderPath2 = @"C:\Users\張肇恩\OneDrive\桌面\web form\test\test\reference";
            string searchPattern = "*.csv";

            string[] csvFiles = Directory.GetFiles(folderPath, searchPattern);
            //ReadAirData(csvFiles);
            ReadCsvData(csvFiles);
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = tabControl1.SelectedIndex;

            // 在此處執行與所選擇的TabPage相關的操作
            switch (selectedIndex)
            {
                case 0:
                    {
                        Series seriesA = new Series("陽明");
                        seriesA.ChartType = SeriesChartType.Column;
                        seriesA.LegendText = "陽明";
                        seriesA.Color = Color.SteelBlue;
                        seriesA.YValueType = ChartValueType.String;

                        var SortedList = YM_portToFreq.ToList();
                        SortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                        Dictionary<string, string> Max_ten = new Dictionary<string, string>();

                        for (int i = 0; i < 10; i++)
                        {
                            seriesA.Points.AddXY(SortedList[0].Key, SortedList[0].Value);
                            Max_ten[SortedList[0].Key] = YM_DateToPort[SortedList[0].Key];
                            SortedList.RemoveAt(0);
                        }


                        ChartForm chartForm = new ChartForm();
                        tabpage1.Controls.Add(chartForm.DrawChart1(seriesA, Max_ten));
                        
                    }
                    break;

                case 1:
                    {
                        Series seriesB = new Series("萬海");
                        seriesB.ChartType = SeriesChartType.Column;
                        seriesB.LegendText = "萬海";
                        seriesB.Color = Color.SteelBlue;
                        seriesB.XValueType = ChartValueType.String;
                        seriesB.YValueType = ChartValueType.String;

                        var SortedList = WH_portToFreq.ToList();
                        SortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                        Dictionary<string, string> Max_ten = new Dictionary<string, string>();

                        for (int i = 0; i < 10; i++)
                        {
                            seriesB.Points.AddXY(SortedList[0].Key, SortedList[0].Value);
                            Max_ten[SortedList[0].Key] = WH_DateToPort[SortedList[0].Key];
                            SortedList.RemoveAt(0);
                        }

                        ChartForm chartForm = new ChartForm();
                        tabPage5.Controls.Add(chartForm.DrawChart1(seriesB, Max_ten));
                    }
                    break;

                case 2:
                    {
                        Series seriesC = new Series("長榮");
                        seriesC.ChartType = SeriesChartType.Column;
                        seriesC.LegendText = "長榮";
                        seriesC.Color = Color.SteelBlue;
                        seriesC.YValueType = ChartValueType.String;

                        var SortedList = EVE_portToFreq.ToList();
                        SortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                        Dictionary<string, string> Max_ten = new Dictionary<string, string>();

                        for (int i = 0; i < 10; i++)
                        {
                            seriesC.Points.AddXY(SortedList[0].Key, SortedList[0].Value);
                            Max_ten[SortedList[0].Key] = EVE_DateToPort[SortedList[0].Key];
                            SortedList.RemoveAt(0);
                        }
                        ChartForm chartForm = new ChartForm();
                        tabPage6.Controls.Add(chartForm.DrawChart1(seriesC, Max_ten));
                    }
                    
                    break;

                case 3:
                    {
                        Series seriesA = new Series("陽明");
                        seriesA.ChartType = SeriesChartType.Column;
                        seriesA.LegendText = "陽明";
                        seriesA.Color = Color.Green;

                        List<Series> list = new List<Series>();

                        var SortedList = YM_ShipToFreq.ToList();
                        List<Pair<string, int>>[] sortDateList = new List<Pair<string, int>>[10];

                        SortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        for (int i = 0; i < 10; i++)
                        {
                            seriesA.Points.AddXY(SortedList[0].Key, SortedList[0].Value);
                            list.Add(new Series(SortedList[0].Key));//bug
                            list[i].LegendText = SortedList[0].Key;
                            list[i].ChartType = SeriesChartType.Line;
                            list[i].BorderWidth = 3;
                            SortedList.RemoveAt(0);
                            sortDateList[i] = new List<Pair<string, int>>();
                        }


                        foreach (var ship in YM_ShipToDate)
                        {
                            int index = -1;
                            for (int j = 0; j < 10; j++)
                            {
                                if (list[j].LegendText == ship.Key)// if ship name is in top-10 ship
                                {
                                    index = j;
                                    break;
                                }
                            }
                            if (index >= 0)
                            {
                                foreach (var ele in YM_ShipToDate[ship.Key])
                                {
                                    //list[index].Points.AddXY(ele.Key, ele.Value);
                                    sortDateList[index].Add(new Pair<string, int>(ele.Key, ele.Value));
                                }
                            }
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            IComparer<Pair<string, int>> yearMonthComparer = new YearMonthComparer();
                            sortDateList[i].Sort(yearMonthComparer);
                            foreach (var ele in sortDateList[i])
                            {
                                list[i].Points.AddXY(ele.First, ele.Second);
                            }
                        }
                        ChartForm chartForm = new ChartForm();
                        tabPage7.Controls.Add(chartForm.DrawChart2(seriesA, list));
                        
                    }
                    break;

                case 4:
                    {
                        Series seriesA = new Series("萬海");
                        seriesA.ChartType = SeriesChartType.Column;
                        seriesA.LegendText = "萬海";
                        seriesA.Color = Color.Green;

                        List<Series> list = new List<Series>();

                        var SortedList = WH_ShipToFreq.ToList();
                        List<Pair<string, int>>[] sortDateList = new List<Pair<string, int>>[10];

                        SortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        for (int i = 0; i < 10; i++)
                        {
                            seriesA.Points.AddXY(SortedList[0].Key, SortedList[0].Value);
                            list.Add(new Series(SortedList[0].Key));//bug
                            list[i].LegendText = SortedList[0].Key;
                            list[i].ChartType = SeriesChartType.Line;
                            list[i].BorderWidth = 3;
                            SortedList.RemoveAt(0);
                            sortDateList[i] = new List<Pair<string, int>>();
                        }


                        foreach (var ship in WH_ShipToDate)
                        {
                            int index = -1;
                            for (int j = 0; j < 10; j++)
                            {
                                if (list[j].LegendText == ship.Key)// if ship name is in top-10 ship
                                {
                                    index = j;
                                    break;
                                }
                            }
                            if (index >= 0)
                            {
                                foreach (var ele in WH_ShipToDate[ship.Key])
                                {
                                    //list[index].Points.AddXY(ele.Key, ele.Value);
                                    sortDateList[index].Add(new Pair<string, int>(ele.Key, ele.Value));
                                }
                            }
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            IComparer<Pair<string, int>> yearMonthComparer = new YearMonthComparer();
                            sortDateList[i].Sort(yearMonthComparer);
                            foreach (var ele in sortDateList[i])
                            {
                                list[i].Points.AddXY(ele.First, ele.Second);
                            }
                        }
                        ChartForm chartForm = new ChartForm();
                        tabPage4.Controls.Add(chartForm.DrawChart2(seriesA, list));

                    }
                    break;

                case 5:
                    {
                        Series seriesA = new Series("長榮");
                        seriesA.ChartType = SeriesChartType.Column;
                        seriesA.LegendText = "長榮";
                        seriesA.Color = Color.Green;

                        List<Series> list = new List<Series>();

                        var SortedList = EVE_ShipToFreq.ToList();
                        List<Pair<string, int>>[] sortDateList = new List<Pair<string, int>>[10];

                        SortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        for (int i = 0; i < 10; i++)
                        {
                            seriesA.Points.AddXY(SortedList[0].Key, SortedList[0].Value);
                            list.Add(new Series(SortedList[0].Key));//bug
                            list[i].LegendText = SortedList[0].Key;
                            list[i].ChartType = SeriesChartType.Line;
                            list[i].BorderWidth = 3;
                            SortedList.RemoveAt(0);
                            sortDateList[i] = new List<Pair<string, int>>();
                        }


                        foreach (var ship in EVE_ShipToDate)
                        {
                            int index = -1;
                            for (int j = 0; j < 10; j++)
                            {
                                if (list[j].LegendText == ship.Key)// if ship name is in top-10 ship
                                {
                                    index = j;
                                    break;
                                }
                            }
                            if (index >= 0)
                            {
                                foreach (var ele in EVE_ShipToDate[ship.Key])
                                {
                                    //list[index].Points.AddXY(ele.Key, ele.Value);
                                    sortDateList[index].Add(new Pair<string, int>(ele.Key, ele.Value));
                                }
                            }
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            IComparer<Pair<string, int>> yearMonthComparer = new YearMonthComparer();
                            sortDateList[i].Sort(yearMonthComparer);
                            foreach (var ele in sortDateList[i])
                            {
                                list[i].Points.AddXY(ele.First, ele.Second);
                            }
                        }

                        ChartForm chartForm = new ChartForm();
                        tabPage8.Controls.Add(chartForm.DrawChart2(seriesA, list));
                    }
                    break;

                case 6:
                    {
                        ChartForm chartForm = new ChartForm();
                        tabPage9.Controls.Add(chartForm.DrawChart3(container));
                    }
                    break;

                case 7:
                    {
                        ChartForm chartForm = new ChartForm();
                        List<Series> list = new List<Series>();
                        list.Add(CompanyStock[0][7]);
                        list.Add(CompanyStock[0][8]);
                        list.Add(CompanyStock[0][9]);
                        tabPage10.Controls.Add(
                                        chartForm.DrawChart4(CompanyStock[0][0], CompanyStock[0][1], CompanyStock[0][2],
                                        CompanyStock[0][3], list));
                    }
                    break;

                case 8:
                    {
                        ChartForm chartForm = new ChartForm();
                        List<Series> list1 = new List<Series>();
                        Series s1 = new Series();
                        Series s2 = new Series();
                        Series s3 = new Series();
                        s1.ChartType = SeriesChartType.Line;
                        s2.ChartType = SeriesChartType.Line;
                        s3.ChartType = SeriesChartType.Line;
                        s1.Color = Color.Gold;
                        s2.Color = Color.Orange;
                        s3.Color = Color.Silver;
                        s1.LegendText = "上軌";
                        s2.LegendText = "中軌";
                        s3.LegendText = "下軌";
                        foreach (var point in CompanyStock[0][7].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s1.Points.Add(D);

                        }
                        foreach (var point in CompanyStock[0][8].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s2.Points.Add(D);
                        }
                        foreach (var point in CompanyStock[0][9].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s3.Points.Add(D);
                        }
                        list1.Add(s1);
                        list1.Add(s2);
                        list1.Add(s3);
                        List<Series> list2 = new List<Series>();
                        list2.Add(CompanyStock[0][4]);
                        list2.Add(CompanyStock[0][5]);
                        list2.Add(CompanyStock[0][6]);
                        tabPage11.Controls.Add(chartForm.DrawChart5(list1, list2));
                    }
                    break;

                case 9:
                    {
                        ChartForm chartForm = new ChartForm();
                        List<Series> list = new List<Series>();
                        list.Add(CompanyStock[1][7]);
                        list.Add(CompanyStock[1][8]);
                        list.Add(CompanyStock[1][9]);
                        tabPage12.Controls.Add(
                                        chartForm.DrawChart4(CompanyStock[1][0], CompanyStock[1][1], CompanyStock[1][2],
                                        CompanyStock[1][3], list));
                    }
                    break;

                case 10:
                    {
                        ChartForm chartForm = new ChartForm();
                        List<Series> list1 = new List<Series>();
                        Series s1 = new Series();
                        Series s2 = new Series();
                        Series s3 = new Series();
                        s1.ChartType = SeriesChartType.Line;
                        s2.ChartType = SeriesChartType.Line;
                        s3.ChartType = SeriesChartType.Line;
                        s1.Color = Color.Gold;
                        s2.Color = Color.Orange;
                        s3.Color = Color.Silver;
                        s1.LegendText = "上軌";
                        s2.LegendText = "中軌";
                        s3.LegendText = "下軌";
                        foreach (var point in CompanyStock[1][7].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s1.Points.Add(D);

                        }
                        foreach (var point in CompanyStock[1][8].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s2.Points.Add(D);
                        }
                        foreach (var point in CompanyStock[1][9].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s3.Points.Add(D);
                        }
                        list1.Add(s1);
                        list1.Add(s2);
                        list1.Add(s3);
                        List<Series> list2 = new List<Series>();
                        list2.Add(CompanyStock[1][4]);
                        list2.Add(CompanyStock[1][5]);
                        list2.Add(CompanyStock[1][6]);
                        tabPage13.Controls.Add(chartForm.DrawChart5(list1, list2));
                    }
                    break;

                case 11:
                    {
                        ChartForm chartForm = new ChartForm();
                        List<Series> list = new List<Series>();
                        list.Add(CompanyStock[2][7]);
                        list.Add(CompanyStock[2][8]);
                        list.Add(CompanyStock[2][9]);
                        tabPage14.Controls.Add(
                                        chartForm.DrawChart4(CompanyStock[2][0], CompanyStock[2][1], CompanyStock[2][2],
                                        CompanyStock[2][3], list));
                    }
                    break;

                case 12:
                    {
                        ChartForm chartForm = new ChartForm();
                        List<Series> list1 = new List<Series>();
                        Series s1 = new Series();
                        Series s2 = new Series();
                        Series s3 = new Series();
                        s1.ChartType = SeriesChartType.Line;
                        s2.ChartType = SeriesChartType.Line;
                        s3.ChartType = SeriesChartType.Line;
                        s1.Color = Color.Gold;
                        s2.Color = Color.Orange;
                        s3.Color = Color.Silver;
                        s1.LegendText = "上軌";
                        s2.LegendText = "中軌";
                        s3.LegendText = "下軌";
                        foreach (var point in CompanyStock[2][7].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s1.Points.Add(D);

                        }
                        foreach (var point in CompanyStock[2][8].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s2.Points.Add(D);
                        }
                        foreach (var point in CompanyStock[2][9].Points)
                        {
                            DataPoint D = new DataPoint();
                            D.SetValueXY(point.XValue, point.YValues[0]);
                            DateTime date = DateTime.FromOADate(point.XValue);
                            D.AxisLabel = date.ToString("yyyy-MM-dd");
                            s3.Points.Add(D);
                        }
                        list1.Add(s1);
                        list1.Add(s2);
                        list1.Add(s3);
                        List<Series> list2 = new List<Series>();
                        list2.Add(CompanyStock[2][4]);
                        list2.Add(CompanyStock[2][5]);
                        list2.Add(CompanyStock[2][6]);
                        tabPage15.Controls.Add(chartForm.DrawChart5(list1, list2));
                    }
                    break;
            }

           
        }
    }
    


}
