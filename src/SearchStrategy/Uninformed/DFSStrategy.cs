using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public class DFSStrategy : SearchStrategy
	{
		private Stack<Point> stack;
		bool validAdjFound;

		public DFSStrategy(FMap fMap, string id) : base(fMap, id)
		{
			stack = new Stack<Point>();
		}

		public override void Start()
		{
			if (paused)
			{
				base.Start();
				stack.Clear();
				stack.Push(fMap.Start);
			}
		}

		public override bool Update()
		{
			//guards
			if (!base.Update())
				return false;
			if (stack.Count == 0)
				return false;

			sw.Start();

			//Start algorithm
			while (stack.Count() != 0)
			{
				Point p = stack.Peek();
				List<Point> adj = fMap.Adjacent(p);
				validAdjFound = false;

				//go deeper on first adjacent node
				foreach (Point a in adj)
				{
					if (closedSet[a])
						continue;

					stack.Push(a);
					closedSet[a] = true;
					fMap[a] = stepCount;

					//goal check
					foreach (Point g in fMap.Goals)
					{
						if (a.Equals(g))
						{
							//flush stack
							while (stack.Count() != 0)
								Path.Add(stack.Pop());

							sw.Stop();
							stepCount++;
							return true;
						}
					}
					stepCount++;
					validAdjFound = true;

					//draw gui during loop
					sw.Stop();
					SwinGame.ClearScreen(Color.Black);
					Draw();
					SwinGame.RefreshScreen();
					sw.Start();

					break;
				}
				if (!validAdjFound)
					stack.Pop();
			}
			sw.Stop();
			return false;
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
		}

		private void DrawOpenSet()
		{
			if (DebugMode.Openset)
			{
				List<Point> path = stack.ToList();
				for (int i = 0; i < path.Count(); i++)
					DrawGridBox(path[i], Color.LightBlue, 1);
			}
		}
	}
}
