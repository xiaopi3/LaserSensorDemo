using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace Demo
{
    public class Data
    {
        public int Index { get; set; }
        public long Time { get; set; }
        public double Dist { get; set; }

        public Data(int Index, long Time, double Dist)
        {
            this.Index = Index;
            this.Time = Time;
            this.Dist = Dist;
        }
        public static double getDist(int startStep,int j,long d)
        {
            int j_currentStep = 540 - (j + startStep);
            double j_currentAngle = j_currentStep * 0.25;
            double DL = d * Math.Cos(j_currentAngle*3.14159265/180);
            return DL;
        }
        public static double getXDist(int startStep, int j, long d)
        {
            int j_currentStep = 540 - (j + startStep);
            double j_currentAngle = j_currentStep * 0.25;
            double XDist = d * Math.Sin(j_currentAngle*3.14159265/180);
            return -XDist;
        }
        public static PointF getXY(double L_mid,int startStep,double[] dist,double[] XDist)
        {
            double delta = 50;//50mm
            int width=10;//宽度余量-step
            //先往右遍历
            for (int i = startStep; i < dist.Length-width-1; i++)
            {
                if (dist[i+1] - dist[i] > delta)
                {
                    if (eightInTen(dist, i + 1, 1))//简单健壮性判断
                    {
                        return new PointF((float)XDist[i + 1], (float)dist[i + 1]);
                    }
                }
            }
            //往左遍历
            for (int i = startStep; i > width + 1; i--)
            {
                if (dist[i] - dist[i - 1] > delta)
                {
                    if (eightInTen(dist, i - 1, -1))
                    {
                        return new PointF((float)XDist[i], (float)dist[i]);
                    }
                }
            }
            return new PointF(-1, -1);
        }
        private static bool eightInTen(double[] dist, int i, int flag)
        {
            double epsilon = 20;//20mm的误差
            int j = 0;
            int count = 0;
            if (flag > 0)
            {
                while (j < 10)
                {
                    if (Math.Abs(dist[i + j] - dist[i]) < epsilon)
                    {
                        count++;
                    }
                    j++;
                }
            }
            else
            {
                while (j < 10)
                {
                    if (Math.Abs(dist[i] - dist[i - j]) < epsilon)
                    {
                        count++;
                    }
                    j++;
                }
            }
            return count >= 8 ? true : false;
        }
    }
}
