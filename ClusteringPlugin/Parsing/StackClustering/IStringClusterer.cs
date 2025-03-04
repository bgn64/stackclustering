namespace ClusteringPlugin.StackClustering
{
    internal interface IStringClusterer
    {
        ClusterResult ClusterStrings(string[] inputStrings);
    }
}
