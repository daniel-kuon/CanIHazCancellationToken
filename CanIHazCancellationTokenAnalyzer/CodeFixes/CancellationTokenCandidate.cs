namespace CanIHazCancellationTokenAnalyzer.CodeFixes;

public class CancellationTokenCandidate(CancellationTokenCandidate.CandidateType type, string identifier)
{
    public CandidateType Type { get; } = type;

    public string Identifier { get; } = identifier;

    public string Key => $"{Type}_{Identifier}";

    public string Description => $"{Type.ToString().ToLower()} '{Identifier}'";

    public enum CandidateType
    {
        Parameter,
        Property,
        Field
    }
}
