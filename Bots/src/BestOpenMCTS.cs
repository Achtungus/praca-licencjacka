using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using System;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

public class OpenMCTSNode
{
    public Dictionary<Move, OpenMCTSNode> children = new();
    public double score = 0;
    public int visits = 0;
    public Move? move;
    public OpenMCTSNode(Move? _move)
    {
        move = _move;
    }

    public void AddChild(Move mv)
    {
        if (!children.ContainsKey(mv))
        {
            children[mv] = new OpenMCTSNode(mv);
        }
    }
    public Move SelectBestMoveByUCB(List<Move> possibleMoves)
    {
        Move? ret = null;
        double maks = -1;
        foreach (Move mv in possibleMoves) // mozna przyspieszyc chyba KeyValuePair<Move, OpenMCTSNode>
        {
            if (!children.ContainsKey(mv))
            {
                return mv;
            }
            OpenMCTSNode node = children[mv];
            Debug.Assert(node.visits != 0);
            double sc = (node.score / (double)node.visits) + 1.41 * (Math.Log(visits) / node.visits);
            if (sc > maks)
            {
                maks = sc;
                ret = mv;
            }
        }
        return ret!;
    }

    public Move SelectBestMove()
    {
        Move? ret = null;
        double maks = -1;
        foreach (Move mv in children.Keys) // mozna przyspieszyc chyba KeyValuePair<Move, OpenMCTSNode>
        {
            OpenMCTSNode node = children[mv];
            if (node.visits == 0) continue;
            double sc = (node.score / (double)node.visits);
            Console.WriteLine(mv.Command.ToString() + " " + sc.ToString() + " " + node.score.ToString() + " " + node.visits.ToString());
            if (sc > maks)
            {
                maks = sc;
                ret = mv;
            }
        }
        Debug.Assert(ret is not null);
        return ret!;
    }

}


public class BestOpenMCTS : AI
{
    static Random rnd = new();
    SeededRandom rng = new(123);
    OpenMCTSNode? root = null;
    TimeSpan usedTimeInTurn = TimeSpan.FromSeconds(0);
    TimeSpan timeForMoveComputation = TimeSpan.FromSeconds(0.3);
    TimeSpan turnTimeout = TimeSpan.FromSeconds(29.9);
    bool startOfTurn = true;
    GameStrategy strategy = new GameStrategy(10, GamePhase.EarlyGame);
    int totCreated = 0;

    public BestOpenMCTS()
    {
    }

    public double SimulateWithBacktrack(OpenMCTSNode node, SeededGameState gameState, List<Move> possibleMoves, SeededRandom rng)
    {
        double val = 0;
        Debug.Assert(node.move is not null);
        if (node.move!.Command == CommandEnum.END_TURN || (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN))
        {
            Debug.Assert(gameState.CurrentPlayer.PlayerID == PlayerEnum.PLAYER2);
            val = strategy.Heuristic(gameState);
        }
        else
        {
            List<Move> notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
            Move move = notEndMoves.PickRandom(rng);
            (gameState, possibleMoves) = gameState.ApplyMove(move);
            node.AddChild(move);
            val = SimulateWithBacktrack(node.children[move], gameState, possibleMoves, rng);
        }
        node.visits += 1;
        node.score += val;
        return val;
    }

    double SelectionWithBacktrack(OpenMCTSNode node, SeededGameState gameState, List<Move> possibleMoves, SeededRandom rng)
    {
        if (node.move is not null && (node.move.Command == CommandEnum.END_TURN || node.visits == 0))
        {
            Debug.Assert(node.move is not null);
            return SimulateWithBacktrack(node, gameState, possibleMoves, rng);
        }

        Move move = node.SelectBestMoveByUCB(possibleMoves);
        if (!node.children.ContainsKey(move))
        {
            node.AddChild(move);
        }
        if (move.Command == CommandEnum.END_TURN)
        {
            possibleMoves = new();
        }
        else
        {
            (gameState, possibleMoves) = gameState.ApplyMove(move);
        }
        double result = SelectionWithBacktrack(node.children[move], gameState, possibleMoves, rng);
        node.visits += 1;
        node.score += result;
        return result;
    }
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        if (availablePatrons.Contains(PatronId.DUKE_OF_CROWS)) return PatronId.DUKE_OF_CROWS;
        return availablePatrons.PickRandom(rng);
    }

    void SelectStrategy(GameState gameState)
    {
        var currentPlayer = gameState.CurrentPlayer;
        int cardCount = currentPlayer.Hand.Count + currentPlayer.CooldownPile.Count + currentPlayer.DrawPile.Count;
        Debug.Assert(currentPlayer.Played.Count == 0);
        int points = gameState.CurrentPlayer.Prestige;
        if (points >= 27 || gameState.EnemyPlayer.Prestige >= 30)
        {
            strategy = new GameStrategy(cardCount, GamePhase.LateGame);
        }
        else if (points <= 10 && gameState.EnemyPlayer.Prestige <= 13)
        {
            strategy = new GameStrategy(cardCount, GamePhase.EarlyGame);
        }
        else
        {
            strategy = new GameStrategy(cardCount, GamePhase.MidGame);
        }
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfTurn)
        {
            SelectStrategy(gameState);
            startOfTurn = false;
        }
        root = new OpenMCTSNode(null);
        int actionCounter = 0;
        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < timeForMoveComputation)
        {
            ulong seed = (ulong)rnd.Next(1, 1000000000);
            SeededGameState gS = gameState.ToSeededGameState(123);
            SelectionWithBacktrack(root, gS, possibleMoves, rng);
            actionCounter++;
        }
        Console.WriteLine(actionCounter);
        Move move = root!.SelectBestMove();
        if (move.Command == CommandEnum.END_TURN)
        {
            startOfTurn = true;
        }
        Console.WriteLine(move.Command);
        return move;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        Console.WriteLine("-------------------------------------------");
        Console.WriteLine(state.Reason);
        Console.WriteLine(state.AdditionalContext);
        // Console.WriteLine(finalBoardState!.InitialSeed);
        // Console.WriteLine(state.Winner);
        if (finalBoardState!.EnemyPlayer.PlayerID == PlayerEnum.PLAYER1)
            Console.WriteLine("Player 1: " + finalBoardState!.EnemyPlayer.Prestige.ToString());
        else
            Console.WriteLine("Player 1: " + finalBoardState!.CurrentPlayer.Prestige.ToString());
        if (finalBoardState!.EnemyPlayer.PlayerID == PlayerEnum.PLAYER2)
            Console.WriteLine("Player 2: " + finalBoardState!.EnemyPlayer.Prestige.ToString());
        else
            Console.WriteLine("Player 2: " + finalBoardState!.CurrentPlayer.Prestige.ToString());
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