using System;


public interface IRNG : IService
{
    public static bool IsInit { get; }
    public static int Seed { get; }
    public void Init(int seed);
    public int Range(int minInclusive, int maxExclusive);

}

public class RNG : IRNG
{
    public bool IsInit { get; private set; }
    public int Seed { get; private set; }

    private Random _random;
    
    public void Init(int seed)
    {
        if (IsInit) throw new Exception("RNG se intento inicializar 2 vecces");

        IsInit = true;
        Seed = seed;
        _random = new Random(seed);
    }

    public int Range(int minInclusive, int maxExclusive)
    {
        if (!IsInit) throw new Exception("RNG llamado sin inicializar");
        return _random.Next(minInclusive, maxExclusive);
    }
}
