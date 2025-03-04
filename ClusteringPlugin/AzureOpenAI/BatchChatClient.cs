using Azure.AI.OpenAI;
using OpenAI.Batch;
using OpenAI.Files;
using System.ClientModel.Primitives;
using System.ClientModel;
using System.Text.Json;
using System.Text;
using System.IO;
using System;

namespace ClusteringPlugin.AzureOpenAI
{
    internal class BatchChatClient
    {
        readonly OpenAIFileClient fileClient;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        readonly BatchClient batchClient;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        public BatchChatClient(AzureOpenAIClient client)
        {
            fileClient = client.GetOpenAIFileClient();

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            batchClient = client.GetBatchClient();
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        }

        public string[] CompleteChats(string[] chats)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < chats.Length; i++)
            {
                stringBuilder.AppendLine($"{{\"custom_id\": \"{i}\", \"method\": \"POST\", \"url\": \"/chat/completions\", \"body\": {{\"model\": \"gpt-35-turbo\", \"messages\": [{{\"role\": \"system\", \"content\": \"You are an AI assistant that helps people find information.\"}}, {{\"role\": \"user\", \"content\": \"{chats[i]}\"}}]}}}}");
            }

            string fileName = $"{Path.GetRandomFileName()}.jsonl";
            OpenAIFile inputFile = fileClient.UploadFile(new BinaryData(stringBuilder.ToString()), fileName, FileUploadPurpose.Batch);

            BatchRequest batchRequest = new BatchRequest()
            {
                InputFileId = inputFile.Id,
                Endpoint = "/chat/completions",
                CompletionWindow = "24h"
            };
            string json = JsonSerializer.Serialize(batchRequest);

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            CreateBatchOperation operation = batchClient.CreateBatch(BinaryContent.Create(new BinaryData(json)), true, new RequestOptions());
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            ClientResult result = operation.GetBatch(new RequestOptions());
            BatchResponse? batchResponse = result.GetRawResponse().Content.ToObjectFromJson<BatchResponse>();

            if (batchResponse == null || batchResponse.Status != "completed")
            {
                throw new Exception();
            }

            BinaryData outputFile = fileClient.DownloadFile(batchResponse.OutputFileId);
            string[] completions = new string[chats.Length];
            string tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, outputFile.ToString());

            foreach (string line in File.ReadLines(tmpFile))
            {
                JsonDocument doc = JsonDocument.Parse(line);

                if (!doc.RootElement.TryGetProperty("custom_id", out JsonElement customId) ||
                    !doc.RootElement.TryGetProperty("response", out JsonElement response) ||
                    !response.TryGetProperty("body", out JsonElement body) ||
                    !body.TryGetProperty("choices", out JsonElement choices) ||
                    !choices[0].TryGetProperty("message", out JsonElement message) ||
                    !message.TryGetProperty("content", out JsonElement content))
                {
                    throw new Exception();
                }

                completions[int.Parse(customId.ToString())] = content.ToString();
            }

            File.Delete(tmpFile);

            return completions;
        }
    }
}
