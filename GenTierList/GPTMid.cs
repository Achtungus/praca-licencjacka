using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Evolution;


public class GamePhaseStrategyMid : IGamePhaseStrategy
{
    static private List<string> taunts = new List<string> { "Stubborn Shadow", "Banneret", "Knight Commander", "Shield Bearer", "Bangkorai Sentries", "Knights of Saint Pelin" };
    int cardLimit = 16;
    static private int prestigePlus = 5;
    static private int prestigeMinus = -5;
    static private int comboPower = 2;
    static private List<int> patronDuke = new List<int> { -30, 0, 30 }; //favoured/neutral/unvafoured
    static private List<int> patronAnsei = new List<int> { 10, 0, -10 };
    static private int overCardLimitPenalty = 10;
    int heuristicMin = -500;
    int heuristicMax = 500;
    private int agentBonus = 10;
    private int enemyAgentPenalty = 8;
    private int upcomingBonus = 5;
    private int knowingCardCombo = 2;
    readonly TierListEv cardTierList;
    readonly int cardCount;
    public GamePhaseStrategyMid(TierListEv cardTierList, int cardCount)
    {
        this.cardTierList = cardTierList;
        this.cardCount = cardCount;
    }

    public int BasicProperties(SeededGameState gameState)
    {
        int val = 0;
        int power = gameState.CurrentPlayer.Power;
        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            if (taunts.Contains(agent.RepresentingCard.Name)) power -= agent.CurrentHp;
        }
        power = Math.Max(power, 0);
        int afterRoundPoint = gameState.CurrentPlayer.Prestige + power;
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
        if (enemyPatronDist == 2) val = -200;
        if (ourPatrons == 4) val = heuristicMax;
        return val;
    }

    public int CardsValues(SeededGameState gameState)
    {
        cardLimit = Math.Max(cardCount, 16);
        int val = 0;
        List<UniqueCard> ourCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
        List<UniqueCard> enemyCards = gameState.EnemyPlayer.Hand.Concat(gameState.EnemyPlayer.Played).Concat(gameState.EnemyPlayer.CooldownPile.Concat(gameState.EnemyPlayer.DrawPile)).ToList();
        Dictionary<PatronId, int> ourCombos = new Dictionary<PatronId, int>();
        Dictionary<PatronId, int> enemyCombos = new Dictionary<PatronId, int>();
        int cnt = ourCards.Count();
        if (cnt > cardLimit) val -= (cnt - cardLimit) * overCardLimitPenalty;
        foreach (UniqueCard card in ourCards)
        {
            val += (int)cardTierList.GetCardTier(card.Name, 1);
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
        foreach (UniqueCard card in gameState.CurrentPlayer.KnownUpcomingDraws)
        {
            val += (int)HandTierList.GetCardTier(card.Name) * upcomingBonus;
            if (ourCombos.ContainsKey(card.Deck)) val += ourCombos[card.Deck] * knowingCardCombo;
        }
        foreach (UniqueCard card in gameState.EnemyPlayer.KnownUpcomingDraws)
        {
            val -= (int)HandTierList.GetCardTier(card.Name) * upcomingBonus;
            if (enemyCombos.ContainsKey(card.Deck)) val -= ourCombos[card.Deck] * knowingCardCombo;
        }
        foreach (UniqueCard card in enemyCards)
        {
            val -= (int)cardTierList.GetCardTier(card.Name, 1);
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
        foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents)
        {
            val += agent.CurrentHp * prestigePlus + agentBonus;
            // val += AgentTierList.GetCardTier(agent.RepresentingCard.Name, 0) + agent.CurrentHp;
        }
        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            // val -= 30;
            val -= (int)AgentTierList.GetCardTier(agent.RepresentingCard.Name) * enemyAgentPenalty + agent.CurrentHp;
        }

        // penalty for good card in tavern
        // foreach (UniqueCard card in gameState.TavernAvailableCards)
        // {
        //     val -= (int)cardTierList.GetCardTier(card.Name, 1) / 3;
        // }
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