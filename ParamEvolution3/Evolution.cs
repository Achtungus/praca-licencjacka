using ScriptsOfTribute;

namespace ParamEvolution3;

public class Generation
{
    static readonly Random rnd = new Random();
    const int generationSize = 10;
    const int noOfChildren = 30;
    const int noOfFights = 25;
    const double crossoverProbability = 0.7;
    int noOfGeneration = 0;
    List<int> scores = new();
    List<GameParams> currentGeneration = new();

    public void CreateFirstGeneration(GameParams gameParams)
    {
        currentGeneration.Add(gameParams);
        for (int i = 0; i + 1 < generationSize; i++)
        {
            currentGeneration.Add(gameParams.Mutate(1.0, 0.5));
        }
        for (int i = 0; i < generationSize; i++)
        {
            scores.Add(0);
        }
    }

    public void SaveGeneration()
    {
        string dirPath = $"../../../Generations/Generation_{noOfGeneration}";
        Directory.CreateDirectory(dirPath);
        for (int i = 0; i < generationSize; i++)
        {
            using (StreamWriter sw = File.CreateText(dirPath + $"/GameParams_{i}_{scores[i]}.txt"))
            {
                sw.Write(currentGeneration[i].ToString());
            }
        }
    }

    List<GameParams> SelectChildren(List<GameParams> children)
    {
        int totStates = noOfFights * 2 * noOfChildren;
        Console.WriteLine($"Total states: {totStates}");

        ulong initSeed = (ulong)rnd.NextInt64(100000, 2000000000);
        int[] wins = new int[noOfChildren];

        int totPlayed = 0;
        ulong[] seedzik = new ulong[noOfFights];
        ulong[] seedzik2 = new ulong[noOfFights];
        for (int i = 0; i < noOfFights; i++)
        {
            seedzik[i] = (ulong)rnd.NextInt64(1, 1000000000);
            seedzik2[i] = (ulong)rnd.NextInt64(1, 1000000000);
        }

        Parallel.For(0, noOfChildren * 2, i =>
        {
            int childIdx = i / 2;
            if (i % 2 == 1)
            {
                for (ulong k = 0; k < noOfFights; k++)
                {
                    var cMCTS = new BestMCTS(children[childIdx], seedzik[k]);
                    var cgMCTS = new BestMCTS(currentGeneration[0], seedzik2[k]);
                    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cMCTS, cgMCTS);
                    game.Seed = initSeed + k + 1;
                    var (endState, endBoardState) = game.Play();
                    if (endState.Winner == PlayerEnum.PLAYER1) Interlocked.Increment(ref wins[childIdx]);
                    Interlocked.Increment(ref totPlayed);
                    if (totPlayed % 100 == 0)
                    {
                        Console.WriteLine($"{totPlayed} / {totStates}");
                    }
                }
            }
            else
            {

                for (ulong k = 0; k < noOfFights; k++)
                {
                    var cMCTS = new BestMCTS(children[childIdx], seedzik2[k]);
                    var cgMCTS = new BestMCTS(currentGeneration[0], seedzik[k]);
                    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cgMCTS, cMCTS);
                    game.Seed = initSeed + k + 1;
                    var (endState, endBoardState) = game.Play();
                    if (endState.Winner == PlayerEnum.PLAYER2) Interlocked.Increment(ref wins[childIdx]);
                    Interlocked.Increment(ref totPlayed);
                    if (totPlayed % 100 == 0)
                    {
                        Console.WriteLine($"{totPlayed} / {totStates}");
                    }
                }
            }
        });

        Console.WriteLine($"Generation {noOfGeneration}:");
        for (int i = 0; i < noOfChildren; i++)
        {
            Console.WriteLine($"Child {i}: {wins[i]} wins");
        }
        Console.WriteLine("------------------------------------------------------");

        List<(int, int)> newWins = new();
        for (int i = 0; i < noOfChildren; i++)
        {
            newWins.Add((wins[i], i));
        }

        newWins.Sort();
        newWins.Reverse();

        int noOfWorst = (int)(0.15 * generationSize);
        List<GameParams> best = new();

        scores.Clear();

        for (int i = 0; i + noOfWorst < generationSize; i++)
        {
            best.Add(children[newWins[i].Item2]);
            scores.Add(newWins[i].Item1);
        }

        newWins.Reverse();
        for (int i = 0; i < noOfWorst; i++)
        {
            best.Add(children[newWins[i].Item2].Mutate(0.6, 0.4));
            scores.Add(newWins[i].Item1);
        }

        return best;
    }
    public void NextGeneration()
    {
        List<GameParams> children = new();

        while (children.Count < (int)(noOfChildren * crossoverProbability))
        {
            int a = rnd.Next(0, currentGeneration.Count - 1);
            int b = rnd.Next(0, currentGeneration.Count - 1);
            GameParams child = GameParams.Combine(currentGeneration[a], currentGeneration[b]);
            children.Add(child.Mutate(0.4, 0.05));
        }

        foreach (var parent in currentGeneration.Take(2))
        {
            children.Add(parent);
        }

        while (children.Count < noOfChildren)
        {
            int idx = rnd.Next(0, currentGeneration.Count - 1);
            children.Add(currentGeneration[idx].Mutate(0.3, 0.2));
        }

        children = SelectChildren(children);

        noOfGeneration += 1;
        currentGeneration = children;
    }

}

class Program
{
    static void Main(string[] args)
    {
        Generation g = new();
        g.CreateFirstGeneration(new GameParams());
        while (true)
        {
            g.SaveGeneration();
            g.NextGeneration();
        }
    }
}