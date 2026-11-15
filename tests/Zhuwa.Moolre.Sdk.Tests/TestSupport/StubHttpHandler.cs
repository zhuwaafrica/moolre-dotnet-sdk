using System.Net;
using System.Text;

namespace Zhuwa.Moolre;

/// <summary>Answers every request with a fixed response (or a failure) and remembers what was sent.</summary>
internal sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;

    private StubHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) => _respond = respond;

    public HttpRequestMessage? Request { get; private set; }

    public string? RequestBody { get; private set; }

    public int CallCount { get; private set; }

    public static StubHttpHandler Returning(string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        }));

    public static StubHttpHandler ReturningFixture(string fixture, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        Returning(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture)), statusCode);

    /// <summary>Answers each request with the next body in order (the last one repeats), and remembers every request URL.</summary>
    public static StubHttpHandler ReturningInOrder(params string[] bodies)
    {
        var next = 0;
        return new((_, _) =>
        {
            var body = bodies[Math.Min(next, bodies.Length - 1)];
            next++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        });
    }

    public List<Uri> RequestUris { get; } = [];

    public static StubHttpHandler Throwing(Exception exception) => new((_, _) => Task.FromException<HttpResponseMessage>(exception));

    public static StubHttpHandler NeverAnswering() =>
        new(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            return new HttpResponseMessage();
        });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        Request = request;
        RequestUris.Add(request.RequestUri!);
        RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return await _respond(request, cancellationToken);
    }
}
