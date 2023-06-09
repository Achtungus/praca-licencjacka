using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

public interface IGamePhaseStrategy
{
    public int BasicProperties(SeededGameState gameState);
    public int PatronsBonuses(SeededGameState gameState);
    public int CardsValues(SeededGameState gameState, int startRoundcardsCount);
}