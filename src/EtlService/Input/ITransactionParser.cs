namespace EtlService.Input;

public interface ITransactionParser
{
    Task<IReadOnlyList<Transaction>> ParseFileAsync(string path);

    MetaLog ResetMetaLog();
}
