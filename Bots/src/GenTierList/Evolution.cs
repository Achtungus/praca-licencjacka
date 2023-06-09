// using System.IO;
// using System.Threading;

// namespace Evolution;

// public class Generation
// {
//     const int numberOfChildren = 6;
//     const int numberOfFightsOneSite = 100;
//     int currentGenerationNumber = 0;

//     List<TierListEv> currentGeneration = new();
//     public void CreateFirstGeneration(TierListEv tierList)
//     {
//         currentGeneration.Add(tierList);
//         for (int i = 0; i < numberOfChildren - 1; i++)
//         {
//             currentGeneration.Add(tierList.Mutate(20, 1.0));
//         }
//     }

//     void SaveGeneration()
//     {
//         //
//     }

//     List<TierListEv> SelectChildren(List<TierListEv> children)
//     {
//         List<(int, int)> wins = new(children.Count);
//         for (int i = 0; i < wins.Count; i++)
//         {
//             wins[i] = (0, i);
//         }

//         int totalForOneChild = numberOfFightsOneSite * 2 * currentGeneration.Count;

//         for (int i = 0; i < children.Count; i++)
//         {
//             for (int j = 0; j < currentGeneration.Count; j++)
//             {
//                 for (int k = 0; k < numberOfFightsOneSite; k++)
//                 {
//                     var game = new ScriptsOfTribute.AI.ScriptsOfTribute(population[thread_index * 2 + j], population[thread_index * 2 + 1 + j]);
//                     var (endState, endBoardState) = game.Play();

//                     if (endState.Winner == PlayerEnum.PLAYER1)
//                     {
//                         winners[j / 2 + thread_index] = population[thread_index * 2 + j];
//                     }
//                     else
//                     {
//                         winners[j / 2 + thread_index] = population[thread_index * 2 + 1 + j];
//                     }
//                 }
//             }
//         }

//         SaveGeneration();

//         return best;
//     }

//     public void NextGeneration()
//     {
//         List<TierListEv> children = new();
//         children.AddRange(currentGeneration);
//         for (int i = 0; i < children.Count; i++)
//         {
//             for (int j = i + 1; j < children.Count; j++)
//             {
//                 TierListEv child = TierListEv.Combine(new List<TierListEv> { children[i], children[j] });
//                 children.Add(child.Mutate(3, 0.05));
//             }
//         }

//         children = SelectChildren(children);

//         currentGenerationNumber++;
//         currentGeneration = children;
//     }

// }

// class Program
// {
//     static void Main(string[] args)
//     {
//         Generation g = new();
//         g.CreateFirstGeneration(new TierListEv());
//         while (true)
//         {
//             g.NextGeneration();
//         }
//     }

// }