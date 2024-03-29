using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;
using System;
using System.Collections;

namespace Evolution;

public class MCTSNode
{
    // List to array
    public List<(MCTSNode?, Move)> children = new();
    public SeededGameState gameState;
    public double score = 0;
    public ulong visits = 0;
    public bool endTurn;
    public MCTSNode(SeededGameState myState, List<Move>? possibleMoves = null)
    {
        gameState = myState;
        if (possibleMoves is not null)
        {
            foreach (Move move in possibleMoves)
            {
                children.Add((null, move));
            }
            endTurn = false;
        }
        else
        {
            endTurn = true;
        }
    }

    public static double UCBScore(MCTSNode? child, ulong parentSimulations)
    {
        // Maybe pass log already, jeśli lewe to nieprawda, to prawe tez
        if (child is null || child.visits == 0)
        {
            return int.MaxValue;
        }
        return child.score + 1.41 * Math.Sqrt((Math.Log(parentSimulations)) / child.visits);
    }

    public int SelectBestChildIndex()
    {
        // TODO: Ew. losować z najlepszych / premiować częstość odwiedzania
        int bestChildIndex = 0;
        double bestScore = double.MinValue;

        int index = 0;
        foreach (var (child, move) in this.children)
        {
            if (child is null)
            {
                ++index;
                continue;
            }

            if (child.score > bestScore)
            {
                bestScore = child.score;
                bestChildIndex = index;
            }

            ++index;
        }

        return bestChildIndex;
    }

