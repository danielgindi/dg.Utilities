using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities
{
    public static class Location
    {
        [Obsolete]
        public static double AVERAGE_KM_PER_LATITUDE_DEGREE { get { return LocationHelper.AVERAGE_KM_PER_LATITUDE_DEGREE; } }

        [Obsolete]
        public static double AVERAGE_KM_PER_LONGITUDE_DEGREE_AT_40_DEGREES { get { return LocationHelper.AVERAGE_KM_PER_LONGITUDE_DEGREE_AT_40_DEGREES; } }

        [Obsolete]
        public static double AVERAGE_KM_EARTH_RADIUS { get { return LocationHelper.AVERAGE_KM_EARTH_RADIUS; } }

        [Obsolete]
        public static double DistanceBetweenTwoCoordinatesInKilometers(double lat1, double lon1, double lat2, double lon2)
        {
            return LocationHelper.DistanceBetweenTwoCoordinatesInKilometers(lat1, lon1, lat2, lon2);
        }
    }
}
