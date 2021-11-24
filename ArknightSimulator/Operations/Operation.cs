using System;
using System.Collections.Generic;
using System.Text;
using ArknightSimulator.Enemies;

namespace ArknightSimulator.Operations
{
    public class Operation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public PointType[][] Map { get; set; }
        public int DeploymentLimit { get; set; }
        public int InitialCost { get; set; }
        public int MaxCost { get; set; }
        public int HomeLife { get; set; }
        public int EnemyCount { get; set; }

        // 关卡转换数据(格子范围)
        public Point TopLeft { get; set; }
        public Point BottomLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomRight { get; set; }

        // y 轴分割比例，3D视角非线性，x 轴暂时视为均匀分布
        public int[] Factors { get; set; }

        public List<EnemyMovement> TimeLine { get; set; }

        public List<EnemyTemplate> AvailableEnemies { get; set; }



        // 坐标转换（格子坐标到实际坐标）
        public Point GetPosition(Point point)
        {
            // 计算累加比例
            List<int> weights = new List<int>();
            weights.Add(0);
            int sum = 0;
            foreach(int i in Factors)
            {
                sum += i;
                weights.Add(sum);
            }
            weights.Add(0);

            //point = new Point(3,2);

            // 计算上下格点
            int ceil = (int)Math.Ceiling(point.Y);
            int floor = (int)Math.Floor(point.Y);
            if (ceil == floor)
                ceil += 1;

            // 根据上下格点，插值计算该点在y轴上的比例
            double ratio = weights[ceil] * (point.Y - floor) + weights[floor] * (ceil - point.Y);
            ratio /= sum;

            // 根据格子范围四点，插值计算水平线左右两点（若格子x轴一定水平，Y计算也可只计算一次）
            Point leftPoint = new Point();
            leftPoint.X = TopLeft.X * (1 - ratio) + BottomLeft.X * ratio;
            leftPoint.Y = TopLeft.Y * (1 - ratio) + BottomLeft.Y * ratio;
            Point rightPoint = new Point();
            rightPoint.X = TopRight.X * (1 - ratio) + BottomRight.X * ratio;
            rightPoint.Y = TopRight.Y * (1 - ratio) + BottomRight.Y * ratio;

            // 暂设x轴方向格子分布均匀，直接插值
            Point position = new Point();
            position.X = leftPoint.X * (MapWidth - point.X) / MapWidth + rightPoint.X * point.X / MapWidth;
            position.Y = leftPoint.Y * (MapWidth - point.X) / MapWidth + rightPoint.Y * point.X / MapWidth;


            return position;
        }



    }
}
