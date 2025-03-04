// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK;
using System.Collections.Generic;
using System.Threading;
using System;
using ClusteringPlugin.Parsing;

namespace ClusteringPlugin.Cookers
{
    public sealed class StackCooker
        : SourceDataCooker<Event, ParsingContext, Type>
    {
        public static readonly DataCookerPath DataCookerPath =
            DataCookerPath.ForSource(nameof(TraceSourceParser), nameof(StackCooker));

        public StackCooker()
            : base(DataCookerPath)
        {
            this.StackEvents = new List<StackEvent>();
        }

        public override string Description => "Stack Event cooker.";

        public override ReadOnlyHashSet<Type> DataKeys =>
            new ReadOnlyHashSet<Type>(new HashSet<Type>(new[] { typeof(StackEvent) }));

        [DataOutput]
        public List<StackEvent> StackEvents { get; }

        public override DataProcessingResult CookDataElement(
            Event data,
            ParsingContext context,
            CancellationToken cancellationToken)
        {
            StackEvents.Add((StackEvent)data);

            return DataProcessingResult.Processed;
        }
    }
}
