using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

public class MCTSNode
{
    public List<(MCTSNode?, Move)> children = new();
    public SeededGameState gameState;
    public double score = 0;
    public int visits = 0;
    public bool full = false;
    public bool endTurn;
    public MCTSNode(SeededGameState myState, List<Move>? possibleMoves = null)
    {
        gameState = myState;
        if (possibleMoves is not null)
        {
            children = possibleMoves.ConvertAll<(MCTSNode?, Move)>(move => (null, move));
            endTurn = false;
        }
        else
        {
            full = true;
            endTurn = true;
        }
    }

    public static double UCBScore(MCTSNode? child, int parentSimulations)
    {
        if (child is null || child.visits == 0) return double.MaxValue;
        if (child.full) return double.MinValue;
        return child.score + 1.41 * Math.Sqrt(Math.Log(parentSimulations) / (double)child.visits);
    }

    public int SelectBestChildIndex()
    {
        int bestChildIndex = 0;
        double bestScore = double.MinValue;

        int index = -1;
        foreach (var (child, move) in this.children)
        {
            index += 1;

            if (child is null) continue;

            if (child.score > bestScore)
            {
                bestScore = child.score;
                bestChildIndex = index;
            }
        }

        return bestChildIndex;
    }

    public double Simulate(MCTSNode node, GameStrategy strategy, SeededRandom rng)
    {
        if (node.endTurn || (node.children.Count == 1 && node.children[0].Item2.Command == CommandEnum.END_TURN))
        {
            return strategy.Heuristic(node.gameState);
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
        return strategy.Heuristic(gameState);
    }
}


public class BestMCTS : AI
{
    static Random rnd = new();
    readonly List<ulong> seeds = new();
    readonly List<SeededRandom> rngs = new();
    const int noOfRoots = 4;
    MCTSNode?[] roots = new MCTSNode?[noOfRoots];
    TimeSpan usedTimeInTurn = TimeSpan.FromSeconds(0);
    TimeSpan timeForMoveComputation = TimeSpan.FromSeconds(0.4);
    TimeSpan turnTimeout = TimeSpan.FromSeconds(29.9);
    bool startOfTurn = true;
    GameStrategy strategy = new GameStrategy(10, GamePhase.EarlyGame);
    int totCreated = 0;

    public BestMCTS()
    {
        for (int i = 0; i < noOfRoots; i++)
        {
            seeds.Add((ulong)rnd.Next());
            rngs.Add(new(seeds[i]));
        }
    }


