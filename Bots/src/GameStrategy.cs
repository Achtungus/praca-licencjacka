using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

public class GameStrategy
{
    static readonly string[] taunts = new string[] {
        "Stubborn Shadow", "Banneret", "Knight Commander", "Shield Bearer", "Bangkorai Sentries", "Knights of Saint Pelin"
    };
    static readonly Dictionary<Param, double[]> weight = new Dictionary<Param, double[]> {
        { Param.OurPrestige,   new double[] { 10, 60, 200 } },
        { Param.EnemyPrestige, new double[] { -10, -60, -200 } },
        { Param.CardLimit,     new double[] { 20, 17, 17 } }, // do zbadania
        { Param.ComboPower,    new double[] { 3, 3, 3 } }, 
        { Param.OurAgent,      new double[] { 5, 5, 5 } }, // niezauwazalne
        { Param.EnemyAgent,    new double[] { -60, -80, -150 } },
        { Param.OverCardLimitPenalty, new double[] { 805, 805, 805 } },
        { Param.UpcomingCard,  new double[] { 15, 25, 100 } },
        { Param.TierMultiplier, new double[] {10, 10, 10}},
        { Param.KnowingCardCombo, new double [] {1, 1, 1}}, //epsilon
        { Param.After40Bonus, new double [] {300, 300, 300}},
    };

    static private int stalaCoriolisa = 300;
    static private List<int> comboBonus = new List<int>() { 1, 20, 100, 211, 540 };
    const int heuristicMin = -10000;
    const int heuristicMax = 10000;
    readonly int cardCount;
    readonly GamePhase currentGamePhase;

    public GameStrategy(int cardCount, GamePhase currentGamePhase)
    {
        this.cardCount = cardCount;
        this.currentGamePhase = currentGamePhase;
    }
    double GetWeight(Param param) => weight[param][(int)currentGamePhase];
    bool IsTaunt(string agentName) => Array.Exists(taunts, name => name == agentName);

