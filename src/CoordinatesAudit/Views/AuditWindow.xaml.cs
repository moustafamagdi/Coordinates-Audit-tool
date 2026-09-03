using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            _rows.Clear();
            foreach (LinkInstanceData link in _links)
            {
                AuditComparisonResult result = _comparisonEngine.Compare(link, reference, horizontal, vertical, angular);
                _rows.Add(AuditRowViewModel.Create(link, result));
            }

            int pass = _rows.Count(row => row.Status == "PASS");
            int warning = _rows.Count(row => row.Status == "WARNING");
            int fail = _rows.Count(row => row.Status == "FAIL");
            int unavailable = _rows.Count(row => row.Status == "UNAVAILABLE");
            StatusSummaryText.Text = $"Total: {_rows.Count}   PASS: {pass}   WARNING: {warning}   FAIL: {fail}   UNAVAILABLE: {unavailable}";
            if (_rows.Count > 0) ResultsGrid.SelectedIndex = 0;
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
