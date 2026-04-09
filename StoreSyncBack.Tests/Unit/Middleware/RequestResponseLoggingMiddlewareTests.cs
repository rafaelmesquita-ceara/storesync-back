using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StoreSyncBack.Middleware;

namespace StoreSyncBack.Tests.Unit.Middleware
{
    public class RequestResponseLoggingMiddlewareTests
    {
        // ── Helper: logger que captura mensagens reais ────────────────────────
        private sealed class TestLogger : ILogger<RequestResponseLoggingMiddleware>
        {
            public List<(LogLevel Level, string Message)> Entries { get; } = [];

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add((logLevel, formatter(state, exception)));
            }
        }

        private static RequestResponseLoggingMiddleware Build(
            RequestDelegate next, TestLogger logger)
            => new(next, logger);

        private static DefaultHttpContext MakeContext(
            string method = "GET",
            string path = "/api/test",
            string? requestBody = null,
            string requestContentType = "application/json")
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Method = method;
            ctx.Request.Path = path;
            ctx.Request.ContentType = requestContentType;

            var bodyBytes = requestBody is null
                ? Array.Empty<byte>()
                : Encoding.UTF8.GetBytes(requestBody);

            ctx.Request.Body = new MemoryStream(bodyBytes);
            ctx.Response.Body = new MemoryStream();
            return ctx;
        }

        // ── InvokeAsync: comportamento geral ─────────────────────────────────

        [Fact]
        public async Task InvokeAsync_CaminhoFeliz_ChamaProximoMiddleware()
        {
            var nextCalled = false;
            var logger = new TestLogger();
            var middleware = Build(_ => { nextCalled = true; return Task.CompletedTask; }, logger);
            var ctx = MakeContext();
            ctx.Response.StatusCode = 200;

            await middleware.InvokeAsync(ctx);

            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_RespostaDoNext_BodyRepassadoAoClienteCorretamente()
        {
            var expectedBody = "{\"id\":\"abc\"}";
            var logger = new TestLogger();
            var middleware = Build(ctx =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(expectedBody);
            }, logger);

            var ctx = MakeContext();
            var originalStream = new MemoryStream();
            ctx.Response.Body = originalStream;

            await middleware.InvokeAsync(ctx);

            originalStream.Seek(0, SeekOrigin.Begin);
            var actual = await new StreamReader(originalStream).ReadToEndAsync();
            actual.Should().Be(expectedBody);
        }

        // ── InvokeAsync: nível de log por status code ─────────────────────────

        [Fact]
        public async Task InvokeAsync_Status200_LogaEmInformation()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(MakeContext());

            logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Information);
        }

        [Fact]
        public async Task InvokeAsync_Status201_LogaEmInformation()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 201; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(MakeContext());

            logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Information);
        }

        [Fact]
        public async Task InvokeAsync_Status400_LogaEmWarning()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 400; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(MakeContext());

            logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning);
        }

        [Fact]
        public async Task InvokeAsync_Status404_LogaEmWarning()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 404; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(MakeContext());

            logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning);
        }

        [Fact]
        public async Task InvokeAsync_Status500_LogaEmError()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 500; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(MakeContext());

            logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Error);
        }

        // ── InvokeAsync: headers sensíveis ────────────────────────────────────

        [Fact]
        public async Task InvokeAsync_HeaderAuthorization_RedactedNaMensagemDeLog()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, logger);

            var ctx = MakeContext();
            ctx.Request.Headers["Authorization"] = "Bearer super-secret-token";

            await middleware.InvokeAsync(ctx);

            var msg = logger.Entries.Single().Message;
            msg.Should().Contain("[REDACTED]");
            msg.Should().NotContain("super-secret-token");
        }

        [Fact]
        public async Task InvokeAsync_HeaderCookie_RedactedNaMensagemDeLog()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, logger);

            var ctx = MakeContext();
            ctx.Request.Headers["Cookie"] = "session=abc123xyz";

            await middleware.InvokeAsync(ctx);

            var msg = logger.Entries.Single().Message;
            msg.Should().Contain("[REDACTED]");
            msg.Should().NotContain("abc123xyz");
        }

        // ── InvokeAsync: corpo da requisição ──────────────────────────────────

        [Fact]
        public async Task InvokeAsync_RequestComBody_BodyApareceNoLog()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, logger);

            var ctx = MakeContext(requestBody: "{\"name\":\"Produto A\"}");

            await middleware.InvokeAsync(ctx);

            logger.Entries.Single().Message.Should().Contain("Produto A");
        }

        [Fact]
        public async Task InvokeAsync_ContentTypeBinario_NaoLogaConteudo()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, logger);

            var ctx = MakeContext(requestBody: "bytes...", requestContentType: "image/png");

            await middleware.InvokeAsync(ctx);

            var msg = logger.Entries.Single().Message;
            msg.Should().Contain("binário");
            msg.Should().NotContain("bytes...");
        }

        // ── InvokeAsync: exceções ─────────────────────────────────────────────

        [Fact]
        public async Task InvokeAsync_ExcecaoNoNext_PropagaExcecao()
        {
            var logger = new TestLogger();
            var middleware = Build(_ => throw new InvalidOperationException("Erro simulado"), logger);

            var act = async () => await middleware.InvokeAsync(MakeContext());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Erro simulado");
        }

        [Fact]
        public async Task InvokeAsync_ExcecaoNoNext_LogaEmErrorComMensagemDaExcecao()
        {
            var logger = new TestLogger();
            var middleware = Build(_ => throw new InvalidOperationException("Erro simulado"), logger);

            try { await middleware.InvokeAsync(MakeContext()); } catch { /* esperado */ }

            logger.Entries.Should().ContainSingle(e =>
                e.Level == LogLevel.Error &&
                e.Message.Contains("Erro simulado"));
        }

        // ── InvokeAsync: informações de rota no log ───────────────────────────

        [Fact]
        public async Task InvokeAsync_CaminhoFeliz_LogContemMetodoECaminho()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 200; return Task.CompletedTask; }, logger);

            var ctx = MakeContext(method: "POST", path: "/api/products");

            await middleware.InvokeAsync(ctx);

            var msg = logger.Entries.Single().Message;
            msg.Should().Contain("POST");
            msg.Should().Contain("/api/products");
        }

        [Fact]
        public async Task InvokeAsync_CaminhoFeliz_LogContemStatusCodeEDuracao()
        {
            var logger = new TestLogger();
            var middleware = Build(ctx => { ctx.Response.StatusCode = 201; return Task.CompletedTask; }, logger);

            await middleware.InvokeAsync(MakeContext(method: "POST"));

            var msg = logger.Entries.Single().Message;
            msg.Should().Contain("201");
            // Duração em ms está sempre presente (número seguido de "ms")
            msg.Should().MatchRegex(@"\d+ms");
        }
    }
}
