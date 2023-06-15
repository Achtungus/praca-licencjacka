using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bots;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var aiType = typeof(AI);
var allDlls = currentDirectory.GetFiles("*.dll").ToList();
List<Type> allBots = allDlls.Select(f => f.FullName)
    .Select(Assembly.LoadFile)
    .SelectMany(a => a.GetTypes())
    .Where(t => aiType.IsAssignableFrom(t) && !t.IsInterface)
    .ToList();

var noOfRunsOption = new Option<int>(
    name: "--runs",
    description: "Number of games to run.",
    getDefaultValue: () => 1);
noOfRunsOption.AddAlias("-n");

var threadsOption = new Option<int>(
    name: "--threads",
    description: "Number of CPU threads to use.",
    getDefaultValue: () => 1);
threadsOption.AddAlias("-t");

var seedOption = new Option<ulong?>(
    name: "--seed",
    description: "Specify seed for RNG. In case of multiple games, each subsequent game will be played with <seed+1>.",
    getDefaultValue: () => null);
seedOption.AddAlias("-s");


Type? cachedBot = null;

Type? FindBot(string name, out string? errorMessage)
{
    errorMessage = null;

    bool findByFullName = name.Contains('.');

    if (cachedBot is not null && (findByFullName ? cachedBot.FullName : cachedBot.Name) == name)
    {
        return cachedBot;
    }

    var botCount = allBots.Count(t => (findByFullName ? t.FullName : t.Name) == name);

    if (botCount == 0)
    {
        errorMessage = $"Bot {name} not found in any DLLs. List of bots found:\n";
        errorMessage += string.Join('\n', allBots.Select(b => b.FullName));
        return null;
    }

    if (botCount > 1 && !findByFullName)
    {
        errorMessage = "More than one bots with the same name found. Please, specify full name of the target bot: <namespace>.Name. Bots found:\n";
        errorMessage += string.Join('\n', allBots.Select(b => b.FullName));
        return null;
    }
    // TODO: Support also specifying which file to use.
    else if (botCount > 1 && findByFullName)
    {
        errorMessage = "More than one bots with the same full name found. This means you have different DLLs with the same namespaces and bot names.\n" +
                       "This use case is not yet supported. List of all found bots:\n";
        errorMessage += string.Join('\n', allBots.Select(b => b.FullName));
        return null;
    }

    cachedBot = allBots.First(t => (findByFullName ? t.FullName : t.Name) == name);

    if (cachedBot.GetConstructor(Type.EmptyTypes) is null)
    {
        errorMessage = $"Bot {name} bot can't be instantiated as it doesn't provide a parameterless constructor.";
    }

    return cachedBot;
}

Type? ParseBotArg(ArgumentResult arg)
{
    if (arg.Tokens.Count != 1)
    {
        arg.ErrorMessage = "Bot name must be a single token.";
        return null;
    }

    var botType = FindBot(arg.Tokens[0].Value, out var errorMessage);
    if (errorMessage is not null)
    {
        arg.ErrorMessage = errorMessage;
        return null;
    }

    return botType!;
}

var bot1NameArgument = new Argument<Type?>(name: "bot1Name", description: "Name of the first bot.", parse: ParseBotArg);
var bot2NameArgument = new Argument<Type?>(name: "bot2Name", description: "Name of the second bot.", parse: ParseBotArg);

var mainCommand = new RootCommand("A game runner for bots.")
{
    noOfRunsOption,
    threadsOption,
    seedOption,
    bot1NameArgument,
    bot2NameArgument,
};

ScriptsOfTribute.AI.ScriptsOfTribute PrepareGame(AI bot1, AI bot2, ulong seed)
{
    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1!, bot2!);
    game.Seed = seed;
    return game;
}

var returnValue = 0;
mainCommand.SetHandler((runs, noOfThreads, maybeSeed, bot1Type, bot2Type) =>
{
    if (noOfThreads < 1)
    {
        Console.Error.WriteLine("ERROR: Can't use less than 1 thread.");
        returnValue = -1;
    }

    ulong actualSeed;
    if (maybeSeed is null)
    {
        actualSeed = (ulong)new Random().NextInt64();
    }
    else
    {
        actualSeed = (ulong)maybeSeed;
    }


    if (noOfThreads > Environment.ProcessorCount)
    {
        Console.Error.WriteLine($"WARNING: More threads ({noOfThreads}) specified than logical processor count ({Environment.ProcessorCount}).");
    }

    Parallel.For(0, runs,
        runIndex =>
        {
            var results = new EndGameState[amount];
            var bot1 = (AI?)Activator.CreateInstance(bot1Type!);
            var bot2 = (AI?)Activator.CreateInstance(bot2Type!);
            var game = PrepareGame(bot1!, bot2!, seed);
            seed += 1;
            var (endReason, _) = game.Play();
            results[i] = endReason;
            // Interlocked.Add(ref totalSize, size);
        });


    var watch = Stopwatch.StartNew();
    var currentSeed = actualSeed;
    for (var i = 0; i < noOfThreads; i++)
    {
        var spawnAdditionalGame = gamesPerThreadRemainder <= 0 ? 0 : 1;
        gamesPerThreadRemainder -= 1;
        var gamesToPlay = gamesPerThread + spawnAdditionalGame;
        Console.WriteLine($"Playing {gamesToPlay} games in thread #{i}");
        var threadNo = i;
        var currentSeedCopy = currentSeed;
        threads[i] = Task.Factory.StartNew(() => PlayGames(gamesToPlay, bot1Type!, bot2Type!, threadNo, currentSeedCopy));
        currentSeed += (ulong)gamesToPlay;
    }
    Task.WaitAll(threads.ToArray<Task>());

    var timeTaken = watch.ElapsedMilliseconds;

    var counter = new GameEndStatsCounter();
    threads.SelectMany(t => t.Result).ToList().ForEach(r => counter.Add(r));

    Console.WriteLine($"\nInitial seed used: {actualSeed}");
    Console.WriteLine($"Total time taken: {timeTaken}ms");
    Console.WriteLine("\nStats from the games played:");
    Console.WriteLine(counter.ToString());

}, noOfRunsOption, threadsOption, seedOption, bot1NameArgument, bot2NameArgument);

mainCommand.Invoke(args);

return returnValue;