    int heuristicMin = -500;
    int heuristicMax = 500;
    int after40bonus = 30;
    public double Heuristic(SeededGameState gameState, IGamePhaseStrategy strategy)
    {
        int patronsBonuses = strategy.PatronsBonuses(gameState);
        if (patronsBonuses == heuristicMax) return 1;
        if (patronsBonuses == 3 * heuristicMin) return 0;
        int basicProperties = strategy.BasicProperties(gameState);
        int cardsValues = strategy.CardsValues(gameState);
        // cardsValues = Math.Clamp(cardsValues, -200, 200);
        int val = basicProperties + patronsBonuses + cardsValues;
        int power = gameState.CurrentPlayer.Power;
        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
        {
            if (BestMCTS.taunts.Contains(agent.RepresentingCard.Name)) power -= agent.CurrentHp;
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

    public double Simulate(MCTSNode node, IGamePhaseStrategy strategy, SeededRandom rng)
    {
        if (node.endTurn || node.children.Count == 1)
        {
            return Heuristic(node.gameState, strategy);
        }
        SeededGameState gameState = node.gameState;
        List<Move> possibleMoves = node.children.ConvertAll<Move>(m => m.Item2);
        List<Move> notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        Move move = notEndMoves.PickRandom(rng);
        while (move.Command != CommandEnum.END_TURN)
        {
            (gameState, possibleMoves) = gameState.ApplyMove(move);
            notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
            if (notEndMoves.Count > 0)
            {
                move = notEndMoves.PickRandom(rng);
            }
            else
            {
                move = Move.EndTurn();
            }

        }
        return Heuristic(gameState, strategy);
    }
}

public class BestMCTS : AI
{
    public uint expectedMoves = 8;
    public int playedMoves = -1;
    private const ulong seed = 123;
    private readonly SeededRandom rng = new(seed);
    private MCTSNode? root = null;
    TimeSpan usedTimeInTurn = TimeSpan.FromSeconds(0);
    TimeSpan timeForMoveComputation = TimeSpan.FromSeconds(0.1);
    TimeSpan turnTimeout = TimeSpan.FromSeconds(29.9);
    bool startOfTurn = true;
    readonly TierListEv cardTierList;
    public ulong roundNumber = 1;
    IGamePhaseStrategy strategy;
    public int cardsCount = 0;
    public static readonly List<string> taunts = new List<string> { "Stubborn Shadow", "Banneret", "Knight Commander", "Shield Bearer", "Bangkorai Sentries", "Knights of Saint Pelin" };
    bool shouldPrint = false;
    public BestMCTS(TierListEv cardTierList, bool shouldPrint = false)
    {
        this.cardTierList = cardTierList;
        this.shouldPrint = shouldPrint;
        strategy = new GamePhaseStrategyEarly(cardTierList, 10);
    }

    Move? getInstantMove(List<Move> moves)
    {
        foreach (Move mv in moves)
        {
            if (mv.Command == CommandEnum.PLAY_CARD)
            {
                var mvCopy = mv as SimpleCardMove;
                if (instantPlay.Contains(mvCopy!.Card.Name))
                {
                    return mv;
                }
            }
        }
        return null;
    }

    private double run(MCTSNode node, SeededRandom rng)
    {
        if (node.endTurn || node.visits == 0)
        {
            double score = node.Simulate(node, strategy, rng);
            node.visits += 1;
            node.score = score;
            return score;
        }

        double maxScore = double.MinValue;
        int selectedChild = 0;

        int index = 0;
        foreach (var (child, move) in node.children)
        {
            double score = MCTSNode.UCBScore(child, node.visits);
            if (score > maxScore)
            {
                maxScore = score;
                selectedChild = index;
            }
            ++index;
        }

        if (node.children[selectedChild].Item1 is null)
        {
            var move = node.children[selectedChild].Item2;
            if (move.Command == CommandEnum.END_TURN)
            {
                node.children[selectedChild] = (new MCTSNode(node.gameState), move);
            }
            else
            {
                var (childGameState, childPossibleMoves) = node.gameState.ApplyMove(move);
                Move? instantMove = getInstantMove(childPossibleMoves);
                if (instantMove is null)
                {
                    node.children[selectedChild] = (new MCTSNode(childGameState, childPossibleMoves), move);
                }
                else
                {
                    node.children[selectedChild] = (new MCTSNode(childGameState, new List<Move> { instantMove }), move);
                }
            }
        }

        double res = run(node.children[selectedChild].Item1!, rng);
        node.visits += 1;
        node.score = Math.Max(node.score, res);
        return node.score;
    }

    private bool CheckIfPossibleMovesAreTheSame(MCTSNode node, List<Move> possibleMoves2)
    {
        if (node.children.Count != possibleMoves2.Count)
        {
            return false;
        }
        foreach (var (_, move) in node.children)
        {
            if (!possibleMoves2.Contains(move))
            {
                return false;
            }
        }
        return true;
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        if (availablePatrons.Contains(PatronId.DUKE_OF_CROWS)) return PatronId.DUKE_OF_CROWS;
        return availablePatrons.PickRandom(rng);
    }
    public static List<string> instantPlay = new List<string>{
        // HLAALU
        // "Currency Exchange",
        "Luxury Exports",
        // "Oathman",
        "Ebony Mine",
        // "Hlaalu Councilor",
        // "Hlaalu Kinsman",
        // "House Embassy",
        // "House Marketplace",
        // "Hireling",
        // "Hostile Takeover",
        "Kwama Egg Mine",
        // "Customs Seizure",
        "Goods Shipment",

        // RED EAGLE
        "Midnight Raid",
        // "Blood Sacrifice",
        // "Bloody Offering",
        // "Bonfire",
        // "Briarheart Ritual",
        // "Clan-Witch",
        // "Elder Witch",
        // "Hagraven",
        // "Hagraven Matron",
        // "Imperial Plunder",
        // "Imperial Spoils",
        // "Karth Man-Hunter",
        "War Song",

        // DUKE OF CROWS
        // "Blackfeather Knave",
        // "Plunder",
        // "Toll of Flesh",
        // "Toll of Silver",
        // "Murder of Crows",
        // "Pilfer",
        // "Squawking Oratory",
        // "Law of Sovereign Roost",
        // "Pool of Shadow",
        // "Scratch",
        // "Blackfeather Brigand",
        // "Blackfeather Knight",
        // "Peck",

        // ANSEI
        // "Conquest",
        // "Grand Oratory",
        // "Hira's End",
        // "Hel Shira Herald",
        // "March on Hattu",
        // "Shehai Summoning",
        // "Warrior Wave",
        // "Ansei Assault",
        // "Ansei's Victory",
        // "Battle Meditation",
        // "No Shira Poet",
        // "Way of the Sword",

        // PELIN
        // "Rally",
        "Siege Weapon Volley",
        "The Armory",
        "Banneret",
        "Knight Commander",
        "Reinforcements",
        "Archers' Volley",
        "Legion's Arrival",
        "Shield Bearer",
        "Bangkorai Sentries",
        "Knights of Saint Pelin",
        "The Portcullis",
        "Fortify",

        // RAJHIN
        // "Bag of Tricks", <- contract action
        "Bewilderment",
        "Grand Larceny",
        "Jarring Lullaby",
        "Jeering Shadow",
        // "Moonlit Illusion", <- contract action
        "Pounce and Profit",
        "Prowling Shadow",
        // "Ring's Guile", <- contract action
        "Shadow's Slumber",
        // "Slight of Hand",
        "Stubborn Shadow",
        // "Twilight Revelry",
        "Swipe",

        // TREASURY
        // "Ambush", <- contract action
        // "Barterer", <- contract action
        // "Black Sacrament", <- contract action
        // "Blackmail", <- contract action
        "Gold",
        // "Harvest Season", <- contract action
        // "Imprisonment", <- contract action
        // "Ragpicker", <- contract action
        // "Tithe", <- contract action
        "Writ of Coin",
        // "Unknown", <- ?
    };
    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfTurn)
        {
            List<UniqueCard> ourCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
            cardsCount = ourCards.Count();
            int points = gameState.CurrentPlayer.Prestige;
            if (points >= 25 || gameState.EnemyPlayer.Prestige >= 30) strategy = new GamePhaseStrategyLate(cardTierList, cardsCount);
            else if (points <= 10 && gameState.EnemyPlayer.Prestige <= 13) strategy = new GamePhaseStrategyEarly(cardTierList, cardsCount);
            else strategy = new GamePhaseStrategyMid(cardTierList, cardsCount);
        }
        playedMoves += 1;
        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            roundNumber += 1;
            startOfTurn = true;
            playedMoves = -1;
            return Move.EndTurn();
        }

