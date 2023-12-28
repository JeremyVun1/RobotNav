using System;
using System.IO;
using SwinGameSDK;

namespace RobotNav
{
	public class RobotNav : IRobotNav
	{
		public string Filename { get; set; }
		public string Method { get; set; }

		private SearchStrategy searchStrategy;
		private InputHandler inputHandler;
		private bool isExitRequested = false;
		private GAOpts gaOpts;

		public RobotNav(string[] args)
		{
			Filename = $"data/{args[0]}.txt";
			if (!File.Exists(Filename))
			{
				Console.WriteLine($"Map {Filename} not found");
			}

			Method = args.Length >= 2
				? args[1]
				: "BFS";

			gaOpts = new GAOpts(args);

			SwinGame.OpenGraphicsWindow("Robot Nav Jeremy Vun 2726092", 1024, 1200);

			Init();
		}

		public void Init()
		{
			isExitRequested = false;
			searchStrategy = SearchStrategyFactory.Create(Filename, Method, gaOpts);//, Method, popSize, mutRate, fitMulti, diversity, elite, deepeningInc);

			var w = searchStrategy.fMap.Width * searchStrategy.gridW;
			var h = searchStrategy.fMap.Height * searchStrategy.gridH + 155;
			SwinGame.ChangeScreenSize(w, h);

			inputHandler = new InputHandler(searchStrategy, this);
		}

		public void Run()
		{
			while (!isExitRequested)
			{
				SwinGame.ClearScreen(Color.Black);

				inputHandler.Update();

				if (searchStrategy.Update())
					PrintOutput();

				searchStrategy.Draw();

				SwinGame.RefreshScreen(60);
			}
		}		

		public void Exit()
		{
			if (searchStrategy is GAStrategy)
				PrintOutput();

			isExitRequested = true;
		}

		public void PrintOutput()
		{
			Console.WriteLine("{0} {1} {2}", Filename, Method, searchStrategy.NumberOfNodes);
			Console.WriteLine(searchStrategy.PathString);
		}
	}
}
