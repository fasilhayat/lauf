namespace MyCompany.Yggdrasil.HealthCheck;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

/// <summary>
/// Provides methods for writing health check responses in JSON format for use in health check UIs.
/// </summary>
public static class HealthWriter
{
    /// <summary>
    /// Specifies the default content type for JSON data.
    /// </summary>
    private const string DefaultContentType = "application/json";
    private static readonly byte[] EmptyResponse = "{}"u8.ToArray();
    private static readonly Lazy<JsonSerializerOptions> Options = new(CreateJsonOptions);
    private static List<string>? _assemblies;

    /// <summary>
    /// Writes a health check UI response to the HTTP response stream in JSON format.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <param name="report">The health report to serialize, or null to write an empty response.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public static async Task WriteHealthUiResponse(HttpContext httpContext, HealthReport? report)
    {
        httpContext.Response.ContentType = DefaultContentType;

        if (report == null)
        {
            await httpContext.Response.WriteAsync(Encoding.UTF8.GetString(EmptyResponse));
            return;
        }

        var response = GenerateReport(report);
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, response, Options.Value);
    }

    /// <summary>
    /// Serializes a health report to a JSON stream suitable for UI responses.
    /// </summary>
    /// <param name="report">The health report to serialize, or null to return an empty response.</param>
    /// <returns>A stream containing the serialized health report in JSON format.</returns>
    public static Stream WriteHealthUiResponse(HealthReport? report)
    {
        if (report == null) return new MemoryStream(EmptyResponse);

        var memoryStream = new MemoryStream();
        JsonSerializer.Serialize(memoryStream, GenerateReport(report), Options.Value);
        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <summary>
    /// Creates an object containing health report details, including status, duration, assembly information, and report
    /// entries.
    /// </summary>
    /// <param name="report">The health report to generate the summary from.</param>
    /// <returns>An object with health report status, total duration, assembly details, and entry information.</returns>
    private static object GenerateReport(HealthReport report)
    {
        var entries = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description,
            duration = entry.Value.Duration.ToString("c", CultureInfo.InvariantCulture), // use "c" format
            tags = entry.Value.Tags,
            data = entry.Value.Data
        }).ToList();

        var assemblyInfo = Assembly.GetEntryAssembly()?.FullName ?? "NA";

        if (_assemblies == null)
        {
            _assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName != null)
                .Select(a => a.FullName!)
                .ToList();
        }

        var reportInfo = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString("c", CultureInfo.InvariantCulture), // use "c" format
            assembly = assemblyInfo,
            assemblies = _assemblies,
            entries
        };
        return reportInfo;
    }

    /// <summary>
    /// Configures and returns a JsonSerializerOptions instance with camel case property naming, trailing comma support,
    /// null value ignoring, and custom converters.
    /// </summary>
    /// <returns>A configured JsonSerializerOptions instance.</returns>
    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new TimeSpanConverter());

        return options;
    }
}