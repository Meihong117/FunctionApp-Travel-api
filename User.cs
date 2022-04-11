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

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string FirstName { get; set; }

    [JsonProperty("familyname")]
    public string LastName { get; set; }


    [JsonProperty("option")]
    public string Option { get; set; }
}