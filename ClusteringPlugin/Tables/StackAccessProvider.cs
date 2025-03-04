// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClusteringPlugin.Stacks;
using Microsoft.Performance.SDK.Processing;
using StackFrame = ClusteringPlugin.Stacks.StackFrame;

// StackDescCPtr type is defined in Microsoft.Performance.PerfCore4.HeapSnapshot
// We use this namespace because PolicyHelper tries to find the policy by attaching 
// full type name + policy(AccessProvider) in the assembly
namespace ClusteringPlugin.AccessProviders
{
    public struct StackAccessProvider
        : ICollectionAccessProvider<Stack, string>,
          ISpecializedKeyComparer<string>
    {
        private const string ByModule = "ByModule";
        private const string ByFunction = "ByFunction";

        private static readonly List<string> Modes = new List<string> { ByModule, ByFunction };
        private static readonly IReadOnlyList<string> ReadOnlyModes = Modes.AsReadOnly();

        public string PastEndValue => "NA";

        public bool HasUniqueStart => false;

        public IReadOnlyList<string> ProjectionModes => ReadOnlyModes;

        public bool Equals(Stack x, Stack y)
        {
            return x.Equals(y);
        }

        public int GetCount(Stack collection)
        {
            if (collection == null || collection.Frames == null)
            {
                return 0;
            }

            return collection.Frames.Length;
        }

        public int GetHashCode(Stack x)
        {
            return x.GetHashCode();
        }

        public Stack GetParent(Stack collection)
        {
            throw new NotImplementedException();
        }

        public IComparer<string> GetSpecializedKeyComparer(string mode)
        {
            throw new NotImplementedException();
        }


        public string GetValue(Stack collection, int index)
        {
            if (collection == null || collection.Frames.Length == 0)
            {
                return "NA!NA";
            }

            StackFrame frame;
            int count = collection.Frames.Length;

            if (index >= count)
            {
                frame = collection.Frames[0];

                if (frame == null)
                {
                    return "NA!NA";
                }

                return $"{frame.Module}!{frame.Function}";
            }

            frame = collection.Frames[count - (index + 1)];

            if (frame == null)
            {
                return "NA!NA";
            }

            return $"{frame.Module}!{frame.Function}";
        }

        public bool IsNull(Stack value)
        {
            if (value == null || value.Frames.Length == 0)
                return true;

            return false;
        }
    }
}
