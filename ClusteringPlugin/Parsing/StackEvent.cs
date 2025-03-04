// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ClusteringPlugin.Stacks;
using Microsoft.Performance.SDK;
using System;

namespace ClusteringPlugin.Parsing
{
    public class StackEvent : Event
    {
        public override Timestamp Timestamp => TimeStamp;

        public override Type GetKey()
        {
            return typeof(StackEvent);
        }

        public Timestamp TimeStamp { get; set; }

        public int ClusterId { get; set; }

        public Stack? Stack { get; set; }
    }
}
