using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.SNSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SetPermissions
{
    public class SetPermissionsFunction
    {
        private readonly IAmazonS3 _amazonS3;

        /// <summary>
        ///     Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        ///     the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        ///     region the Lambda function is executed in.
        /// </summary>
        public SetPermissionsFunction()
        {
            _amazonS3 = new AmazonS3Client();
        }

        /// <summary>
        ///     Constructs an instance with a pre-configured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="amazonS3"></param>
        public SetPermissionsFunction(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        /// <summary>
        ///     This method is called for every Lambda invocation. This method takes in an SNS event object and can be used to respond to SNS notifications.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Handler(SNSEvent input, ILambdaContext context)
        {
            var message = JsonConvert.DeserializeObject<S3Event>(input.Records[0].Sns.Message);

            var sourceBucket = message.Records[0].S3.Bucket.Name;
            var sourceKey = WebUtility.UrlDecode(message.Records[0].S3.Object.Key);

            context.Logger.LogLine($"sourceBucket: {sourceBucket}, sourceKey: {sourceKey}");

            await _amazonS3.PutACLAsync(new PutACLRequest
            {
                BucketName = sourceBucket,
                Key = sourceKey,
                CannedACL = S3CannedACL.PublicRead
            });
        }
    }
}