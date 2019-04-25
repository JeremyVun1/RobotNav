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
		private int popSize, generations, bestGeneration, fitnessMulti;
		private double mutRate, bestTime;
		private int deepCount; // iterative deepening

		private List<Point> bestPathFound;
		public List<MoveDir> BestDNA { get; private set; }

		public bool ValueDiversity { get; set; }
		public bool Elite { get; set; }
		double BestDNAFitness;

		public override int PathSizeOutput { get { return bestPathFound == null ? -1 : bestPathFound.Count() - 1; } }

		protected override int pathSize {
			get {
				Individual best = BestParent(parents);
				if (best == null)
					return bestPathFound == null ? -1 : bestPathFound.Count();

				int result = best.DNALength;
				if (bestPathFound == null || bestPathFound.Count()-1 > result)
				{
					bestPathFound = best.Path;
					BestDNA = best.Dna;
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

			popSize = opt.popSize; // population size
			mutRate = opt.mutRate; // mutation rate
			fitnessMulti = opt.fitMulti; //fitness multiplier
			ValueDiversity = opt.diversity; //whether we value candidate diversity or not
			Elite = opt.elite; //Mix in the genes of the best path found when creating next generations
			deepCount = 1;

			parents = new Individual[Math.Max((int)(popSize*0.1),2)]; //parents as a fifth of the population
			dnaPool = new List<MasterGene>(); //action counter for crossovers
			BestDNA = new List<MoveDir>();

			//initialise generation
			generation = new Individual[popSize];
			for (int i = 0; i < popSize; i++)
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
				int count = 0;
				while (!CheckIfGoal(generation[i]) && count < deepCount)
				{
					generation[i].Move((MoveDir)rng.Next(0, 4), fMap);
					count++;
				}
			}

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
			int dnaAvgLength = AverageDnaLength(generation);

			//score fitness of candidates
			List<ScoreCard> scores = new List<ScoreCard>();
			double fitnessAvg = ScoreFitness(scores, generation, dnaAvgLength);

			//select top 10% of population
			SelectParents(scores);

			//calculate diversity score of all candidates relative to prior fitness selection
			//add diversity score to all the candidate scorecards
			ScoreDiversity(scores, fitnessAvg);

			//create next generation of candidates
			SpawnCandidates();
			dnaPool.Clear();

			//increment generation counter and iterative deepening counter
			generations++;
			deepCount = Math.Min(deepCount+1, 2147483647); // 32bit int max

			sw.Stop();
			return false;
		}

		private void SpawnCandidates()
		{
			for (int i = 0; i < popSize; i++)
			{
				Individual child = new Individual(fMap.Start);
				for (int j = 0; j < dnaPool.Count(); j++)
				{
					//use master dna pool until we reach the goal
					if (CheckIfGoal(child))
						break;

					child.Move(dnaPool[j].GetDNA(rng, mutRate), fMap);
				}

				//if child still hasn't reachted the goal, fill out dna with random moves until it does
				int count = 0;
				while (!CheckIfGoal(child) && count < deepCount)
				{
					child.Move((MoveDir)rng.Next(0, 4), fMap);
					count++;
				}

				generation[i] = child;
			}
		}

		private void ScoreDiversity(List<ScoreCard> scores, double fitnessAvg)
		{
			if (ValueDiversity)
			{
				for (int i = 0; i < popSize; i++)
				{
					double dScore = Diversity(generation[i].Dna, dnaPool, fitnessAvg);
					scores[i].AddScore(dScore);
				}

				//re-select top 10%
				SelectParents(scores);
			}
		}

		private double ScoreFitness(List<ScoreCard> scores, Individual[] generation, int dnaAvgLength)
		{
			double result = 0.0;
			int n = generation.Length;
			for (int i = 0; i < generation.Length; i++)
			{
				double fitness = Fitness(generation[i].DNALength, dnaAvgLength);
				result += fitness;
				scores.Add(new ScoreCard(i, fitness));
			}

			//mix in our elite fitness if opted for
			if (Elite && BestDNA != null)
			{
				BestDNAFitness = Fitness(BestDNA.Count(), dnaAvgLength);
				result += BestDNAFitness;
				n++;
			}

			return result / n;
		}

		private int AverageDnaLength(Individual[] generation)
		{
			int result = 0;
			int n = popSize;
			for (int i = 0; i < generation.Count(); i++)
				result += generation[i].DNALength;

			//mix in our elite length if opted for
			if (Elite && BestDNA != null)
			{
				result += BestDNA.Count();
				n++;
			}

			return result / n;
		}

		private Individual BestParent(Individual[] parents)
		{
			foreach (Individual i in parents)
			{
				if (i == null)
					return null;

				if (i.ReachedGoal)
					return i;
			}
			return null;
		}

		private void SelectParents(List<ScoreCard> scores)
		{
			//sort scorecards in descending fitness score
			scores.Sort((x, y) => y.Score.CompareTo(x.Score));

			for (int i = 0; i < parents.Length; i++)
			{
				//choose parents
				parents[i] = generation[scores[i].Individual];

				//Mix parent into master dna pool
				List<MoveDir> dna = parents[i].Dna;
				for (int j = 0; j < dna.Count(); j++)
				{
					if (dnaPool.Count() <= j)
						dnaPool.Add(new MasterGene());

					dnaPool[j].Mix(dna[j], scores[i].Individual);
				}
			}

			// mix in our elites if opted for
			if (Elite && BestDNA != null)
			{
				dnaPool.Add(new MasterGene());

				for (int i = 0; i < BestDNA.Count(); i++)
				{
					if (dnaPool.Count() <= i)
						dnaPool.Add(new MasterGene());

					dnaPool[i].Mix(BestDNA[i], BestDNAFitness);
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
		private bool CheckIfGoal(Individual i)
		{
			foreach (Point g in fMap.Goals)
			{
				if (i.Pos.Equals(g))
				{
					i.ReachedGoal = true;
					return true;
				}
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
			if (bestPathFound == null)
				return;
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
			SwinGame.DrawText("Population, Mutation: " + popSize + ", " + mutRate*100 + "%", Color.White, col[0], uiTop + 45);
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
