using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotNav
{
	public struct GAOpts
	{
		public int popSize, fitMulti, deepeningInc;
		public double mutRate;
		public bool diversity, elite;

		public GAOpts(string popSize, string mutRate, string fitMulti, string diversity, string elite, string deepeningInc)
		{
			if (!int.TryParse(popSize, out this.popSize))
				this.popSize = 20;

			if (!double.TryParse(mutRate, out this.mutRate))
				this.mutRate = 0.04;

			if (!int.TryParse(fitMulti, out this.fitMulti))
				this.fitMulti = 2;

			if (diversity != null && diversity.ToLower() == "true")
				this.diversity = true;
			else this.diversity = false;

			if (elite != null && elite.ToLower() == "true")
				this.elite = true;
			else this.elite = true;

			if (!int.TryParse(deepeningInc, out this.deepeningInc))
				this.deepeningInc = 1;
		}
	}
}
