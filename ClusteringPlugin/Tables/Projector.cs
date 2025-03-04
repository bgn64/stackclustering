// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ClusteringPlugin.Parsing;
using ClusteringPlugin.Stacks;
using Microsoft.Performance.SDK;

namespace ClusteringPlugin.Tables
{
    internal class Projector
    {
        public static Timestamp Timestamp(Event e)
        {
            return e.Timestamp;
        }

        public static int ClusterId(StackEvent e)
        {
            return e.ClusterId;
        }

        public static Stack Stack(StackEvent e)
        {
            return e.Stack;
        }
    }
}