    List<Move>? getInstantMoves(List<Move> moves, SeededGameState gameState)
    {
        moves.Sort(new MoveComparer());
        if (moves.Count == 1) return null;
        if (gameState.BoardState == BoardState.CHOICE_PENDING)
        {
            List<Move> toReturn = new();
            switch (gameState.PendingChoice!.ChoiceFollowUp)
            {
                case ChoiceFollowUp.COMPLETE_TREASURY:
                    List<Move> gold = new();
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        UniqueCard card = mcm!.Choices[0];
                        if (card.CommonId == CardId.BEWILDERMENT) return new List<Move> { mv };
                        if (card.CommonId == CardId.BEWILDERMENT && gold.Count == 0) gold.Add(mv);
                        if (card.Cost == 0) toReturn.Add(mv); // moze tez byc card.Type == 'Starter'
                    }
                    if (gold.Count == 1) return gold;
                    if (toReturn.Count > 0) return toReturn;
                    return new List<Move> { moves[0] };
                case ChoiceFollowUp.DESTROY_CARDS:
                    List<(Move, double)> choices = new();
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        if (mcm!.Choices.Count != 1) continue;
                        choices.Add((mv, strategy.CardEvaluation(mcm!.Choices[0], gameState)));
                    }
                    choices.Sort(new PairOnlySecond());
                    List<CardId> cards = new();
                    for (int i = 0; i < Math.Min(3, choices.Count); i++)
                    {
                        var mcm = choices[i].Item1 as MakeChoiceMove<UniqueCard>;
                        cards.Add(mcm!.Choices[0].CommonId);
                    }
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        bool flag = true;
                        foreach (UniqueCard card in mcm!.Choices)
                        {
                            if (!cards.Contains(card.CommonId))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag) toReturn.Add(mv);
                    }
                    if (toReturn.Count > 0) return toReturn;
                    return null;
                case ChoiceFollowUp.REFRESH_CARDS: // tu i tak musi byc duzo wierzcholkow i guess
                    List<(Move, double)> possibilities = new();
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        double val = 0;
                        foreach (UniqueCard card in mcm!.Choices)
                        {
                            val += strategy.CardEvaluation(card, gameState);
                        }
                        possibilities.Add((mv, val));
                    }
                    possibilities.Sort(new PairOnlySecond());
                    possibilities.Reverse();
                    if (gameState.PendingChoice.MaxChoices == 3)
                    {
                        for (int i = 0; i < Math.Min(10, possibilities.Count); i++)
                        {
                            toReturn.Add(possibilities[i].Item1);
                        }
                    }
                    if (gameState.PendingChoice.MaxChoices == 2)
                    {
                        for (int i = 0; i < Math.Min(6, possibilities.Count); i++)
                        {
                            toReturn.Add(possibilities[i].Item1);
                        }
                    }
                    if (gameState.PendingChoice.MaxChoices == 1)
                    {
                        for (int i = 0; i < Math.Min(3, possibilities.Count); i++)
                        {
                            toReturn.Add(possibilities[i].Item1);
                        }
                    }
                    if (toReturn.Count == 0) return null;
                    return toReturn;
                default:
                    return null;
            }
        }
        foreach (Move mv in moves)
        {
            if (mv.Command == CommandEnum.PLAY_CARD)
            {
                var mvCopy = mv as SimpleCardMove;
                if (InstantPlayCards.IsInstantPlay(mvCopy!.Card.CommonId))
                {
                    return new List<Move> { mv };
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
            index += 1;
        }

        if (node.children[selectedChild].Item1 is null)
        {
            totCreated += 1;
            var move = node.children[selectedChild].Item2;
            if (move.Command == CommandEnum.END_TURN)
            {
                node.children[selectedChild] = (new MCTSNode(node.gameState), move);
            }
            else
            {
                var (childGameState, childPossibleMoves) = node.gameState.ApplyMove(move);
                List<Move>? instantMoves = getInstantMoves(childPossibleMoves, childGameState);
                if (instantMoves is null)
                {
                    node.children[selectedChild] = (new MCTSNode(childGameState, childPossibleMoves), move);
                }
                else
                {
                    node.children[selectedChild] = (new MCTSNode(childGameState, instantMoves), move);
                }
            }
        }

        double result = run(node.children[selectedChild].Item1!, rng);
        node.full = true;
        foreach (var (child, move) in node.children)
        {
            node.full &= (child is not null && child.full);
        }

        node.visits += 1;
        node.score = Math.Max(node.score, result);
        return result;
    }

    static private bool CheckIfSameCards(List<UniqueCard> l, List<UniqueCard> r)
    {
        var balance = new Dictionary<CardId, int>();

        // var ls = l.ConvertAll(card => card.CommonId).OrderBy(id => id);
        // var rs = r.ConvertAll(card => card.CommonId).OrderBy(id => id);
        // return Enumerable.SequenceEqual(ls, rs);

        foreach (UniqueCard card in l)
        {
            if (balance.ContainsKey(card.CommonId))
            {
                balance[card.CommonId] += 1;
            }
            else
            {
                balance[card.CommonId] = 1;
            }
        }
        foreach (UniqueCard card in r)
        {
            if (balance.ContainsKey(card.CommonId))
            {
                balance[card.CommonId] -= 1;
            }
            else
            {
                return false;
            }
        }

        return balance.Values.All(cnt => cnt == 0);
    }
    private bool CheckIfSameGameStateAfterOneMove(MCTSNode node, GameState gameState)
    {
        return (CheckIfSameCards(node.gameState.CurrentPlayer.Hand, gameState.CurrentPlayer.Hand)
        && CheckIfSameCards(node.gameState.TavernAvailableCards, gameState.TavernAvailableCards)
        && CheckIfSameCards(node.gameState.CurrentPlayer.CooldownPile, gameState.CurrentPlayer.CooldownPile) // chyba niepotrzebne
        && CheckIfSameCards(node.gameState.CurrentPlayer.DrawPile, gameState.CurrentPlayer.DrawPile)    // chyba niepotrzebne
        );
    }

    static private bool CheckIfSameEffects(List<UniqueEffect> e1, List<UniqueEffect> e2)
    {
        var balance = new Dictionary<(CardId, EffectType, int, int), int>();
        foreach (UniqueEffect ef in e1)
        {
            if (balance.ContainsKey((ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)))
            {
                balance[(ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)] += 1;
            }
            else
            {
                balance[(ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)] = 1;
            }
        }
        foreach (UniqueEffect ef in e2)
        {
            if (balance.ContainsKey((ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)))
            {
                balance[(ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)] -= 1;
            }
            else
            {
                return false;
            }
        }

        return balance.Values.All(cnt => cnt == 0);
    }
    static private bool AreIsomorphic(Move mv1, Move mv2)
    {
        if (mv1.Command != mv2.Command) return false;
        if (mv1.Command == CommandEnum.END_TURN) return true;
        if (mv1.Command == CommandEnum.CALL_PATRON)
        {
            var spm1 = mv1 as SimplePatronMove;
            var spm2 = mv2 as SimplePatronMove;
            return (spm1!.PatronId == spm2!.PatronId);
        }
        if (mv1.Command == CommandEnum.MAKE_CHOICE)
        {
            var mcm1 = mv1 as MakeChoiceMove<UniqueCard>;
            var mcm2 = mv2 as MakeChoiceMove<UniqueCard>;
            if (mcm1 is null && mcm2 is null)
            {
                var mcm21 = mv1 as MakeChoiceMove<UniqueEffect>;
                var mcm22 = mv2 as MakeChoiceMove<UniqueEffect>;
                return CheckIfSameEffects(mcm21!.Choices, mcm22!.Choices);
            }
            else if (mcm1 is not null && mcm2 is not null)
            {
                return CheckIfSameCards(mcm1!.Choices, mcm2!.Choices);
            }
            return false;
        }
        var scm1 = mv1 as SimpleCardMove;
        var scm2 = mv2 as SimpleCardMove;
        return (scm1!.Card.CommonId == scm2!.Card.CommonId);
    }
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        if (availablePatrons.Contains(PatronId.DUKE_OF_CROWS)) return PatronId.DUKE_OF_CROWS;
        return availablePatrons.PickRandom(rngs[0]);
    }

    void ChooseStrategy(GameState gameState)
    {
        var currentPlayer = gameState.CurrentPlayer;
        int cardCount = currentPlayer.Hand.Count + currentPlayer.CooldownPile.Count + currentPlayer.DrawPile.Count;
        Debug.Assert(currentPlayer.Played.Count == 0);
        int points = gameState.CurrentPlayer.Prestige;
        if (points >= 27 || gameState.EnemyPlayer.Prestige >= 30) strategy = new GameStrategy(cardCount, GamePhase.LateGame);
        else if (points <= 10 && gameState.EnemyPlayer.Prestige <= 13) strategy = new GameStrategy(cardCount, GamePhase.EarlyGame);
        else strategy = new GameStrategy(cardCount, GamePhase.MidGame);
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfTurn) ChooseStrategy(gameState);

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            startOfTurn = true;
            return Move.EndTurn();
        }

        if (startOfTurn)
        {
            for (int i = 0; i < noOfRoots; i++)
            {
                SeededGameState seededGameState = gameState.ToSeededGameState(seeds[i]);
                List<Move>? instantMoves = getInstantMoves(possibleMoves, seededGameState);
                if (instantMoves is null)
                {
                    roots[i] = new MCTSNode(seededGameState, possibleMoves);
                }
                else
                {
                    roots[i] = new MCTSNode(seededGameState, instantMoves);
                }
            }
            startOfTurn = false;
        }
        for (int i = 0; i < noOfRoots; i++)
        {
            if (!CheckIfSameGameStateAfterOneMove(roots[i]!, gameState))
            {
                SeededGameState seededGameState = gameState.ToSeededGameState(seeds[i]);
                List<Move>? instantMoves = getInstantMoves(possibleMoves, seededGameState);
                if (instantMoves is null)
                {
                    roots[i] = new MCTSNode(seededGameState, possibleMoves);
                }
                else
                {
                    roots[i] = new MCTSNode(seededGameState, instantMoves);
                }
            }
            else
            {
                if (gameState.BoardState != roots[i]!.gameState.BoardState) Console.WriteLine("Tutaj");
                Debug.Assert(gameState.BoardState == roots[i]!.gameState.BoardState);
            }
        }


        Move move;
        if (usedTimeInTurn + timeForMoveComputation >= turnTimeout)
        {
            Debug.Assert(false);
            move = possibleMoves.PickRandom(rngs[0]);
        }
        else
        {
            int actionCounter = 0;
            for (int i = 0; i < noOfRoots; i++)
            {
                totCreated = 0;
                Stopwatch s = new Stopwatch();
                s.Start();
                while (s.Elapsed < timeForMoveComputation / noOfRoots && !roots[i]!.full)
                {
                    run(roots[i]!, rngs[i]);
                    actionCounter++;
                }
                // if (!roots[i]!.full) Console.WriteLine(actionCounter.ToString());
            }
            for (int i = 1; i < noOfRoots; i++) // Sanity check
            {
                Debug.Assert(roots[i]!.children.Count == roots[0]!.children.Count);
                for (int j = 0; j < roots[0]!.children.Count; j++) Debug.Assert(AreIsomorphic(roots[0]!.children[j].Item2, roots[i]!.children[j].Item2));
            }

            int idx = 0;
            double val = 0;
            for (int j = 0; j < roots[0]!.children.Count; j++)
            {
                double cur = 0;
                for (int i = 0; i < noOfRoots; i++)
                {
                    cur += roots[i]!.children[j].Item1!.score;
                }
                cur /= noOfRoots;
                Debug.Assert(cur >= 0.0 && cur <= 1.0);
                if (cur > val)
                {
                    val = cur;
                    idx = j;
                }
            }
            move = roots[0]!.children[idx].Item2;
            for (int i = 0; i < noOfRoots; i++)
            {
                roots[i] = roots[i]!.children[idx].Item1;
            }
        }
        Move move_out = possibleMoves[0];
        foreach (Move mv in possibleMoves)
        {
            if (AreIsomorphic(move, mv))
            {
                move_out = mv;
                break;
            }
        }
        if (move.Command == CommandEnum.END_TURN)
        {
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            startOfTurn = true;
        }
        return move_out;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        if (state.Reason == GameEndReason.INCORRECT_MOVE || state.Reason == GameEndReason.TURN_TIMEOUT || state.Reason == GameEndReason.BOT_EXCEPTION)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(state.Reason);
            Console.WriteLine(finalBoardState!.InitialSeed);
            Console.WriteLine(state.Winner);
            if (finalBoardState!.EnemyPlayer.PlayerID == PlayerEnum.PLAYER1)
                Console.WriteLine("Player 1: " + finalBoardState!.EnemyPlayer.Prestige.ToString());
            else
                Console.WriteLine("Player 1: " + finalBoardState!.CurrentPlayer.Prestige.ToString());
            if (finalBoardState!.EnemyPlayer.PlayerID == PlayerEnum.PLAYER2)
                Console.WriteLine("Player 2: " + finalBoardState!.EnemyPlayer.Prestige.ToString());
            else
                Console.WriteLine("Player 2: " + finalBoardState!.CurrentPlayer.Prestige.ToString());
        }
        // Console.WriteLine(state.AdditionalContext);
        // if (state.Reason == GameEndReason.INCORRECT_MOVE)
        // {
        //     Console.WriteLine("-------------------------------------------");
        //     Console.WriteLine("-------------------------------------------");
        //     Console.WriteLine(finalBoardState.CurrentPlayer.CooldownPile);
        //     Console.WriteLine(finalBoardState.CurrentPlayer.DrawPile);
        //     Console.WriteLine(finalBoardState.CurrentPlayer.Hand);
        //     Console.WriteLine(finalBoardState.CurrentPlayer.Played);
        //     Console.WriteLine("-------------------------------------------");
        //     Console.WriteLine(finalBoardState.EnemyPlayer.CooldownPile);
        //     Console.WriteLine(finalBoardState.EnemyPlayer.DrawPile);
        //     Console.WriteLine(finalBoardState.EnemyPlayer.Hand);
        //     Console.WriteLine(finalBoardState.EnemyPlayer.Played);
        //     Console.WriteLine("-------------------------------------------");
        // }
    }
}

