using Microsoft.Windows.EventTracing.Processes;
using Microsoft.Windows.EventTracing.Symbols;
using Microsoft.Windows.EventTracing;
using System;
using System.Linq;

namespace ClusteringPlugin.Stacks
{
    internal class StackProvider
    {
        string etlPath;

        public StackProvider(string etlPath) 
        {
            this.etlPath = etlPath;
        }

        public Stack[] GetStacks(Func<IStackSnapshot, bool> predicate)
        {
            using (ITraceProcessor trace = Microsoft.Windows.EventTracing.TraceProcessor.Create(etlPath))
            {
                IPendingResult<IProcessDataSource> pendingProcessData = trace.UseProcesses();
                IPendingResult<IStackDataSource> pendingStackData = trace.UseStacks();
                IPendingResult<ISymbolDataSource> pendingSymbolData = trace.UseSymbols();

                trace.Process();

                IProcessDataSource processData = pendingProcessData.Result;
                IStackDataSource stackData = pendingStackData.Result;
                ISymbolDataSource symbolData = pendingSymbolData.Result;

                symbolData.LoadSymbolsForConsoleAsync(SymCachePath.Automatic).GetAwaiter().GetResult();

                return stackData.Stacks
                    .Where(predicate)
                    .Where(s => s.GetDebuggerString() != string.Empty)
                    .Select(s => new Stack(s))
                    .ToArray();
            }
        }
    }
}
