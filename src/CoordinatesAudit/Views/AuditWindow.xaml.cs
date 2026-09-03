using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using CoordinatesAudit.Models;
using CoordinatesAudit.Services;
using CoordinatesAudit.ViewModels;

namespace CoordinatesAudit.Views
{
    public partial class AuditWindow : Window
    {
        private readonly HostCoordinateReport _host;
        private readonly IReadOnlyList<LinkInstanceData> _links;
        private readonly ObservableCollection<AuditRowViewModel> _rows = new ObservableCollection<AuditRowViewModel>();
        private readonly CoordinateComparisonEngine _comparisonEngine = new CoordinateComparisonEngine();
        private readonly CsvAuditExporter _csvExporter = new CsvAuditExporter();
        private ReferenceModelOption _lastReference;
        private double _lastHorizontalTolerance;
        private double _lastVerticalTolerance;
        private double _lastAngularTolerance;

        public AuditWindow(HostCoordinateReport host, IReadOnlyList<LinkInstanceData> links)
        {
            InitializeComponent();
            _host = host;
            _links = links;
            ResultsGrid.ItemsSource = _rows;
            ProjectSummaryText.Text = $"{host.ModelTitle}  |  {host.ProjectLocationName}  |  {host.LengthUnit}";
            ReferenceComboBox.ItemsSource = BuildReferenceOptions();
            ReferenceComboBox.SelectedIndex = 0;
            RunComparison();
        }

        private IReadOnlyList<ReferenceModelOption> BuildReferenceOptions()
        {
            var options = new List<ReferenceModelOption>
            {
                new ReferenceModelOption
                {
                    Id = "HOST",
                    DisplayName = "Host - " + _host.ModelTitle,
                    InternalOrigin = _host.InternalOriginRaw,
                    ProjectBasePoint = _host.ProjectBasePoint.InternalPositionRaw,
                    SurveyPoint = _host.SurveyPoint.InternalPositionRaw,
                    RotationRadians = 0.0
                }
            };

            options.AddRange(_links.Where(link => link.TransformData != null).Select(link => new ReferenceModelOption
            {
                Id = link.InstanceId,
                DisplayName = "Link - " + link.LinkTypeName + " [" + link.InstanceId + "]",
                InternalOrigin = link.TransformData.LinkedInternalOriginInHostRaw,
                ProjectBasePoint = link.TransformData.LinkedProjectBasePointInHostRaw,
                SurveyPoint = link.TransformData.LinkedSurveyPointInHostRaw,
                RotationRadians = link.TransformData.TotalRotationRadians
            }));
            return options;
        }

        private void RunComparison_Click(object sender, RoutedEventArgs e) => RunComparison();

        private void RunComparison()
        {
            if (!(ReferenceComboBox.SelectedItem is ReferenceModelOption reference)) return;
            if (!TryReadTolerance(HorizontalToleranceTextBox.Text, "horizontal", out double horizontal) ||
                !TryReadTolerance(VerticalToleranceTextBox.Text, "vertical", out double vertical) ||
                !TryReadTolerance(AngularToleranceTextBox.Text, "rotation", out double angular)) return;

            _lastReference = reference;
            _lastHorizontalTolerance = horizontal;
            _lastVerticalTolerance = vertical;
            _lastAngularTolerance = angular;

            _rows.Clear();
            _rows.Add(AuditRowViewModel.CreateHost(_host, reference.Id == "HOST"));
            foreach (LinkInstanceData link in _links)
            {
                AuditComparisonResult result = _comparisonEngine.Compare(link, reference, horizontal, vertical, angular);
                _rows.Add(AuditRowViewModel.Create(link, result));
            }

            IReadOnlyList<AuditRowViewModel> linkRows = _rows.Where(row => row.RowType == "Link").ToList();
            int pass = linkRows.Count(row => row.Status == "PASS");
            int warning = linkRows.Count(row => row.Status == "WARNING");
            int fail = linkRows.Count(row => row.Status == "FAIL");
            int unavailable = linkRows.Count(row => row.Status == "UNAVAILABLE");
            StatusSummaryText.Text = $"Links: {linkRows.Count}   PASS: {pass}   WARNING: {warning}   FAIL: {fail}   UNAVAILABLE: {unavailable}";
            if (_rows.Count > 0) ResultsGrid.SelectedIndex = 0;
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_lastReference == null || _rows.Count == 0)
            {
                MessageBox.Show("Run the comparison before exporting.", "Coordinate Auditor", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Export Coordinate Audit Report",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                AddExtension = true,
                FileName = MakeSafeFileName(_host.ModelTitle) + "_Coordinate_Audit_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv"
            };

            if (dialog.ShowDialog(this) != true) return;

            try
            {
                _csvExporter.Export(dialog.FileName, _host, _lastReference,
                    _lastHorizontalTolerance, _lastVerticalTolerance, _lastAngularTolerance,
                    _rows.ToList());
                MessageBox.Show("The CSV report was exported successfully.", "Coordinate Auditor", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("The report could not be exported.\n\n" + exception.Message,
                    "Coordinate Auditor", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (char invalidCharacter in System.IO.Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidCharacter, '_');
            }
            return value;
        }

        private static bool TryReadTolerance(string text, string name, out double value)
        {
            bool valid = double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                         double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            if (!valid || value < 0.0)
            {
                MessageBox.Show($"Enter a non-negative number for the {name} tolerance.", "Coordinate Auditor", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DetailsTextBox.Text = (ResultsGrid.SelectedItem as AuditRowViewModel)?.Details ?? string.Empty;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
