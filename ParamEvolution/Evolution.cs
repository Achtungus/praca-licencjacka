using ScriptsOfTribute;

using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace ParamEvolution;

public class Generation
{
    static Random rnd = new Random();
    const int numberOfChildren = 10;
    const int numberOfFights = 250;
    int currentGenerationNumber = 0;

    List<GameParams> currentGeneration = new();
    List<int> scores = new();
    public void CreateFirstGeneration(GameParams gameParams)
    {
        currentGeneration.Add(gameParams);
        for (int i = 0; i + 1 < numberOfChildren; i++)
        {
            currentGeneration.Add(gameParams.Mutate(1.0, 0.3));
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
            using (StreamWriter sw = File.CreateText(dirPath + $"/GameParams_{i}_{scores[i]}.txt"))
            {
                sw.Write(currentGeneration[i].ToString());
            }
        }
    }

    List<GameParams> SelectChildren(List<GameParams> children)
    {
        List<(int, int, int)> fights = new();
        for (int i = 0; i < children.Count; i++)
        {
            fights.Add((i, 0, 1));
            fights.Add((i, 0, 2));
        }

        int totStates = children.Count * 2 * numberOfFights;
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
                if (player == 1)
                {
                    for (ulong k = 0; k < numberOfFights; k++)
                    {
                        var cMCTS = new BestMCTS(children[i]);
                        var cgMCTS = new BestMCTS(currentGeneration[j]);
                        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cMCTS, cgMCTS);
                        game.Seed = initSeed + k;
                        var (endState, endBoardState) = game.Play();
                        if (endState.Winner == PlayerEnum.PLAYER1)
                        {
                            wins[i] += 1;
                        }
                    }
                }
                else
                {
                    for (ulong k = 0; k < numberOfFights; k++)
                    {
                        var cMCTS = new BestMCTS(children[i]);
                        var cgMCTS = new BestMCTS(currentGeneration[j]);
                        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cgMCTS, cMCTS);
                        game.Seed = initSeed + k;
                        var (endState, endBoardState) = game.Play();
                        if (endState.Winner == PlayerEnum.PLAYER2)
                        {
                            wins[i] += 1;
                        }
                    }
                }
            }

            Console.WriteLine($"Thread #{threadNo} finished.");
            return wins;
        }

        const int noOfThreads = 14;
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
        List<GameParams> best = new();
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
        List<GameParams> children = new();
        // children.AddRange(currentGeneration);

        for (int i = 0; i < 20; i++)
        {
            int a = rnd.Next(0, currentGeneration.Count - 1);
            int b = rnd.Next(0, currentGeneration.Count - 1);
            GameParams child = GameParams.Combine(currentGeneration[a], currentGeneration[b]);
            children.Add(child.Mutate(0.3, 0.1));
        }

        // for (int i = 0; i < currentGeneration.Count; i++)
        // {
        //     for (int j = i + 1; j < currentGeneration.Count; j++)
        //     {
        //         GameParams child = GameParams.Combine(currentGeneration[i], currentGeneration[j]);
        //         children.Add(child.Mutate(3, 0.05));
        //     }
        // }

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
        g.CreateFirstGeneration(new GameParams());
        while (true)
        {
            g.SaveGeneration();
            g.NextGeneration();
        }
    }

}