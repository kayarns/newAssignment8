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
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace getStatsForType
{
    public class Stat
    {
        public int count;
        public double averageRating;
    }
    public class Function
    {

        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private string tableName = "RatingsByType";
        public async Task<Stat> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            string type = "";
            Dictionary<string, string> dict = (Dictionary<string, string>)input.QueryStringParameters;
            dict.TryGetValue("type", out type);
            GetItemResponse res = await client.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
            {
                {"type", new AttributeValue {S = type} }
            });

            Document myDoc = Document.FromAttributeMap(res.Item);
            Stat myStat = JsonConvert.DeserializeObject<Stat>(myDoc.ToJson());

            return myStat;
        }
    }
}
