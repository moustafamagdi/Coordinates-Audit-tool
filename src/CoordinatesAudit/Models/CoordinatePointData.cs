namespace CoordinatesAudit.Models
{
    public sealed class CoordinatePointData
    {
        public CoordinatePointData(string name, string eastWest, string northSouth, string elevation, string internalPosition, bool pinned)
        {
            Name = name;
            EastWest = eastWest;
            NorthSouth = northSouth;
            Elevation = elevation;
            InternalPosition = internalPosition;
            Pinned = pinned;
        }

        public string Name { get; }
        public string EastWest { get; }
        public string NorthSouth { get; }
        public string Elevation { get; }
        public string InternalPosition { get; }
        public bool Pinned { get; }
    }
}