        if (startOfTurn || !CheckIfPossibleMovesAreTheSame(root!, possibleMoves))
        {
            SeededGameState seededGameState = gameState.ToSeededGameState(seed);
            root = new MCTSNode(seededGameState, possibleMoves);
            startOfTurn = false;
        }

        Move move;
        if (usedTimeInTurn + timeForMoveComputation >= turnTimeout)
        {
            move = possibleMoves.PickRandom(rng);
        }
        else
        {
            ulong actionCounter = 0;
            // if (playedMoves < expectedMoves) timeForMoveComputation = (turnTimeout - usedTimeInTurn) / (expectedMoves - playedMoves);
            // else timeForMoveComputation = TimeSpan.FromSeconds(0.05);
            // Stopwatch s = new Stopwatch();
            // s.Start();
            // while (s.Elapsed < timeForMoveComputation)
            // {
            for (int i = 0; i < 2000; i++)
            {
                run(root!, rng);
                actionCounter++;
            }
            // }
            // usedTimeInTurn += timeForMoveComputation;
            // if (shouldPrint)
            //     Console.WriteLine(actionCounter);
            int bestChildIndex = root!.SelectBestChildIndex();

            (root, move) = root!.children[bestChildIndex];
            Debug.Assert(root is not null);
        }

        if (move.Command == CommandEnum.END_TURN)
        {
            playedMoves = -1;
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            roundNumber += 1;
            startOfTurn = true;
        }
        return move;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        Console.WriteLine("-------------------------------------------");
        if (state.Reason != GameEndReason.PRESTIGE_OVER_40_NOT_MATCHED)
            Console.WriteLine(state.Reason + state.AdditionalContext);
        if (finalBoardState!.EnemyPlayer.PlayerID == PlayerEnum.PLAYER1)
            Console.WriteLine("Player 1: " + finalBoardState!.EnemyPlayer.Prestige.ToString());
        else
            Console.WriteLine("Player 1: " + finalBoardState!.CurrentPlayer.Prestige.ToString());
        if (finalBoardState!.EnemyPlayer.PlayerID == PlayerEnum.PLAYER2)
            Console.WriteLine("Player 2: " + finalBoardState!.EnemyPlayer.Prestige.ToString());
        else
            Console.WriteLine("Player 2: " + finalBoardState!.CurrentPlayer.Prestige.ToString());
    }
}
