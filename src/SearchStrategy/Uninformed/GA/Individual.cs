using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotNav.GA
{
	public class Individual
	{
		public int DNALength { get { return Dna.Count(); } }
		public Point Pos { get; private set; }
		public List<MoveDir> Dna { get; private set; }

		public List<Point> Path { get; private set; }

		public Individual(Point pos)
		{
			Pos = pos;

			Dna = new List<MoveDir>();
			Path = new List<Point>();
			Path.Add(pos);
		}

		public void Move(MoveDir dir, FMap fMap)
		{
			//bounds and wall checks
			List<Point> adj = fMap.Adjacent(Pos);
			foreach (Point a in adj)
			{
				switch (dir)
				{
					case MoveDir.N:
						if (a.Y < Pos.Y)
						{
							UpdatePosition(a, dir);
							return;
						}
						break;
					case MoveDir.W:
						if (a.X < Pos.X)
						{
							UpdatePosition(a, dir);
							return;
						}
						break;
					case MoveDir.S:
						if (a.Y > Pos.Y)
						{
							UpdatePosition(a, dir);
							return;
						}
						break;
					case MoveDir.E:
						if (a.X > Pos.X)
						{
							UpdatePosition(a, dir);
							return;
						}
						break;
				}
			}
		}

		private void UpdatePosition(Point a, MoveDir dir)
		{
			//trim out redundant back and forth movement
			if (Dna.Count() > 1 && (int)dir == (int)(Dna.Last() + 2) % 4) {
				//reset our position back
				switch(Dna.Last())
				{
					case MoveDir.N:
						Pos = new Point(Pos.X, Pos.Y+1);
						break;
					case MoveDir.W:
						Pos = new Point(Pos.X+1, Pos.Y);
						break;
					case MoveDir.S:
						Pos = new Point(Pos.X, Pos.Y-1);
						break;
					case MoveDir.E:
						Pos = new Point(Pos.X-1, Pos.Y);
						break;
				}
				//remove last dna record
				Dna.RemoveAt(Dna.Count() - 1);
				Path.RemoveAt(Path.Count() - 1);
				return;
			}

			Path.Add(a);
			Pos = a;
			Dna.Add(dir);
		}
	}
}
