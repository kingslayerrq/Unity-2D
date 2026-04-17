using System;

public static class SeedUtility
{
    public static int DeriveSeed(int runSeed, RngCategory category)
    {
        unchecked
        {
            int seed = runSeed;
            seed = seed * 397 ^ (int)category;
            return seed;
        }
    }

    public static int DeriveNodeSeed(int runSeed, int act, int floor, int index, RngCategory category)
    {
        unchecked
        {
            int seed = runSeed;
            seed = seed * 397 ^ act;
            seed = seed * 397 ^ floor;
            seed = seed * 397 ^ index;
            seed = seed * 397 ^ (int)category;
            return seed;
        }
    }
}
public enum RngCategory
{
    Map = 1,
    Loot = 2,
    Event = 3,
    Combat = 4,
    Shop = 5,
    Unknown = 6,
    UnknownOutcome = 7,
    UnknownContent = 8,
    NodeAssign = 9,
    NodeBuild = 10
}