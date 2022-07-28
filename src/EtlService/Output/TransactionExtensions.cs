namespace EtlService.Output;

public static class TransactionExtensions
{
    public static IEnumerable<OutputRoot> ToOutputObject(this IEnumerable<Transaction> transactions)
    {
        var outputsByCity = new Dictionary<string, OutputRoot>();

        foreach (var transaction in transactions)
        {
            ToOutputObjectInternal(transaction, outputsByCity);
        }

        return outputsByCity.Values;
    }

    private static void ToOutputObjectInternal(
        this Transaction transaction,
        IDictionary<string, OutputRoot> outputsByCity)
    {
        var payer = new OutputPayer(
            transaction.FirstName + " " + transaction.LastName,
            transaction.Payment,
            transaction.Date.ToString("yyyy-dd-MM"),
            transaction.AccountNumber
        );

        if (!outputsByCity.TryGetValue(transaction.City, out var output))
        {
            output = new OutputRoot(transaction.City, new Dictionary<string, OutputService>());
            outputsByCity.Add(transaction.City, output);
        }

        if (!output.ServicesByName.TryGetValue(transaction.City, out var service))
        {
            service = new OutputService(transaction.Service, new List<OutputPayer> {payer});
            output.ServicesByName.Add(transaction.Service, service);
            return;
        }

        service.Payers.Add(payer);
    }
}
