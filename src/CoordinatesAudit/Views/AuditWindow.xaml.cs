using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        private readonly DispatcherTimer _autoRunTimer;
        private bool _isInitialized;
        private ReferenceModelOption _lastReference;
        private double _lastHorizontalTolerance;
        private double _lastVerticalTolerance;
        private double _lastAngularTolerance;

        public AuditWindow(HostCoordinateReport host, IReadOnlyList<LinkInstanceData> links)
        {
            _autoRunTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
            _autoRunTimer.Tick += AutoRunTimer_Tick;
            InitializeComponent();
            _host = host;
            _links = links;
            ResultsGrid.ItemsSource = _rows;
            ProjectSummaryText.Text = $"{host.ModelTitle}  |  {host.ProjectLocationName}  |  {host.LengthUnit}";
            HostLocationText.Text = host.ProjectLocationName + "  |  True North " + host.AngleToTrueNorth;
            HostProjectBasePointText.Text = host.ProjectBasePoint.InternalPosition;
            HostSurveyPointText.Text = host.SurveyPoint.InternalPosition;
            HostInternalOriginText.Text = host.InternalOriginPosition;
            ReferenceComboBox.ItemsSource = BuildReferenceOptions();
            ReferenceComboBox.SelectedIndex = 0;
            _isInitialized = true;
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

        private void ReferenceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            _autoRunTimer.Stop();
            RunComparison();
        }

        private void ToleranceTextBox_TextChanged(object sender, TextChangedEventArgs e) => ScheduleComparison();

        private void ScheduleComparison()
        {
            if (!_isInitialized) return;
            _autoRunTimer.Stop();
            _autoRunTimer.Start();
        }

        private void AutoRunTimer_Tick(object sender, EventArgs e)
        {
            _autoRunTimer.Stop();
            RunComparison();
        }

        private void RunComparison()
        {
            if (!(ReferenceComboBox.SelectedItem is ReferenceModelOption reference)) return;
            if (!TryReadTolerance(HorizontalToleranceTextBox.Text, out double horizontal) ||
                !TryReadTolerance(VerticalToleranceTextBox.Text, out double vertical) ||
                !TryReadTolerance(AngularToleranceTextBox.Text, out double angular))
            {
                InputValidationText.Text = "Enter non-negative tolerance values.";
                return;
            }
            InputValidationText.Text = string.Empty;

            _lastReference = reference;
            _lastHorizontalTolerance = horizontal;
            _lastVerticalTolerance = vertical;
            _lastAngularTolerance = angular;
            ActiveReferenceText.Text = "Active: " + reference.DisplayName + "  •  Updated " + DateTime.Now.ToString("HH:mm:ss");

            _rows.Clear();
            _rows.Add(AuditRowViewModel.CreateHost(_host, reference.Id == "HOST"));
            foreach (LinkInstanceData link in _links)
            {
                AuditComparisonResult result = _comparisonEngine.Compare(link, reference, horizontal, vertical, angular);
                AuditRowViewModel row = AuditRowViewModel.Create(link, result);
                if (reference.Id == link.InstanceId)
                {
                    row.Status = "REFERENCE";
                    row.Reason = "This linked-model instance is the selected reference.";
                    row.Details = "SELECTED REFERENCE MODEL\n\n" + row.Details;
                }
                _rows.Add(row);
            }

            IReadOnlyList<AuditRowViewModel> linkRows = _rows.Where(row => row.RowType == "Link").ToList();
            int pass = linkRows.Count(row => row.Status == "PASS");
            int warning = linkRows.Count(row => row.Status == "WARNING");
            int fail = linkRows.Count(row => row.Status == "FAIL");
            int unavailable = linkRows.Count(row => row.Status == "UNAVAILABLE");
            int referenceRows = linkRows.Count(row => row.Status == "REFERENCE");
            StatusSummaryText.Text = $"Links: {linkRows.Count}   PASS: {pass}   WARNING: {warning}   FAIL: {fail}   UNAVAILABLE: {unavailable}   REFERENCE: {referenceRows}";
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

        private static bool TryReadTolerance(string text, out double value)
        {
            bool valid = double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
                         double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            return valid && value >= 0.0;
        }

        private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DetailsTextBox.Text = (ResultsGrid.SelectedItem as AuditRowViewModel)?.Details ?? string.Empty;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
