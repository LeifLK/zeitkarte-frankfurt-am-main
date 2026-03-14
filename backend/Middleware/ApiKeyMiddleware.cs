public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var incomingKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API key missing");
            return;
        }

        var validKey = config["ApiKey"];
        if (!string.Equals(incomingKey, validKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        await _next(context);
    }
}