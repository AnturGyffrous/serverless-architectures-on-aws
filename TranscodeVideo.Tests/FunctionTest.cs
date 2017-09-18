using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ElasticTranscoder;
using Amazon.ElasticTranscoder.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3.Util;
using Moq;
using Xunit;

namespace TranscodeVideo.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task FunctionHandlerWillStartElasticTranscoderJob()
        {
            // Arrange
            var s3Event = new S3Event
            {
                Records = new List<S3EventNotification.S3EventNotificationRecord>
                {
                    new S3EventNotification.S3EventNotificationRecord
                    {
                        S3 = new S3EventNotification.S3Entity
                        {
                            Object = new S3EventNotification.S3ObjectEntity
                            {
                                Key = "My+Holiday%3A+Gr%C3%B6na+Lund+%28Jan+%2715%29.mp4"
                            }
                        }
                    }
                }
            };

            var elasticTranscoder = Mock.Of<IAmazonElasticTranscoder>();
            Mock.Get(elasticTranscoder)
                .Setup(x => x.CreateJobAsync(It.IsAny<CreateJobRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CreateJobResponse()));

            var logger = Mock.Of<ILambdaLogger>();

            var context = Mock.Of<ILambdaContext>();
            Mock.Get(context)
                .Setup(x => x.Logger)
                .Returns(logger);

            var lambda = new Function(elasticTranscoder);

            // Act
            await lambda.FunctionHandler(s3Event, context);

            // Assert
            Mock.Get(logger).Verify(x => x.LogLine(It.Is<string>(m => m.Contains("My Holiday: Gröna Lund (Jan '15)"))));
            Mock.Get(elasticTranscoder)
                .Verify(x => x.CreateJobAsync(It.IsAny<CreateJobRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}