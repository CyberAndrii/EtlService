using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace EtlService.Input;

public class TransactionParser : ITransactionParser
{
    private readonly ILogger<TransactionParser> _logger;
    private MetaLog _metaLog;

    public TransactionParser(ILogger<TransactionParser> logger)
    {
        _logger = logger;
        _metaLog = new MetaLog();
    }

    public async Task<IReadOnlyList<Transaction>> ParseFileAsync(string path)
    {
        if (!path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(path);
            _logger.LogWarning("Invalid file found: {}", name);
            _metaLog.AddInvalidFileName(name);

            return Array.Empty<Transaction>();
        }

        var transactions = await ProcessFileAsync(path);
        _metaLog.IncrementParsedFilesCount();

        return transactions;
    }

    public MetaLog ResetMetaLog()
    {
        var previousMetaLog = _metaLog;
        _metaLog = new MetaLog();
        return previousMetaLog;
    }

    private async Task<IReadOnlyList<Transaction>> ProcessFileAsync(string path)
    {
        var rows = await File.ReadAllLinesAsync(path);
        var hasHeader = path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

        if (rows.Length < (hasHeader ? 2 : 1))
        {
            return Array.Empty<Transaction>();
        }

        var transactions = new List<Transaction>();

        await Parallel.ForEachAsync(rows.Skip(hasHeader ? 1 : 0), (row, cts) =>
        {
            if (!TryParseRow(row, out var transaction))
            {
                return ValueTask.CompletedTask;
            }

            lock (transactions)
            {
                transactions.Add(transaction);
            }

            return ValueTask.CompletedTask;
        });

        return transactions;
    }

    private bool TryParseRow(string row, [NotNullWhen(true)] out Transaction? transaction)
    {
        if (!TryParseRowInternal(row, out transaction))
        {
            _logger.LogWarning("Invalid line found: {}", row);
            _metaLog.IncrementInvalidLinesCount();
            return false;
        }

        _metaLog.IncrementValidLinesCount();

        return true;
    }

    private bool TryParseRowInternal(string row, [NotNullWhen(true)] out Transaction? transaction)
    {
        transaction = null;
        var columns = Regex
            .Split(row, @",(?=(?:[^""]*""[^""]*"")*[^""]*$)", RegexOptions.Compiled, TimeSpan.FromSeconds(1))
            .Select(x => x.Trim())
            .ToArray();

        try
        {
            if (columns.Length != 7)
            {
                return false;
            }

            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];

                if (string.IsNullOrWhiteSpace(column))
                {
                    return false;
                }

                if (column.StartsWith('"') && column.EndsWith('"'))
                {
                    columns[i] = column.Substring(1, column.Length - 2);
                }
            }

            if (!decimal.TryParse(columns[3], out var payment) ||
                !DateOnly.TryParseExact(columns[4], "yyyy-dd-MM",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                !long.TryParse(columns[5], out var accountNumber))
            {
                return false;
            }

            transaction = new Transaction(
                FirstName: columns[0],
                LastName: columns[1],
                City: columns[2][..columns[2].IndexOf(',')],
                Payment: payment,
                Date: date,
                AccountNumber: accountNumber,
                Service: columns[6]);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
