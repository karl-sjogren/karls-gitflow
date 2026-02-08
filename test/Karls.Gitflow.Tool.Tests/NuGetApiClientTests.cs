using System.Net;
using Karls.Gitflow.Tool.Infrastructure;

namespace Karls.Gitflow.Tool.Tests;

public class NuGetApiClientTests : IDisposable {
    private NuGetApiClient? _sut;

    public void Dispose() {
        _sut?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetLatestVersionAsync_WhenValidResponse_ReturnsLatestVersionAsync() {
        // Arrange
        var mockHttpHandler = new MockHttpMessageHandler();
        mockHttpHandler.SetResponse(HttpStatusCode.OK, """
        {
            "versions": ["0.0.1", "0.0.2", "0.0.3", "0.0.7", "0.0.5"]
        }
        """);

        var httpClient = new HttpClient(mockHttpHandler);
        _sut = new NuGetApiClient(httpClient);

        // Act
        var result = await _sut.GetLatestVersionAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(new Version("0.0.7"));
    }

    [Fact]
    public async Task GetLatestVersionAsync_FiltersOutPrereleaseVersionsAsync() {
        // Arrange
        var mockHttpHandler = new MockHttpMessageHandler();
        mockHttpHandler.SetResponse(HttpStatusCode.OK, """
        {
            "versions": ["0.0.1", "0.0.2-beta", "0.0.3", "0.0.4-alpha.1"]
        }
        """);

        var httpClient = new HttpClient(mockHttpHandler);
        _sut = new NuGetApiClient(httpClient);

        // Act
        var result = await _sut.GetLatestVersionAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(new Version("0.0.3"));
    }

    [Fact]
    public async Task GetLatestVersionAsync_WhenHttpRequestFails_ReturnsNullAsync() {
        // Arrange
        var mockHttpHandler = new MockHttpMessageHandler();
        mockHttpHandler.SetResponse(HttpStatusCode.InternalServerError, "");

        var httpClient = new HttpClient(mockHttpHandler);
        _sut = new NuGetApiClient(httpClient);

        // Act
        var result = await _sut.GetLatestVersionAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    private class MockHttpMessageHandler : HttpMessageHandler {
        private HttpStatusCode _statusCode;
        private string _content = string.Empty;

        public void SetResponse(HttpStatusCode statusCode, string content) {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }
}
