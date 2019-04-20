using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwinGameSDK;
using RobotNav.GA;

namespace RobotNav
{
	public class GAStrategy : SearchStrategy
	{
		private Individual[] generation;
		private Individual[] parents;
		private List<MasterGene> dnaPool;
		private int n, generations, bestGeneration, fitnessMulti;
		private double m, bestTime;

		private List<Point> bestPathFound;
		public List<MoveDir> BestDNA { get; private set; }
		private double bestDNAFitness;

		public override int PathSizeOutput { get { return bestPathFound.Count() - 1; } }

		public bool ValueDiversity { get; set; }
		public bool Elite { get; set; }

		protected override int pathSize {
			get {
				if (parents[0] == null)
					return 0;

				int result = parents[0].DNALength;
				if (bestPathFound.Count()-1 > result)
				{
					bestPathFound = parents[0].Path;
					BestDNA = parents[0].Dna;
					bestTime = sw.Elapsed.TotalMilliseconds;
					bestGeneration = generations;
				}

				return bestPathFound.Count()-1;
			}
		}

		private Random rng;
		private bool started;
		private bool finished;

		public GAStrategy(FMap fMap, string id, GAOpts opt) : base(fMap, id)
		{
			rng = new Random(Guid.NewGuid().GetHashCode());

			n = opt.popSize; // population size
			m = opt.mutRate; // mutation rate
			fitnessMulti = opt.fitMulti; //fitness multiplier
			ValueDiversity = opt.diversity; //whether we value candidate diversity or not
			Elite = opt.elite; //Mix in the genes of the best path found when creating next generations

			parents = new Individual[Math.Max((int)(n*0.1),2)]; //parents as a fifth of the population
			dnaPool = new List<MasterGene>(); //action counter for crossovers
			BestDNA = new List<MoveDir>();

			//initialise generation
			generation = new Individual[n];
			for (int i = 0; i < n; i++)
			{
				generation[i] = new Individual(fMap.Start);
			}

			started = false;
		}

		public override void Start()
		{
			base.Start();

			//seed initial generation with random actions
			//keep choosing random actions until we hit a goal point
			for (int i = 0; i < generation.Length; i++)
			{
				while (!CheckIfGoal(generation[i].Pos))
				{
					generation[i].Move((MoveDir)rng.Next(0, 4), fMap);
				}
			}
			bestPathFound = generation[0].Path;

			generations = 0;
			started = true;
			finished = false;
		}

		public override bool Update()
		{
			//guards
			if (fMap == null)
				return false;
			if (finished)
			{
				finished = false;
				return true;
			}
			if (!started || paused)
				return false;

			sw.Start();
			//algorithm start

			//get dna average length of the generation
			int dnaAvgLength = 0;
			for (int i = 0; i < generation.Count(); i++)
				dnaAvgLength += generation[i].DNALength;
			dnaAvgLength = dnaAvgLength / n;

			//score fitness of candidates
			double fitnessAvg = 0.0;
			List<ScoreCard> scores = new List<ScoreCard>();
			for (int i = 0; i < generation.Count(); i++)
			{
				double fitness = Fitness(generation[i].DNALength, dnaAvgLength);
				fitnessAvg += fitness;
				scores.Add(new ScoreCard(i, fitness));
			}
			fitnessAvg = fitnessAvg / generation.Count();

			//elitism modifier option
			if (Elite && BestDNA != null)
				bestDNAFitness = Fitness(BestDNA.Count(), dnaAvgLength);

			//select top 10%
			SelectParents(scores);

			//calculate diversity score of all candidates relative to prior selection
			//and add diversity score to the candidates scorecard
			if (ValueDiversity)
			{
				for (int i = 0; i < n; i++)
				{
					double dScore = Diversity(generation[i].Dna, dnaPool, fitnessAvg);
					scores[i].AddScore(dScore);
				}

				if (Elite && BestDNA != null)
					bestDNAFitness += Diversity(BestDNA, dnaPool, bestDNAFitness);

				//re-select top 10%
				SelectParents(scores);
			}

			//create next generation of candidates
			for (int i = 0; i < n; i++)
			{
				Individual child = new Individual(fMap.Start);
				for (int j = 0; j < dnaPool.Count(); j++)
				{
					//use master dna pool until we reach the goal
					if (CheckIfGoal(child.Pos))
						break;

					child.Move(dnaPool[j].GetDNA(rng, m), fMap);
				}

				//if child still hasn't reachted the goal, fill out dna with random moves until it does
				while (!CheckIfGoal(child.Pos))
					child.Move((MoveDir)rng.Next(0, 4), fMap);

				generation[i] = child;
			}

			//increment generation counter
			generations++;

			sw.Stop();
			return false;
		}

