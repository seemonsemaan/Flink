using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Flink.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Flink.Models;
using Microsoft.AspNetCore.Http;
using RestSharp;

namespace Flink.Controllers
{
    public class HomeController : Controller
    {
        private readonly string baseUrl = "http://localhost:8081/v1";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View(await GetIndexMethod());
        }

        private async Task<object> GetIndexMethod()
        {
            List<JarFile> jarFiles = new List<JarFile>();
            using (HttpClient http = new HttpClient())
            {
                var result = await http.GetAsync(baseUrl + "/jars");
                var jsonString = await result.Content.ReadAsStringAsync();
                JarsResult jars = JsonSerializer.Deserialize<JarsResult>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                jarFiles = jars.files.ToList();
            }

            List<Job> jobs = new List<Job>();
            using (HttpClient http = new HttpClient())
            {
                var result = await http.GetAsync(baseUrl + "/jobs/overview");
                var jsonString = await result.Content.ReadAsStringAsync();
                JobOverviewResult jobsResult = JsonSerializer.Deserialize<JobOverviewResult>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                jobs = jobsResult.jobs.ToList();
            }

            IndexResult res = new IndexResult();
            res.JarFiles = jarFiles;
            res.Jobs = jobs;
            return res;
        }

        public async Task<IActionResult> RunJar(string id)
        {
            if(id == null)
                return NotFound();
            return View(model:id);
        }

        [HttpPost]
        public async Task<IActionResult> RunJob(RunJobParam param)
        {
            using (HttpClient http = new HttpClient())
            {
                StringContent content = new StringContent("", Encoding.UTF8, "application/json");
                if (param.programArgs != null)
                {
                    JobInput temp = new JobInput();
                    temp.programArgs = param.programArgs;
                    var body = JsonSerializer.Serialize(temp);
                    content = new StringContent(body, Encoding.UTF8, "application/json");
                }
                var result = await http.PostAsync(baseUrl + "/jars/" + param.id + "/run", content);
                var jsonString = await result.Content.ReadAsStringAsync();
            }
            
            return View("StdOut", CatLog());
        }

        public async Task<IActionResult> StdOut()
        {

            var logPath = await GetLogPath();
            Console.WriteLine("-------------------------------");
            var bash = "sudo docker cp jobmanager:/opt/flink/log/" + logPath + " /home/seemon/Desktop/";
            Console.WriteLine(bash.Bash());
            Console.WriteLine("-------------------------------");
            return View(model: CatLog());
        }

        private string CatLog()
        {
            return "cat /home/seemon/Desktop/*.out".Bash();
        }

        private async Task<string> GetLogPath()
        {
            string taskMan;
            using (HttpClient http = new HttpClient())
            {
                var result = await http.GetAsync(baseUrl + "/taskmanagers");
                var jsonString = await result.Content.ReadAsStringAsync();
                TaskManagerResult res = JsonSerializer.Deserialize<TaskManagerResult>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                taskMan = res.taskmanagers.ElementAt(0).id;
            }
            
            using (HttpClient http = new HttpClient())
            {
                var result = await http.GetAsync(baseUrl + "/taskmanagers/"+taskMan+"/logs");
                var jsonString = await result.Content.ReadAsStringAsync();
                LogsResult res = JsonSerializer.Deserialize<LogsResult>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var l = res.logs.FirstOrDefault(l => l.name.Contains("taskexecutor") && l.name.Contains("out"));
                if (l == null)
                {
                    return "";
                }

                return l.name;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
            // using (HttpClient http = new HttpClient())
            // {
            //     var content = new MultipartFormDataContent();
            //     content.Headers.ContentType = new MediaTypeHeaderValue("application/x-java-archive");
            //     foreach (var file in files)
            //     {
            //         var filePath = Path.GetTempFileName();
            //         using (var stream = System.IO.File.Create(filePath))
            //         {
            //             // The formFile is the method parameter which type is IFormFile
            //             // Saves the files to the local file system using a file name generated by the app.
            //             await file.CopyToAsync(stream);
            //         }
            //         content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(filePath)), file.Name, Path.GetFileName(filePath));
            //     }
            //     var result = await http.PostAsync(baseUrl + "/jars/upload", content);
            //     var jsonString = await result.Content.ReadAsStringAsync();
            // }

            var client = new RestClient(baseUrl + "/jars/upload");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-java-archive");
            request.AddFile("WordCounttt", "/home/seemon/Desktop/examples/streaming/WordCount.jar");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return await Index();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}