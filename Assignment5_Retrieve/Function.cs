using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Newtonsoft.Json;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Assignment5_Retrieve
{
    public class Five
    {
        public string itemId;
        public string description;
        public double rating;
        public string type;
        public string company;
        public string lastInstanceOfWord;
    }
    public class Function
    {

        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private string tableName = "AssignmentFive";
        public async Task<Five> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            string itemId = "";
            Dictionary<string, string> dict = (Dictionary<string, string>)input.QueryStringParameters;
            dict.TryGetValue("itemId", out itemId);
            GetItemResponse res = await client.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
            {
                {"itemId", new AttributeValue {S = itemId } }
            }
            );

            Document myDoc = Document.FromAttributeMap(res.Item);
            Five myFive = JsonConvert.DeserializeObject<Five>(myDoc.ToJson());

            return myFive;
        }
    }
}
