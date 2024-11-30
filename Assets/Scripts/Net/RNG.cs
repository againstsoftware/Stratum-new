using System;

public static class RNG
{
    public static bool IsInit { get; private set; }
    public static int Seed { get; private set; }

    private static Random _random;
    
    public static void Init(int seed)
    {
        if (IsInit) throw new Exception("RNG se intento inicializar 2 vecces");

        IsInit = true;
        Seed = seed;
        _random = new Random(seed);
    }

    public static int Range(int minInclusive, int maxExclusive)
    {
        if (!IsInit) throw new Exception("RNG llamado sin inicializar");
        return _random.Next(minInclusive, maxExclusive);
    }
}
