namespace CoordinatesAudit.Models
{
    public sealed class LinkInstanceData
    {
        public string LinkTypeName { get; set; }
        public string LinkTypeId { get; set; }
        public string InstanceName { get; set; }
        public string InstanceId { get; set; }
        public string Status { get; set; }
        public string Path { get; set; }
        public string ReferenceType { get; set; }
        public string AttachmentType { get; set; }
        public string Workset { get; set; }
        public string Pinned { get; set; }
        public bool IsLoaded { get; set; }
        public bool HasInstance { get; set; }
        public string CoordinateReadStatus { get; set; }
        public HostCoordinateReport CoordinateReport { get; set; }
        public string TransformReadStatus { get; set; }
        public LinkTransformData TransformData { get; set; }
    }
}
