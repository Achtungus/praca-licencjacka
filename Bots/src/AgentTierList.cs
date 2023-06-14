namespace Bots;

public static class AgentTier
{
    const int S = 5;
    const int A = 4;
    const int B = 3;
    const int C = 2;
    const int D = 1;
    const int E = 0;
    const int CURSE = -1;

    static readonly Dictionary<string, int> agentTierDict = new Dictionary<string, int> {
        { "Oathman",                C },
        { "Hlaalu Councilor",       S },
        { "Hlaalu Kinsman",         S },
        { "Hireling",               C },
        { "Clan-Witch",             A },
        { "Elder Witch",            A },
        { "Hagraven",               A },
        { "Hagraven Matron",        A },
        { "Karth Man-Hunter",       B },
        { "Blackfeather Knave",     B },
        { "Blackfeather Brigand",   B },
        { "Blackfeather Knight",    B },
        { "Hel Shira Herald",       B },
        { "No Shira Poet",          C },
        { "Banneret",               S },
        { "Knight Commander",       S },
        { "Shield Bearer",          B },
        { "Bangkorai Sentries",     B },
        { "Knights of Saint Pelin", A },
        { "Jeering Shadow",         C },
        { "Prowling Shadow",        B },
        { "Stubborn Shadow",        C },
    };

    public static int GetCardTier(string cardName) => agentTierDict[cardName];
}