    double BasicProperties(SeededGameState gameState)
    {
        double value = 0;
        int power = gameState.CurrentPlayer.Power;
        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            if (IsTaunt(agent.RepresentingCard.Name))
            {
                power -= agent.CurrentHp;
            }
        }
        power = Math.Max(power, 0);
        int afterRoundPoints = gameState.CurrentPlayer.Prestige + power;
        value += afterRoundPoints * GetWeight(Param.OurPrestige);
        value += gameState.EnemyPlayer.Prestige * GetWeight(Param.EnemyPrestige);
        return value;
    }
    double PatronsBonuses(SeededGameState gameState)
    {
        double val = 0;
        int enemyPatronDist = 0;
        int ourPatrons = 0;
        foreach (var (patron, owner) in gameState.PatronStates.All)
        {
            if (owner == gameState.CurrentPlayer.PlayerID)
            {
                switch (patron)
                {
                    case PatronId.TREASURY:
                        break;
                    case PatronId.ANSEI:
                        enemyPatronDist += 1;
                        break;
                    default:
                        enemyPatronDist += 2;
                        break;
                }
                if (patron != PatronId.TREASURY)
                    ourPatrons += 1;
                val += PatronTierList.GetPatronTier(patron, currentGamePhase).favoured;
            }
            else if (owner == PlayerEnum.NO_PLAYER_SELECTED)
            {
                enemyPatronDist += 1;
                val += PatronTierList.GetPatronTier(patron, currentGamePhase).neutral;
            }
            else
            {
                val += PatronTierList.GetPatronTier(patron, currentGamePhase).unfavoured;
            }
        }
        if (enemyPatronDist == 1) return 3 * heuristicMin;
        if (enemyPatronDist == 2) val = heuristicMin;
        if (ourPatrons == 4) val = heuristicMax;
        return val;
    }


    double CardsValues(SeededGameState gameState)
    {
        int cardLimit = Math.Max(cardCount, (int)GetWeight(Param.CardLimit));
        double value = 0;

        var ourCardsDraw = gameState.CurrentPlayer.DrawPile;
        var ourCardsHand = gameState.CurrentPlayer.Hand;
        var ourCardsPlayed = gameState.CurrentPlayer.Played;
        var ourCardsCooldown = gameState.CurrentPlayer.CooldownPile;

        var enemyCardsDraw = gameState.EnemyPlayer.DrawPile;
        var enemyCardsHand = gameState.EnemyPlayer.Hand;
        var enemyCardsPlayed = gameState.EnemyPlayer.Played;
        var enemyCardsCooldown = gameState.EnemyPlayer.CooldownPile;

        var ourCombos = new Dictionary<PatronId, int>();
        var enemyCombos = new Dictionary<PatronId, int>();

        int ourCardsCount = ourCardsHand.Count + ourCardsDraw.Count + ourCardsPlayed.Count + ourCardsCooldown.Count;
        value -= Math.Max((ourCardsCount - cardLimit) * GetWeight(Param.OverCardLimitPenalty), 0);

        foreach (UniqueCard card in ourCardsDraw)
        {
            value += GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (ourCombos.ContainsKey(card.Deck))
                {
                    ourCombos[card.Deck] += 1;
                }
                else
                {
                    ourCombos[card.Deck] = 1;
                }
            }
        }
        foreach (UniqueCard card in ourCardsHand)
        {
            value += GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (ourCombos.ContainsKey(card.Deck))
                {
                    ourCombos[card.Deck] += 1;
                }
                else
                {
                    ourCombos[card.Deck] = 1;
                }
            }
        }
        foreach (UniqueCard card in ourCardsPlayed)
        {
            value += GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (ourCombos.ContainsKey(card.Deck))
                {
                    ourCombos[card.Deck] += 1;
                }
                else
                {
                    ourCombos[card.Deck] = 1;
                }
            }
        }
        foreach (UniqueCard card in ourCardsCooldown)
        {
            value += GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (ourCombos.ContainsKey(card.Deck))
                {
                    ourCombos[card.Deck] += 1;
                }
                else
                {
                    ourCombos[card.Deck] = 1;
                }
            }
        }

        //enemy
        foreach (UniqueCard card in enemyCardsDraw)
        {
            value -= GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (enemyCombos.ContainsKey(card.Deck))
                {
                    enemyCombos[card.Deck] += 1;
                }
                else
                {
                    enemyCombos[card.Deck] = 1;
                }
            }
        }
        foreach (UniqueCard card in enemyCardsHand)
        {
            value -= GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (enemyCombos.ContainsKey(card.Deck))
                {
                    enemyCombos[card.Deck] += 1;
                }
                else
                {
                    enemyCombos[card.Deck] = 1;
                }
            }
        }
        foreach (UniqueCard card in enemyCardsPlayed)
        {
            value -= GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (enemyCombos.ContainsKey(card.Deck))
                {
                    enemyCombos[card.Deck] += 1;
                }
                else
                {
                    enemyCombos[card.Deck] = 1;
                }
            }
        }
        foreach (UniqueCard card in enemyCardsCooldown)
        {
            value -= GPCardTierList.GetCardTier(card.Name, currentGamePhase) * GetWeight(Param.TierMultiplier);
            if (card.Deck != PatronId.TREASURY)
            {
                if (enemyCombos.ContainsKey(card.Deck))
                {
                    enemyCombos[card.Deck] += 1;
                }
                else
                {
                    enemyCombos[card.Deck] = 1;
                }
            }
        }


        value += CombosValue(ourCombos) - CombosValue(enemyCombos);
        foreach (UniqueCard card in gameState.CurrentPlayer.KnownUpcomingDraws)
        {
            value += (int)HandTierList.GetCardTier(card.Name) * GetWeight(Param.UpcomingCard);
            if (ourCombos.ContainsKey(card.Deck))
            {
                value += ourCombos[card.Deck] * GetWeight(Param.KnowingCardCombo);
            }
        }
        foreach (UniqueCard card in gameState.EnemyPlayer.KnownUpcomingDraws)
        {
            value -= (int)HandTierList.GetCardTier(card.Name) * GetWeight(Param.UpcomingCard);
            if (enemyCombos.ContainsKey(card.Deck))
            {
                value -= ourCombos[card.Deck] * GetWeight(Param.KnowingCardCombo);
            }
        }

        foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents)
        {
            value += agent.CurrentHp * GetWeight(Param.OurPrestige) + GetWeight(Param.OurAgent);
        }

        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            value += AgentTier.GetCardTier(agent.RepresentingCard.Name) * GetWeight(Param.EnemyAgent) - agent.CurrentHp;
        }
        return value;
    }

    public double Heuristic(SeededGameState gameState)
    {
        double patronsBonuses = PatronsBonuses(gameState);
        if (patronsBonuses == heuristicMax) return 1;
        if (patronsBonuses == 3 * heuristicMin) return 0;
        double basicProperties = BasicProperties(gameState);
        double cardsValues = CardsValues(gameState);
        double val = basicProperties + patronsBonuses + cardsValues;
        int power = gameState.CurrentPlayer.Power;
        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            if (IsTaunt(agent.RepresentingCard.Name)) power -= agent.CurrentHp;
        }
        power = Math.Max(power, 0);
        int afterRoundPoint = gameState.CurrentPlayer.Prestige + power;
        if (gameState.EnemyPlayer.Prestige >= 40 && afterRoundPoint <= gameState.EnemyPlayer.Prestige) return 0;
        if (afterRoundPoint >= 80) return 1;
        if (afterRoundPoint >= 40)
        {
            val += GetWeight(Param.After40Bonus) * (afterRoundPoint - gameState.EnemyPlayer.Prestige);
        }
        return ((double)Math.Clamp(val + heuristicMax, 0.0, 2.0 * heuristicMax) / (2.0 * heuristicMax));
    }

    // double CombosValue(Dictionary<PatronId, int> dict)
    // {
    //     double val = 0;
    //     int cnt = 0;
    //     foreach (KeyValuePair<PatronId, int> el in dict)
    //     {
    //         cnt += el.Value;
    //     }
    //     double wsp = 1;
    //     if (cnt > GetWeight(Param.CardLimit))
    //     {
    //         wsp = GetWeight(Param.CardLimit) / cnt;
    //     }
    //     foreach (KeyValuePair<PatronId, int> el in dict)
    //     {
    //         val += comboBonus[Math.Min(comboBonus.Count-1, el.Value)] + Math.Max(0, (el.Value - comboBonus.Count)*stalaCoriolisa); // moze dodac wspolczynnik
    //     }
    //     return val*wsp;
    // }
    double CombosValue(Dictionary<PatronId, int> dict)
    {
        double val = 0;
        int cnt = 0;
        foreach (KeyValuePair<PatronId, int> el in dict)
        {
            cnt += el.Value;
        }
        double wsp = 1;
        if (cnt > GetWeight(Param.CardLimit))
        {
            wsp = GetWeight(Param.CardLimit) / cnt;
        }
        foreach (KeyValuePair<PatronId, int> el in dict)
        {
            val += Math.Pow(wsp * el.Value, GetWeight(Param.ComboPower));
        }
        return val;
    }
}

public enum GamePhase
{
    EarlyGame = 0,
    MidGame = 1,
    LateGame = 2,
}

public enum Param
{
    OurPrestige,
    EnemyPrestige,
    CardLimit,
    ComboPower,
    OurAgent,
    EnemyAgent,
    OverCardLimitPenalty,
    UpcomingCard,
    TierMultiplier,
    KnowingCardCombo,
    After40Bonus,
}