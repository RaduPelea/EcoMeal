using System.Text.Json;

namespace Ecomeal.client.Services;

public class DistanceService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(4) };

    public async Task<(double Km, int? Minutes)?[]> GetDrivingAsync(
        double userLat, double userLon, IReadOnlyList<(double Lat, double Lon)?> destinations)
    {
        var result = new (double Km, int? Minutes)?[destinations.Count];
        var known = new List<int>();
        for (var i = 0; i < destinations.Count; i++)
            if (destinations[i] is not null)
                known.Add(i);

        if (known.Count == 0)
            return result;

        try
        {
            var coords = string.Join(';', new[] { Fmt(userLon, userLat) }
                .Concat(known.Select(i => Fmt(destinations[i]!.Value.Lon, destinations[i]!.Value.Lat))));
            var url = $"https://router.project-osrm.org/table/v1/driving/{coords}?sources=0&annotations=distance,duration";

            using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));
            if (doc.RootElement.GetProperty("code").GetString() != "Ok")
                throw new InvalidOperationException("OSRM error");

            var distances = doc.RootElement.GetProperty("distances")[0];
            var durations = doc.RootElement.GetProperty("durations")[0];

            for (var k = 0; k < known.Count; k++)
            {
                var dist = distances[k + 1];
                var dur = durations[k + 1];
                if (dist.ValueKind == JsonValueKind.Null)
                    continue;

                var minutes = dur.ValueKind == JsonValueKind.Null
                    ? (int?)null
                    : (int)Math.Round(dur.GetDouble() / 60);
                result[known[k]] = (Math.Round(dist.GetDouble() / 1000, 1), minutes);
            }
        }
        catch
        {
            foreach (var i in known)
            {
                var d = destinations[i]!.Value;
                result[i] = (Math.Round(HaversineKm(userLat, userLon, d.Lat, d.Lon), 1), null);
            }
        }

        return result;
    }

    private static string Fmt(double lon, double lat) =>
        $"{lon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * r * Math.Asin(Math.Sqrt(a));
    }
}
