using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotNav.GA
{
	//use different struct to score and sort to be more cache friendly.
	//We don't need to look at the action sequences to do this
	public class ScoreCard
	{
		public int Individual { get; private set; }
		public double Score { get; private set; }

		public ScoreCard(int i, double s)
		{
			Individual = i;
			Score = s;
		}

		public void AddScore(double s)
		{
			Score += s;
		}
	}
}
