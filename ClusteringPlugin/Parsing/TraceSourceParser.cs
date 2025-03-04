// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;
using System;
using System.Threading;
using Microsoft.Performance.SDK;
using Azure.AI.OpenAI;
using ClusteringPlugin.StackClustering;
using ClusteringPlugin.StackPreprocessing.FunctionDescriptions;
using ClusteringPlugin.StackPreprocessing;
using ClusteringPlugin.Stacks;
using System.ClientModel;
using System.Linq;

namespace ClusteringPlugin.Parsing
{
    public sealed class TraceSourceParser
        : SourceParser<Event, ParsingContext, Type>
    {
        static readonly Uri AzureEndpoint = new Uri("https://8045fa23-b507-4dfd-9426-2c0e323faf8c.openai.azure.com/");
        static readonly string FileCache = @"C:\Users\benjaming\source\repos\Clustering\ClusteringPlugin\Parsing\StackPreprocesing\FunctionDescriptions\cache.txt";
        static readonly ApiKeyCredential Credential = new ApiKeyCredential("");

        private ParsingContext context;
        private IEnumerable<IDataSource> dataSources;
        private DataSourceInfo? dataSourceInfo;

        public TraceSourceParser(IEnumerable<IDataSource> dataSources)
        {
            context = new ParsingContext();

            // Store the datasources so we can parse them later
            this.dataSources = dataSources;
        }

        // The ID of this Parser.
        public override string Id => nameof(TraceSourceParser);

        // Information about the Data Sources being parsed.
        public override DataSourceInfo? DataSourceInfo => dataSourceInfo;

        public override void ProcessSource(ISourceDataProcessor<Event, ParsingContext, Type> dataProcessor, ILogger logger, IProgress<int> progress, CancellationToken cancellationToken)
        {
            Timestamp? firstEventTimestamp = null;
            Timestamp? lastEventTimestamp = null;

            foreach (IDataSource dataSource in dataSources)
            {
                ProcessDataSource(dataSource, ref firstEventTimestamp, ref lastEventTimestamp, dataProcessor, progress, cancellationToken);
            }

            long firstEventTimestampNanoseconds = firstEventTimestamp.HasValue ? firstEventTimestamp.Value.ToNanoseconds : 0;
            long lastEventTimestampnanoseconds = lastEventTimestamp.HasValue ? lastEventTimestamp.Value.ToNanoseconds : firstEventTimestampNanoseconds + 1;
            DateTime firstEventWallClockUtc = DateTime.UtcNow;
            dataSourceInfo = new DataSourceInfo(firstEventTimestampNanoseconds, lastEventTimestampnanoseconds, firstEventWallClockUtc);
        }

        public void ProcessDataSource(IDataSource dataSource, ref Timestamp? firstEventTimestamp, ref Timestamp? lastEventTimestamp,
            ISourceDataProcessor<Event, ParsingContext, Type> dataProcessor, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (!(dataSource is FileDataSource fileDataSource))
            {
                return;
            }

            StackProvider stackProvider = new StackProvider(fileDataSource.FullPath);
            Stack[] stacks = stackProvider.GetStacks(s => s.Process?.ImageName?.Contains("SampleWorkload") ?? false);

            /*AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(AzureEndpoint, Credential);
            FileCache fileCache = new FileCache(FileCache);
            IStackPreprocessor stackPreprocessor = new FunctionDescriptionStackPreprocessor(
                new AzureFunctionDescriptionProvider(azureOpenAIClient, fileCache),
                new TextCleaner());
            IStringClusterer stringClusterer = new APStringClusterer(-10);
            IStackClusterer stackClusterer = new StackClusterer(stackPreprocessor, stringClusterer);*/
            IStackClusterer stackClusterer = new OccurrenceAPClusterer();

            ClusterResult result = stackClusterer.ClusterStacks(stacks);

            Console.WriteLine($"{result.NumberOfClusters}\t{result.SilhouetteScore}");

            for (int i = 0; i < stacks.Length; i++)
            {
                Event e = new StackEvent()
                {
                    TimeStamp = Timestamp.FromMilliseconds(i), // sample timestamp
                    ClusterId = result.Labels[i],
                    Stack = stacks[i]
                };

                dataProcessor.ProcessDataElement(e, context, cancellationToken);

                if (firstEventTimestamp == null || firstEventTimestamp.Value > e.Timestamp)
                {
                    firstEventTimestamp = e.Timestamp;
                }

                if (lastEventTimestamp == null || lastEventTimestamp.Value < e.Timestamp)
                {
                    lastEventTimestamp = e.Timestamp;
                }
            }
        }
    }
}