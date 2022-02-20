using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Estelle.Function
{
    public static class PostUser
    {
        [FunctionName("PostUser")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "update", Route = "postuser")] HttpRequest req,
            [CosmosDB(databaseName: "DB", collectionName: "db-container",
            ConnectionStringSetting = "CosmosDbConnectionString"
            )]IAsyncCollector<dynamic> documentsOut,
            ILogger log)
        {
            string name = req.Query["name"];
            string familyname = req.Query["familyname"];
            string id = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            familyname = familyname ?? data?.familyname;
            id = id ?? data?.id;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(familyname) && !string.IsNullOrEmpty(id))
            {
                // Add a JSON document to the output container.
                await documentsOut.AddAsync(new
                {
                    id = id,
                    name = name,
                    familyname = familyname
                });
            }
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Succeed";

            return new OkObjectResult(responseMessage);
        }
    }

    public static class GetAllUser
    {
        [FunctionName("GetAllUser")]
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "users")]HttpRequest req,
            [CosmosDB("DB", "db-container",
                ConnectionStringSetting = "CosmosDbConnectionString",
                SqlQuery = "SELECT * FROM c")]
                IEnumerable<User> Result,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.->getall");

            //var
            List<User> userList = new List<User>();
            // Console.WriteLine(Result);
            foreach (User user in Result)
            {
                userList.Add(user);
            }
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(userList);
            return jsonString;
        }
    }

    public static class GetSpecificUser
    {
        [FunctionName("GetSpecificUser")]
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "user/{id}")]HttpRequest req,
            [CosmosDB("DB", "db-container",
                ConnectionStringSetting = "CosmosDbConnectionString",
                SqlQuery = "SELECT * FROM c WHERE c.id={id}")]
                IEnumerable<User> Result,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.->get");

            User newUser = new User();

            foreach (User user in Result)
            {
                newUser = user;
            }
            var jsonString1 = Newtonsoft.Json.JsonConvert.SerializeObject(newUser);
            return jsonString1;
        }
    }


}
