using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace ParamEvolution3;

public class GameStrategy
{
    static readonly HashSet<CardId> taunts = new HashSet<CardId> {
        CardId.STUBBORN_SHADOW, CardId.BANNERET, CardId.KNIGHT_COMMANDER, CardId.SHIELD_BEARER, CardId.BANGKORAI_SENTRIES, CardId.KNIGHTS_OF_SAINT_PELIN
    };

    const int heuristicMin = -10000;
    const int heuristicMax = 10000;
    readonly int cardCount;
    readonly GamePhase currentGamePhase;
    readonly GameParams gameParams;

    public GameStrategy(int cardCount, GamePhase currentGamePhase, GameParams gameParams)
    {
        this.cardCount = cardCount;
        this.currentGamePhase = currentGamePhase;
        this.gameParams = gameParams;
    }

    double GetWeight(Param param) => gameParams.GetParamValue(param, currentGamePhase);
    bool IsTaunt(CardId agentId) => taunts.Contains(agentId);

    double BasicProperties(SeededGameState gameState)
    {
        double value = 0;
        int power = gameState.CurrentPlayer.Power;
        foreach (var agent in gameState.EnemyPlayer.Agents)
        {
            if (IsTaunt(agent.RepresentingCard.CommonId))
            {
                power -= agent.CurrentHp;
            }
        }
        power = Math.Max(power, 0);
        int afterRoundPoints = gameState.CurrentPlayer.Prestige + power;
        value += afterRoundPoints * GetWeight(Param.Prestige);
        value -= gameState.EnemyPlayer.Prestige * GetWeight(Param.Prestige);
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
                    case PatronId.TREASURY: // moze do wywalenia
                        break;
                    case PatronId.ANSEI:
                        enemyPatronDist += 1;
                        break;
                    default:
                        enemyPatronDist += 2;
                        break;
                }

