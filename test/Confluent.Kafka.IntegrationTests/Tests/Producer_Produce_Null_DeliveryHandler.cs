// Copyright 2016-2017 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.

#pragma warning disable xUnit1026

using System;
using System.Collections.Generic;
using Xunit;


namespace Confluent.Kafka.IntegrationTests
{
    /// <summary>
    ///     Test every Producer.ProduceAsync method overload that provides
    ///     delivery reports via an Action callback.
    ///     (null key/value case)
    /// </summary>
    public static partial class Tests
    {
        [Theory, MemberData(nameof(KafkaParameters))]
        public static void Producer_Produce_Null_DeliveryHandler(string bootstrapServers, string singlePartitionTopic, string partitionedTopic)
        {
            var producerConfig = new Dictionary<string, object> 
            { 
                { "bootstrap.servers", bootstrapServers }
            };

            int count = 0;
            Action<Message> dh = (Message dr) =>
            {
                Assert.Equal(ErrorCode.NoError, dr.Error.Code);
                Assert.Equal((Partition)0, dr.Partition);
                Assert.Equal(singlePartitionTopic, dr.Topic);
                Assert.True(dr.Offset >= 0);
                Assert.Null(dr.Key);
                Assert.Null(dr.Value);
                Assert.Equal(TimestampType.CreateTime, dr.Timestamp.Type);
                Assert.True(Math.Abs((DateTime.UtcNow - dr.Timestamp.UtcDateTime).TotalMinutes) < 1.0);
                count += 1;
            };

            using (var producer = new Producer(producerConfig))
            {
                producer.Produce(singlePartitionTopic, Partition.Any, null, 0, 0, null, 0, 0, Timestamp.Default, null, dh);
                producer.Produce(singlePartitionTopic, Partition.Any, null, 0, 0, null, 0, 0, Timestamp.Default, null, dh);
                producer.Produce(singlePartitionTopic, 0, null, 0, 0, null, 0, 0, Timestamp.Default, null, dh);
                Assert.Throws<ArgumentException>(() => 
                    producer.Produce(singlePartitionTopic, Partition.Any, null, -123, int.MinValue, null, int.MaxValue, 44, Timestamp.Default, null, dh)
                );
                producer.Flush(TimeSpan.FromSeconds(10));
            }

            Assert.Equal(3, count);
        }
    }
}