using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.DTO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

namespace DotNetRestaurantManagement.Reports
{
    public class FrequentlyBoughtTogetherReport : Report
    {
        public FrequentlyBoughtTogetherReport(
            IEnumerable<FrequentlyBoughtItems> reportData,
            int combinationSize)
        {
            BuildReport(reportData, combinationSize);
        }

        private void BuildReport(
            IEnumerable<FrequentlyBoughtItems> reportData,
            int combinationSize)
        {
            Name = StringConstants.ReportFileName;
            PageSettings.PaperKind = PaperKind.A4;
            PageSettings.Landscape = true;

            PageSettings.Margins = new MarginsU(
                Unit.Inch(0.4),
                Unit.Inch(0.4),
                Unit.Inch(0.4),
                Unit.Inch(0.4));

            Width = Unit.Inch(10.2);
            var reportHeader = new ReportHeaderSection
            {
                Height = Unit.Inch(0.8)
            };

            var title = new TextBox
            {
                Name = StringConstants.Title,
                Value = StringConstants.ReportTitle,
                Location = new PointU(
                    Unit.Inch(0),
                    Unit.Inch(0.1)),
                Size = new SizeU(
                    Unit.Inch(10.2),
                    Unit.Inch(0.5))
            };

            title.Style.Font.Bold = true;
            title.Style.Font.Size = Unit.Point(18);
            title.Style.VerticalAlign = VerticalAlign.Middle;

            reportHeader.Items.Add(title);
            Items.Add(reportHeader);

            var detailSection = new DetailSection
            {
                Height = Unit.Inch(0.5)
            };

            var table = CreateTable(
                reportData,
                combinationSize);

            detailSection.Items.Add(table);
            Items.Add(detailSection);
        }

        private Table CreateTable(
            IEnumerable<FrequentlyBoughtItems> reportData,
            int combinationSize)
        {
            var table = new Table
            {
                Name = StringConstants.ReportFileName,
                DataSource = reportData,
                Location = new PointU(
                    Unit.Inch(0),
                    Unit.Inch(0)),
                Size = new SizeU(
                    Unit.Inch(10.2),
                    Unit.Inch(0.35))
            };

            var detailGroup = new TableGroup
            {
                Name = "DetailGroup"
            };

            detailGroup.Groupings.Add(new Grouping(null));
            table.RowGroups.Add(detailGroup);
            table.Body.Rows.Add(new TableBodyRow(Unit.Inch(0.35)));
            int columnCount = combinationSize + 1;
            double columnWidth = 10.2 / columnCount;

            for (int i = 0; i < columnCount; i++)
            {
                table.Body.Columns.Add(
                    new TableBodyColumn(
                        Unit.Inch(columnWidth)));

                var headerTextBox = new TextBox
                {
                    Name = $"Header{i}",
                    Value = GetColumnHeader(
                        i,
                        combinationSize),
                    Size = new SizeU(
                        Unit.Inch(columnWidth),
                        Unit.Inch(0.35))
                };

                headerTextBox.Style.Font.Bold = true;
                headerTextBox.Style.BackgroundColor =
                    Color.LightGray;

                headerTextBox.Style.BorderStyle.Default =
                    BorderType.Solid;

                headerTextBox.Style.BorderWidth.Default =
                    Unit.Point(1);

                headerTextBox.Style.Padding.Left =
                    Unit.Point(5);

                headerTextBox.Style.Padding.Right =
                    Unit.Point(5);

                headerTextBox.Style.VerticalAlign =
                    VerticalAlign.Middle;

                var columnGroup = new TableGroup
                {
                    Name = $"ColumnGroup{i}",
                    ReportItem = headerTextBox
                };

                table.ColumnGroups.Add(columnGroup);
            }

            var item1 = CreateFieldTextBox(
                "Item1",
                "=Fields.Item1",
                columnWidth);

            table.Body.SetCellContent(
                0,
                0,
                item1);

            var item2 = CreateFieldTextBox(
                "Item2",
                "=Fields.Item2",
                columnWidth);

            table.Body.SetCellContent(
                0,
                1,
                item2);

            if (combinationSize == 3)
            {
                var item3 = CreateFieldTextBox(
                    "Item3",
                    "=Fields.Item3",
                    columnWidth);

                table.Body.SetCellContent(
                    0,
                    2,
                    item3);
            }

            var numberOfTimesBought =
                CreateFieldTextBox(
                    "NumberOfTimesBought",
                    "=Fields.NumberOfTimesBought",
                    columnWidth);

            table.Body.SetCellContent(
                0,
                combinationSize,
                numberOfTimesBought);

            return table;
        }

        private string GetColumnHeader(
            int columnIndex,
            int combinationSize)
        {
            if (columnIndex < combinationSize)
            {
                return $"Item {columnIndex + 1}";
            }

            return StringConstants.ReportFileName;
        }

        private TextBox CreateFieldTextBox(
            string name,
            string expression,
            double width)
        {
            var textBox = new TextBox
            {
                Name = name,
                Value = expression,
                Size = new SizeU(
                    Unit.Inch(width),
                    Unit.Inch(0.35))
            };

            textBox.Style.BorderStyle.Default =
                BorderType.Solid;

            textBox.Style.BorderWidth.Default =
                Unit.Point(1);

            textBox.Style.Padding.Left =
                Unit.Point(5);

            textBox.Style.Padding.Right =
                Unit.Point(5);

            textBox.Style.VerticalAlign =
                VerticalAlign.Middle;

            return textBox;
        }
    }
}
