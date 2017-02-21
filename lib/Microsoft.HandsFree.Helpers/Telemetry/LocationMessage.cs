using System;
using System.Linq;
using NGeoNames;
using NGeoNames.Entities;

namespace Microsoft.HandsFree.Helpers.Telemetry
{
    public class LocationMessage : TelemetryMessage<InventoryMessages>
    {
        public LocationMessage(string tableName) : base(tableName) { }

        public string HostName;
        public string UsernameSecure;
        public string LocationSecure { get; private set; }

        private static readonly ReverseGeoCode<ExtendedGeoName> Rgc =
            new ReverseGeoCode<ExtendedGeoName>(GeoFileReader.ReadExtendedGeoNames(@"cities1000.txt"));

        public Tuple<double, double> Location
        {
            // Convert Location from Lat/Long into City, State for better human readibility as well as better privacy protection
            set
            {
                var point = Rgc.CreateFromLatLong(value.Item1, value.Item2);
                var geoName = Rgc.NearestNeighbourSearch(point, 1).First();
                LocationSecure = geoName.NameASCII + ", " + geoName.Admincodes[0]; // Admincodes[0] seems to be State
            }
        }
    }
}
