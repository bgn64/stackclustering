using ClusteringPlugin.Stacks;

namespace ClusteringPlugin.StackPreprocessing
{
    internal interface IStackPreprocessor
    {
        string[] PreprocessStacks(Stack[] stacks);
    }
}
