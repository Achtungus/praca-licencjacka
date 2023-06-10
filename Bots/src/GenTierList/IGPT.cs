using ScriptsOfTribute.Serializers;

namespace Evolution;

interface IGamePhaseStrategy
{
    int BasicProperties(SeededGameState gameState);
    int PatronsBonuses(SeededGameState gameState);
    int CardsValues(SeededGameState gameState, TierListEv cardTierList, int startRoundCardsCount);
}