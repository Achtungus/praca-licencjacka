using ScriptsOfTribute.Serializers;

namespace Evolution;

public interface IGamePhaseStrategy
{
    int BasicProperties(SeededGameState gameState);
    int PatronsBonuses(SeededGameState gameState);
    int CardsValues(SeededGameState gameState);
}