using System.Text.Json;

namespace EtlService;

public class EtlService : IHostedService, IDisposable
{
    private readonly ILogger<EtlService> _logger;
    private readonly ITransactionParser _transactionParser;
    private readonly IOptions<FoldersConfiguration> _foldersConfig;
    private readonly FileSystemWatcher _inputFolderWatcher;

    public EtlService(
        ILogger<EtlService> logger,
        ITransactionParser transactionParser,
        IOptions<FoldersConfiguration> foldersConfig)
    {
        _logger = logger;
        _transactionParser = transactionParser;
        _foldersConfig = foldersConfig;

        Directory.CreateDirectory(foldersConfig.Value.Input);

        _inputFolderWatcher = new FileSystemWatcher(foldersConfig.Value.Input)
        {
            EnableRaisingEvents = false, //
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName,
        };

        _inputFolderWatcher.Changed += HandleFileCreated;
        _inputFolderWatcher.Created += HandleFileCreated;
        _inputFolderWatcher.Renamed += HandleFileCreated;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _inputFolderWatcher.EnableRaisingEvents = true;
        _logger.LogInformation("Watching for new input files");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping watching for new input files");
        _inputFolderWatcher.EnableRaisingEvents = false;

        return Task.CompletedTask;
    }

    private void HandleFileCreated(object sender, FileSystemEventArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                await HandleFileCreatedAsync(e.FullPath);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occured while processing a file: {}", e.FullPath);
            }
        });
    }

    private async Task HandleFileCreatedAsync(string path)
    {
        var transactions = await _transactionParser.ParseFileAsync(path);
        var output = transactions.ToOutputObject();

        var outputDir = Path.Combine(_foldersConfig.Value.Output, $"{DateTime.Today:MM-dd-yyyy}");
        var outputFile = Path.Combine(outputDir, $"output {Guid.NewGuid()}.json");

        Directory.CreateDirectory(outputDir);
        await using var fileStream = File.Open(outputFile, FileMode.CreateNew, FileAccess.Write);

        await JsonSerializer.SerializeAsync(fileStream, output);
    }

    public void Dispose()
    {
        _inputFolderWatcher.Dispose();
    }
}
