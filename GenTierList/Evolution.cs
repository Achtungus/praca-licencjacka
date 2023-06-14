using ScriptsOfTribute;

using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace Evolution;

public class Generation
{
    static Random rnd = new Random();
    const int numberOfChildren = 4;
    const int numberOfFights = 50;
    int currentGenerationNumber = 0;

    List<TierListEv> currentGeneration = new();
    List<int> scores = new();
    public void CreateFirstGeneration(TierListEv tierList)
    {
        currentGeneration.Add(tierList);
        for (int i = 0; i + 1 < numberOfChildren; i++)
        {
            currentGeneration.Add(tierList.Mutate(30, 1.0));
        }
        for (int i = 0; i < numberOfChildren; i++)
        {
            scores.Add(0);
        }
    }

    public void SaveGeneration()
    {
        string dirPath = $"../../../Generations/Generation_{currentGenerationNumber}";
        Directory.CreateDirectory(dirPath);
        for (int i = 0; i < currentGeneration.Count; i++)
        {
            using (StreamWriter sw = File.CreateText(dirPath + $"/TierList_{i}_{scores[i]}.txt"))
            {
                sw.Write(currentGeneration[i].ToString());
            }
        }
    }

    List<TierListEv> SelectChildren(List<TierListEv> children)
    {
        List<(int, int, int)> fights = new();
        for (int i = 0; i < children.Count; i++)
        {
            for (int j = 0; j < currentGeneration.Count; j++)
            {
                fights.Add((i, j, 1));
                fights.Add((i, j, 2));
            }
        }

        int totStates = currentGeneration.Count * children.Count * 2 * numberOfFights;
        Console.WriteLine($"{totStates} stanow");

        ulong initSeed = (ulong)rnd.Next(1, 2000000000);
        List<int> PlayGames(List<(int, int, int)> fightsForThread, int threadNo)
        {
            List<int> wins = new();
            for (int i = 0; i < children.Count; i++)
            {
                wins.Add(0);
            }
            foreach (var (i, j, player) in fightsForThread)
            {
                ulong seed = initSeed;
                if (player == 1)
                {
                    for (int k = 0; k < numberOfFights; k++)
                    {
                        var cMCTS = new BestMCTS(children[i]);
                        var cgMCTS = new BestMCTS(currentGeneration[j]);
                        if (i == 0 && threadNo == 0) cgMCTS = new BestMCTS(currentGeneration[j], true);
                        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cMCTS, cgMCTS);
                        game.Seed = seed;
                        var (endState, endBoardState) = game.Play();
                        if (endState.Winner == PlayerEnum.PLAYER1)
                        {
                            wins[i] += 1;
                        }
                        seed += 1;
                    }
                }
                else
                {
                    for (int k = 0; k < numberOfFights; k++)
                    {
                        var cMCTS = new BestMCTS(children[i]);
                        var cgMCTS = new BestMCTS(currentGeneration[j]);
                        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cgMCTS, cMCTS);
                        game.Seed = seed;
                        var (endState, endBoardState) = game.Play();
                        if (endState.Winner == PlayerEnum.PLAYER2)
                        {
                            wins[i] += 1;
                        }
                        seed += 1;
                    }
                }
            }

            Console.WriteLine($"Thread #{threadNo} finished.");
            return wins;
        }

        const int noOfThreads = 12;
        var threads = new Task<List<int>>[noOfThreads];
        var watki = new List<(int, int, int)>[noOfThreads];
        for (int i = 0; i < noOfThreads; i++)
        {
            watki[i] = new();
        }
        for (var i = 0; i < noOfThreads; i++)
        {
            var threadFights = new List<(int, int, int)>();
            for (int j = 0; j < fights.Count; j++)
            {
                if (j % noOfThreads == i)
                    threadFights.Add(fights[j]);
            }
            var xd = i;
            Console.WriteLine($"Playing {threadFights.Count} games in thread #{i}");
            threads[i] = Task.Factory.StartNew(() => PlayGames(threadFights, xd));
        }
        Task.WaitAll(threads.ToArray<Task>());

        List<(int, int)> wins = new();
        for (int i = 0; i < children.Count; i++)
        {
            wins.Add((0, i));
        }

        for (int i = 0; i < noOfThreads; i++)
        {
            var res = threads[i].Result;
            for (int j = 0; j < numberOfChildren; j++)
            {
                wins[j] = (wins[j].Item1 + res[j], wins[j].Item2);
            }
        }

        Console.WriteLine($"Generation {currentGenerationNumber}:");
        for (int i = 0; i < children.Count; i++)
        {
            Console.WriteLine($"Child {i}: {wins[i].Item1} wins");
        }
        Console.WriteLine("------------------------------------------------------");


        wins.Sort();
        wins.Reverse();

        scores.Clear();
        int numberOfWorst = 1;
        List<TierListEv> best = new();
        for (int i = 0; i + numberOfWorst < numberOfChildren; i++)
        {
            best.Add(children[wins[i].Item2]);
            scores.Add(wins[i].Item1);
        }

        wins.Reverse();
        for (int i = 0; i < numberOfWorst; i++)
        {
            best.Add(children[wins[i].Item2].Mutate(20, 0.05));
            scores.Add(wins[i].Item1);
        }

        return best;
    }

    public void NextGeneration()
    {
        List<TierListEv> children = new();
        children.AddRange(currentGeneration);
        for (int i = 0; i < currentGeneration.Count; i++)
        {
            for (int j = i + 1; j < currentGeneration.Count; j++)
            {
                TierListEv child = TierListEv.Combine(new List<TierListEv> { currentGeneration[i], currentGeneration[j] });
                children.Add(child.Mutate(3, 0.05));
            }
        }

        children = SelectChildren(children);

        currentGenerationNumber++;
        currentGeneration = children;
    }

}

class Program
{
    static void Main(string[] args)
    {
        Generation g = new();
        g.CreateFirstGeneration(new TierListEv());
        while (true)
        {
            g.SaveGeneration();
            g.NextGeneration();
        }
    }

}