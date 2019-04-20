using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public class JPSStrategy : SearchStrategy
	{
		private List<Point> openSet;
		private Dictionary<Point, Point> parent;
		private FMap gMap;

		protected override int pathSize
		{
			get
			{
				if (Path.Count() == 0)
					return 0;
				return gMap[Path[0]];
			}
		}

		public JPSStrategy(FMap fMap, FMap gMap, string id) : base(fMap, id)
		{
			openSet = new List<Point>();
			parent = new Dictionary<Point, Point>();

			this.gMap = gMap;
		}

		public override void Start()
		{
			base.Start();
			parent.Clear();

			openSet.Clear();
			openSet.Add(fMap.Start);
			
			fMap[fMap.Start] = ManhattanDist(fMap.Start, fMap.Goals);
			gMap[fMap.Start] = 0;

			stepCount = 0;
		}

		public override bool Update()
		{
			//guards
			if (!base.Update())
				return false;
			if (openSet.Count() == 0)
				return false;

			sw.Start();

			//algorithm start
			while (openSet.Count() != 0)
			{
				Point node = GetBestNode(openSet);
				closedSet[node] = true;

				//goal check
				if (CheckIfGoal(node))
					return true;

				//get adjacents/successors of the node
				List<Point> adj = PrunedNeighbours(node);
				Point jp = new Point(0, 0);
				for (int i = 0; i < adj.Count(); i++)
				{
					stepCount = 0;

					//scan for a jump point in the direction of node -> adjacent[i]
					jp = Scan(node, adj[i]);
					if (jp.X == -1 || closedSet[jp])
						continue;
					else openSet.Add(jp);

					//set f, g, h costs of our jump points. Where h is difference between f map and g map.
					int g = gMap[node] + ManhattanDist(jp, node);
					if (g < gMap[jp] || gMap[jp] == 0)
					{
						gMap[jp] = g;
						fMap[jp] = g + ManhattanDist(jp, fMap.Goals); //f = g + h
						parent[jp] = node;
					}

					//draw gui during loop
					sw.Stop();
					SwinGame.ClearScreen(Color.Black);
					Draw();
					DrawGridBox(jp, Color.Pink);
					SwinGame.RefreshScreen();
					sw.Start();
				}
			}
			sw.Stop();
			return false;
		}

		//recursive directional scan
		private Point Scan(Point p, Point c)
		{
			int dx = c.X - p.X;
			int dy = c.Y - p.Y;
			stepCount++;

			//goal check
			if (CheckIfGoal(c))
				return c;

			//wall check (no jump point)
			if (fMap[c] == -1 || c.X < 0 || c.X == fMap.Width || c.Y < 0 || c.Y == fMap.Height)
				return new Point(-1, -1);
			
			//forced neighbour case checks
			//horizontal direction forced neighbours
			if (dx != 0)
			{
				if (fMap[c.X, c.Y - 1] != -1 && fMap[c.X-dx, c.Y-1] == -1 ||
					fMap[c.X, c.Y + 1] != -1 && fMap[c.X-dx, c.Y+1] == -1)
				{
					return c;
				}
			}
			//vertical direction forced neighbours
			else if (dy != 0)
			{
				if (fMap[c.X-1, c.Y] != -1 && fMap[c.X-1, c.Y-dy] == -1 ||
					fMap[c.X+1, c.Y] != -1 && fMap[c.X+1, c.Y-dy] == -1)
				{
					return c;
				}
				//left and right scan
				Point right = Scan(c, new Point(c.X + 1, c.Y));
				Point left = Scan(c, new Point(c.X - 1, c.Y));
				if (left.X != -1 || right.X != -1)
					return c;
			}

			return Scan(c, new Point(c.X + dx, c.Y + dy));
		}

		//return list of neighbours in the direction that we are coming from
		private List<Point> PrunedNeighbours(Point x)
		{
			List<Point> result = new List<Point>();

			//if we have a parent, then we are moving in a direction relative to the parent
			if (parent.ContainsKey(x))
			{
				Point p = parent[x];
				int dx = (x.X - p.X) / Math.Max(Math.Abs(x.X - p.X), 1);
				int dy = (x.Y - p.Y) / Math.Max(Math.Abs(x.Y - p.Y), 1);

				if (dx != 0)
				{
					if (fMap[x.X, x.Y-1] != -1)
					{
						result.Add(new Point(x.X, x.Y - 1));
					}
					if (fMap[x.X, x.Y + 1] != -1)
					{
						result.Add(new Point(x.X, x.Y + 1));
					}
					if (fMap[x.X + dx, x.Y] != -1)
					{
						result.Add(new Point(x.X + dx, x.Y));
					}
				}
				else if (dy != 0)
				{
					if (fMap[x.X-1, x.Y] != -1)
					{
						result.Add(new Point(x.X-1, x.Y));
					}
					if (fMap[x.X+1, x.Y] != -1)
					{
						result.Add(new Point(x.X+1, x.Y));
					}
					if (fMap[x.X, x.Y+dy] != -1)
					{
						result.Add(new Point(x.X, x.Y + dy));
					}
				}
			}
			else
			{
				//no parent, which means we are at the starting node
				// return all adjacent nodes
				result = fMap.Adjacent(x);
			}

			return result;
		}

		//check if our point is a goal state
		private bool CheckIfGoal(Point p)
		{
			foreach (Point g in fMap.Goals)
			{
				if (p.Equals(g))
				{
					sw.Stop();

					//transfer to closed set to show jump points discovered
					foreach(Point o in openSet)
						closedSet[o] = true;

					openSet.Clear();
					BuildPath(g);
					return true;
				}
			}
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
			closedSet[lowPoint] = true;

			return lowPoint;
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

		//lowest manhattan dist to set of points (multiple active goals)
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

		//manhattan dist between two points
		private int ManhattanDist(Point a, Point b)
		{
			return (Math.Abs(a.Y - b.Y) + Math.Abs(a.X - b.X));
		}

		//GUI DRAWS
		protected override void DrawPath()
		{
			if (DebugMode.Path)
			{
				if (Path.Count() == 0)
					return;

				Point last = Path[0];
				DrawGridBox(Path[0], Color.LightGreen, 1);
				for (int i = 1; i < Path.Count(); i++)
				{
					Point curr = Path[i];
					DrawGridBox(curr, Color.LightGreen, 1);

					//draw line direction
					SwinGame.DrawLine(Color.Red, last.X*gridW + gridW/2, last.Y*gridH + gridH/2, curr.X*gridW + gridW/2, curr.Y*gridH + gridH/2);

					last = curr;
				}
			}
		}
	}
}
