using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

public class GamePhaseStrategyEarly : IGamePhaseStrategy
{
    int cardLimit = 16;
    static private int prestigePlus = 1;
    static private int prestigeMinus = -1;
    static private int comboPower = 3;
    static private List<int> patronDuke = new List<int> { -30, 0, 30 }; //favoured/neutral/unvafoured
    static private List<int> patronAnsei = new List<int> { 10, 0, -10 };
    int heuristicMin = -500;
    int heuristicMax = 500;
    static private int overCardLimitPenalty = 0;
    private int agentBonus = 10;
    public GamePhaseStrategyEarly() { }

    public int BasicProperties(SeededGameState gameState)
    {
        int val = 0;
        int afterRoundPoint = gameState.CurrentPlayer.Prestige + gameState.CurrentPlayer.Power;
        val += afterRoundPoint * prestigePlus;
        val += gameState.EnemyPlayer.Prestige * prestigeMinus;
        return val;
    }

    public int PatronsBonuses(SeededGameState gameState)
    {
        int val = 0;
        int enemyPatronDist = 0;
        int ourPatrons = 0;
        foreach (KeyValuePair<PatronId, PlayerEnum> pat in gameState.PatronStates.All)
        {
            if (pat.Key == PatronId.TREASURY) continue;
            if (pat.Key == PatronId.PELIN)
            {
                if (pat.Value == gameState.CurrentPlayer.PlayerID)
                {
                    enemyPatronDist += 2;
                    ourPatrons++;
                }
                else if (pat.Value == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    enemyPatronDist += 1;
                }
                else
                {

                }
            }
            else if (pat.Key == PatronId.HLAALU)
            {
                if (pat.Value == gameState.CurrentPlayer.PlayerID)
                {
                    enemyPatronDist += 2;
                    ourPatrons++;
                }
                else if (pat.Value == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    enemyPatronDist += 1;
                }
                else
                {

                }
            }
            else if (pat.Key == PatronId.DUKE_OF_CROWS)
            {
                if (pat.Value == gameState.CurrentPlayer.PlayerID)
                {
                    enemyPatronDist += 2;
                    val += patronDuke[0];
                    ourPatrons++;
                }
                else if (pat.Value == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    enemyPatronDist += 1;
                    val += patronDuke[1];
                }
                else
                {
                    val += patronDuke[2];
                }
            }
            else if (pat.Key == PatronId.ANSEI)
            {
                if (pat.Value == gameState.CurrentPlayer.PlayerID)
                {
                    enemyPatronDist += 1;
                    val += patronAnsei[0];
                    ourPatrons++;
                }
                else if (pat.Value == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    enemyPatronDist += 1;
                    val += patronAnsei[1];
                }
                else
                {
                    val += patronAnsei[2];
                }
            }
            else if (pat.Key == PatronId.RAJHIN)
            {
                if (pat.Value == gameState.CurrentPlayer.PlayerID)
                {
                    enemyPatronDist += 2;
                    ourPatrons++;
                }
                else if (pat.Value == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    enemyPatronDist += 1;
                }
                else
                {

                }
            }
            else if (pat.Key == PatronId.RED_EAGLE)
            {
                if (pat.Value == gameState.CurrentPlayer.PlayerID)
                {
                    ourPatrons++;
                    enemyPatronDist += 2;
                }
                else if (pat.Value == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    enemyPatronDist += 1;
                }
                else
                {

                }
            }
        }
        if (enemyPatronDist == 1) return 3 * heuristicMin;
        if (enemyPatronDist == 2) val = heuristicMin;
        if (ourPatrons == 4) val = heuristicMax;
        return val;
    }
    public int CardsValues(SeededGameState gameState, int startRoundcardsCount)
    {
        int val = 0;
        List<UniqueCard> ourCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
        List<UniqueCard> enemyCards = gameState.EnemyPlayer.Hand.Concat(gameState.EnemyPlayer.Played).Concat(gameState.EnemyPlayer.CooldownPile.Concat(gameState.EnemyPlayer.DrawPile)).ToList();
        Dictionary<PatronId, int> ourCombos = new Dictionary<PatronId, int>();
        Dictionary<PatronId, int> enemyCombos = new Dictionary<PatronId, int>();
        int cnt = ourCards.Count();
        if (cnt > cardLimit) val -= (cnt - cardLimit) * overCardLimitPenalty;
        foreach (UniqueCard card in ourCards)
        {
            val += (int)GamePhaseTierList.GetCardTier(card.Name, 0);
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
            if (card.Deck == PatronId.DUKE_OF_CROWS) val += 5;

        }
        foreach (UniqueCard card in enemyCards)
        {
            val -= (int)GamePhaseTierList.GetCardTier(card.Name, 0);
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

        int combos = (int)(calcCombos(ourCombos) - calcCombos(enemyCombos));
        val += combos;

        foreach (UniqueCard card in gameState.CurrentPlayer.KnownUpcomingDraws)
        {
            val += (int)GamePhaseTierList.GetCardTier(card.Name, 0);
        }
        foreach (UniqueCard card in gameState.EnemyPlayer.KnownUpcomingDraws)
        {
            val -= (int)GamePhaseTierList.GetCardTier(card.Name, 0);
        }
        foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents)
        {
            val += agent.CurrentHp * prestigePlus + agentBonus;
            // val += AgentTierList.GetCardTier(agent.RepresentingCard.Name, 0) + agent.CurrentHp;
        }

        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            val -= 30;
            // val -= AgentTierList.GetCardTier(agent.RepresentingCard.Name, 0) + agent.CurrentHp;
        }
        return val;
    }

    private double calcCombos(Dictionary<PatronId, int> dict)
    {
        double val = 0;
        int cnt = 0;
        foreach (KeyValuePair<PatronId, int> el in dict)
        {
            cnt += el.Value;
        }
        double wsp = 1;
        if (cnt > cardLimit)
        {
            wsp = cardLimit / cnt;
        }
        foreach (KeyValuePair<PatronId, int> el in dict)
        {
            val += Math.Pow(wsp * el.Value, comboPower);
        }
        return val;
    }

}