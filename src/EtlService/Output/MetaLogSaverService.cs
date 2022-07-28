namespace EtlService.Output;

public class MetaLogSaverService : IHostedService
{
    private readonly ILogger<MetaLogSaverService> _logger;
    private readonly IOptions<FoldersConfiguration> _foldersConfig;
    private readonly ITransactionParser _transactionParser;
    private Timer? _timer;

    public MetaLogSaverService(
        ILogger<MetaLogSaverService> logger,
        IOptions<FoldersConfiguration> foldersConfig,
        ITransactionParser transactionParser)
    {
        _logger = logger;
        _foldersConfig = foldersConfig;
        _transactionParser = transactionParser;

        if (string.IsNullOrWhiteSpace(_foldersConfig.Value.Output))
        {
            throw new ArgumentException("Output folder is not configured", nameof(foldersConfig));
        }
    }

    public Task StartAsync(CancellationToken ct)
    {
        _timer = new Timer(_ => OnTimerFired(ct),
            null, MilliSecondsUntilNextSave(), Timeout.Infinite);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_timer != null)
        {
            await _timer.DisposeAsync();
        }

        _timer = null;
    }

    // 'internal virtual' for mocking
    // ReSharper disable once MemberCanBeProtected.Global
    internal virtual int MilliSecondsUntilNextSave()
    {
        // return DateTime.Today.AddSeconds(3).Millisecond; // for testing
        return (int) (DateTime.Today.AddDays(1.0) - DateTime.Now).TotalMilliseconds + 1000;
    }

    // 'internal virtual' for mocking
    internal virtual DateOnly DateOfMetaLog()
    {
        return DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
    }

    private void OnTimerFired(CancellationToken ct)
    {
        Task.Run(async () =>
        {
            try
            {
                await SaveMetaLog(ct);
            }
            catch (Exception exception)
            {
                _logger.LogError("Failed to save meta.log", exception);
            }
            finally
            {
                _timer?.Change(MilliSecondsUntilNextSave(), Timeout.Infinite);
            }
        }, ct);
    }

    private async Task SaveMetaLog(CancellationToken ct)
    {
        var metaLog = _transactionParser.ResetMetaLog();
        var fileDir = Path.Combine(_foldersConfig.Value.Output, $"{DateOfMetaLog():MM-dd-yyyy}");
        var filePath = Path.Combine(fileDir, "meta.log");

        var contents = @$"parsed_files: {metaLog.ParsedFilesCount}
parsed_lines: {metaLog.ValidLinesCount}
found_errors: {metaLog.InvalidLinesCount}
invalid_files: [{string.Join(", ", metaLog.InvalidFileNames)}]
";

        _logger.LogInformation("{Yesterday's meta.log:\n}", contents);

        Directory.CreateDirectory(fileDir);
        await File.WriteAllTextAsync(filePath, contents, cancellationToken: ct);
    }
}
