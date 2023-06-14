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
    static readonly Dictionary<Param, int[]> weight = new Dictionary<Param, int[]> {
        { Param.OurPrestige,   new int[] { 0, 0, 0 } },
        { Param.EnemyPrestige, new int[] { 0, 0, 0 } },
        { Param.CardLimit,     new int[] { 0, 0, 0 } },
        { Param.ComboPower,    new int[] { 2, 0, 0 } },
        { Param.OurAgent,      new int[] { 5, 0, 0 } },
        { Param.EnemyAgent,    new int[] { 5, 0, 0 } },
        { Param.OverCardLimit, new int[] { 0, 0, 0 } },
        { Param.CurrentPhase,  new int[] { 0, 0, 0 } },
        { Param.UpcomingCard,  new int[] { 0, 0, 0 } },
    };
    const int heuristicMin = -500;
    const int heuristicMax = 500;
    readonly int cardCount;
    readonly GamePhase currentGamePhase;

    public GameStrategy(int cardCount, GamePhase currentGamePhase)
    {
        this.cardCount = cardCount;
        this.currentGamePhase = currentGamePhase;
    }
    int GetWeight(Param param) => weight[param][(int)currentGamePhase];
    bool IsTaunt(string agentName) => Array.Exists(taunts, name => name == agentName);

    int BasicProperties(SeededGameState gameState)
    {
        int value = 0;
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
    int PatronsBonuses(SeededGameState gameState)
    {
        int val = 0;
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


    int CardsValues(SeededGameState gameState)
    {
        int cardLimit = Math.Max(cardCount, 100);
        int value = 0;

        var ourCardsHand = gameState.CurrentPlayer.Hand;
        var ourCardsDraw = gameState.CurrentPlayer.DrawPile;
        var ourCardsPlayed = gameState.CurrentPlayer.Played;
        var ourCardsCooldown = gameState.CurrentPlayer.CooldownPile;

        var enemyCardsHand = gameState.EnemyPlayer.Hand;
        var enemyCardsDraw = gameState.EnemyPlayer.DrawPile;
        var enemyCardsPlayed = gameState.EnemyPlayer.Played;
        var enemyCardsCooldown = gameState.EnemyPlayer.CooldownPile;

        var ourCombos = new Dictionary<PatronId, int>();
        var enemyCombos = new Dictionary<PatronId, int>();

        int ourCardsCount = ourCardsHand.Count + ourCardsDraw.Count + ourCardsPlayed.Count + ourCardsCooldown.Count;
        value -= Math.Max((ourCardsCount - cardLimit) * GetWeight(Param.OverCardLimit), 0);

        foreach (UniqueCard card in ourCardsDraw)
        {
            value += (int)GPCardTierList.GetCardTier(card.Name, currentGamePhase);
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

        foreach (UniqueCard card in enemyCardsDraw)
        {
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

        // foreach (UniqueCard card in ourCardsRest)
        // {
        //     val += (int)GPCardTierList.GetCardTier(card.Name, 2) / 2;
        //     if (card.Deck != PatronId.TREASURY)
        //     {
        //         if (ourCombos.ContainsKey(card.Deck))
        //         {
        //             ourCombos[card.Deck] += 1;
        //         }
        //         else
        //         {
        //             ourCombos[card.Deck] = 1;
        //         }
        //     }
        // }
        // foreach (UniqueCard card in enemyCardsRest)
        // {
        //     val -= (int)GPCardTierList.GetCardTier(card.Name, 2) / 2;
        //     if (card.Deck != PatronId.TREASURY)
        //     {
        //         if (enemyCombos.ContainsKey(card.Deck))
        //         {
        //             enemyCombos[card.Deck] += 1;
        //         }
        //         else
        //         {
        //             enemyCombos[card.Deck] = 1;
        //         }
        //     }
        // }


        // foreach (UniqueCard card in ourCards)
        // {
        //     val += (int)GPCardTierList.GetCardTier(card.Name, 0);
        //     if (card.Deck != PatronId.TREASURY)
        //     {
        //         if (ourCombos.ContainsKey(card.Deck))
        //         {
        //             ourCombos[card.Deck] += 1;
        //         }
        //         else
        //         {
        //             ourCombos[card.Deck] = 1;
        //         }
        //     }
        //     if (card.Deck == PatronId.DUKE_OF_CROWS) val += 5;

        // }
        // foreach (UniqueCard card in enemyCards)
        // {
        //     val -= (int)GPCardTierList.GetCardTier(card.Name, 0);
        //     if (card.Deck != PatronId.TREASURY)
        //     {
        //         if (enemyCombos.ContainsKey(card.Deck))
        //         {
        //             enemyCombos[card.Deck] += 1;
        //         }
        //         else
        //         {
        //             enemyCombos[card.Deck] = 1;
        //         }
        //     }
        // }

        // value += (int)(CombosValue(ourCombos) - CombosValue(enemyCombos));
        // foreach (UniqueCard card in gameState.CurrentPlayer.KnownUpcomingDraws)
        // {
        //     value += (int)HandTierList.GetCardTier(card.Name) * weights.GetWeight(Param.UpcomingCard);
        //     if (ourCombos.ContainsKey(card.Deck))
        //     {
        //         value += ourCombos[card.Deck] * knowingCardCombo;
        //     }
        // }
        // foreach (UniqueCard card in gameState.EnemyPlayer.KnownUpcomingDraws)
        // {
        //     value -= (int)HandTierList.GetCardTier(card.Name) * weights.GetWeight(Param.UpcomingCard);
        //     if (enemyCombos.ContainsKey(card.Deck))
        //     {
        //         value -= ourCombos[card.Deck] * knowingCardCombo;
        //     }
        // }

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
        int after40bonus = 30;
        int patronsBonuses = PatronsBonuses(gameState);
        if (patronsBonuses == heuristicMax) return 1;
        if (patronsBonuses == 3 * heuristicMin) return 0;
        int basicProperties = BasicProperties(gameState);
        int cardsValues = CardsValues(gameState);
        int val = basicProperties + patronsBonuses + cardsValues;
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
            val = after40bonus * (afterRoundPoint - gameState.EnemyPlayer.Prestige) + val / 10;
            // if(patronsBonuses == heuristicMin){
            //     val -= 2*after40bonus;
            // }
        }
        return ((double)Math.Clamp(val + heuristicMax, 0.0, 2.0 * heuristicMax) / (2.0 * heuristicMax));
    }

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
    OverCardLimit,
    CurrentPhase,
    UpcomingCard,
}