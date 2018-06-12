using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SCIP_library;
using System.Net.Sockets;
using OxyPlot;
using OxyPlot.Series;
using System.Drawing;

namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        NetworkStream stream = null;
        TcpClient urg = null;
        List<Data> data = null;
        PlotModel SimplePlotModel = null;

        public MainWindow()
        {
            InitializeComponent();
            listViewData.ItemsSource = data;
        }

        private void bt_Click_Start(object sender, RoutedEventArgs e)
        {
            int start_step = 540 - (int)(slider1.Value / 0.25);
            int end_step = 540 + (int)(slider2.Value / 0.25);
            int GET_NUM = int.Parse(ScanNum.Text.Trim());

            SimplePlotModel = new PlotModel();
            //线条
            var lineSerial = new LineSeries() { Title = "距离" };
            
            //data.Clear();
            data = new System.Collections.Generic.List<Data>();

            try
            {
                string ip = IPAddr.Text.Trim();
                int port = int.Parse(Port.Text.Trim());
                urg = new TcpClient();
                urg.Connect(ip, port);
                stream = urg.GetStream();

                write(stream, SCIP_Writer.SCIP2());// 切换到SCIP2.0状态
                read_line(stream); // ignore echo back 读一行，并抛弃返回值
                write(stream, SCIP_Writer.MD(start_step, end_step));//写入初始配置参数
                read_line(stream);  // ignore echo back 读一行，并抛弃返回值

                List<long> distances = new List<long>();
                long time_stamp = 0;// 时间戳-毫秒
                for (int i = 0; i < GET_NUM; ++i)
                {
                    //每次循环清空图数据
                    SimplePlotModel.Series.Remove(lineSerial);
                    lineSerial.Points.Clear();
                    string receive_data = read_line(stream);
                    if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances))
                    {
                        MessageBox.Show("错误数据：" + receive_data);
                        
                        break;// 此时收到的数据开头非MD且二位非00或99，则数据出错，输出数据到屏幕，并结束循环
                    }
                    if (distances.Count == 0)
                    {
                        continue;// 同上，此时没有接收到距离数据，输出到屏幕，继续下次循环
                    }
                    //垂直距离
                    double[] dist = new double[distances.Count];
                    //水平距离
                    double[] Xdist = new double[distances.Count];
                    double L_mid=-1;
                    // 一次测量中的数据
                    for (int j = 0; j < distances.Count; j++)
                    {
                        //获取所有店的垂直距离
                        double d = Data.getDist(start_step,j, distances[j]);
                        data.Add(new Data(j, time_stamp, d));
                        lineSerial.Points.Add(new DataPoint(j, d));
                        //单独将垂直距离存到一个数组中
                        dist[j] = d;
                        Xdist[j] = Data.getXDist(start_step, j, distances[j]);
                        //获取中心点直线距离
                        if (j == (540-start_step))
                            L_mid = d;
                    }
                    if (L_mid != -1)
                    {
                        PointF p = Data.getXY(L_mid, start_step, dist, Xdist);
                        PointX.Text = p.X.ToString("F2");
                        PointY.Text = p.Y.ToString("F2");
                    }
                    
                    listViewData.Items.Refresh();
                    SimplePlotModel.Series.Add(lineSerial);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        static string read_line(NetworkStream stream)
        {
            if (stream.CanRead)
            {
                StringBuilder sb = new StringBuilder();
                bool is_NL2 = false;//指示循环结束
                bool is_NL = false;//指示数据是否结束符
                do
                {
                    char buf = (char)stream.ReadByte();
                    if (buf == '\n')
                    {
                        if (is_NL)
                        {
                            is_NL2 = true;
                        }
                        else
                        {
                            is_NL = true;
                        }
                    }
                    else
                    {
                        is_NL = false;
                    }
                    sb.Append(buf);
                } while (!is_NL2);

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// write data
        /// </summary>
        static bool write(NetworkStream stream, string data)
        {
            if (stream.CanWrite)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                stream.Write(buffer, 0, buffer.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void bt_Click_End(object sender, RoutedEventArgs e)
        {
            try
            {
                write(stream, SCIP_Writer.QT());    // stop measurement mode 关闭激光，禁用测量状态
                read_line(stream); // ignore echo back
                stream.Close();
                urg.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        
    }
}
