using System;
using UniSharperEditor.Data.Metadata;

namespace MetadataDatabaseFileGeneratorTest;

public class ConsoleLogger : ILogger
{
    public void Log(object message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void LogWarning(object message)
    {
        Console.WriteLine($"[WARN] {message}");
    }

    public void LogError(object message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public void LogException(Exception exception)
    {
        Console.WriteLine($"[EXCEPTION] {exception.Message}");
    }
}