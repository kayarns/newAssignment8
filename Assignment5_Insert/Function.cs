using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Assignment5_Insert
{
    [Serializable]
    class Five
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
        private static string tableName = "AssignmentFive";
        public async Task<string> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            Five newFive = JsonConvert.DeserializeObject<Five>(input.Body);


                if(newFive.company == "b" || newFive.company == "B")
                {
                   newFive.rating = newFive.rating / 2.0;
                }

            List<string> listDescrip = new List<string>(newFive.description.Split(' '));
            newFive.lastInstanceOfWord = listDescrip.FindLast(x => x.ToLower().Contains("o"));

            int position = 0;
            int position1 = 0;

            position = newFive.description.LastIndexOf("e");
            position1 = newFive.description.LastIndexOf("o");

            if (position > position1)
            {
                string lastInstanceOf = listDescrip.FindLast(x => x.ToLower().Contains("e"));
                newFive.lastInstanceOfWord = lastInstanceOf.ToLower();
            }
            else if (position1 > position)
            {
                string lastInstanceOf1 = listDescrip.FindLast(x => x.ToLower().Contains("o"));
                newFive.lastInstanceOfWord = lastInstanceOf1.ToLower();
            }
            else if (position == -1)
            {
                string lastInstanceOf1 = listDescrip.FindLast(x => x.ToLower().Contains("o"));
                newFive.lastInstanceOfWord = lastInstanceOf1.ToLower();
            }
            else if (position1 == -1)
            {
                string lastInstanceOf = listDescrip.FindLast(x => x.ToLower().Contains("e"));
                newFive.lastInstanceOfWord = lastInstanceOf.ToLower();
            }

            Table five = Table.LoadTable(client, tableName);


            PutItemOperationConfig config = new PutItemOperationConfig();
            config.ReturnValues = ReturnValues.AllOldAttributes;
            Document result = await five.PutItemAsync(Document.FromJson(JsonConvert.SerializeObject(newFive)), config);

            return input.Body;
        }
    }
}
