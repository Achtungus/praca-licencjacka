namespace ParamEvolution;

public static class InstantPlayCards
{
    static readonly HashSet<string> instantPlayCards = new HashSet<string> {
        // HLAALU
        // "Currency Exchange",
        "Luxury Exports",
        // "Oathman",
        "Ebony Mine",
        // "Hlaalu Councilor",
        // "Hlaalu Kinsman",
        // "House Embassy",
        // "House Marketplace",
        // "Hireling",
        // "Hostile Takeover",
        "Kwama Egg Mine",
        // "Customs Seizure",
        "Goods Shipment",

        // RED EAGLE
        "Midnight Raid",
        // "Blood Sacrifice",
        // "Bloody Offering",
        // "Bonfire",
        // "Briarheart Ritual",
        // "Clan-Witch",
        // "Elder Witch",
        // "Hagraven",
        // "Hagraven Matron",
        // "Imperial Plunder",
        // "Imperial Spoils",
        // "Karth Man-Hunter",
        "War Song",

        // DUKE OF CROWS
        // "Blackfeather Knave",
        // "Plunder",
        // "Toll of Flesh",
        // "Toll of Silver",
        // "Murder of Crows",
        // "Pilfer",
        // "Squawking Oratory",
        // "Law of Sovereign Roost",
        // "Pool of Shadow",
        // "Scratch",
        // "Blackfeather Brigand",
        // "Blackfeather Knight",
        // "Peck",

        // ANSEI
        // "Conquest",
        // "Grand Oratory",
        // "Hira's End",
        // "Hel Shira Herald",
        // "March on Hattu",
        // "Shehai Summoning",
        // "Warrior Wave",
        // "Ansei Assault",
        // "Ansei's Victory",
        // "Battle Meditation",
        // "No Shira Poet",
        // "Way of the Sword",

        // PELIN
        // "Rally",
        "Siege Weapon Volley",
        "The Armory",
        "Banneret",
        "Knight Commander",
        "Reinforcements",
        "Archers' Volley",
        "Legion's Arrival",
        "Shield Bearer",
        "Bangkorai Sentries",
        "Knights of Saint Pelin",
        "The Portcullis",
        "Fortify",

        // RAJHIN
        // "Bag of Tricks", <- contract action
        "Bewilderment",
        "Grand Larceny",
        "Jarring Lullaby",
        "Jeering Shadow",
        // "Moonlit Illusion", <- contract action
        "Pounce and Profit",
        "Prowling Shadow",
        // "Ring's Guile", <- contract action
        "Shadow's Slumber",
        // "Slight of Hand",
        "Stubborn Shadow",
        // "Twilight Revelry",
        "Swipe",

        // TREASURY
        // "Ambush", <- contract action
        // "Barterer", <- contract action
        // "Black Sacrament", <- contract action
        // "Blackmail", <- contract action
        "Gold",
        // "Harvest Season", <- contract action
        // "Imprisonment", <- contract action
        // "Ragpicker", <- contract action
        // "Tithe", <- contract action
        "Writ of Coin",
        // "Unknown", <- ?
    };

    public static bool IsInstantPlay(string cardName)
    {
        return instantPlayCards.Contains(cardName);
    }
}