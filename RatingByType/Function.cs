using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.S3.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RatingByType
{
    public class Item
    {
        public string itemId;
        public string description;
        public int rating;
        public string type;
        public string company;
        public string lastInstanceOfWord;
    }
    public class Stat
    {
        public int count;
        public double averageRating;
    }
    public class Function
    {

        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private string tableName = "RatingsByType";
        public async Task<List<Item>> FunctionHandler(DynamoDBEvent input, ILambdaContext context)
        {
            Table table = Table.LoadTable(client, "RatingsByType");
            List<Item> items = new List<Item>();
            List<DynamoDBEvent.DynamodbStreamRecord> records = (List<DynamoDBEvent.DynamodbStreamRecord>)input.Records;

            int count = 0;
            double currentAvg = 0;
            double total = 0;
            

            if (records.Count > 0)
            {
                DynamoDBEvent.DynamodbStreamRecord record = records[0];
                if (record.EventName.Equals("INSERT"))
                {
                    Document myDoc = Document.FromAttributeMap(record.Dynamodb.NewImage);
                    Item myItem = JsonConvert.DeserializeObject<Item>(myDoc.ToJson());

                    string type = myItem.type;
                    GetItemResponse res = await client.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
                    {
                        {"type", new AttributeValue { S = type} }
                    }
                        );
                    Document myDoc1 = Document.FromAttributeMap(res.Item);
                    Stat myStat = JsonConvert.DeserializeObject<Stat>(myDoc1.ToJson());
                    count = myStat.count;
                    currentAvg = myStat.averageRating;
                    if(count == 0)
                    {
                        total += myItem.rating;
                    }
                    else
                    {
                        if (count >= 2)
                        {
                            total = (myItem.rating + (currentAvg * count)) / (count + 1);
                        }
                        else
                        {
                            total = (myItem.rating + currentAvg) / (count + 1);
                        }
                    }

                    var request = new UpdateItemRequest
                    {
                        TableName = "RatingsByType",
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { "type", new AttributeValue { S = myItem.type } }
                        },
                        AttributeUpdates = new Dictionary<string, AttributeValueUpdate>()
                        {
                            {
                                "count",
                                new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue { N = "1" } }
                            },
                            {
                                "averageRating",
                                new AttributeValueUpdate { Action = "PUT", Value = new AttributeValue { N = total.ToString() } }
                            },
                        },
                    };
                    await client.UpdateItemAsync(request);
                }
            }

            return items;
        }
    }
}
