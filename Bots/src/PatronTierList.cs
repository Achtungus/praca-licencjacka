using ScriptsOfTribute;

namespace Bots;

public record struct PatronTier(int favoured, int neutral, int unfavoured);

public static class PatronTierList
{
    static readonly Dictionary<PatronId, PatronTier[]> patronTierDict = new Dictionary<PatronId, PatronTier[]> {
        { PatronId.ANSEI,         new PatronTier[] { new(50, 0, -50), new(50, 0, -50), new(50, 0, -50) } },
        { PatronId.DUKE_OF_CROWS, new PatronTier[] { new(-300, 0, 300), new(-500, 0, 500), new(-600, 0, 600) } },
        { PatronId.HLAALU,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.PELIN,         new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.RAJHIN,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.RED_EAGLE,     new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.TREASURY,      new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
    };

    public static PatronTier GetPatronTier(PatronId patron, GamePhase gamePhase) => patronTierDict[patron][(int)gamePhase];
}