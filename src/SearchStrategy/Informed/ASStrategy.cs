using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public class ASStrategy : SearchStrategy
	{
		private List<Point> openSet;
		private Dictionary<Point, Point> parent;
		private FMap gMap;


		public ASStrategy(FMap fMap, FMap gMap, string id) : base(fMap, id)
		{
			openSet = new List<Point>();
			parent = new Dictionary<Point, Point>();
			this.gMap = gMap;
		}

		public override void Start()
		{
			base.Start();
			openSet.Clear();
			openSet.Add(fMap.Start);
			stepCount = 0;
			fMap[fMap.Start] = ManhattanDist(fMap.Start, fMap.Goals);
			gMap[fMap.Start] = 0;
		}

		public override bool Update()
		{
			if (!base.Update())
				return false;

			if (openSet.Count() == 0)
				return false;

			sw.Start();

			while (openSet.Count() != 0)
			{
				//find best node in open set
				stepCount++;
				Point lowPoint = openSet[0];
				int lowDist = fMap[lowPoint];
				for (int i = 0; i < openSet.Count(); i++)
				{
					Point lowPoint2 = openSet[i];
					int lowDist2 = fMap[lowPoint2];

					if (lowDist2 < lowDist)
					{
						lowPoint = lowPoint2;
						lowDist = lowDist2;
					}

					//if two nodes have same f cost, choose the one with lowest h cost
					if (lowDist == lowDist2)
					{
						if ((fMap[lowPoint] - gMap[lowPoint]) > (fMap[lowPoint2] - gMap[lowPoint2]))
						{
							lowPoint = lowPoint2;
						}
					}
				}

				//check if goal
				foreach (Point g in fMap.Goals)
				{
					if (lowPoint.Equals(g))
					{
						sw.Stop();
						openSet.Clear();
						BuildPath(g);
						return true;
					}
				}

				//not goal, keep exploring adjacents
				openSet.Remove(lowPoint);
				List<Point> adj = fMap.Adjacent(lowPoint);
				foreach (Point a in adj)
				{
					//update g scores if found better path the neighbour
					if (gMap[a] > gMap[lowPoint] + 1)
						gMap[a] = gMap[lowPoint] + 1;

					if (closedSet[a])
						continue;

					//f = g + h;
					gMap[a] = gMap[lowPoint] + 1;
					fMap[a] = gMap[a] + ManhattanDist(a, fMap.Goals);
					openSet.Add(a);
					closedSet[a] = true;
					parent.Add(a, lowPoint);
				}

				//draw screen during recursion
				sw.Stop();
				SwinGame.ClearScreen(Color.Black);
				Draw();
				SwinGame.RefreshScreen();
				sw.Start();
				//return false;
			}
			sw.Stop();
			return false;
		}

		private int ManhattanDist(Point a, List<Point> b)
		{
			//find manhattan dist to closest goal
			int mDist = (Math.Abs(a.Y - b[0].Y) + Math.Abs(a.X - b[0].X));
			for (int i = 1; i < b.Count(); i++)
			{
				int mDist2 = (Math.Abs(a.Y - b[i].Y) + Math.Abs(a.X - b[i].X));
				mDist = mDist2 < mDist ? mDist2 : mDist;
			}

			return mDist;
		}

		private int ManhattanDist(Point a, Point b)
		{
			return (Math.Abs(a.Y - b.Y) + Math.Abs(a.X - b.X));
		}

		private void BuildPath(Point c)
		{
			Path.Clear();
			Point p;

			Path.Add(c);
			while (parent.ContainsKey(c))
			{
				p = parent[c];
				Path.Add(p);
				c = p;
			}
		}

		public override void Draw()
		{
			if (!DebugMode.Draw)
				return;

			DrawGrid();
			DrawExplored();
			DrawOpenSet();
			DrawPath();
			DrawStartGoals();
			DrawGridScores();
			//DrawGCost();
			DrawUI();
		}

		private void DrawOpenSet()
		{
			for(int i=0; i<openSet.Count(); i++)
			{
				DrawGridBox(openSet[i], Color.LightBlue, 1);
			}
		}

		private void GCost()
		{
			if (!DebugMode.MoveCost)
				return;

			for (int i = 0; i < gMap.Width; i++)
			{
				for (int j = 0; j < gMap.Height; j++)
				{
					if (gMap[i][j] != 0)
						SwinGame.DrawText(gMap[i][j].ToString(), Color.Black, i * gridW + gridW / 2, j * gridH + gridH / 2);
				}
			}
		}
	}
}
