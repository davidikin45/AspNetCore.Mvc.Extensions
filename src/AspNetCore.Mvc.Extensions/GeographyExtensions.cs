using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions
{
    public static class GeographyExtensions
    {
        public static Point GetCentralGeoCoordinate(this IList<Point> geoCoordinates)
        {
            if (geoCoordinates.Count == 1)
            {
                return geoCoordinates.Single();
            }

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Y * Math.PI / 180;
                var longitude = geoCoordinate.X * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = geoCoordinates.Count;

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return CreatePoint(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }

        public static Point CreatePoint(double lat, double lon)
        {
            return CreatePoint(lat, lon, 4326);
        }

        public static Point CreatePoint(double lat, double lon, int srid = 4326)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: srid);

            var point = geometryFactory.CreatePoint(new Coordinate(lon, lat));

            return (Point)point;
        }
    }
}
