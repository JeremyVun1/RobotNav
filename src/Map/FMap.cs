using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotNav
{
	public class FMap
	{
		private int[][] mapArr;
		public int Width { get { return mapArr.Length; } }
		public int Height { get { return Width == 0 ? 0 : mapArr[0].Length; } }

		//[x,y] operator overload to access data structure
		public int this[Point p] {
			get {
				if (p.X<0 || p.X>Width-1 || p.Y<0 || p.Y>Height-1)
					return -1;
				return mapArr[p.X][p.Y];
			}
			set {
				if (p.X<0 || p.X>Width-1 || p.Y<0 || p.Y>Height-1)
					return;
				mapArr[p.X][p.Y] = value;
			}
		}

		public int this[int x, int y]
		{
			get {
				if (x<0 || x>Width-1 || y<0 || y>Height-1)
					return -1;
				return mapArr[x][y];
			}
			set {
				if (x<0 || x>Width-1 || y<0 || y>Height-1)
					return;
				mapArr[x][y] = value;
			}
		}

		public int[] this[int x] {
			get {
				if (x<0 || x>Width-1)
					return null;
				return mapArr[x];
			}
			set {
				if (x<0 || x>Width-1)
					return;
				mapArr[x] = value;
			}
		}

		public Point Start;
		public List<Point> Goals;

		public FMap(int[][] map, Point start, List<Point> goals)
		{
			mapArr = map;
            Start = start;
			Goals = goals;
		}

		public List<Point> Adjacent(Point p)
		{
			List<Point> result = new List<Point>();
			
			//up
			if (p.Y-1 >= 0 && mapArr[p.X][p.Y-1] != -1)
				result.Add(new Point(p.X, p.Y-1));

			//left
			if (p.X-1 >= 0 && mapArr[p.X-1][p.Y] != -1)
				result.Add(new Point(p.X-1, p.Y));

			//down
			if (p.Y+1 < mapArr[p.X].Length && mapArr[p.X][p.Y+1] != -1)
				result.Add(new Point(p.X, p.Y+1));

			//right
			if (p.X+1 < mapArr.Length && mapArr[p.X+1][p.Y] != -1)
				result.Add(new Point(p.X+1, p.Y));

			return result;
		}

		public void Print()
		{
			for (int row = 0; row < mapArr[0].Length; row++)
			{
				for (int col = 0; col < mapArr.Length; col++)
				{
					Console.Write("[" + mapArr[col][row] + "] ");
				}
				Console.Write("\n");
			}
		}
	}
}
