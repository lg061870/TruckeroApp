﻿using System.Collections.Concurrent;

namespace TruckeroApp.ServiceClients.Mock;

public static class AuthMockStore
{
    public static readonly ConcurrentDictionary<Guid, AuthToken> Tokens = new();
    public static AuthToken? Latest { get; set; }
}