                if (patron != PatronId.TREASURY)
                {
                    ourPatrons += 1;
                }
                if (patron == PatronId.DUKE_OF_CROWS) val += PatronTierList.GetPatronTier(patron, currentGamePhase).favoured * GetWeight(Param.Crow);
                if (patron == PatronId.ANSEI) val += PatronTierList.GetPatronTier(patron, currentGamePhase).favoured * GetWeight(Param.Ansei);
            }
            else if (owner == PlayerEnum.NO_PLAYER_SELECTED)
            {
                if (patron != PatronId.TREASURY)
                {
                    enemyPatronDist += 1;
                }
                if (patron == PatronId.DUKE_OF_CROWS) val += PatronTierList.GetPatronTier(patron, currentGamePhase).neutral * GetWeight(Param.Crow);
                if (patron == PatronId.ANSEI) val += PatronTierList.GetPatronTier(patron, currentGamePhase).neutral * GetWeight(Param.Ansei);
            }
            else
            {
                if (patron == PatronId.DUKE_OF_CROWS) val += PatronTierList.GetPatronTier(patron, currentGamePhase).unfavoured * GetWeight(Param.Crow);
                if (patron == PatronId.ANSEI) val += PatronTierList.GetPatronTier(patron, currentGamePhase).unfavoured * GetWeight(Param.Ansei);
            }
        }
        if (enemyPatronDist == 1) return 3 * heuristicMin;
        if (enemyPatronDist == 2) val = heuristicMin;
        if (ourPatrons == 4) val = heuristicMax;
        return val;
    }


    double CardsValues(SeededGameState gameState)
    {
        double value = 0;

        var ourCardsDraw = gameState.CurrentPlayer.DrawPile;
        var ourCardsHand = gameState.CurrentPlayer.Hand;
        var ourCardsPlayed = gameState.CurrentPlayer.Played;
        var ourCardsCooldown = gameState.CurrentPlayer.CooldownPile;
        var ourCards = ourCardsDraw.Concat(ourCardsHand.Concat(ourCardsPlayed.Concat(ourCardsCooldown)));

        var enemyCardsDraw = gameState.EnemyPlayer.DrawPile;
        var enemyCardsHand = gameState.EnemyPlayer.Hand;
        var enemyCardsPlayed = gameState.EnemyPlayer.Played;
        var enemyCardsCooldown = gameState.EnemyPlayer.CooldownPile;
        var enemyCards = enemyCardsDraw.Concat(enemyCardsHand.Concat(enemyCardsPlayed.Concat(enemyCardsCooldown)));

        double wspOur = Math.Min(1.0, GetWeight(Param.CardLimit) / (double)ourCards.Count());
        double wspEnemy = Math.Min(1.0, GetWeight(Param.CardLimit) / (double)enemyCards.Count());

        var ourCombos = new Dictionary<PatronId, int>();
        var enemyCombos = new Dictionary<PatronId, int>();

        foreach (UniqueCard card in ourCards)
        {
            value += GPCardTierList.GetCardTier((int)card.CommonId, currentGamePhase) * GetWeight(Param.TierMultiplier) * wspOur;
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

        foreach (UniqueCard card in enemyCards)
        {
            value -= GPCardTierList.GetCardTier((int)card.CommonId, currentGamePhase) * GetWeight(Param.TierMultiplier) * wspEnemy;
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

        List<double> agentBonuses = new();
        foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents)
        {
            if (agent.RepresentingCard.Type != CardType.CONTRACT_AGENT)
            {
                value += GPCardTierList.GetCardTier((int)agent.RepresentingCard.CommonId, currentGamePhase) * GetWeight(Param.TierMultiplier);
                if (ourCombos.ContainsKey(agent.RepresentingCard.Deck))
                {
                    ourCombos[agent.RepresentingCard.Deck] += 1;
                }
                else
                {
                    ourCombos[agent.RepresentingCard.Deck] = 1;
                }
            }
            double toAdd = agent.CurrentHp * GetWeight(Param.Prestige) * GetWeight(Param.OurAgent);
            agentBonuses.Add(toAdd);
            value += toAdd;
        }

        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            if (agent.RepresentingCard.Type != CardType.CONTRACT_AGENT)
            {
                value -= GPCardTierList.GetCardTier((int)agent.RepresentingCard.CommonId, currentGamePhase) * GetWeight(Param.TierMultiplier);
                if (enemyCombos.ContainsKey(agent.RepresentingCard.Deck))
                {
                    enemyCombos[agent.RepresentingCard.Deck] += 1;
                }
                else
                {
                    enemyCombos[agent.RepresentingCard.Deck] = 1;
                }
            }
            value += AgentTier.GetCardTier(agent.RepresentingCard.CommonId) * GetWeight(Param.EnemyAgent) * (((double)agent.CurrentHp + 2) / ((double)agent.RepresentingCard.HP) + 2);
            // to u gory nie jestem pewiem
        }

        value += CombosValue(ourCombos, wspOur) - CombosValue(enemyCombos, wspEnemy);

        foreach (UniqueCard card in gameState.CurrentPlayer.KnownUpcomingDraws.Take(3))
        {
            value += HandTierList.GetCardTier(card.CommonId) * GetWeight(Param.UpcomingCard);
            if (ourCombos.ContainsKey(card.Deck))
            {
                value += ourCombos[card.Deck] * GetWeight(Param.KnowingCardCombo) * wspOur;
            }
        }

        foreach (UniqueCard card in gameState.EnemyPlayer.KnownUpcomingDraws.Take(3))
        {
            value -= HandTierList.GetCardTier(card.CommonId) * GetWeight(Param.UpcomingCard);
            if (enemyCombos.ContainsKey(card.Deck))
            {
                value -= ourCombos[card.Deck] * GetWeight(Param.KnowingCardCombo) * wspEnemy;
            }
        }

        int howManyToCut = 0;
        foreach (UniqueCard card in gameState.TavernAvailableCards)
        {
            if (card.Type != CardType.CONTRACT_ACTION && card.Type != CardType.CONTRACT_AGENT)
            {
                value += enemyCardEvaluation(card, gameState) * GetWeight(Param.TavernPenatly);
            }
            if (card.CommonId == CardId.AMBUSH) howManyToCut += 2;
            if (card.CommonId == CardId.BLACK_SACRAMENT) howManyToCut += 1;
        }
        agentBonuses.Sort();
        agentBonuses.Reverse();
        // Kara za knockout agent w tawernie jeśli mamy agentów
        foreach (double v in agentBonuses.Take(howManyToCut))
        {
            value -= v * (0.75);
        }

        return value;
    }

    public double Heuristic(SeededGameState gameState)
    {
        double patronsBonuses = PatronsBonuses(gameState);

        if (patronsBonuses == heuristicMax)
        {
            return 1;
        }

        if (patronsBonuses == 3 * heuristicMin)
        {
            return 0;
        }

        double basicProperties = BasicProperties(gameState);
        double cardsValues = CardsValues(gameState);
        double result = basicProperties + patronsBonuses + cardsValues;

        int power = gameState.CurrentPlayer.Power;
        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            if (IsTaunt(agent.RepresentingCard.CommonId))
            {
                power -= agent.CurrentHp;
            }
        }
        power = Math.Max(power, 0);

        int afterRoundPoint = gameState.CurrentPlayer.Prestige + power;

        if (gameState.EnemyPlayer.Prestige >= 40 && afterRoundPoint <= gameState.EnemyPlayer.Prestige)
        {
            return 0;
        }

        if (afterRoundPoint >= 80)
        {
            return 1;
        }

        if (afterRoundPoint >= 40)
        {
            result += (afterRoundPoint - gameState.EnemyPlayer.Prestige) * GetWeight(Param.After40Bonus);
        }

        // Console.WriteLine(((double)Math.Clamp(val + heuristicMax, 0.0, 2.0 * heuristicMax) / (2.0 * heuristicMax)));
        return ((double)Math.Clamp(result + heuristicMax, 0.0, 2.0 * heuristicMax) / (2.0 * heuristicMax));
    }

    double CombosValue(Dictionary<PatronId, int> dict, double wsp = 1)
    {
        double val = 0;
        int cnt = 0;
        foreach (KeyValuePair<PatronId, int> el in dict)
        {
            cnt += el.Value;
        }
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
    public double CardEvaluation(UniqueCard card, SeededGameState gameState)
    {
        double val = GPCardTierList.GetCardTier((int)card.CommonId, currentGamePhase);

        var ourCombos = new Dictionary<PatronId, int>();
        ourCombos[card.Deck] = 0;

        foreach (UniqueCard c in gameState.CurrentPlayer.DrawPile)
        {
            if (c.Deck == card.Deck) ourCombos[c.Deck] += 1;
        }
        foreach (UniqueCard c in gameState.CurrentPlayer.Hand)
        {
            if (c.Deck == card.Deck) ourCombos[c.Deck] += 1;
        }
        foreach (UniqueCard c in gameState.CurrentPlayer.Played)
        {
            if (c.Deck == card.Deck) ourCombos[c.Deck] += 1;
        }
        foreach (UniqueCard c in gameState.CurrentPlayer.CooldownPile)
        {
            if (c.Deck == card.Deck) ourCombos[c.Deck] += 1;
        }

        val += CombosValue(ourCombos);
        if (ourCombos[card.Deck] > 1)
        {
            ourCombos[card.Deck] -= 1;
            val -= CombosValue(ourCombos);
        }
        return val;
    }
    public double enemyCardEvaluation(UniqueCard card, SeededGameState gameState)
    {
        double val = GPCardTierList.GetCardTier((int)card.CommonId, currentGamePhase);

        var enemyCombos = new Dictionary<PatronId, int>();
        enemyCombos[card.Deck] = 0;
        foreach (UniqueCard c in gameState.CurrentPlayer.DrawPile)
        {
            if (c.Deck == card.Deck) enemyCombos[c.Deck] += 1;
        }
        foreach (UniqueCard c in gameState.CurrentPlayer.Hand)
        {
            if (c.Deck == card.Deck) enemyCombos[c.Deck] += 1;
        }
        foreach (UniqueCard c in gameState.CurrentPlayer.Played)
        {
            if (c.Deck == card.Deck) enemyCombos[c.Deck] += 1;
        }
        foreach (UniqueCard c in gameState.CurrentPlayer.CooldownPile)
        {
            if (c.Deck == card.Deck) enemyCombos[c.Deck] += 1;
        }

        val += CombosValue(enemyCombos);
        if (enemyCombos[card.Deck] > 1)
        {
            enemyCombos[card.Deck] -= 1;
            val -= CombosValue(enemyCombos);
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
    Ansei,
    Crow,
    Prestige,
    CardLimit,
    ComboPower,
    OurAgent,
    EnemyAgent,
    UpcomingCard,
    TierMultiplier,
    KnowingCardCombo,
    After40Bonus,
    TavernPenatly,
}