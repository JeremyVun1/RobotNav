using System;

namespace RobotNav
{
	public static class Program
	{
		public static void Main(params string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("required args: <filename> <method> <opt>");
				return;
			}

			RobotNav rn = new RobotNav(args);
			rn.Run();
		}
	}
}