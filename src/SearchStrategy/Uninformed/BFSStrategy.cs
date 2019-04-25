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
			//guards
			if (!base.Update())
				return false;
			if (openSet.Count == 0)
				return false;

			sw.Start();

			//start algorithm
			while (openSet.Count() != 0)
			{
				List<Point> nextSet = new List<Point>();
				for (int i = 0; i < openSet.Count(); i++)
				{
					Point p = openSet[i];

					//expand and get successors
					List<Point> adj = fMap.Adjacent(p);
					foreach (Point a in adj)
					{
						if (closedSet[a])
							continue;

						parent.Add(a, p); //add parent
						closedSet[a] = true; //add to closed set
						nextSet.Add(a); //add to next frontier
						
						fMap[a] = fMap[p]+1; //record g cost

						//goal check
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

				//draw screen during loop
				sw.Stop();
				SwinGame.ClearScreen(Color.Black);
				Draw();
				SwinGame.RefreshScreen();
				sw.Start();
			}
			sw.Stop();
			return false;
		}

		//goal check
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

		//build path by unrolling parents
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
			if (DebugMode.Openset)
			{
				for (int i = 0; i < openSet.Count(); i++)
				{
					DrawGridBox(openSet[i], Color.LightBlue, 1);
				}

				SwinGame.DrawText("OpenSet: " + openSet.Count(), Color.White, 20, SwinGame.ScreenHeight() - 20);
			}
		}

		private void DrawAllParents()
		{
			foreach(KeyValuePair<Point, Point> pair in parent)
			{
				Point child = pair.Key;
				Point parent = pair.Value;
				//draw line direction
				SwinGame.DrawLine(Color.Red, child.X * gridW + gridW / 2, child.Y * gridH + gridH / 2, parent.X * gridW + gridW / 2, parent.Y * gridH + gridH / 2);
			}
		}
	}
}
