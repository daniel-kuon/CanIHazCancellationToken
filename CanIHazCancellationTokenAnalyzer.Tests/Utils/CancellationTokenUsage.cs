namespace CanIHazCancellationTokenAnalyzer.Tests.Utils;

public enum CancellationTokenUsage
{
    None,
    PassesCancellationToken,
    UsesNoneCancellationToken,
    UsesDefaultCancellationToken,
}
