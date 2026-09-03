using Autodesk.Revit.DB;

namespace CoordinatesAudit.Models
{
    public sealed class CoordinatePointData
    {
        public CoordinatePointData(string name, string eastWest, string northSouth, string elevation, string internalPosition, bool pinned, XYZ internalPositionRaw)
        {
            Name = name;
            EastWest = eastWest;
            NorthSouth = northSouth;
            Elevation = elevation;
            InternalPosition = internalPosition;
            Pinned = pinned;
            InternalPositionRaw = internalPositionRaw;
        }

        public string Name { get; }
        public string EastWest { get; }
        public string NorthSouth { get; }
        public string Elevation { get; }
        public string InternalPosition { get; }
        public bool Pinned { get; }
        public XYZ InternalPositionRaw { get; }
    }
}