public class PairOnlySecond : Comparer<(Move, double)>
{
    public override int Compare((Move, double) a, (Move, double) b)
    {
        return a.Item2.CompareTo(b.Item2);
    }
}
public class MoveComparer : Comparer<Move>
{
    private ulong Hash(Move x)
    {
        ulong ret = 0;

        if (x.Command == CommandEnum.CALL_PATRON)
        {
            var mx = x as SimplePatronMove;
            ret = (ulong)mx!.PatronId;
        }
        else if (x.Command == CommandEnum.MAKE_CHOICE)
        {
            var mx = x as MakeChoiceMove<UniqueCard>;
            if (mx is not null)
            {
                var ids = mx!.Choices.Select(card => (ulong)card.CommonId).OrderBy(id => id);
                foreach (ulong a in ids) ret = ret * (ulong)200 + a;
            }
            else
            {
                var mxp = x as MakeChoiceMove<UniqueEffect>;
                var ids = mxp!.Choices.Select(ef => (ulong)ef.ParentCard.CommonId).OrderBy(id => id);
                foreach (ulong a in ids) ret = ret * (ulong)200 + a;
                ret += (ulong)1000000000;
            }
        }
        else if (x.Command != CommandEnum.END_TURN)
        {
            var mx = x as SimpleCardMove;
            ret = (ulong)mx!.Card.CommonId;
        }
        return ret + (ulong)100000000000 * (ulong)x.Command;
    }
    public override int Compare(Move x, Move y)
    {
        ulong hx = Hash(x);
        ulong hy = Hash(y);
        return hx.CompareTo(hy);
    }
}