using System.Collections.Immutable;

namespace EtlService.Input;

public class MetaLog
{
    private readonly HashSet<string> _invalidFileNames = new();
    private uint _parsedFilesCount;
    private uint _validLinesCount;
    private uint _invalidLinesCount;

    public uint ParsedFilesCount => _parsedFilesCount;

    public uint ValidLinesCount => _validLinesCount;

    public uint InvalidLinesCount => _invalidLinesCount;

    public IReadOnlySet<string> InvalidFileNames
    {
        get
        {
            lock (_invalidFileNames)
            {
                return _invalidFileNames.ToImmutableHashSet();
            }
        }
    }

    public void AddInvalidFileName(string fileName)
    {
        lock (_invalidFileNames)
        {
            _invalidFileNames.Add(fileName);
        }
    }

    public void IncrementParsedFilesCount() => Interlocked.Increment(ref _parsedFilesCount);

    public void IncrementValidLinesCount() => Interlocked.Increment(ref _validLinesCount);

    public void IncrementInvalidLinesCount() => Interlocked.Increment(ref _invalidLinesCount);
}
