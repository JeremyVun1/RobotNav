using RobotNav.Extensions;

namespace RobotNav
{
	public struct GAOpts
	{
		public double mutRate, popSize, fitMulti, deepeningInc;
		public bool diversity, elite;

		public GAOpts(string[] args)
		{
			mutRate = args.GetValueOrDefaultAsDouble(3, 0.04);
			popSize = args.GetValueOrDefaultAsDouble(2, 20);
			fitMulti = args.GetValueOrDefaultAsDouble(2, 2);
			deepeningInc = args.GetValueOrDefaultAsDouble(2, 1);

			diversity = args.GetValueOrDefaultAsBool(2, false);
			elite = args.GetValueOrDefaultAsBool(2, false);
		}
	}
}
