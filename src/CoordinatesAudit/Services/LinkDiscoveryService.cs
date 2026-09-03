using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.Revit.DB;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.Services
{
    public sealed class LinkDiscoveryService
    {
        public IReadOnlyList<LinkInstanceData> Discover(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var types = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .OrderBy(type => type.Name)
                .ToList();

            var instancesByType = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .GroupBy(instance => instance.GetTypeId())
                .ToDictionary(group => group.Key, group => group.OrderBy(instance => instance.Name).ToList());

            var results = new List<LinkInstanceData>();
            var coordinateReportsByType = new Dictionary<ElementId, HostCoordinateReport>();
            var coordinateErrorsByType = new Dictionary<ElementId, string>();
            var coordinateReader = new HostCoordinateReader();
            var transformReader = new LinkTransformReader();
            foreach (RevitLinkType linkType in types)
            {
                string path = GetExternalPath(document, linkType.Id);
                string referenceType = GetReferenceType(document, linkType.Id);
                string status = linkType.GetLinkedFileStatus().ToString();
                bool isLoaded = linkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;

                if (!instancesByType.TryGetValue(linkType.Id, out List<RevitLinkInstance> instances) || instances.Count == 0)
                {
                    results.Add(CreateTypeOnlyRow(linkType, path, referenceType, status, isLoaded));
                    continue;
                }

                foreach (RevitLinkInstance instance in instances)
                {
                    Document linkedDocument = instance.GetLinkDocument();
                    HostCoordinateReport coordinateReport = null;
                    string coordinateReadStatus;
                    LinkTransformData transformData = null;
                    string transformReadStatus;

                    if (!isLoaded || linkedDocument == null)
                    {
                        coordinateReadStatus = "Unavailable: link document is not loaded";
                    }
                    else if (coordinateReportsByType.TryGetValue(linkType.Id, out coordinateReport))
                    {
                        coordinateReadStatus = "Available";
                    }
                    else if (coordinateErrorsByType.TryGetValue(linkType.Id, out string cachedError))
                    {
                        coordinateReadStatus = cachedError;
                    }
                    else
                    {
                        try
                        {
                            coordinateReport = coordinateReader.Read(linkedDocument, document.Application.VersionBuild);
                            coordinateReportsByType.Add(linkType.Id, coordinateReport);
                            coordinateReadStatus = "Available";
                        }
                        catch (Exception exception)
                        {
                            coordinateReadStatus = "Unavailable: " + exception.Message;
                            coordinateErrorsByType.Add(linkType.Id, coordinateReadStatus);
                        }
                    }

                    if (!isLoaded || linkedDocument == null)
                    {
                        transformReadStatus = "Unavailable: link document is not loaded";
                    }
                    else
                    {
                        try
                        {
                            transformData = transformReader.Read(document, instance, linkedDocument);
                            transformReadStatus = "Available";
                        }
                        catch (Exception exception)
                        {
                            transformReadStatus = "Unavailable: " + exception.Message;
                        }
                    }

                    results.Add(new LinkInstanceData
                    {
                        LinkTypeName = linkType.Name,
                        LinkTypeId = FormatElementId(linkType.Id),
                        InstanceName = instance.Name,
                        InstanceId = FormatElementId(instance.Id),
                        Status = status,
                        Path = path,
                        ReferenceType = referenceType,
                        AttachmentType = linkType.AttachmentType.ToString(),
                        Workset = GetWorksetName(document, instance.WorksetId),
                        Pinned = instance.Pinned ? "Yes" : "No",
                        IsLoaded = isLoaded && instance.GetLinkDocument() != null,
                        HasInstance = true,
                        CoordinateReadStatus = coordinateReadStatus,
                        CoordinateReport = coordinateReport,
                        TransformReadStatus = transformReadStatus,
                        TransformData = transformData
                    });
                }
            }

            return results;
        }

        private static LinkInstanceData CreateTypeOnlyRow(RevitLinkType linkType, string path, string referenceType, string status, bool isLoaded)
        {
            return new LinkInstanceData
            {
                LinkTypeName = linkType.Name,
                LinkTypeId = FormatElementId(linkType.Id),
                InstanceName = "No placed instance",
                InstanceId = "-",
                Status = status,
                Path = path,
                ReferenceType = referenceType,
                AttachmentType = linkType.AttachmentType.ToString(),
                Workset = "-",
                Pinned = "-",
                IsLoaded = isLoaded,
                HasInstance = false,
                CoordinateReadStatus = "Unavailable: no placed instance",
                CoordinateReport = null,
                TransformReadStatus = "Unavailable: no placed instance",
                TransformData = null
            };
        }

        private static string GetExternalPath(Document document, ElementId typeId)
        {
            try
            {
                ExternalFileReference reference = ExternalFileUtils.GetExternalFileReference(document, typeId);
                ModelPath modelPath = reference?.GetAbsolutePath();
                return modelPath == null ? "Unavailable" : ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }
            catch (Exception)
            {
                return "Unavailable";
            }
        }

        private static string GetReferenceType(Document document, ElementId typeId)
        {
            try
            {
                ExternalFileReference reference = ExternalFileUtils.GetExternalFileReference(document, typeId);
                return reference?.PathType.ToString() ?? "Unavailable";
            }
            catch (Exception)
            {
                return "Unavailable";
            }
        }

        private static string GetWorksetName(Document document, WorksetId worksetId)
        {
            try
            {
                Workset workset = document.GetWorksetTable().GetWorkset(worksetId);
                return workset?.Name ?? "Unavailable";
            }
            catch (Exception)
            {
                return "Unavailable";
            }
        }

        private static string FormatElementId(ElementId id)
        {
            return id.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
