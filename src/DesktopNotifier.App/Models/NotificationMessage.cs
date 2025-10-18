using System;

namespace DesktopNotifier.Models;

public sealed record NotificationMessage(
    string Id,
    string Application,
    string Topic,
    string Text,
    DateTimeOffset CreatedAt
);
