namespace VRSoundboard;

internal sealed class DeviceNameCache
{
    private readonly object _gate = new();
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly long _lifetimeMilliseconds;

    public DeviceNameCache(TimeSpan lifetime)
    {
        if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lifetime));
        _lifetimeMilliseconds = Math.Max(1, (long)lifetime.TotalMilliseconds);
    }

    public string? Get(string? endpointId, long nowMilliseconds, Func<string?> resolve)
    {
        ArgumentNullException.ThrowIfNull(resolve);
        var key = endpointId ?? string.Empty;
        lock (_gate)
        {
            if (_entries.TryGetValue(key, out var entry) && nowMilliseconds < entry.ExpiresAt)
                return entry.Name;

            var name = resolve();
            _entries[key] = new Entry(name, nowMilliseconds + _lifetimeMilliseconds);
            return name;
        }
    }

    public void Invalidate(string? endpointId)
    {
        lock (_gate) _entries.Remove(endpointId ?? string.Empty);
    }

    public void Clear()
    {
        lock (_gate) _entries.Clear();
    }

    private sealed record Entry(string? Name, long ExpiresAt);
}
