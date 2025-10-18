using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DesktopNotifier.Models;

namespace DesktopNotifier.Services;

public sealed class NotificationClient : IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public NotificationClient(string baseUrl, string clientId)
    {
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        {
            baseUrl += "/";
        }

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute)
        };

        _httpClient.DefaultRequestHeaders.Add("X-Client-Id", clientId);
    }

    public async Task<IReadOnlyList<NotificationMessage>> FetchNotificationsAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync("notifications", cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(SerializerOptions, cancellationToken)
                      ?? new List<NotificationDto>();

        List<NotificationMessage> notifications = new(payload.Count);
        foreach (NotificationDto dto in payload)
        {
            var createdAt = dto.CreatedAt ?? DateTimeOffset.UtcNow;
            notifications.Add(new NotificationMessage(
                dto.Id ?? Guid.NewGuid().ToString(),
                dto.Application ?? "Unbekannte Anwendung",
                dto.Topic ?? "Benachrichtigung",
                dto.Text ?? string.Empty,
                createdAt.ToLocalTime()));
        }

        return notifications;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private sealed record NotificationDto
    {
        public string? Id { get; init; }
        public string? Application { get; init; }
        public string? Topic { get; init; }
        public string? Text { get; init; }
        public DateTimeOffset? CreatedAt { get; init; }
    }
}
