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
        public List<EnemyMovement> TimeLine { get; set; }

        public List<EnemyTemplate> AvailableEnemies { get; set; }

        // 坐标转换
        public Point GetPosition(Point point)
        {
            // 关卡转换数据（可作为关卡属性），这里用关卡0-1
            // 格子范围
            Point topLeft = new Point(251, 188);
            Point bottomLeft = new Point(55, 817);
            Point topRight = new Point(1233, 188);
            Point bottomRight = new Point(1549, 816);
            // y 轴分割比例，3D视角非线性，x 轴暂时视为均匀分布
            //int[] factors = { 98, 106, 128, 130, 161 };
            int[] factors = { 101, 123, 120, 126, 159 };
            // 计算累加比例
            List<int> weights = new List<int>();
            weights.Add(0);
            int sum = 0;
            foreach(int i in factors)
            {
                sum += i;
                weights.Add(sum);
            }

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
            leftPoint.X = topLeft.X * (1 - ratio) + bottomLeft.X * ratio;
            leftPoint.Y = topLeft.Y * (1 - ratio) + bottomLeft.Y * ratio;
            Point rightPoint = new Point();
            rightPoint.X = topRight.X * (1 - ratio) + bottomRight.X * ratio;
            rightPoint.Y = topRight.Y * (1 - ratio) + bottomRight.Y * ratio;

            // 暂设x轴方向格子分布均匀，直接插值
            Point position = new Point();
            position.X = leftPoint.X * (MapWidth - point.X) / MapWidth + rightPoint.X * point.X / MapWidth;
            position.Y = leftPoint.Y * (MapWidth - point.X) / MapWidth + rightPoint.Y * point.X / MapWidth;


            return position;
        }
    }
}
