using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotNav.GA
{
	public enum MoveDir { N = 0, W = 1, S = 2, E = 3 }

	public class MasterGene
	{
		private double[] aCount;
		private double totalCount
		{
			get
			{
				double total = 0;
				foreach (double c in aCount)
					total += c;

				return total;
			}
		}

		public double AvgDna
		{
			get
			{
				double total = totalCount;
				double result = 0.0;

				for (int i=1; i<aCount.Length; i++)
				{
					double pct = aCount[i] / total;
					result += pct * i;
				}

				return result;
			}
		}

		public MasterGene()
		{
			aCount = new double[4];
		}

		public void Mix(MoveDir a, double score = 1.0)
		{
			aCount[(int)a] += score;
		}

		public MoveDir GetDNA(Random rng, double mutation)
		{
			double total = totalCount;
			double x = rng.NextDouble();

			//mutate
			if (x < mutation)
				return (MoveDir)rng.Next(0, 4);

			//get from dna pool for this sequence element
			double nThreshhold = aCount[0] / total;
			double wThreshhold = aCount[1] / total + nThreshhold;
			double sThreshhold = aCount[2] / total + wThreshhold;
			
			if (x < nThreshhold)
				return MoveDir.N;
			if (x < wThreshhold)
				return MoveDir.W;
			if (x < sThreshhold)
				return MoveDir.S;
			return MoveDir.E;
		}
	}
}
