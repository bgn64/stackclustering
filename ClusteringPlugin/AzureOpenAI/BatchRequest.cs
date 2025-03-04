using System.Text.Json.Serialization;

namespace ClusteringPlugin.AzureOpenAI
{
    internal class BatchRequest
    {
        [JsonPropertyName("input_file_id")]
        public string? InputFileId { get; set; }
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }
        [JsonPropertyName("completion_window")]
        public string? CompletionWindow { get; set; }
    }
}
