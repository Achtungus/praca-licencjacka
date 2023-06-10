using ScriptsOfTribute;

using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace Evolution;

public class Generation
{
    const int numberOfChildren = 8;
    const int numberOfFights = 100;
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
        List<(int, int)> wins = new();
        for (int i = 0; i < children.Count; i++)
        {
            wins.Add((0, i));
        }
        int totalForOneChild = numberOfFights * 2 * currentGeneration.Count;

        List<BestMCTS> currentGenerationMCTS = new();
        foreach (TierListEv tierList in currentGeneration)
        {
            currentGenerationMCTS.Add(new BestMCTS(tierList));
        }
        List<BestMCTS> childrenMCTS = new();
        foreach (TierListEv tierList in children)
        {
            childrenMCTS.Add(new BestMCTS(tierList));
        }

        List<(int, int, int)> fights = new();

        for (int i = 0; i < children.Count; i++)
        {
            for (int j = 0; j < currentGeneration.Count; j++)
            {
                fights.Add((i, j, 1));
                fights.Add((i, j, 2));
            }
        }

        Console.WriteLine($"{currentGeneration.Count * children.Count * 200} stanow");

        ConcurrentBag<int> cb = new ConcurrentBag<int>();
        Parallel.ForEach(fights, (args) =>
        {
            Console.WriteLine(cb.Count);
            var (i, j, player) = args;
            if (player == 1)
            {
                for (int k = 0; k < numberOfFights; k++)
                {
                    var cMCTS = new BestMCTS(children[i]);
                    var cgMCTS = new BestMCTS(currentGeneration[j]);
                    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cMCTS, cgMCTS);
                    var (endState, endBoardState) = game.Play();
                    if (endState.Winner == PlayerEnum.PLAYER1)
                    {
                        cb.Add(i);
                    }
                }
            }
            else
            {
                for (int k = 0; k < numberOfFights; k++)
                {
                    var cMCTS = new BestMCTS(children[i]);
                    var cgMCTS = new BestMCTS(currentGeneration[j]);
                    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(cgMCTS, cMCTS);
                    var (endState, endBoardState) = game.Play();
                    if (endState.Winner == PlayerEnum.PLAYER2)
                    {
                        cb.Add(i);
                    }
                }

            }
        });

        foreach (int idx in cb)
        {
            wins[idx] = (wins[idx].Item1 + 1, idx);
        }

        wins.Sort();
        wins.Reverse();

        scores.Clear();
        List<TierListEv> best = new();
        for (int i = 0; i + 2 < numberOfChildren; i++)
        {
            best.Add(children[wins[i].Item2]);
            scores.Add(wins[i].Item1);
        }

        wins.Reverse();
        for (int i = 0; i < 2; i++)
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