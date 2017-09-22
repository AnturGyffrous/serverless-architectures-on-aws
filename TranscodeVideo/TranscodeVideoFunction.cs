using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace TranscodeVideo
{
    public class TranscodeVideoFunction
    {
        private readonly IAmazonElasticTranscoder _elasticTranscoder;

        /// <summary>
        ///     Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        ///     the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        ///     region the Lambda function is executed in.
        /// </summary>
        public TranscodeVideoFunction()
        {
            _elasticTranscoder = new AmazonElasticTranscoderClient();
        }

        /// <summary>
        ///     Constructs an instance with a pre-configured Elastic Transcoder client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="elasticTranscoder"></param>
        public TranscodeVideoFunction(IAmazonElasticTranscoder elasticTranscoder)
        {
            _elasticTranscoder = elasticTranscoder;
        }

        /// <summary>
        ///     This method is called for every Lambda invocation. This method takes in an S3 event object and can be used to respond to S3 notifications.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Handler(S3Event input, ILambdaContext context)
        {
            var key = input.Records?[0].S3.Object.Key;

            var sourceKey = WebUtility.UrlDecode(key);

            var outputKey = sourceKey.Split('.')[0];

            context.Logger.LogLine($"key: {key}, sourceKey: {sourceKey}, outputKey: {outputKey}");

            var job = new CreateJobRequest
            {
                PipelineId = "1505766337361-sv0ahs",
                OutputKeyPrefix = $"{outputKey}/",
                Input = new JobInput {Key = sourceKey},
                Outputs = new List<CreateJobOutput>(new[]
                {
                    new CreateJobOutput {Key = $"{outputKey}-1080p.mp4", PresetId = "1351620000001-000001"},
                    new CreateJobOutput {Key = $"{outputKey}-720p.mp4", PresetId = "1351620000001-000010"},
                    new CreateJobOutput {Key = $"{outputKey}-web-720p.mp4", PresetId = "1351620000001-100070"}
                })
            };

            await _elasticTranscoder.CreateJobAsync(job);
        }
    }
}