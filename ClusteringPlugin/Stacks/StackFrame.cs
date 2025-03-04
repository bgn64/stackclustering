using System;
using System.Collections.Generic;
using DLStackFrame = Microsoft.Windows.EventTracing.Symbols.StackFrame;

namespace ClusteringPlugin.Stacks
{
    public class StackFrame : IEquatable<StackFrame?>
    {
        public string Module { get; }

        public string Function { get; }

        public StackFrame(DLStackFrame stackFrame)
        {
            Module = stackFrame.Image?.FileName ?? string.Empty;
            Function = stackFrame.Symbol?.FunctionName ?? string.Empty;
        }

        public StackFrame(string module, string function)
        {
            Module = module;
            Function = function;
        }

        public override string ToString()
        {
            return $"{Module}!{Function}";
        }

        public static StackFrame Parse(string s)
        {
            string[] parts = s.Split('!');

            if (parts.Length != 2)
            {
                throw new Exception();
            }

            return new StackFrame(parts[0], parts[1]);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StackFrame);
        }

        public bool Equals(StackFrame? other)
        {
            return other is not null &&
                   Module == other.Module &&
                   Function == other.Function;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Module, Function);
        }

        public static bool operator ==(StackFrame? left, StackFrame? right)
        {
            return EqualityComparer<StackFrame>.Default.Equals(left, right);
        }

        public static bool operator !=(StackFrame? left, StackFrame? right)
        {
            return !(left == right);
        }
    }
}