		private void SelectParents(List<ScoreCard> scores)
		{
			scores.Sort((x, y) => y.Score.CompareTo(x.Score));
			for (int i = 0; i < parents.Length; i++)
			{
				//populate parents with high scoring candidates
				parents[i] = generation[scores[i].Individual];

				//create master dna pool for reproduction and diversity comparison
				List<MoveDir> dna = parents[i].Dna;
				for (int j = 0; j < dna.Count(); j++)
				{
					if (dnaPool.Count() <= j)
						dnaPool.Add(new MasterGene());

					dnaPool[j].Mix(dna[j], scores[i].Individual);
				}
			}

			// elitism - mix globally best found dna into pool
			if (Elite && BestDNA != null)
			{
				dnaPool.Add(new MasterGene());

				for(int i=0; i<BestDNA.Count(); i++)
				{
					dnaPool[dnaPool.Count() - 1].Mix(BestDNA[i], bestDNAFitness);
				}
			}
		}

		//fitness function. Dna lengths that are better than average score exponentially better
		private double Fitness(int dnaLength, int dnaAvgLength)
		{			
			double result = (double)dnaAvgLength / (double)dnaLength;
			return Math.Pow(result, fitnessMulti);
		}

		private double Diversity(List<MoveDir> dna, List<MasterGene> dnaPool, double scale)
		{
			double d = 0.0;
			int n = Math.Min(dna.Count(), dnaPool.Count());

			//iterate through dna sequence and calculate diversity score compared to the master dna pool
			// N & N = 0
			// N & E = 0.5
			// N & W = 1
			for (int i=0; i<n; i++)
			{
				d += Math.Abs(((double)dna[i] - dnaPool[i].AvgDna)) % 2;
			}
			d = d / n;

			//normalise to be a multiplier of fitness score
			return Math.Pow(scale * d, fitnessMulti);
		}

		//goal check
		private bool CheckIfGoal(Point p)
		{
			foreach (Point g in fMap.Goals)
			{
				if (p.Equals(g))
					return true;
			}
			return false;
		}

		//GUI DRAWS
		public override void Draw()
		{
			if (!DebugMode.Draw)
				return;

			DrawGrid();
			DrawUI();
			DrawChildren();
			DrawPath();
			DrawStartGoals();
		}

		private void DrawChildren()
		{
			for (int i = 0; i < generation.Length; i++)
			{
				int g = 0;
				foreach (Point p in generation[i].Path)
				{
					g++;
					DrawGridBox(p, Color.LightBlue, 1);
					SwinGame.DrawText(g.ToString(), Color.DarkGray, p.X * gridW + gridW / 2 - 5, p.Y * gridH + gridH / 2 - 5);
				}
			}
		}

		protected override void DrawPath()
		{
			if (parents[0] == null)
				return;

			foreach (Point p in parents[0].Path)
			{
				DrawGridBox(p, Color.LightGreen, 1);
			}

			int g = -1;
			foreach (Point p in bestPathFound)
			{
				g++;
				DrawGridBox(p, Color.DarkGreen, 1);
				SwinGame.DrawText(g.ToString(), Color.Black, p.X * gridW + gridW / 2 - 5, p.Y * gridH + gridH / 2 - 5);
			}
		}

		protected override void DrawMetrics(int uiTop, int[] col)
		{
			SwinGame.DrawText("Algorithm: " + id, Color.White, col[0], uiTop + 15);
			SwinGame.DrawText("Generations: " + generations, Color.White, col[0], uiTop + 30);
			SwinGame.DrawText("Population, Mutation: " + n + ", " + m*100 + "%", Color.White, col[0], uiTop + 45);
			SwinGame.DrawText("Diversity, Elite: " + ValueDiversity + ", " + Elite, Color.White, col[0], uiTop + 60);
			SwinGame.DrawText("Running time (ms): " + sw.Elapsed.TotalMilliseconds, Color.White, col[0], uiTop + 75);

			SwinGame.DrawText("Best Path Size: " + pathSize, Color.White, col[0], uiTop + 90);
			SwinGame.DrawText("Best Path Time: " + bestTime, Color.White, col[0], uiTop + 105);
			SwinGame.DrawText("Best Path Generation: " + bestGeneration, Color.White, col[0], uiTop + 120);

			SwinGame.DrawText("Best DNA: " + DNAString(BestDNA), Color.White, col[0], uiTop + 135);

			SwinGame.DrawText("[D]Toggle Diversity", Color.White, col[5], uiTop + 95);
			SwinGame.DrawText("[E]Toggle Elites", Color.White, col[5], uiTop + 110);

			SwinGame.DrawText("[P] Pause", Color.White, col[5], uiTop + 80);
		}

		private string DNAString(List<MoveDir> dna)
		{
			string result = "";
			foreach(MoveDir m in dna)
			{
				result += m.ToString();
			}
			return result;
		}

		public override string PathString
		{
			get
			{
				string result = "";
				
				for (int i=0; i<BestDNA.Count(); i++)
				{
					switch (BestDNA[i])
					{
						case MoveDir.N:
							result += "Up; ";
							break;
						case MoveDir.W:
							result += "West; ";
							break;
						case MoveDir.S:
							result += "South; ";
							break;
						case MoveDir.E:
							result += "East; ";
							break;
					}
				}

				return result;
			}
		}

		public override void TogglePause()
		{
			base.TogglePause();
			if (paused)
				finished = true;
		}
	}
}
