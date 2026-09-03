using System;
using System.Globalization;
using Autodesk.Revit.DB;
using CoordinatesAudit.Models;

namespace CoordinatesAudit.Services
{
    public sealed class LinkTransformReader
    {
        public LinkTransformData Read(Document hostDocument, RevitLinkInstance instance, Document linkedDocument)
        {
            if (hostDocument == null) throw new ArgumentNullException(nameof(hostDocument));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (linkedDocument == null) throw new InvalidOperationException("The linked document is not loaded.");

            Transform instanceTransform = instance.GetTransform();
            Transform totalTransform = instance.GetTotalTransform();
            BasePoint projectBasePoint = BasePoint.GetProjectBasePoint(linkedDocument);
            BasePoint surveyPoint = BasePoint.GetSurveyPoint(linkedDocument);
            InternalOrigin internalOrigin = InternalOrigin.Get(linkedDocument);
            XYZ internalOriginInHost = totalTransform.OfPoint(internalOrigin.Position);
            XYZ projectBasePointInHost = totalTransform.OfPoint(projectBasePoint.Position);
            XYZ surveyPointInHost = totalTransform.OfPoint(surveyPoint.Position);
            double totalRotation = GetZRotation(totalTransform);

            return new LinkTransformData
            {
                InstanceTranslation = FormatPoint(hostDocument, instanceTransform.Origin),
                InstanceRotation = FormatAngle(hostDocument, GetZRotation(instanceTransform)),
                TotalTranslation = FormatPoint(hostDocument, totalTransform.Origin),
                TotalRotation = FormatAngle(hostDocument, totalRotation),
                Scale = GetScaleDescription(totalTransform),
                Mirrored = totalTransform.Determinant < 0.0 ? "Yes" : "No",
                LinkedInternalOriginInHost = FormatPoint(hostDocument, internalOriginInHost),
                LinkedProjectBasePointInHost = FormatPoint(hostDocument, projectBasePointInHost),
                LinkedSurveyPointInHost = FormatPoint(hostDocument, surveyPointInHost),
                LinkedInternalOriginInHostRaw = internalOriginInHost,
                LinkedProjectBasePointInHostRaw = projectBasePointInHost,
                LinkedSurveyPointInHostRaw = surveyPointInHost,
                TotalRotationRadians = totalRotation
            };
        }

        private static double GetZRotation(Transform transform)
        {
            XYZ xAxis = transform.BasisX.Normalize();
            return Math.Atan2(xAxis.Y, xAxis.X);
        }

        private static string GetScaleDescription(Transform transform)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "X: {0:0.######} | Y: {1:0.######} | Z: {2:0.######}",
                transform.BasisX.GetLength(),
                transform.BasisY.GetLength(),
                transform.BasisZ.GetLength());
        }

        private static string FormatPoint(Document document, XYZ point)
        {
            return $"X: {FormatLength(document, point.X)} | Y: {FormatLength(document, point.Y)} | Z: {FormatLength(document, point.Z)}";
        }

        private static string FormatLength(Document document, double value)
        {
            return UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, value, false);
        }

        private static string FormatAngle(Document document, double value)
        {
            return UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Angle, value, false);
        }
    }
}
