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
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Net;
using System.Linq;
using Microsoft.Extensions.Primitives;



namespace Estelle.Function
{
    public static class PostUser
    {
        [FunctionName("PostUser")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "postuser")] HttpRequest req,
            [CosmosDB(databaseName: "DB", collectionName: "db-container", ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<dynamic> documentsOut, ILogger log)
        {
            string id = req.Query["id"];
            string name = req.Query["name"];
            string familyname = req.Query["familyname"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            name = name ?? data?.name;
            familyname = familyname ?? data?.familyname;
            id = id ?? data?.id;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(familyname) && !string.IsNullOrEmpty(id))
            {
                await documentsOut.AddAsync(new
                {
                    id = id,
                    name = name,
                    familyname = familyname
                });
            }
            string responseMessage = (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(familyname))
                ? "This HTTP triggered function executed successfully. Pass a id/name/familyname in the query string or in the request body for a personalized response."
                : $"Succeed";

            return new OkObjectResult(responseMessage);
        }
    }

    public static class GetAllUser
    {
        [FunctionName("GetAllUser")]
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,
            [CosmosDB("DB", "db-container", ConnectionStringSetting = "CosmosDbConnectionString", SqlQuery = "SELECT * FROM c")] IEnumerable<User> Result, ILogger log)
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{id}")] HttpRequest req,
            [CosmosDB("DB", "db-container", ConnectionStringSetting = "CosmosDbConnectionString", SqlQuery = "SELECT * FROM c WHERE c.id={id}")] IEnumerable<User> Result, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.->get");

            List<User> newUser = new List<User>();

            foreach (User user in Result)
            {
                newUser.Add(user);
            }
            var jsonString1 = Newtonsoft.Json.JsonConvert.SerializeObject(newUser);
            return jsonString1;
        }
    }

    public static class DeleteUser
    {
        [FunctionName("DeleteUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "deleteuser/{id}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDbConnectionString", SqlQuery = "SELECT * FROM c WHERE c.id={id}")] DocumentClient client, ILogger log, string id)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            var collectionUri = UriFactory.CreateDocumentCollectionUri("DB", "db-container");
            // Console.WriteLine(id);
            var document = client.CreateDocumentQuery(collectionUri, option).Where(t => t.Id == id)
                    .AsEnumerable().FirstOrDefault();
            // Console.WriteLine(document);

            if (document == null)
            {
                return new NotFoundResult();
            }
            await client.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(document.Id) });
            return new OkResult();
        }
    }

    public static class UpdateUser
    {
        [FunctionName("UpdateUser")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updateuser/{id}")] HttpRequest req, [CosmosDB(ConnectionStringSetting = "CosmosDbConnectionString", SqlQuery = "SELECT * FROM c WHERE c.id={id}")] DocumentClient client, ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // Console.WriteLine(id);

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            var collectionUri = UriFactory.CreateDocumentCollectionUri("DB", "db-container");

            var document = client.CreateDocumentQuery(collectionUri, option).Where(t => t.Id == id).AsEnumerable().FirstOrDefault();

            if (document == null || id != document.Id)
            {
                return new NotFoundResult();
            }
            // Console.WriteLine(document.Id);
            document.SetPropertyValue("id", data.id);
            document.SetPropertyValue("name", data.name);
            document.SetPropertyValue("familyname", data.familyname);

            await client.ReplaceDocumentAsync(document);

            return new OkResult();
        }
    }



}

