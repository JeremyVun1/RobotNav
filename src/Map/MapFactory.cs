using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;
using System.IO;

namespace RobotNav
{
	public static class MapFactory
	{
		public static FMap CreateFMap(string filename)
		{
			//Check that file exists
			string filePath = filename;
			if (!File.Exists(filePath))
			{
				Console.WriteLine("Cannot find " + filePath);
				return new FMap(null, new Point(0, 0), null);
			}

			//Load file and create [][] array representation
			StreamReader file = new StreamReader(filePath);

			Point dimensions = ParsePoint(file.ReadLine());
			Point start = ParsePoint(file.ReadLine());
			List<Point> goals = ParsePoints(file.ReadLine());

			List<Rectangle> walls = new List<Rectangle>();
			string buffer;
			while ((buffer = file.ReadLine()) != null)
			{
				walls.Add(ParseRectangle(buffer));
			}

			int[][] terrain = new int[dimensions.Y][];
			for (int i = 0; i < dimensions.Y; i++)
			{
				terrain[i] = new int[dimensions.X];
			}

			foreach (Rectangle wall in walls)
			{
				for (int i = 0; i < wall.Width; i++)
				{
					for (int j = 0; j < wall.Height; j++)
					{
						terrain[i + (int)wall.X][j + (int)wall.Y] = -1;
					}
				}
			}

			file.Close();
			return new FMap(terrain, start, goals);
		}

		private static Point ParsePoint(string point)
		{
			string[] xy = point.Trim(' ','[', ']', '(', ')').Split(',');
			return new Point(int.Parse(xy[0]), int.Parse(xy[1]));
		}

		private static List<Point> ParsePoints(string points)
		{
			List<Point> result = new List<Point>();
			foreach(string p in points.Split('|'))
			{
				result.Add(ParsePoint(p));
			}
			return result;
		}

		private static Rectangle ParseRectangle(string rect)
		{
			string[] xywh = rect.Trim('(', ')', ' ').Split(',');
			return SwinGame.CreateRectangle(int.Parse(xywh[0]), int.Parse(xywh[1]), int.Parse(xywh[2]), int.Parse(xywh[3]));
		}
	}
}