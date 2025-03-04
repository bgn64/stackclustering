using ClusteringPlugin.StackPreprocessing;
using ClusteringPlugin.Stacks;

namespace ClusteringPlugin.StackClustering
{
    internal class StackClusterer : IStackClusterer
    {
        readonly IStackPreprocessor stackPreprocessor;
        readonly IStringClusterer stringClusterer;

        public StackClusterer(IStackPreprocessor stackPreprocessor, IStringClusterer stringClusterer)
        {
            this.stackPreprocessor = stackPreprocessor;
            this.stringClusterer = stringClusterer;
        }

        public ClusterResult ClusterStacks(Stack[] stacks)
        {
            string[] inputStrings = stackPreprocessor.PreprocessStacks(stacks);
            
            return stringClusterer.ClusterStrings(inputStrings);
        }
    }
}
