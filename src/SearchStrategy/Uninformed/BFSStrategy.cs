using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SwinGameSDK;
using System.Diagnostics;

namespace RobotNav
{
	public class BFSStrategy : SearchStrategy
	{
		private List<Point> openSet;
		private Dictionary<Point, Point> parent;

		public BFSStrategy(FMap fMap, string id) : base(fMap, id)
		{
			openSet = new List<Point>();
			parent = new Dictionary<Point, Point>();
		}

		public override bool Update()
		{
			if (!base.Update())
				return false;

			if (openSet.Count == 0)
				return false;

			//do work if things in open set
			while (openSet.Count() != 0)
			{
				List<Point> nextSet = new List<Point>();
				for (int i = 0; i < openSet.Count(); i++)
				{
					sw.Start();
					Point p = openSet[i];
					List<Point> adj = fMap.Adjacent(p);
					
					foreach (Point a in adj)
					{
						if (closedSet[a])
							continue;

						parent.Add(a, p);
						closedSet[a] = true;
						nextSet.Add(a);

						//record g cost
						fMap[a] = fMap[p]+1;

						//check goal
						if (CheckIfGoal(a))
						{
							sw.Stop();
							openSet.Clear();
							return true;
						}
					}
					sw.Stop();
				}
				openSet = nextSet;
				stepCount++;

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

		private bool CheckIfGoal(Point a)
		{
			foreach (Point g in fMap.Goals)
			{
				if (a.Equals(g))
				{
					BuildPath(g);
					return true;
				}
			}
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

		public override void Start()
		{
			if (paused)
			{
				base.Start();
				openSet.Clear();
				openSet.Add(fMap.Start);
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
