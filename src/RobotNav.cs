using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;

namespace RobotNav
{
	public class RobotNav : IRobotNav
	{
		private SearchStrategy searchStrategy;
		private InputHandler inputHandler;
		private bool exit, forceStart;
		public string Filename { get; set; }
		public string Method { get; set; }
		private string popSize, mutRate, fitMulti, diversity, elite;

		public RobotNav(string[] args)
		{
			//Swingame GUI library
			SwinGame.OpenGraphicsWindow("Robot Nav Jeremy Vun 2726092", 600, 700);

			exit = false;
			Filename = args[0];

			//launch GUI with the defined method
			if (args.Length >= 2)
			{
				Method = args[1];
				forceStart = true;
			}
			//default launch the GUI without a method
			else
			{
				Method = "BFS";
				forceStart = false;
			}

			//capture command line arguments
			if (args.Length >= 3)
				popSize = args[2];
			if (args.Length >= 4)
				mutRate = args[3];
			if (args.Length >= 5)
				fitMulti = args[4];
			if (args.Length >= 6)
				diversity = args[5];
			if (args.Length >= 7)
				elite = args[6];

			Init();
		}

		public void Init()
		{
			searchStrategy = SearchStrategyFactory.Create(Filename, Method, popSize, mutRate, fitMulti, diversity, elite);
			inputHandler = new InputHandler(searchStrategy, this);

			//resize window so it looks better
			SwinGame.ChangeScreenSize(searchStrategy.fMap.Width * searchStrategy.gridW, searchStrategy.fMap.Height * searchStrategy.gridH + 155);

			if (forceStart)
			{
				searchStrategy.Start();
				forceStart = false;
			}
		}

		public void Run()
		{
			while (!exit)
			{
				SwinGame.ClearScreen(Color.Black);

				inputHandler.Update();

				if (searchStrategy.Update())
					PrintOutput();

				searchStrategy.Draw();

				SwinGame.RefreshScreen(60);
			}
		}

		public void PrintOutput()
		{
			Console.WriteLine("{0} {1} {2}", Filename, Method, searchStrategy.fMap.Width * searchStrategy.fMap.Height);
			Console.WriteLine(searchStrategy.PathString);
		}

		public void Exit()
		{
			if (searchStrategy is GAStrategy)
				PrintOutput();
			exit = true;
		}
	}
}
