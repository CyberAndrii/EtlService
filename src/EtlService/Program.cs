using Microsoft.Extensions.DependencyInjection;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<EtlService.EtlService>();
        services.AddHostedService<MetaLogSaverService>();
        services.Configure<FoldersConfiguration>(context.Configuration.GetSection("Folders"));
        services.AddSingleton<ITransactionParser, TransactionParser>();

#if AUTOCREATE_TEST_TRANSACTION_ON_STARTUP
        services.AddHostedService<TestTransactionCreatorService>();
#endif
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();
