using ScriptsOfTribute;

namespace Bots;

public enum HandTierEnum
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

public class HandCardTier
{
    public string Name;
    public PatronId Deck;
    public HandTierEnum Tier;

    public HandCardTier(string name, PatronId deck, HandTierEnum tier)
    {
        Name = name;
        Deck = deck;
        Tier = tier;
    }
}
public class HandTierList
{
    private static HandCardTier[] HandTierArray = {
        new HandCardTier("Currency Exchange", PatronId.HLAALU, HandTierEnum.B),
        new HandCardTier("Luxury Exports", PatronId.HLAALU, HandTierEnum.D),
        new HandCardTier("Oathman", PatronId.HLAALU, HandTierEnum.D),
        // new HandCardTier("Ebony Mine", PatronId.HLAALU, HandTierEnum.C),
        new HandCardTier("Hlaalu Councilor", PatronId.HLAALU, HandTierEnum.S),
        new HandCardTier("Hlaalu Kinsman", PatronId.HLAALU, HandTierEnum.S),
        new HandCardTier("House Embassy", PatronId.HLAALU, HandTierEnum.S),
        new HandCardTier("House Marketplace", PatronId.HLAALU, HandTierEnum.A),
        new HandCardTier("Hireling", PatronId.HLAALU, HandTierEnum.D),
        new HandCardTier("Hostile Takeover", PatronId.HLAALU, HandTierEnum.D),
        // new HandCardTier("Kwama Egg Mine", PatronId.HLAALU, HandTierEnum.C),
        new HandCardTier("Customs Seizure", PatronId.HLAALU, HandTierEnum.D),
        new HandCardTier("Goods Shipment", PatronId.HLAALU, HandTierEnum.CURSE),

        new HandCardTier("Midnight Raid", PatronId.RED_EAGLE, HandTierEnum.B),
        // new HandCardTier("Blood Sacrifice", PatronId.RED_EAGLE, HandTierEnum.A),
        // new HandCardTier("Bloody Offering", PatronId.RED_EAGLE, HandTierEnum.A),
        // new HandCardTier("Bonfire", PatronId.RED_EAGLE, HandTierEnum.D),
        // new HandCardTier("Briarheart Ritual", PatronId.RED_EAGLE, HandTierEnum.C),
        // new HandCardTier("Clan-Witch", PatronId.RED_EAGLE, HandTierEnum.C),
        // new HandCardTier("Elder Witch", PatronId.RED_EAGLE, HandTierEnum.C),
        new HandCardTier("Hagraven", PatronId.RED_EAGLE, HandTierEnum.B),
        new HandCardTier("Hagraven Matron", PatronId.RED_EAGLE, HandTierEnum.A),
        // new HandCardTier("Imperial Plunder", PatronId.RED_EAGLE, HandTierEnum.B),
        // new HandCardTier("Imperial Spoils", PatronId.RED_EAGLE, HandTierEnum.B),
        // new HandCardTier("Karth Man-Hunter", PatronId.RED_EAGLE, HandTierEnum.C),
        new HandCardTier("War Song", PatronId.RED_EAGLE, HandTierEnum.CURSE),

        new HandCardTier("Blackfeather Knave", PatronId.DUKE_OF_CROWS, HandTierEnum.B),
        new HandCardTier("Plunder", PatronId.DUKE_OF_CROWS, HandTierEnum.S),
        new HandCardTier("Toll of Flesh", PatronId.DUKE_OF_CROWS, HandTierEnum.C),
        new HandCardTier("Toll of Silver", PatronId.DUKE_OF_CROWS, HandTierEnum.C),
        new HandCardTier("Murder of Crows", PatronId.DUKE_OF_CROWS, HandTierEnum.A),
        new HandCardTier("Pilfer", PatronId.DUKE_OF_CROWS, HandTierEnum.B),
        new HandCardTier("Squawking Oratory", PatronId.DUKE_OF_CROWS, HandTierEnum.A),
        // new HandCardTier("Law of Sovereign Roost", PatronId.DUKE_OF_CROWS, HandTierEnum.C),
        new HandCardTier("Pool of Shadow", PatronId.DUKE_OF_CROWS, HandTierEnum.C),
        new HandCardTier("Scratch", PatronId.DUKE_OF_CROWS, HandTierEnum.D),
        new HandCardTier("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, HandTierEnum.D),
        new HandCardTier("Blackfeather Knight", PatronId.DUKE_OF_CROWS, HandTierEnum.B),
        new HandCardTier("Peck", PatronId.DUKE_OF_CROWS, HandTierEnum.CURSE),

        new HandCardTier("Conquest", PatronId.ANSEI, HandTierEnum.C),
        // new HandCardTier("Grand Oratory", PatronId.ANSEI, HandTierEnum.B),
        new HandCardTier("Hira's End", PatronId.ANSEI, HandTierEnum.S),
        new HandCardTier("Hel Shira Herald", PatronId.ANSEI, HandTierEnum.S),
        new HandCardTier("March on Hattu", PatronId.ANSEI, HandTierEnum.S),
        new HandCardTier("Shehai Summoning", PatronId.ANSEI, HandTierEnum.S),
        new HandCardTier("Warrior Wave", PatronId.ANSEI, HandTierEnum.C),
        new HandCardTier("Ansei Assault", PatronId.ANSEI, HandTierEnum.A),
        new HandCardTier("Ansei's Victory", PatronId.ANSEI, HandTierEnum.S),
        // new HandCardTier("Battle Meditation", PatronId.ANSEI, HandTierEnum.C),
        new HandCardTier("No Shira Poet", PatronId.ANSEI, HandTierEnum.C),
        new HandCardTier("Way of the Sword", PatronId.ANSEI, HandTierEnum.CURSE),

        new HandCardTier("Rally", PatronId.PELIN, HandTierEnum.A),
        new HandCardTier("Siege Weapon Volley", PatronId.PELIN, HandTierEnum.C),
        new HandCardTier("The Armory", PatronId.PELIN, HandTierEnum.B),
        new HandCardTier("Banneret", PatronId.PELIN, HandTierEnum.S),
        new HandCardTier("Knight Commander", PatronId.PELIN, HandTierEnum.S),
        new HandCardTier("Reinforcements", PatronId.PELIN, HandTierEnum.D),
        new HandCardTier("Archers' Volley", PatronId.PELIN, HandTierEnum.D),
        new HandCardTier("Legion's Arrival", PatronId.PELIN, HandTierEnum.D),
        // new HandCardTier("Shield Bearer", PatronId.PELIN, HandTierEnum.C),
        new HandCardTier("Bangkorai Sentries", PatronId.PELIN, HandTierEnum.A),
        new HandCardTier("Knights of Saint Pelin", PatronId.PELIN, HandTierEnum.A),
        new HandCardTier("The Portcullis", PatronId.PELIN, HandTierEnum.D),
        new HandCardTier("Fortify", PatronId.PELIN, HandTierEnum.CURSE),

        // new HandCardTier("Bag of Tricks", PatronId.RAJHIN, HandTierEnum.D),
        new HandCardTier("Bewilderment", PatronId.RAJHIN, HandTierEnum.CURSE),
        new HandCardTier("Grand Larceny", PatronId.RAJHIN, HandTierEnum.C),
        new HandCardTier("Jarring Lullaby", PatronId.RAJHIN, HandTierEnum.C),
        new HandCardTier("Jeering Shadow", PatronId.RAJHIN, HandTierEnum.E),
        // new HandCardTier("Moonlit Illusion", PatronId.RAJHIN, HandTierEnum.B),
        new HandCardTier("Pounce and Profit", PatronId.RAJHIN, HandTierEnum.C),
        new HandCardTier("Prowling Shadow", PatronId.RAJHIN, HandTierEnum.B),
        // new HandCardTier("Ring's Guile", PatronId.RAJHIN, HandTierEnum.C),
        new HandCardTier("Shadow's Slumber", PatronId.RAJHIN, HandTierEnum.B),
        new HandCardTier("Slight of Hand", PatronId.RAJHIN, HandTierEnum.CURSE),
        new HandCardTier("Stubborn Shadow", PatronId.RAJHIN, HandTierEnum.D),
        new HandCardTier("Twilight Revelry", PatronId.RAJHIN, HandTierEnum.C),
        new HandCardTier("Swipe", PatronId.RAJHIN, HandTierEnum.CURSE),

        // new HandCardTier("Ambush", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        // new HandCardTier("Barterer", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        // new HandCardTier("Black Sacrament", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        // new HandCardTier("Blackmail", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        new HandCardTier("Gold", PatronId.TREASURY, HandTierEnum.CURSE),
        // new HandCardTier("Harvest Season", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        // new HandCardTier("Imprisonment", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        // new HandCardTier("Ragpicker", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        // new HandCardTier("Tithe", PatronId.TREASURY, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION, HandTierEnum.CONTRACT_ACTION),
        new HandCardTier("Writ of Coin", PatronId.TREASURY, HandTierEnum.CURSE),
        // new HandCardTier("Unknown", PatronId.TREASURY, HandTierEnum.UNKNOWN, HandTierEnum.UNKNOWN, HandTierEnum.UNKNOWN)
    };

    public static HandTierEnum GetCardTier(string cardName)
    {
        return HandTierEnum.E;
        return Array.Find(HandTierArray, x => x.Name == cardName).Tier;
    }
}