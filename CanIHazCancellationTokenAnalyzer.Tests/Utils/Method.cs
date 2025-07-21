using System;

namespace CanIHazCancellationTokenAnalyzer.Tests.Utils;

[Flags]
public enum Method
{
    None,
    IsAsync,
}
