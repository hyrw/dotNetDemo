using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace WpfViewModelLocator.LogConfig;

public class ClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue)) return;

        string sourceContext = sourceContextValue.ToString();
        ReadOnlySpan<char> sourceContextSpan = sourceContext.AsSpan().Trim('"');

        int lastDotIndex = sourceContextSpan.LastIndexOf('.');
        ReadOnlySpan<char> classNameSpan = lastDotIndex switch
        {
            -1 => sourceContextSpan,
            _ => sourceContextSpan[(lastDotIndex + 1)..],
        };

        string className = classNameSpan.ToString();
        var classNameProperty = propertyFactory.CreateProperty("ClassName", className);
        logEvent.AddPropertyIfAbsent(classNameProperty);
    }
}

public static class LoggerEnrichmentConfigurationExtensions
{
    public static LoggerConfiguration WithClassName(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<ClassNameEnricher>();
    }
}

