// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ClusteringPlugin.AccessProviders;
using ClusteringPlugin.Cookers;
using ClusteringPlugin.Parsing;
using ClusteringPlugin.Stacks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using System;
using System.Collections.Generic;

namespace ClusteringPlugin.Tables
{
    [Table]
    public sealed class StackTable
    {
        public static TableDescriptor TableDescriptor =>
           new TableDescriptor(
              Guid.Parse("{b056a97b-16b9-479d-b981-852351aa7bfb}"),
              "Stacks",
              "Events from Stacks",
              "Stacks",
              requiredDataCookers: new List<DataCookerPath>
              {
              StackCooker.DataCookerPath
              });

        private static readonly ColumnConfiguration countColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("d00f3f15-e5d2-45d4-89bc-baf67d353633"), "Count"),
            new UIHints
            {
                IsVisible = true,
                Width = 100,
                AggregationMode = AggregationMode.Sum
            });

        private static readonly ColumnConfiguration timestampColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("fb0f3936-05c1-40e0-9f4d-b7985b1f9aa4"), "Timestamp"),
            new UIHints
            {
                IsVisible = true,
                Width = 100,
                CellFormat = TimestampFormatter.FormatMillisecondsGrouped,
            });

        private static readonly ColumnConfiguration clusterIdColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("32abc2e5-bebb-4269-9113-0a4b3db0e895"), "Cluster Id"),
            new UIHints
            {
                IsVisible = true,
                Width = 100
            });

        private static readonly ColumnConfiguration stackColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("9bec060a-f1b2-47b2-abe2-fec7ab751745"), "Stack"),
            new UIHints
            {
                IsVisible = true,
                Width = 100,
            });

        //
        // This method, with this exact signature, is required so that the runtime can 
        // build your table once all cookers have processed their data.
        //
        public static void BuildTable(
            ITableBuilder tableBuilder,
            IDataExtensionRetrieval requiredData
        )
        {
            List<StackEvent> data =
                requiredData.QueryOutput<List<StackEvent>>(new DataOutputPath(StackCooker.DataCookerPath, nameof(StackCooker.StackEvents)));

            ITableBuilderWithRowCount tableBuilderWithRowCount = tableBuilder.SetRowCount(data.Count);
            StackAccessProvider stackAccessProvider = new StackAccessProvider();

            var baseProjection = Projection.Index(data);
            var timestampProjection = baseProjection.Compose(Projector.Timestamp);
            var clusterIdProjection = baseProjection.Compose(Projector.ClusterId);
            var stackProjection = baseProjection.Compose(Projector.Stack);

            tableBuilderWithRowCount.AddColumn(countColumn, Projection.Constant(1));
            tableBuilderWithRowCount.AddColumn(timestampColumn, timestampProjection);
            tableBuilderWithRowCount.AddColumn(clusterIdColumn, clusterIdProjection);
            tableBuilderWithRowCount.AddHierarchicalColumn(stackColumn,
                stackProjection, stackAccessProvider);

            var tableConfig = new TableConfiguration("Stacks")
            {
                Columns = new[]
                {
                    clusterIdColumn,
                    stackColumn,
                    TableConfiguration.PivotColumn,
                    countColumn,
                    TableConfiguration.GraphColumn,
                    timestampColumn
                },
            };

            tableBuilder.AddTableConfiguration(tableConfig);
            tableBuilder.SetDefaultTableConfiguration(tableConfig);
        }
    }
}