using Microsoft.AspNetCore.Mvc.Testing;

namespace EduOnline.IntegrationTest;

public abstract class WebApiIntegrationTestFixture<TStartup> : IDisposable
where TStartup : class
{
    private readonly WebApiTestFactory<TStartup> _factory;
    private bool _disposedValue;

    public HttpClient Client { get; init; }
    public Guid Id { get; set; }

    protected WebApiIntegrationTestFixture()
    {
        var options = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            BaseAddress = new Uri("http://localhost"),
            HandleCookies = true,
            MaxAutomaticRedirections = 7
        };

        _factory = new WebApiTestFactory<TStartup>();
        Client = _factory.CreateClient(options);
    }

    public bool CapturarGuidInserido(HttpResponseMessage response)
    {
        var recursoFoiInserido = Guid.TryParse(response.Headers.Location?.Segments.LastOrDefault(), out var newId);
        Id = newId;

        return recursoFoiInserido;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                Client.Dispose();
                _factory.Dispose();
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // and set large fields to null here
            _disposedValue = true;
        }
    }
}
