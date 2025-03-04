using System.Text.Json.Serialization;

namespace ClusteringPlugin.AzureOpenAI
{
    internal class BatchResponse
    {
        [JsonPropertyName("output_file_id")]
        public string? OutputFileId { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
