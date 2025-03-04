using ClusteringPlugin.Stacks;

namespace ClusteringPlugin.StackPreprocessing.FunctionDescriptions
{
    internal interface IFunctionDescriptionProvider
    {
        string GetFunctionDescription(StackFrame stackFrame);

        string[] GetFunctionDescriptions(StackFrame[] stackFrames);
    }
}
