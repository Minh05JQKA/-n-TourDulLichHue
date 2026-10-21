namespace HueTour.UseCases.Common;

/// <summary>
/// Calculates great-circle distances between geographic coordinates using the Haversine formula.
/// </summary>
public static class GeoDistanceCalculator
{
    /// <summary>
    /// Returns the distance in kilometers between two latitude/longitude coordinates.
    /// </summary>
    public static double CalculateKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusKm = 6371.0088;
        var latitudeDelta = DegreesToRadians(lat2 - lat1);
        var longitudeDelta = DegreesToRadians(lng2 - lng1);
        var latitude1Radians = DegreesToRadians(lat1);
        var latitude2Radians = DegreesToRadians(lat2);
        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2)
            + Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians)
            * Math.Pow(Math.Sin(longitudeDelta / 2), 2);
        var centralAngle = 2 * Math.Asin(Math.Min(1, Math.Sqrt(haversine)));
        return earthRadiusKm * centralAngle;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180);
}
