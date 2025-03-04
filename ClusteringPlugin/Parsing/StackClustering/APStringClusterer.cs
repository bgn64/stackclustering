namespace ClusteringPlugin.StackClustering
{
    internal class APStringClusterer : PythonStringClusterer
    {
        static readonly string ScriptPath = @"C:\Users\benjaming\source\repos\Clustering\Clustering\StackClustering\ap_clustering.py";

        public APStringClusterer(int preference) : base(ScriptPath, $"{preference}")
        {
        }
    }
}
