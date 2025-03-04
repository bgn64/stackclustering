using Azure.AI.OpenAI;
using ClusteringPlugin.AzureOpenAI;
using ClusteringPlugin.Stacks;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Linq;

namespace ClusteringPlugin.StackPreprocessing.FunctionDescriptions
{
    internal class AzureFunctionDescriptionProvider : IFunctionDescriptionProvider
    {
        private readonly BatchChatClient batchChatClient;
        private readonly AzureOpenAIClient client;
        private readonly FileCache cache;

        public AzureFunctionDescriptionProvider(AzureOpenAIClient client, FileCache cache)
        {
            batchChatClient = new BatchChatClient(client);
            this.client = client;
            this.cache = cache;
        }

        public string GetFunctionDescription(StackFrame stackFrame)
        {
            if (string.IsNullOrEmpty(stackFrame.Function) || string.IsNullOrEmpty(stackFrame.Module))
            {
                return string.Empty;
            }

            string key = $"{stackFrame.Module}!{stackFrame.Function}";

            return cache.GetOrAdd(key, k =>
            {
                try
                {
                    // Create a chat client
                    ChatClient chatClient = client.GetChatClient("gpt-35-turbo");
                    ChatMessage message = ChatMessage.CreateUserMessage($"Provide a response consisting of 5 or less key words that describe the purpose of the function '{stackFrame.Function}' from the module '{stackFrame.Module}'.");
                    ClientResult<ChatCompletion> result = chatClient.CompleteChat(message);

                    string res = string.Concat(result.Value?.Content.Select(p => p.Text) ?? Array.Empty<string>());

                    return res;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            });
        }

        public string[] GetFunctionDescriptions(StackFrame[] stackFrames)
        {
            string[] uncachedStackFrames = stackFrames
                .Where(sf => !string.IsNullOrEmpty(sf.Module) && !string.IsNullOrEmpty(sf.Function))
                .Select(sf => sf.ToString())
                .Distinct()
                .Where(s => cache.Get(s) == null)
                .ToArray();

            if (uncachedStackFrames.Length > 0)
            {
                string[] chats = new string[uncachedStackFrames.Length];

                for (int i = 0; i < uncachedStackFrames.Length; i++)
                {
                    StackFrame stackFrame = StackFrame.Parse(uncachedStackFrames[i]);
                    chats[i] = $"Provide a response consisting of 5 or less key words that describe the purpose of the function '{stackFrame.Function}' from the module '{stackFrame.Module}'.";
                }

                string[] descriptions = batchChatClient.CompleteChats(chats);

                for (int i = 0; i < uncachedStackFrames.Length; i++)
                {
                    cache.Add(uncachedStackFrames[i], descriptions[i]);
                }
            }

            return stackFrames
                .Select(GetFunctionDescription)
                .ToArray();
        }
    }
}
