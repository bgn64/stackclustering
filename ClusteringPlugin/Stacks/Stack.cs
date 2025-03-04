using Microsoft.Windows.EventTracing.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusteringPlugin.Stacks
{
    public class Stack : IEquatable<Stack?>
    {
        public StackFrame[] Frames { get; }

        public Stack(IStackSnapshot stackSnapshot) 
        {
            Frames = stackSnapshot.Frames.Select(f => new StackFrame(f)).ToArray();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Stack);
        }

        public bool Equals(Stack? other)
        {
            if (other == null || other.Frames.Length != Frames.Length)
            {
                return false;
            }

            for (int i = 0; i < Frames.Length; i++)
            {
                if (other.Frames[i] != Frames[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            for (int i = 0; i < Frames.Length; i++)
            {
                hashCode = HashCode.Combine(Frames[i], hashCode);
            }

            return HashCode.Combine(hashCode);
        }

        public static bool operator ==(Stack? left, Stack? right)
        {
            return EqualityComparer<Stack>.Default.Equals(left, right);
        }

        public static bool operator !=(Stack? left, Stack? right)
        {
            return !(left == right);
        }
    }
}
