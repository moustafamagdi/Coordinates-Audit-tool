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
                        HasInstance = true
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
                HasInstance = false
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
