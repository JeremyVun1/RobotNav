using SwinGameSDK;

namespace RobotNav
{
	public class InputHandler
	{
		private SearchStrategy strategy;
		private IRobotNav robotNav;

		public InputHandler(SearchStrategy strategy, IRobotNav robotNav)
		{
			this.strategy = strategy;
			this.robotNav = robotNav;
		}

		public void Update()
		{
			SwinGame.ProcessEvents();

			if (SwinGame.KeyTyped(KeyCode.Num1Key))
			{
				robotNav.Method = "BFS";
				robotNav.Init();
			}

			if (SwinGame.KeyTyped(KeyCode.Num2Key))
			{
				robotNav.Method = "DFS";
				robotNav.Init();
			}

			if (SwinGame.KeyTyped(KeyCode.Num3Key))
			{
				robotNav.Method = "GBFS";
				robotNav.Init();
			}

			if (SwinGame.KeyTyped(KeyCode.Num4Key))
			{
				robotNav.Method = "AS";
				robotNav.Init();
			}

			if (SwinGame.KeyTyped(KeyCode.Num5Key))
			{
				robotNav.Method = "ASFS";
				robotNav.Init();
			}

			if (SwinGame.KeyTyped(KeyCode.Num6Key))
			{
				robotNav.Method = "JPS";
				robotNav.Init();
			}

			if (SwinGame.KeyTyped(KeyCode.Num7Key))
			{
				robotNav.Method = "GA";
				robotNav.Init();
			}


			if (SwinGame.KeyTyped(KeyCode.PKey))
				strategy.TogglePause();

			if (SwinGame.KeyTyped(KeyCode.SpaceKey))
				strategy.Start();

			if (SwinGame.KeyTyped(KeyCode.RKey))
				robotNav.Init();

			if (SwinGame.KeyTyped(KeyCode.EscapeKey) || SwinGame.QuitRequested())
				robotNav.Exit();

			if (SwinGame.KeyTyped(KeyCode.DKey))
			{
				if (strategy is GAStrategy)
				{
					GAStrategy strat = (GAStrategy)strategy;
					strat.ValueDiversity = !strat.ValueDiversity;
				}
			}

			if (SwinGame.KeyTyped(KeyCode.EKey))
			{
				if (strategy is GAStrategy)
				{
					GAStrategy strat = (GAStrategy)strategy;
					strat.Elite = !strat.Elite;
				}
			}
		}
	}
}
