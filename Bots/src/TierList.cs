using ScriptsOfTribute;

namespace Bots;

public enum GamePhaseTierEnum
{
    /*
    S = 1000,
    A = 400,
    B = 200,
    C = 90,
    D = 40,
    */
    S = 50,
    A = 30,
    B = 10,
    C = 3,
    D = 1,
    UNKNOWN = 0,
    CONTRACT_ACTION = 0,
    CURSE = -3,
}

public class PhaseCardTier
{
    public string Name;
    public PatronId Deck;
    public List<GamePhaseTierEnum> Tier;

    public PhaseCardTier(string name, PatronId deck, GamePhaseTierEnum tierEarly, GamePhaseTierEnum tierMid = GamePhaseTierEnum.B, GamePhaseTierEnum tierLate = GamePhaseTierEnum.B)
    {
        Name = name;
        Deck = deck;
        Tier = new List<GamePhaseTierEnum> { tierEarly, tierMid, tierLate };
    }
}
public class GamePhaseTierList
{
    private static PhaseCardTier[] CardTierArray = {
        new PhaseCardTier("Currency Exchange", PatronId.HLAALU, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Luxury Exports", PatronId.HLAALU, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.C),
        new PhaseCardTier("Oathman", PatronId.HLAALU, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Ebony Mine", PatronId.HLAALU, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("Hlaalu Councilor", PatronId.HLAALU, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.C),
        new PhaseCardTier("Hlaalu Kinsman", PatronId.HLAALU, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.C),
        new PhaseCardTier("House Embassy", PatronId.HLAALU, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.C),
        new PhaseCardTier("House Marketplace", PatronId.HLAALU, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.C),
        new PhaseCardTier("Hireling", PatronId.HLAALU, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.D),
        new PhaseCardTier("Hostile Takeover", PatronId.HLAALU, GamePhaseTierEnum.B, GamePhaseTierEnum.C, GamePhaseTierEnum.D),
        new PhaseCardTier("Kwama Egg Mine", PatronId.HLAALU, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("Customs Seizure", PatronId.HLAALU, GamePhaseTierEnum.D, GamePhaseTierEnum.D, GamePhaseTierEnum.D),
        new PhaseCardTier("Goods Shipment", PatronId.HLAALU, GamePhaseTierEnum.D, GamePhaseTierEnum.D, GamePhaseTierEnum.D),

        new PhaseCardTier("Midnight Raid", PatronId.RED_EAGLE, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.S),
        new PhaseCardTier("Blood Sacrifice", PatronId.RED_EAGLE, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.S),
        new PhaseCardTier("Bloody Offering", PatronId.RED_EAGLE, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.S),
        new PhaseCardTier("Bonfire", PatronId.RED_EAGLE, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("Briarheart Ritual", PatronId.RED_EAGLE, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("Clan-Witch", PatronId.RED_EAGLE, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.D),
        new PhaseCardTier("Elder Witch", PatronId.RED_EAGLE, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.D),
        new PhaseCardTier("Hagraven", PatronId.RED_EAGLE, GamePhaseTierEnum.D, GamePhaseTierEnum.B, GamePhaseTierEnum.D),
        new PhaseCardTier("Hagraven Matron", PatronId.RED_EAGLE, GamePhaseTierEnum.D, GamePhaseTierEnum.A, GamePhaseTierEnum.C),
        new PhaseCardTier("Imperial Plunder", PatronId.RED_EAGLE, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Imperial Spoils", PatronId.RED_EAGLE, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Karth Man-Hunter", PatronId.RED_EAGLE, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("War Song", PatronId.RED_EAGLE, GamePhaseTierEnum.D, GamePhaseTierEnum.D, GamePhaseTierEnum.D),

        new PhaseCardTier("Blackfeather Knave", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Plunder", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.S),
        new PhaseCardTier("Toll of Flesh", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Toll of Silver", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Murder of Crows", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Pilfer", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.A, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Squawking Oratory", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.A, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Law of Sovereign Roost", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Pool of Shadow", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Scratch", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.C),
        new PhaseCardTier("Blackfeather Knight", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("Peck", PatronId.DUKE_OF_CROWS, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.C),

        new PhaseCardTier("Conquest", PatronId.ANSEI, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Grand Oratory", PatronId.ANSEI, GamePhaseTierEnum.B, GamePhaseTierEnum.S, GamePhaseTierEnum.S),
        new PhaseCardTier("Hira's End", PatronId.ANSEI, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.S),
        new PhaseCardTier("Hel Shira Herald", PatronId.ANSEI, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("March on Hattu", PatronId.ANSEI, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.A),
        new PhaseCardTier("Shehai Summoning", PatronId.ANSEI, GamePhaseTierEnum.B, GamePhaseTierEnum.B, GamePhaseTierEnum.B),
        new PhaseCardTier("Warrior Wave", PatronId.ANSEI, GamePhaseTierEnum.S, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Ansei Assault", PatronId.ANSEI, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Ansei's Victory", PatronId.ANSEI, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Battle Meditation", PatronId.ANSEI, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.B),
        new PhaseCardTier("No Shira Poet", PatronId.ANSEI, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.C),
        new PhaseCardTier("Way of the Sword", PatronId.ANSEI, GamePhaseTierEnum.D, GamePhaseTierEnum.D, GamePhaseTierEnum.D),

        new PhaseCardTier("Rally", PatronId.PELIN, GamePhaseTierEnum.A, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Siege Weapon Volley", PatronId.PELIN, GamePhaseTierEnum.A, GamePhaseTierEnum.S, GamePhaseTierEnum.B),
        new PhaseCardTier("The Armory", PatronId.PELIN, GamePhaseTierEnum.A, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Banneret", PatronId.PELIN, GamePhaseTierEnum.A, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Knight Commander", PatronId.PELIN, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.A),
        new PhaseCardTier("Reinforcements", PatronId.PELIN, GamePhaseTierEnum.S, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Archers' Volley", PatronId.PELIN, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Legion's Arrival", PatronId.PELIN, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Shield Bearer", PatronId.PELIN, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.B),
        new PhaseCardTier("Bangkorai Sentries", PatronId.PELIN, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.C),
        new PhaseCardTier("Knights of Saint Pelin", PatronId.PELIN, GamePhaseTierEnum.C, GamePhaseTierEnum.A, GamePhaseTierEnum.C),
        new PhaseCardTier("The Portcullis", PatronId.PELIN, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.D),
        new PhaseCardTier("Fortify", PatronId.PELIN, GamePhaseTierEnum.D, GamePhaseTierEnum.D, GamePhaseTierEnum.D),

        new PhaseCardTier("Bag of Tricks", PatronId.RAJHIN, GamePhaseTierEnum.C, GamePhaseTierEnum.B, GamePhaseTierEnum.B),
        new PhaseCardTier("Bewilderment", PatronId.RAJHIN, GamePhaseTierEnum.CURSE, GamePhaseTierEnum.CURSE, GamePhaseTierEnum.CURSE),
        new PhaseCardTier("Grand Larceny", PatronId.RAJHIN, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Jarring Lullaby", PatronId.RAJHIN, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Jeering Shadow", PatronId.RAJHIN, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.C),
        new PhaseCardTier("Moonlit Illusion", PatronId.RAJHIN, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Pounce and Profit", PatronId.RAJHIN, GamePhaseTierEnum.S, GamePhaseTierEnum.S, GamePhaseTierEnum.B),
        new PhaseCardTier("Prowling Shadow", PatronId.RAJHIN, GamePhaseTierEnum.B, GamePhaseTierEnum.C, GamePhaseTierEnum.C),
        new PhaseCardTier("Ring's Guile", PatronId.RAJHIN, GamePhaseTierEnum.C, GamePhaseTierEnum.C, GamePhaseTierEnum.C),
        new PhaseCardTier("Shadow's Slumber", PatronId.RAJHIN, GamePhaseTierEnum.A, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Slight of Hand", PatronId.RAJHIN, GamePhaseTierEnum.A, GamePhaseTierEnum.B, GamePhaseTierEnum.D),
        new PhaseCardTier("Stubborn Shadow", PatronId.RAJHIN, GamePhaseTierEnum.D, GamePhaseTierEnum.C, GamePhaseTierEnum.D),
        new PhaseCardTier("Twilight Revelry", PatronId.RAJHIN, GamePhaseTierEnum.B, GamePhaseTierEnum.A, GamePhaseTierEnum.B),
        new PhaseCardTier("Swipe", PatronId.RAJHIN, GamePhaseTierEnum.D, GamePhaseTierEnum.D, GamePhaseTierEnum.D),

        new PhaseCardTier("Ambush", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Barterer", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Black Sacrament", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Blackmail", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Gold", PatronId.TREASURY, GamePhaseTierEnum.UNKNOWN, GamePhaseTierEnum.CURSE, GamePhaseTierEnum.CURSE),
        new PhaseCardTier("Harvest Season", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Imprisonment", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Ragpicker", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Tithe", PatronId.TREASURY, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION, GamePhaseTierEnum.CONTRACT_ACTION),
        new PhaseCardTier("Writ of Coin", PatronId.TREASURY, GamePhaseTierEnum.S, GamePhaseTierEnum.C, GamePhaseTierEnum.UNKNOWN),
        new PhaseCardTier("Unknown", PatronId.TREASURY, GamePhaseTierEnum.UNKNOWN, GamePhaseTierEnum.UNKNOWN, GamePhaseTierEnum.UNKNOWN)
    };

    public static GamePhaseTierEnum GetCardTier(string cardName, int gamePhase)
    {
        return Array.Find(CardTierArray, x => x.Name == cardName).Tier[gamePhase];
    }
}