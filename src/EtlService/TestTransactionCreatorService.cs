#if AUTOCREATE_TEST_TRANSACTION_ON_STARTUP

namespace EtlService;

public class TestTransactionCreatorService : BackgroundService
{
    private readonly IOptions<FoldersConfiguration> _foldersConfiguration;

    public TestTransactionCreatorService(IOptions<FoldersConfiguration> foldersConfiguration)
    {
        _foldersConfiguration = foldersConfiguration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var path = Path.Combine(_foldersConfiguration.Value.Input, Guid.NewGuid() + ".csv");
        var contents = @" ** header **
John, Doe, ""Lviv, Kleparivska 35, 4"",  500.0, 2022-27-01, 1234567, Water
Mike, Wiksen, ""Lviv, Kleparivska 40, 1"",  720.0, 2022-27-05, 7654321, Heat
Nick, Potter, ""Lviv, Gorodotska 120, 3"",  880.0, 2022-25-03, ""3334444"", Parking
Luke Pan,, ""Lviv, Gorodotska 120, 5"",  40.0, 2022-12-07, 2222111, Gas";

        await File.WriteAllTextAsync(path, contents, stoppingToken);
    }
}

#endif
