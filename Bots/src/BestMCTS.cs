using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;
using System;
using System.Collections;

namespace Bots;

public class MCTSNode
{
    // List to array
    public List<(MCTSNode?, Move)> children = new();
    public SeededGameState gameState;
    public double score = 0;
    public int visits = 0;
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

    public static double UCBScore(MCTSNode? child, int parentSimulations)
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

        int index = -1;
        foreach (var (child, move) in this.children)
        {
            index += 1;

            if (child is null)
            {
                continue;
            }

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
    const ulong seed = 123;
    readonly SeededRandom rng = new(seed);
    MCTSNode? root = null;
    TimeSpan usedTimeInTurn = TimeSpan.FromSeconds(0);
    TimeSpan timeForMoveComputation = TimeSpan.FromSeconds(0.3);
    TimeSpan turnTimeout = TimeSpan.FromSeconds(29.9);
    bool startOfTurn = true;
    int roundNumber = 1;
    GameStrategy strategy = new GameStrategy(10, GamePhase.EarlyGame);
    int cardCount = 0;

    public BestMCTS() { }

    Move? getInstantMove(List<Move> moves)
    {
        foreach (Move mv in moves)
        {
            if (mv.Command == CommandEnum.PLAY_CARD)
            {
                var mvCopy = mv as SimpleCardMove;
                if (InstantPlayCards.IsInstantPlay(mvCopy!.Card.Name))
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
            index += 1;
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

        double result = run(node.children[selectedChild].Item1!, rng);
        node.visits += 1;
        node.score = Math.Max(node.score, result);
        return result;
    }

    private bool CheckIfPossibleMovesAreTheSame(MCTSNode node, List<Move> otherMoves)
    {
        if (node.children.Count != otherMoves.Count)
        {
            return false;
        }
        foreach (var (_, move) in node.children)
        {
            if (!otherMoves.Contains(move))
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

    void ChooseStrategy(GameState gameState)
    {
        var currentPlayer = gameState.CurrentPlayer;
        cardCount = currentPlayer.Hand.Count + currentPlayer.CooldownPile.Count + currentPlayer.DrawPile.Count;
        Debug.Assert(currentPlayer.Played.Count == 0);
        int points = gameState.CurrentPlayer.Prestige;
        if (points >= 27 || gameState.EnemyPlayer.Prestige >= 30) strategy = new GameStrategy(cardCount, GamePhase.LateGame);
        else if (points <= 10 && gameState.EnemyPlayer.Prestige <= 13) strategy = new GameStrategy(cardCount, GamePhase.EarlyGame);
        else strategy = new GameStrategy(cardCount, GamePhase.MidGame);
        Log("23423");
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfTurn) ChooseStrategy(gameState);

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            roundNumber += 1;
            startOfTurn = true;
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
            int actionCounter = 0;
            Stopwatch s = new Stopwatch();
            s.Start();
            while (s.Elapsed < timeForMoveComputation)
            {
                run(root!, rng);
                actionCounter++;
            }
            // Console.WriteLine(actionCounter);
            usedTimeInTurn += timeForMoveComputation;
            (root, move) = root!.children[root!.SelectBestChildIndex()];
        }

        if (move.Command == CommandEnum.END_TURN)
        {
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            roundNumber += 1;
            startOfTurn = true;
        }
        return move;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        Console.WriteLine("-------------------------------------------");
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
