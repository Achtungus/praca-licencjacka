// using ScriptsOfTribute;
// using System.Diagnostics;

// namespace Evolution;

// public class CardTierEv
// {
//     public string Name;
//     public PatronId Deck;
//     public List<int> Tier;
//     public static readonly int maxTierValue = 100;
//     public static readonly int minTierValue = -20;

//     public CardTierEv(string name, PatronId deck, int tierEarly, int tierMid, int tierLate)
//     {
//         this.Name = name;
//         this.Deck = deck;
//         this.Tier = new List<int> {
//             Math.Clamp(tierEarly, minTierValue, maxTierValue),
//             Math.Clamp(tierMid, minTierValue, maxTierValue),
//             Math.Clamp(tierLate, minTierValue, maxTierValue),
//         };
//     }

//     public CardTierEv Copy()
//     {
//         return new CardTierEv(Name, Deck, Tier[0], Tier[1], Tier[2]);
//     }
// }

// public class TierListEv
// {
//     static Random rnd = new Random();
//     static List<CardTierEv> initTierList = new List<CardTierEv> {
//         new CardTierEv("Currency Exchange", PatronId.HLAALU, 50, 50, 30),
//         new CardTierEv("Luxury Exports", PatronId.HLAALU, 50, 50, 3),
//         new CardTierEv("Oathman", PatronId.HLAALU, 30, 30, 10),
//         new CardTierEv("Hlaalu Councilor", PatronId.HLAALU, 30, 30, 3),
//         new CardTierEv("Hlaalu Kinsman", PatronId.HLAALU, 30, 30, 3),
//         new CardTierEv("House Embassy", PatronId.HLAALU, 30, 30, 3),
//         new CardTierEv("House Marketplace", PatronId.HLAALU, 10, 30, 3),
//         new CardTierEv("Hireling", PatronId.HLAALU, 3, 3, 1),
//         new CardTierEv("Hostile Takeover", PatronId.HLAALU, 10, 3, 1),
//         new CardTierEv("Customs Seizure", PatronId.HLAALU, 1, 1, 1),
//         new CardTierEv("Goods Shipment", PatronId.HLAALU, 1, 1, 1),

//         new CardTierEv("Midnight Raid", PatronId.RED_EAGLE, 50, 50, 50),
//         new CardTierEv("Hagraven", PatronId.RED_EAGLE, 1, 10, 1),
//         new CardTierEv("Hagraven Matron", PatronId.RED_EAGLE, 1, 30, 3),
//         new CardTierEv("War Song", PatronId.RED_EAGLE, 1, 1, 1),

//         new CardTierEv("Blackfeather Knave", PatronId.DUKE_OF_CROWS, 50, 50, 30),
//         new CardTierEv("Plunder", PatronId.DUKE_OF_CROWS, 50, 50, 50),
//         new CardTierEv("Toll of Flesh", PatronId.DUKE_OF_CROWS, 50, 50, 30),
//         new CardTierEv("Toll of Silver", PatronId.DUKE_OF_CROWS, 50, 50, 30),
//         new CardTierEv("Murder of Crows", PatronId.DUKE_OF_CROWS, 50, 50, 30),
//         new CardTierEv("Pilfer", PatronId.DUKE_OF_CROWS, 30, 50, 30),
//         new CardTierEv("Squawking Oratory", PatronId.DUKE_OF_CROWS, 30, 50, 30),
//         new CardTierEv("Pool of Shadow", PatronId.DUKE_OF_CROWS, 10, 30, 10),
//         new CardTierEv("Scratch", PatronId.DUKE_OF_CROWS, 30, 30, 10),
//         new CardTierEv("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, 3, 3, 3),
//         new CardTierEv("Blackfeather Knight", PatronId.DUKE_OF_CROWS, 3, 10, 3),
//         new CardTierEv("Peck", PatronId.DUKE_OF_CROWS, 3, 3, 3),

//         new CardTierEv("Conquest", PatronId.ANSEI, 30, 30, 10),
//         new CardTierEv("Hira's End", PatronId.ANSEI, 50, 50, 50),
//         new CardTierEv("Hel Shira Herald", PatronId.ANSEI, 10, 30, 10),
//         new CardTierEv("March on Hattu", PatronId.ANSEI, 30, 30, 30),
//         new CardTierEv("Shehai Summoning", PatronId.ANSEI, 10, 10, 10),
//         new CardTierEv("Warrior Wave", PatronId.ANSEI, 50, 30, 10),
//         new CardTierEv("Ansei Assault", PatronId.ANSEI, 10, 30, 10),
//         new CardTierEv("Ansei's Victory", PatronId.ANSEI, 10, 30, 10),
//         new CardTierEv("No Shira Poet", PatronId.ANSEI, 3, 3, 3),
//         new CardTierEv("Way of the Sword", PatronId.ANSEI, 1, 1, 1),

