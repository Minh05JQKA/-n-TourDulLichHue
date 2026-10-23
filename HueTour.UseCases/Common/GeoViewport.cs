namespace HueTour.UseCases.Common;

/// <summary>Geographic bounds for map and nearby-place searches.</summary>
public sealed record GeoViewport(double West, double South, double East, double North);
