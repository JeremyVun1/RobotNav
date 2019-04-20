using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public class ASFSStrategy : SearchStrategy
	{
		private List<Point> openSet;
		private List<Point> fastStack;
		private Dictionary<Point, Point> parent;
		private FMap gMap;

		public ASFSStrategy(FMap fMap, FMap gMap, string id) : base(fMap, id)
		{
			openSet = new List<Point>();
			fastStack = new List<Point>();
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
			//guards
			if (!base.Update())
				return false;
			if (fastStack.Count() == 0 && openSet.Count() == 0)
				return false;

			sw.Start();

			//algo start
			while (openSet.Count() != 0 || fastStack.Count() != 0)
			{
				stepCount++;
				Point lowPoint;

				//first look in fast stack, then open set
				if (fastStack.Count() != 0)
					lowPoint = GetBestNode(fastStack);
				else lowPoint = GetBestNode(openSet);

				//check if goal
				foreach (Point g in fMap.Goals)
				{
					if (lowPoint.Equals(g))
					{
						sw.Stop();
						openSet.Clear();
						fastStack.Clear();
						BuildPath(g);
						return true;
					}
				}

				//not goal, keep exploring adjacents/successors
				List<Point> adj = fMap.Adjacent(lowPoint);
				foreach (Point a in adj)
				{
					//update g scores if better g score is found
					if (gMap[a] > gMap[lowPoint] + 1)
						gMap[a] = gMap[lowPoint] + 1;

					//check redundancies
					if (closedSet[a])
						continue;

					//f = g + h;
					gMap[a] = gMap[lowPoint] + 1;
					fMap[a] = gMap[a] + ManhattanDist(a, fMap.Goals);

					//fast stacking
					if (fMap[a] <= fMap[lowPoint])
						fastStack.Add(a);
					else openSet.Add(a);

					closedSet[a] = true;
					parent.Add(a, lowPoint);
				}

				//draw to the screen during loop
				sw.Stop();
				SwinGame.ClearScreen(Color.Black);
				Draw();
				SwinGame.RefreshScreen();
				sw.Start();
			}
			sw.Stop();
			return false;
		}

		//get node with lowest f value from the list
		private Point GetBestNode(List<Point> list)
		{
			Point lowPoint = list[0];
			int lowDist = fMap[lowPoint];
			for (int i = 0; i < list.Count(); i++)
			{
				Point lowPoint2 = list[i];
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

			list.Remove(lowPoint);

			return lowPoint;
		}

		//lowest manhattan dist to set of points (multiple active goals)
		private int ManhattanDist(Point a, List<Point> b)
		{
			int mDist = (Math.Abs(a.Y - b[0].Y) + Math.Abs(a.X - b[0].X));
			for (int i = 1; i < b.Count(); i++)
			{
				int mDist2 = (Math.Abs(a.Y - b[i].Y) + Math.Abs(a.X - b[i].X));
				mDist = mDist2 < mDist ? mDist2 : mDist;
			}

			return mDist;
		}

		//manhattan dist between two points
		private int ManhattanDist(Point a, Point b)
		{
			return (Math.Abs(a.Y - b.Y) + Math.Abs(a.X - b.X));
		}

		//return best path to goal by unrolling parents
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

		//GUI DRAWS
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
			
			DrawUI();

			DrawAllParents();
		}

		private void DrawOpenSet()
		{
			for(int i=0; i<openSet.Count(); i++)
			{
				DrawGridBox(openSet[i], Color.LightBlue, 1);
			}
			for (int i = 0; i < fastStack.Count(); i++)
			{
				DrawGridBox(fastStack[i], Color.DeepSkyBlue, 1);
			}

			SwinGame.DrawText("FastStack: " + fastStack.Count(), Color.White, 20, SwinGame.ScreenHeight() - 20);
		}

		private void DrawAllParents()
		{
			foreach (KeyValuePair<Point, Point> pair in parent)
			{
				Point child = pair.Key;
				Point parent = pair.Value;
				//draw line direction
				SwinGame.DrawLine(Color.Red, child.X * gridW + gridW / 2, child.Y * gridH + gridH / 2, parent.X * gridW + gridW / 2, parent.Y * gridH + gridH / 2);
			}
		}
	}
}