//         new CardTierEv("Rally", PatronId.PELIN, 30, 50, 30),
//         new CardTierEv("Siege Weapon Volley", PatronId.PELIN, 30, 50, 10),
//         new CardTierEv("The Armory", PatronId.PELIN, 30, 50, 30),
//         new CardTierEv("Banneret", PatronId.PELIN, 30, 50, 30),
//         new CardTierEv("Knight Commander", PatronId.PELIN, 50, 50, 30),
//         new CardTierEv("Reinforcements", PatronId.PELIN, 50, 30, 10),
//         new CardTierEv("Archers' Volley", PatronId.PELIN, 10, 30, 10),
//         new CardTierEv("Legion's Arrival", PatronId.PELIN, 30, 30, 10),
//         new CardTierEv("Bangkorai Sentries", PatronId.PELIN, 3, 10, 3),
//         new CardTierEv("Knights of Saint Pelin", PatronId.PELIN, 3, 30, 3),
//         new CardTierEv("The Portcullis", PatronId.PELIN, 3, 3, 1),
//         new CardTierEv("Fortify", PatronId.PELIN, 1, 1, 1),

//         new CardTierEv("Bewilderment", PatronId.RAJHIN, -3, -3, -3),
//         new CardTierEv("Grand Larceny", PatronId.RAJHIN, 30, 30, 10),
//         new CardTierEv("Jarring Lullaby", PatronId.RAJHIN, 10, 30, 10),
//         new CardTierEv("Jeering Shadow", PatronId.RAJHIN, 3, 3, 3),
//         new CardTierEv("Pounce and Profit", PatronId.RAJHIN, 50, 50, 10),
//         new CardTierEv("Prowling Shadow", PatronId.RAJHIN, 10, 3, 3),
//         new CardTierEv("Shadow's Slumber", PatronId.RAJHIN, 30, 30, 10),
//         new CardTierEv("Slight of Hand", PatronId.RAJHIN, 30, 10, 1),
//         new CardTierEv("Stubborn Shadow", PatronId.RAJHIN, 1, 3, 1),
//         new CardTierEv("Twilight Revelry", PatronId.RAJHIN, 10, 30, 10),
//         new CardTierEv("Swipe", PatronId.RAJHIN, 1, 1, 1),

//         new CardTierEv("Gold", PatronId.TREASURY, 0, -3, -3),
//         new CardTierEv("Writ of Coin", PatronId.TREASURY, 50, 10, 0),
//     };

//     List<CardTierEv> cardTierList = new();

//     public TierListEv(List<CardTierEv> cardTierList)
//     {
//         foreach (CardTierEv cardTier in cardTierList)
//         {
//             this.cardTierList.Add(cardTier.Copy());
//         }
//     }
//     public TierListEv()
//     {
//         foreach (CardTierEv cardTier in initTierList)
//         {
//             this.cardTierList.Add(cardTier.Copy());
//         }
//     }

//     public TierListEv Mutate(int range, double chance)
//     {
//         TierListEv child = new(this.cardTierList);
//         for (int i = 0; i < child.cardTierList.Count; i++)
//         {
//             for (int j = 0; j < 3; j++)
//             {
//                 if (rnd.NextDouble() < chance)
//                 {
//                     child.cardTierList[i].Tier[j] += rnd.Next(-range, range);
//                 }
//             }
//         }
//         return child;
//     }

//     public static TierListEv Combine(List<TierListEv> parents)
//     {
//         const int maxPatronId = 9;
//         List<int> patronToParent = new(maxPatronId + 1);
//         for (int i = 0; i < patronToParent.Count; i++)
//         {
//             patronToParent[i] = rnd.Next(0, parents.Count - 1);
//         }

//         TierListEv child = new(initTierList);
//         for (int i = 0; i < child.cardTierList.Count; i++)
//         {
//             int parent = patronToParent[(int)child.cardTierList[i].Deck];
//             Debug.Assert(child.cardTierList[i].Name == parents[parent].cardTierList[i].Name);
//             child.cardTierList[i] = parents[parent].cardTierList[i].Copy();
//         }
//         return child;
//     }

//     public int GetCardTier(string cardName, int gamePhase)
//     {
//         return cardTierList.Find(x => x.Name == cardName).Tier[gamePhase];
//     }
// }