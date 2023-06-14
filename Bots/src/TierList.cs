namespace Bots;

public static class GPCardTierList
{
    const int S = 50;
    const int A = 30;
    const int WOC = 20;
    const int B = 10;
    const int C = 3;
    const int D = 1;
    const int UNKNOWN = 0;
    const int CONTRACT_ACTION = 0;
    const int CURSE = -3;

    static readonly Dictionary<string, int[]> cardTierDict = new Dictionary<string, int[]> {
        { "Currency Exchange", new int[] { S, S, A } },
        { "Luxury Exports",    new int[] { S, S, C } },
        { "Oathman",           new int[] { A, A, B } },
        { "Ebony Mine",        new int[] { C, B, C } },
        { "Hlaalu Councilor",  new int[] { A, A, C } },
        { "Hlaalu Kinsman",    new int[] { A, A, C } },
        { "House Embassy",     new int[] { A, A, C } },
        { "House Marketplace", new int[] { B, A, C } },
        { "Hireling",          new int[] { C, C, D } },
        { "Hostile Takeover",  new int[] { B, C, D } },
        { "Kwama Egg Mine",    new int[] { C, B, C } },
        { "Customs Seizure",   new int[] { D, D, D } },
        { "Goods Shipment",    new int[] { D, D, D } },

        { "Midnight Raid",     new int[] { S, S, S } },
        { "Blood Sacrifice",   new int[] { S, S, S } },
        { "Bloody Offering",   new int[] { S, S, S } },
        { "Bonfire",           new int[] { C, B, C } },
        { "Briarheart Ritual", new int[] { C, B, C } },
        { "Clan-Witch",        new int[] { C, B, D } },
        { "Elder Witch",       new int[] { C, B, D } },
        { "Hagraven",          new int[] { D, B, D } },
        { "Hagraven Matron",   new int[] { D, A, C } },
        { "Imperial Plunder",  new int[] { B, A, B } },
        { "Imperial Spoils",   new int[] { B, A, B } },
        { "Karth Man-Hunter",  new int[] { C, B, C } },
        { "War Song",          new int[] { D, D, D } },

        { "Blackfeather Knave",     new int[] { S, S, A } },
        { "Plunder",                new int[] { S, S, S } },
        { "Toll of Flesh",          new int[] { S, S, A } },
        { "Toll of Silver",         new int[] { S, S, A } },
        { "Murder of Crows",        new int[] { S, S, A } },
        { "Pilfer",                 new int[] { A, S, A } },
        { "Squawking Oratory",      new int[] { A, S, A } },
        { "Law of Sovereign Roost", new int[] { B, A, B } },
        { "Pool of Shadow",         new int[] { B, A, B } },
        { "Scratch",                new int[] { A, A, B } },
        { "Blackfeather Brigand",   new int[] { C, C, C } },
        { "Blackfeather Knight",    new int[] { C, B, C } },
        { "Peck",                   new int[] { C, C, C } },

        { "Conquest",          new int[] { A, A, B } },
        { "Grand Oratory",     new int[] { B, S, S } },
        { "Hira's End",        new int[] { S, S, S } },
        { "Hel Shira Herald",  new int[] { B, A, B } },
        { "March on Hattu",    new int[] { A, A, A } },
        { "Shehai Summoning",  new int[] { B, B, B } },
        { "Warrior Wave",      new int[] { S, A, B } },
        { "Ansei Assault",     new int[] { B, A, B } },
        { "Ansei's Victory",   new int[] { B, A, B } },
        { "Battle Meditation", new int[] { C, B, B } },
        { "No Shira Poet",     new int[] { C, C, C } },
        { "Way of the Sword",  new int[] { D, D, D } },

        { "Rally",                  new int[] { A, S, A } },
        { "Siege Weapon Volley",    new int[] { A, S, B } },
        { "The Armory",             new int[] { A, S, A } },
        { "Banneret",               new int[] { A, S, A } },
        { "Knight Commander",       new int[] { S, S, A } },
        { "Reinforcements",         new int[] { S, A, B } },
        { "Archers' Volley",        new int[] { B, A, B } },
        { "Legion's Arrival",       new int[] { A, A, B } },
        { "Shield Bearer",          new int[] { C, B, B } },
        { "Bangkorai Sentries",     new int[] { C, B, C } },
        { "Knights of Saint Pelin", new int[] { C, A, C } },
        { "The Portcullis",         new int[] { C, C, D } },
        { "Fortify",                new int[] { D, D, D } },

        { "Bag of Tricks",     new int[] { C, B, B } },
        { "Bewilderment",      new int[] { CURSE, CURSE, CURSE } },
        { "Grand Larceny",     new int[] { A, A, B } },
        { "Jarring Lullaby",   new int[] { B, A, B } },
        { "Jeering Shadow",    new int[] { C, C, C } },
        { "Moonlit Illusion",  new int[] { B, A, B } },
        { "Pounce and Profit", new int[] { S, S, B } },
        { "Prowling Shadow",   new int[] { B, C, C } },
        { "Ring's Guile",      new int[] { C, C, C } },
        { "Shadow's Slumber",  new int[] { A, A, B } },
        { "Slight of Hand",    new int[] { A, B, D } },
        { "Stubborn Shadow",   new int[] { D, C, D } },
        { "Twilight Revelry",  new int[] { B, A, B } },
        { "Swipe",             new int[] { D, D, D } },

        { "Ambush",          new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Barterer",        new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Black Sacrament", new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Blackmail",       new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Gold",            new [] { UNKNOWN, CURSE, CURSE } },
        { "Harvest Season",  new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Imprisonment",    new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Ragpicker",       new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Tithe",           new [] { CONTRACT_ACTION, CONTRACT_ACTION, CONTRACT_ACTION } },
        { "Writ of Coin",    new [] { WOC, B, UNKNOWN } },
        { "Unknown",         new [] { UNKNOWN, UNKNOWN, UNKNOWN } },
    };

    public static int GetCardTier(string cardName, GamePhase gamePhase) => cardTierDict[cardName][(int)gamePhase];
}