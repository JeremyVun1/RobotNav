using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public abstract class SearchStrategy
	{
		public FMap fMap { get; private set; }
		protected string id;
		protected Dictionary<Point, bool> closedSet;
		public List<Point> Path { get; private set; }

		public virtual UInt32 NumberOfNodes {
			get { return (UInt32)(closedSet.Where(x => x.Value == true).Count()); }
		}

		protected bool paused;
		public int gridW { get; private set; }
		public int gridH { get; private set; }
		protected int stepCount;

		public virtual string PathString
		{
			get
			{
				string result = "";

				if (Path.Count() == 0)
					return "No solution found";

				Path.Reverse();
				Point last = Path[0];
				for (int i=1; i<Path.Count; i++)
				{
					Point curr = Path[i];
					//no change
					if (curr.Equals(last))
					{
						result += "NoOp; ";
						last = curr;
						continue;
					}

					int dx = curr.X - last.X;
					int dy = curr.Y - last.Y;

					if (dx < 0)
					{
						int n = Math.Abs(dx);
						do
						{
							result += "Left; ";
							n--;
						} while (n > 0);
						last = curr;
						continue;
					}
					else if (dx > 0)
					{
						int n = Math.Abs(dx);
						do
						{
							result += "Right; ";
							n--;
						} while (n > 0);
						last = curr;
						continue;
					}
					else if (dy < 0)
					{
						int n = Math.Abs(dy);
						do
						{
							result += "Up; ";
							n--;
						} while (n > 0);
						last = curr;
						continue;
					}
					else if (dy > 0)
					{
						int n = Math.Abs(dy);
						do
						{
							result += "Down; ";
							n--;
						} while (n > 0);
						last = curr;
						continue;
					}
				}

				return result;
			}
		}

		protected virtual int pathSize
		{
			get
			{
				return Path.Count()-1;
			}
		}

		protected Stopwatch sw;

		protected SearchStrategy(FMap fMap, string id)
		{
			this.id = id;
			this.fMap = fMap;

			closedSet = new Dictionary<Point, bool>();
			for (int i = 0; i < fMap.Width; i++)
			{
				for (int j = 0; j < fMap.Height; j++)
				{
					closedSet[new Point(i, j)] = false;
				}
			}

			Path = new List<Point>();
			paused = true;

			gridW = SwinGame.ScreenWidth() / fMap.Width;
			gridH = SwinGame.ScreenHeight()-200 / fMap.Height;
			gridW = gridH = gridW < gridH ? gridW : gridH;

			sw = new Stopwatch();
			sw.Reset();
		}

		public virtual bool Update()
		{
			if (fMap == null || paused)
				return false;
			return true;
		}

		protected void FlushToClosedSet(List<Point> list)
		{
			foreach(Point p in list)
				closedSet[p] = true;

			list.Clear();
		}

		public void PrintMap()
		{
			if (fMap == null)
				return;
		}

		public virtual void TogglePause()
		{
			paused = !paused;
		}

		public virtual void Start()
		{
			if (paused)
			{
				Point start = fMap.Start;
				closedSet[start] = true;
				fMap[start] = 0;
				stepCount = 1;

				paused = false;
			}
		}

		public virtual void Draw()
		{
			if (!DebugMode.Draw)
				return;

			DrawGrid();
			DrawExplored();
			DrawPath();
			DrawStartGoals();
			DrawGridScores();
			DrawUI();
		}

		protected void DrawGrid()
		{
			if (DebugMode.Grids)
			{
				for (int i = 0; i < fMap.Width; i++)
				{
					int[] row = fMap[i];
					for (int j = 0; j < row.Length; j++)
					{
						DrawGridBox(new Point(i, j), gridColor(row[j]), Color.Black);
					}
				}
			}
		}

		protected void DrawExplored()
		{
			if (DebugMode.Explored)
			{
				foreach (KeyValuePair<Point, bool> kv in closedSet)
				{
					if (kv.Value)
					{
						DrawGridBox(kv.Key, Color.Yellow, 1);
					}
				}
			}
		}

		protected void DrawStartGoals()
		{
			if (DebugMode.Start)
			{
				DrawGridBox(fMap.Start, Color.Red, 1);
			}

			if (DebugMode.Goals)
			{
				for (int i = 0; i < fMap.Goals.Count(); i++)
				{
					DrawGridBox(fMap.Goals[i], Color.Green, 1);
				}
			}
		}

		protected virtual void DrawPath()
		{
			if (DebugMode.Path)
			{
				for (int i = 0; i < Path.Count(); i++)
				{
					DrawGridBox(Path[i], Color.LightGreen, 1);

					//draw parent lines
					if (i > 0)
						SwinGame.DrawLine(Color.Red, Path[i].X * gridW + gridW / 2, Path[i].Y * gridH + gridH / 2, Path[i - 1].X * gridW + gridW / 2, Path[i - 1].Y * gridH + gridH / 2);
				}
			}
		}

		protected virtual void DrawGridScores()
		{
			if (DebugMode.TotalGridScore)
			{
				foreach (KeyValuePair<Point, bool> kv in closedSet)
				{
					if (kv.Value)
					{
						Point p = kv.Key;
						SwinGame.DrawText(fMap[p].ToString(), Color.Black, p.X * gridW + gridW / 2 - 5, p.Y * gridH + gridH / 2 - 5);
					}
				}
			}
		}

		protected void DrawUI()
		{
			//build ui co-ords
			int uiTop = fMap.Height * gridH;
			int[] col = new int[7];
			for (int i = 0; i < col.Length; i++)
				col[i] = ((SwinGame.ScreenWidth() / 7) * i) + 20;


			SwinGame.DrawRectangle(Color.Grey, 0 + 2, uiTop + 2, SwinGame.ScreenWidth() - 4, 155 - 4);

			DrawMetrics(uiTop, col);

			DrawCommands(uiTop, col);
		}

		protected virtual void DrawMetrics(int uiTop, int[] col)
		{
			SwinGame.DrawText("Algorithm: " + id, Color.White, col[0], uiTop + 20);
			SwinGame.DrawText("Nodes explored: " + closedSet.Count(x => x.Value) + " / " + fMap.Width * fMap.Height, Color.White, col[0], uiTop + 35);
			SwinGame.DrawText("Steps: " + stepCount, Color.White, col[0], uiTop + 50);
			SwinGame.DrawText("Path Size: " + pathSize, Color.White, col[0], uiTop + 65);

			SwinGame.DrawText("Algorithm time (ms): " + sw.Elapsed.TotalMilliseconds, Color.White, col[0], uiTop + 80);
		}

		protected void DrawCommands(int uiTop, int[] col)
		{
			SwinGame.DrawText("Select an algorithm", Color.White, col[3], uiTop + 15);
			SwinGame.DrawText("[1] BFS", Color.White, col[3], uiTop + 30);
			SwinGame.DrawText("[2] DFS", Color.White, col[3], uiTop + 45);
			SwinGame.DrawText("[3] GBFS", Color.White, col[3], uiTop + 60);
			SwinGame.DrawText("[4] A*", Color.White, col[3], uiTop + 75);
			SwinGame.DrawText("[5] A* Fast Stack", Color.White, col[3], uiTop + 90);
			SwinGame.DrawText("[6] JPS", Color.White, col[3], uiTop + 105);
			SwinGame.DrawText("[7] GA", Color.White, col[3], uiTop + 120);

			//Commands
			SwinGame.DrawText("Commands", Color.White, col[5], uiTop + 20);
			SwinGame.DrawText("[R] Reset", Color.White, col[5], uiTop + 35);
			SwinGame.DrawText("[Space] Start", Color.White, col[5], uiTop + 50);
			SwinGame.DrawText("[Esc] Exit", Color.White, col[5], uiTop + 65);
		}

		private Color gridColor(int x)
		{
			switch (x)
			{
				case -1:
					return Color.Grey;
				case 0:
				default:
					return Color.White;
			}
		}

		protected void DrawGridBox(Point p, Color fill, int trim = 0)
		{
			SwinGame.FillRectangle(fill, p.X * gridW+trim, p.Y * gridH+trim, gridW-(trim*2), gridH-(trim*2));
		}

		protected void DrawGridBox(Point p, Color fill, Color border)
		{
			SwinGame.DrawRectangle(border, p.X * gridW, p.Y * gridH, gridW, gridH);
			DrawGridBox(p, fill, 1);
		}
	}
}
