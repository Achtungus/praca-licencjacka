namespace Bots;

public static class HandTierList
{
    const int S = 5;
    const int A = 4;
    const int B = 3;
    const int C = 2;
    const int D = 1;
    const int E = 0;
    const int CURSE = -1;

    static readonly Dictionary<string, int> handTierDict = new Dictionary<string, int> {
        { "Currency Exchange", B },
        { "Luxury Exports",    D },
        { "Oathman",           D },
        { "Hlaalu Councilor",  S },
        { "Hlaalu Kinsman",    S },
        { "House Embassy",     S },
        { "House Marketplace", A },
        { "Hireling",          D },
        { "Hostile Takeover",  D },
        { "Customs Seizure",   D },
        { "Goods Shipment",    CURSE },

        { "Midnight Raid",   B },
        { "Hagraven",        B },
        { "Hagraven Matron", A },
        { "War Song",        CURSE },

        { "Blackfeather Knave",   B },
        { "Plunder",              S },
        { "Toll of Flesh",        C },
        { "Toll of Silver",       C },
        { "Murder of Crows",      A },
        { "Pilfer",               B },
        { "Squawking Oratory",    A },
        { "Pool of Shadow",       C },
        { "Scratch",              D },
        { "Blackfeather Brigand", D },
        { "Blackfeather Knight",  B },
        { "Peck",                 CURSE },

        { "Conquest",         C },
        { "Hira's End",       S },
        { "Hel Shira Herald", S },
        { "March on Hattu",   S },
        { "Shehai Summoning", S },
        { "Warrior Wave",     C },
        { "Ansei Assault",    A },
        { "Ansei's Victory",  S },
        { "No Shira Poet",    C },
        { "Way of the Sword", CURSE },

        { "Rally",                  A },
        { "Siege Weapon Volley",    C },
        { "The Armory",             B },
        { "Banneret",               S },
        { "Knight Commander",       S },
        { "Reinforcements",         D },
        { "Archers' Volley",        D },
        { "Legion's Arrival",       D },
        { "Bangkorai Sentries",     A },
        { "Knights of Saint Pelin", A },
        { "The Portcullis",         D },
        { "Fortify",                CURSE },

        { "Bewilderment",      CURSE },
        { "Grand Larceny",     C },
        { "Jarring Lullaby",   C },
        { "Jeering Shadow",    E },
        { "Pounce and Profit", C },
        { "Prowling Shadow",   B },
        { "Shadow's Slumber",  B },
        { "Slight of Hand",    CURSE },
        { "Stubborn Shadow",   D },
        { "Twilight Revelry",  C },
        { "Swipe",             CURSE },

        { "Gold",         CURSE },
        { "Writ of Coin", CURSE },
    };

    public static int GetCardTier(string cardName) => handTierDict[cardName];
}