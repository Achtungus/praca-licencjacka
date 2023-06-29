namespace ParamEvolution3;

public class GameParams
{
    static readonly Random rnd = new();

    static readonly Dictionary<Param, double[]> defaultGameParams = new Dictionary<Param, double[]> {
        { Param.Crow,                 new double [] { 300, 500, 600 } },
        { Param.Ansei,                new double [] { 50, 50, 50 } },
        { Param.Prestige,             new double [] { 10, 60, 200 } },
        { Param.CardLimit,            new double [] { 13, 13, 13 } }, // dosyc male zeby chcial niszczyc
        { Param.ComboPower,           new double [] { 3, 3, 3 } },
        { Param.OurAgent,             new double [] { 1.1, 1.1, 1.1 } },
        { Param.EnemyAgent,           new double [] { -60, -80, -150 } },
        { Param.UpcomingCard,         new double [] { 15, 25, 100 } },
        { Param.TierMultiplier,       new double [] {10, 10, 10}},
        { Param.TavernPenatly,        new double [] {-2, -2, -2}},
        { Param.KnowingCardCombo,     new double [] {1, 1, 1}}, //epsilon
        { Param.After40Bonus,         new double [] {300, 300, 300}},
    };

    Dictionary<Param, double[]> gameParams = new();

    public GameParams()
    {
        foreach (var (param, values) in defaultGameParams)
        {
            gameParams[param] = new double[] { values[0], values[1], values[2] };
        }
    }

    public GameParams(GameParams oth)
    {
        foreach (var (param, values) in oth.gameParams)
        {
            gameParams[param] = new double[] { values[0], values[1], values[2] };
        }
    }

    public GameParams Mutate(double chance, double maxEps)
    {
        GameParams newParams = new(this);
        foreach (var key in defaultGameParams.Keys)
        {
            for (int j = 0; j < 3; j++)
            {
                if (rnd.NextDouble() < chance)
                {
                    double eps = (rnd.NextDouble() * 2.0 - 1.0) * maxEps;
                    newParams.gameParams[key][j] *= (1.0 + eps);
                }
            }
        }
        return newParams;
    }

    static public GameParams Combine(GameParams l, GameParams r)
    {
        GameParams newParams = new();

        foreach (var key in defaultGameParams.Keys)
        {
            int first = rnd.Next(2);
            if (first)
            {
                var lValues = l.gameParams[key];
                newParams.gameParams[key] = new double[] { lValues[0], lValues[1], lValues[2] };
            }
            else
            {
                var rValues = r.gameParams[key];
                newParams.gameParams[key] = new double[] { rValues[0], rValues[1], rValues[2] };
            }
        }
        return newParams;
    }

    public override string ToString()
    {
        string res = "";
        foreach (var (param, values) in gameParams)
        {
            res += $"{param}: {values[0]}, {values[1]}, {values[2]}\n";
        }
        return res;
    }

    public double GetParamValue(Param gameParam, GamePhase gamePhase) => gameParams[gameParam][(int)gamePhase];
}