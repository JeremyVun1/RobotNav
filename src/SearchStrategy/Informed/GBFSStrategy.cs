using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public class GBFSStrategy : SearchStrategy
	{
		private List<Point> openSet;
		private Dictionary<Point, Point> parent;

		public GBFSStrategy(FMap fMap, string id) : base(fMap, id)
		{
			openSet = new List<Point>();
			parent = new Dictionary<Point, Point>();
		}

		public override void Start()
		{
			if (paused)
			{
				base.Start();
				openSet.Clear();
				openSet.Add(fMap.Start);
				stepCount = 0;
			}
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
				stepCount++;
				//find best node in open set
				Point lowPoint = openSet[0];
				int lowDist = fMap[lowPoint];
				for (int i = 0; i < openSet.Count(); i++)
				{
					int lowDist2 = fMap[openSet[i]];
					if (lowDist2 < lowDist)
					{
						lowPoint = openSet[i];
						lowDist = lowDist2;
					}
				}

				//close and remove
				openSet.Remove(lowPoint);

				//explore adjacents
				List<Point> adj = fMap.Adjacent(lowPoint);
				foreach (Point a in adj)
				{
					if (closedSet[a])
						continue;

					parent.Add(a, lowPoint);
					closedSet[a] = true;

					//check goal
					foreach (Point g in fMap.Goals)
					{
						if (a.Equals(g))
						{
							fMap[a] = 0;
							openSet.Clear();
							BuildPath(g);
							sw.Stop();
							return true;
						}
					}

					fMap[a] = ManhattanDist(a);
					openSet.Add(a);
				}

				//draw screen during recursion
				sw.Stop();
				SwinGame.ClearScreen(Color.Black);
				Draw();
				SwinGame.RefreshScreen();

				sw.Start();
			}
			sw.Stop();
			return false;
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

		private int ManhattanDist(Point a)
		{
			//find manhattan dist to closest goal
			int mDist = (Math.Abs(a.Y - fMap.Goals[0].Y) + Math.Abs(a.X - fMap.Goals[0].X));
			for (int i = 1; i < fMap.Goals.Count(); i++)
			{
				int mDist2 = (Math.Abs(a.Y - fMap.Goals[i].Y) + Math.Abs(a.X - fMap.Goals[i].X));
				mDist = mDist2 < mDist ? mDist2 : mDist;
			}

			return mDist;
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
			DrawUI();
		}

		private void DrawOpenSet()
		{
			for (int i = 0; i < openSet.Count(); i++)
			{
				DrawGridBox(openSet[i], Color.LightBlue, 1);
			}
		}
	}
}
