using System.Diagnostics;
using System.Text;

namespace StoreSyncBack.Middleware;

public class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int MaxBodyLogChars = 8_000;

    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "Set-Cookie"
    };

    private static readonly HashSet<string> LoggableContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "application/x-www-form-urlencoded",
        "text/plain",
        "text/html",
        "text/xml",
        "application/xml"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestBody = await ReadBodyAsync(context.Request.Body, context.Request.ContentType);
        context.Request.Body.Position = 0;

        var originalResponseBody = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        var sw = Stopwatch.StartNew();
        Exception? thrownException = null;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            thrownException = ex;
            throw;
        }
        finally
        {
            sw.Stop();

            responseBuffer.Seek(0, SeekOrigin.Begin);
            var responseBody = await ReadBodyAsync(responseBuffer, context.Response.ContentType);

            responseBuffer.Seek(0, SeekOrigin.Begin);
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            LogEntry(context, requestBody, responseBody, sw.ElapsedMilliseconds, thrownException);
        }
    }

    private void LogEntry(
        HttpContext ctx,
        string requestBody,
        string responseBody,
        long elapsedMs,
        Exception? exception)
    {
        var req = ctx.Request;
        var statusCode = exception is not null ? 500 : ctx.Response.StatusCode;

        var logLevel = statusCode >= 500 ? LogLevel.Error
                     : statusCode >= 400 ? LogLevel.Warning
                     : LogLevel.Information;

        var reqHeaders = FormatHeaders(req.Headers);
        var respHeaders = FormatHeaders(ctx.Response.Headers);
        var reqBodyDisplay = string.IsNullOrWhiteSpace(requestBody) ? "(vazio)" : requestBody;
        var respBodyDisplay = string.IsNullOrWhiteSpace(responseBody) ? "(vazio)" : responseBody;

        if (exception is not null)
        {
            logger.Log(logLevel,
                exception,
                "HTTP {Method} {Path}{Query} => {StatusCode} ({ElapsedMs}ms)\n" +
                "┌─ Request Headers\n{ReqHeaders}\n" +
                "├─ Request Body\n{ReqBody}\n" +
                "└─ Exception: {ExMessage}",
                req.Method, req.Path, req.QueryString,
                statusCode, elapsedMs,
                reqHeaders, reqBodyDisplay,
                exception.Message);
            return;
        }

        logger.Log(logLevel,
            "HTTP {Method} {Path}{Query} => {StatusCode} ({ElapsedMs}ms)\n" +
            "┌─ Request Headers\n{ReqHeaders}\n" +
            "├─ Request Body\n{ReqBody}\n" +
            "├─ Response Headers\n{RespHeaders}\n" +
            "└─ Response Body\n{RespBody}",
            req.Method, req.Path, req.QueryString,
            statusCode, elapsedMs,
            reqHeaders, reqBodyDisplay,
            respHeaders, respBodyDisplay);
    }

    private static async Task<string> ReadBodyAsync(Stream stream, string? contentType)
    {
        if (stream.Length == 0)
            return string.Empty;

        if (!IsLoggableContentType(contentType))
            return $"(binário — {contentType ?? "desconhecido"})";

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        if (body.Length > MaxBodyLogChars)
            return body[..MaxBodyLogChars] + $"\n... (truncado — {body.Length} chars no total)";

        return body;
    }

    private static bool IsLoggableContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return true;

        return LoggableContentTypes.Any(t => contentType.StartsWith(t, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatHeaders(IHeaderDictionary headers)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in headers)
        {
            var display = SensitiveHeaders.Contains(key) ? "[REDACTED]" : value.ToString();
            sb.Append("  ").Append(key).Append(": ").AppendLine(display);
        }
        return sb.Length > 0 ? sb.ToString().TrimEnd() : "  (nenhum)";
    }
}
