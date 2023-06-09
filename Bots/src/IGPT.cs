using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace Bots;

interface IGamePhaseStrategy
{
    int BasicProperties(SeededGameState gameState);
    int PatronsBonuses(SeededGameState gameState);
    int CardsValues(SeededGameState gameState, int startRoundcardsCount);
}