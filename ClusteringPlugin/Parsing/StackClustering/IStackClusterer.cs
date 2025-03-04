using ClusteringPlugin.Stacks;

namespace ClusteringPlugin.StackClustering
{
    internal interface IStackClusterer
    {
        ClusterResult ClusterStacks(Stack[] stacks);
    }
}
