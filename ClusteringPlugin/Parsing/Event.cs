// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using System;

namespace ClusteringPlugin.Parsing
{
    public abstract class Event : IKeyedDataType<Type>
    {
        public abstract Timestamp Timestamp { get; }
        public abstract Type GetKey();
    }
}
