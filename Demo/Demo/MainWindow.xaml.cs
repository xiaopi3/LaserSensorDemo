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
using System.Net.Sockets;

using SCIP_library;

namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        NetworkStream stream = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void bt_Click_Init(object sender, RoutedEventArgs e)
        {
            string ip=IPAddr.Text.Trim();
            int port=int.Parse(Port.Text.Trim());
            
            TcpClient urg = new TcpClient();
            urg.Connect(ip, port);
            stream = urg.GetStream();
        }

        private void bt_Click_Start(object sender, RoutedEventArgs e)
        {
            int start_step;
            int end_step;
            int GET_NUM;
            try
            {
                write(stream, SCIP_Writer.SCIP2());// 切换到SCIP2.0状态
                read_line(stream); // ignore echo back 读一行，并抛弃返回值
                write(stream, SCIP_Writer.MD(start_step, end_step));//写入初始配置参数
                read_line(stream);  // ignore echo back 读一行，并抛弃返回值

                List<long> distances = new List<long>();
                long time_stamp = 0;// 时间戳-毫秒
                for (int i = 0; i < GET_NUM; ++i)
                {
                    string receive_data = read_line(stream);
                    if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances))
                    {
                        Console.WriteLine(receive_data);
                        break;// 此时收到的数据开头非MD且二位非00或99，则数据出错，输出数据到屏幕，并结束循环
                    }
                    if (distances.Count == 0)
                    {
                        Console.WriteLine(receive_data);
                        continue;// 同上，此时没有接收到距离数据，输出到屏幕，继续下次循环
                    }
                    // show distance data
                    Console.WriteLine("time stamp: " + time_stamp.ToString() + " distance[540] : " + distances[40].ToString());
                }
                write(stream, SCIP_Writer.QT());    // stop measurement mode 关闭激光，禁用测量状态
                read_line(stream); // ignore echo back
                stream.Close();
                urg.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("Press any key.");
                Console.ReadKey();
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
    }
}
