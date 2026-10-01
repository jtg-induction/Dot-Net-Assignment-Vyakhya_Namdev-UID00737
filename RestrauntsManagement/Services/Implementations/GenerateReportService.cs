using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Reports;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class GenerateReportService : IGenerateReportService
    {
        /// <summary>
        /// Generates a PDF report using the specified data and report template
        /// </summary>
        public byte[] Generate<T>(
            IEnumerable<T> reportData,
            string reportFileName,
            bool? showRestaurantData = null,
            int? itemCount = null)
        {
            var reportPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                StringConstants.Reports,
                reportFileName);

            var reportPackager = new ReportPackager();

            Telerik.Reporting.Report report;

            using (var stream = File.OpenRead(reportPath))
            {
                report = reportPackager.Unpackage(stream);
            }

            var detailSection = report.Items
                .OfType<Telerik.Reporting.DetailSection>()
                .FirstOrDefault();

            var table = detailSection?
                .Items
                .OfType<Telerik.Reporting.Table>()
                .FirstOrDefault();

            table.DataSource = reportData;

            if (showRestaurantData.HasValue)
            {
                report.ReportParameters[
                    StringConstants.ShowRestaurantData
                ].Value = showRestaurantData.Value;
            }

            if (itemCount.HasValue)
            {
                report.ReportParameters[
                    StringConstants.ItemCount
                ].Value = itemCount.Value;
            }

            var reportSource = new InstanceReportSource
            {
                ReportDocument = report
            };

            var reportProcessor = new ReportProcessor();

            var result = reportProcessor.RenderReport(
                "PDF",
                reportSource,
                null);

            return result.DocumentBytes;
        }

        /// <summary>
        /// Generates PDF of .cs file Frequently Bought Items report 
        /// </summary>
        public byte[] GenerateFrequentlyBoughtTogether(
            IEnumerable<FrequentlyBoughtItems> reportData,
            int combinationSize)
        {
            var report = new FrequentlyBoughtTogetherReport(
                reportData,
                combinationSize);

            var reportSource = new InstanceReportSource
            {
                ReportDocument = report
            };

            var reportProcessor = new ReportProcessor();

            var result = reportProcessor.RenderReport(
                "PDF",
                reportSource,
                null);

            return result.DocumentBytes;
        }
    }
}
