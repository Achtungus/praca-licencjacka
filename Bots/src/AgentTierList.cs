using ScriptsOfTribute;

namespace Bots;

public enum AgentTierEnum
{
    /*
    S = 1000,
    A = 400,
    B = 200,
    C = 90,
    D = 40,
    */
    S = 5,
    A = 4,
    B = 3,
    C = 2,
    D = 1,
    E = 0,
    CURSE = -1,
}

public class AgentCardTier
{
    public string Name;
    public PatronId Deck;
    public AgentTierEnum Tier;

    public AgentCardTier(string name, PatronId deck, AgentTierEnum tier)
    {
        Name = name;
        Deck = deck;
        Tier = tier;
    }
}
public class AgentTierList
{
    private static AgentCardTier[] AgentTierArray = {
        // new AgentCardTier("Currency Exchange", PatronId.HLAALU, AgentTierEnum.B),
        // new AgentCardTier("Luxury Exports", PatronId.HLAALU, AgentTierEnum.D),
        new AgentCardTier("Oathman", PatronId.HLAALU, AgentTierEnum.C),
        // new AgentCardTier("Ebony Mine", PatronId.HLAALU, AgentTierEnum.C),
        new AgentCardTier("Hlaalu Councilor", PatronId.HLAALU, AgentTierEnum.S),
        new AgentCardTier("Hlaalu Kinsman", PatronId.HLAALU, AgentTierEnum.S),
        // new AgentCardTier("House Embassy", PatronId.HLAALU, AgentTierEnum.S),
        // new AgentCardTier("House Marketplace", PatronId.HLAALU, AgentTierEnum.A),
        new AgentCardTier("Hireling", PatronId.HLAALU, AgentTierEnum.C),
        // new AgentCardTier("Hostile Takeover", PatronId.HLAALU, AgentTierEnum.D),
        // new AgentCardTier("Kwama Egg Mine", PatronId.HLAALU, AgentTierEnum.C),
        // new AgentCardTier("Customs Seizure", PatronId.HLAALU, AgentTierEnum.D),
        // new AgentCardTier("Goods Shipment", PatronId.HLAALU, AgentTierEnum.CURSE),

        // new AgentCardTier("Midnight Raid", PatronId.RED_EAGLE, AgentTierEnum.B),
        // new AgentCardTier("Blood Sacrifice", PatronId.RED_EAGLE, AgentTierEnum.A),
        // new AgentCardTier("Bloody Offering", PatronId.RED_EAGLE, AgentTierEnum.A),
        // new AgentCardTier("Bonfire", PatronId.RED_EAGLE, AgentTierEnum.D),
        // new AgentCardTier("Briarheart Ritual", PatronId.RED_EAGLE, AgentTierEnum.C),
        new AgentCardTier("Clan-Witch", PatronId.RED_EAGLE, AgentTierEnum.A),
        new AgentCardTier("Elder Witch", PatronId.RED_EAGLE, AgentTierEnum.A),
        new AgentCardTier("Hagraven", PatronId.RED_EAGLE, AgentTierEnum.A),
        new AgentCardTier("Hagraven Matron", PatronId.RED_EAGLE, AgentTierEnum.A),
        // new AgentCardTier("Imperial Plunder", PatronId.RED_EAGLE, AgentTierEnum.B),
        // new AgentCardTier("Imperial Spoils", PatronId.RED_EAGLE, AgentTierEnum.B),
        new AgentCardTier("Karth Man-Hunter", PatronId.RED_EAGLE, AgentTierEnum.B),
        // new AgentCardTier("War Song", PatronId.RED_EAGLE, AgentTierEnum.CURSE),

        new AgentCardTier("Blackfeather Knave", PatronId.DUKE_OF_CROWS, AgentTierEnum.B),
        // new AgentCardTier("Plunder", PatronId.DUKE_OF_CROWS, AgentTierEnum.S),
        // new AgentCardTier("Toll of Flesh", PatronId.DUKE_OF_CROWS, AgentTierEnum.C),
        // new AgentCardTier("Toll of Silver", PatronId.DUKE_OF_CROWS, AgentTierEnum.C),
        // new AgentCardTier("Murder of Crows", PatronId.DUKE_OF_CROWS, AgentTierEnum.A),
        // new AgentCardTier("Pilfer", PatronId.DUKE_OF_CROWS, AgentTierEnum.B),
        // new AgentCardTier("Squawking Oratory", PatronId.DUKE_OF_CROWS, AgentTierEnum.A),
        // new AgentCardTier("Law of Sovereign Roost", PatronId.DUKE_OF_CROWS, AgentTierEnum.C),
        // new AgentCardTier("Pool of Shadow", PatronId.DUKE_OF_CROWS, AgentTierEnum.C),
        // new AgentCardTier("Scratch", PatronId.DUKE_OF_CROWS, AgentTierEnum.D),
        new AgentCardTier("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, AgentTierEnum.B),
        new AgentCardTier("Blackfeather Knight", PatronId.DUKE_OF_CROWS, AgentTierEnum.B),
        // new AgentCardTier("Peck", PatronId.DUKE_OF_CROWS, AgentTierEnum.CURSE),

        // new AgentCardTier("Conquest", PatronId.ANSEI, AgentTierEnum.C),
        // new AgentCardTier("Grand Oratory", PatronId.ANSEI, AgentTierEnum.B),
        // new AgentCardTier("Hira's End", PatronId.ANSEI, AgentTierEnum.S),
        new AgentCardTier("Hel Shira Herald", PatronId.ANSEI, AgentTierEnum.B),
        // new AgentCardTier("March on Hattu", PatronId.ANSEI, AgentTierEnum.S),
        // new AgentCardTier("Shehai Summoning", PatronId.ANSEI, AgentTierEnum.S),
        // new AgentCardTier("Warrior Wave", PatronId.ANSEI, AgentTierEnum.C),
        // new AgentCardTier("Ansei Assault", PatronId.ANSEI, AgentTierEnum.A),
        // new AgentCardTier("Ansei's Victory", PatronId.ANSEI, AgentTierEnum.S),
        // new AgentCardTier("Battle Meditation", PatronId.ANSEI, AgentTierEnum.C),
        new AgentCardTier("No Shira Poet", PatronId.ANSEI, AgentTierEnum.C),
        // new AgentCardTier("Way of the Sword", PatronId.ANSEI, AgentTierEnum.CURSE),

        // new AgentCardTier("Rally", PatronId.PELIN, AgentTierEnum.A),
        // new AgentCardTier("Siege Weapon Volley", PatronId.PELIN, AgentTierEnum.C),
        // new AgentCardTier("The Armory", PatronId.PELIN, AgentTierEnum.B),
        new AgentCardTier("Banneret", PatronId.PELIN, AgentTierEnum.S),
        new AgentCardTier("Knight Commander", PatronId.PELIN, AgentTierEnum.S),
        // new AgentCardTier("Reinforcements", PatronId.PELIN, AgentTierEnum.D),
        // new AgentCardTier("Archers' Volley", PatronId.PELIN, AgentTierEnum.D),
        // new AgentCardTier("Legion's Arrival", PatronId.PELIN, AgentTierEnum.D),
        new AgentCardTier("Shield Bearer", PatronId.PELIN, AgentTierEnum.B),
        new AgentCardTier("Bangkorai Sentries", PatronId.PELIN, AgentTierEnum.B),
        new AgentCardTier("Knights of Saint Pelin", PatronId.PELIN, AgentTierEnum.A),
        // new AgentCardTier("The Portcullis", PatronId.PELIN, AgentTierEnum.D),
        // new AgentCardTier("Fortify", PatronId.PELIN, AgentTierEnum.CURSE),

        // new AgentCardTier("Bag of Tricks", PatronId.RAJHIN, AgentTierEnum.D),
        // new AgentCardTier("Bewilderment", PatronId.RAJHIN, AgentTierEnum.CURSE),
        // new AgentCardTier("Grand Larceny", PatronId.RAJHIN, AgentTierEnum.C),
        // new AgentCardTier("Jarring Lullaby", PatronId.RAJHIN, AgentTierEnum.C),
        new AgentCardTier("Jeering Shadow", PatronId.RAJHIN, AgentTierEnum.C),
        // new AgentCardTier("Moonlit Illusion", PatronId.RAJHIN, AgentTierEnum.B),
        // new AgentCardTier("Pounce and Profit", PatronId.RAJHIN, AgentTierEnum.C),
        new AgentCardTier("Prowling Shadow", PatronId.RAJHIN, AgentTierEnum.B),
        // new AgentCardTier("Ring's Guile", PatronId.RAJHIN, AgentTierEnum.C),
        // new AgentCardTier("Shadow's Slumber", PatronId.RAJHIN, AgentTierEnum.B),
        // new AgentCardTier("Slight of Agent", PatronId.RAJHIN, AgentTierEnum.CURSE),
        new AgentCardTier("Stubborn Shadow", PatronId.RAJHIN, AgentTierEnum.C),
        // new AgentCardTier("Twilight Revelry", PatronId.RAJHIN, AgentTierEnum.C),
        // new AgentCardTier("Swipe", PatronId.RAJHIN, AgentTierEnum.CURSE),

        // new AgentCardTier("Ambush", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Barterer", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Black Sacrament", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Blackmail", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Gold", PatronId.TREASURY, AgentTierEnum.CURSE),
        // new AgentCardTier("Harvest Season", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Imprisonment", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Ragpicker", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Tithe", PatronId.TREASURY, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION, AgentTierEnum.CONTRACT_ACTION),
        // new AgentCardTier("Writ of Coin", PatronId.TREASURY, AgentTierEnum.CURSE),
        // new AgentCardTier("Unknown", PatronId.TREASURY, AgentTierEnum.UNKNOWN, AgentTierEnum.UNKNOWN, AgentTierEnum.UNKNOWN)
    };

    public static AgentTierEnum GetCardTier(string cardName)
    {
        return AgentTierEnum.E;
        return Array.Find(AgentTierArray, x => x.Name == cardName).Tier;
    }
}