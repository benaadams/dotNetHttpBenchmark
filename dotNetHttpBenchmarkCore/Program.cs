﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using RestSharp;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace dotNetHttpBenchmarkCore
{
    //[ShortRunJob]
    //[NativeMemoryProfiler]
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporter]
    //[StopOnFirstError]
    public class HttpBenchmark
    {
        private string _api;
        public HttpBenchmark()
        {
            _api = "http://localhost/SampleApi/";
        }

        [Benchmark]
        public void Get_WebRequest() => ExecuteWebRequest(_api + "SampleGet", string.Empty, method: "GET");

        [Benchmark]
        public void Get_HttpClient() => ExecuteHttpClient(_api + "SampleGet", HttpMethod.Get);
        [Benchmark]
        public void Get_WebClient()
        {
            using (var webClient = new WebClient())
            {
                Stream stream = webClient.OpenRead(_api + "SampleGet");
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                }
            }
        }
        [Benchmark]
        public void Get_RestSharp()
        {
            var restClient = new RestClient(_api + "SampleGet");
            var getRequest = new RestRequest(Method.GET);
            var response = restClient.Execute(getRequest);
        }
        [Benchmark]
        public void Post_WebRequest() => ExecuteWebRequest(_api + "SamplePost", string.Empty);

        [Benchmark]
        public void Post_HttpClient() => ExecuteHttpClient(_api + "SamplePost", HttpMethod.Post);

 
        [Benchmark]
        public void Post_WebClient()
        {
            using (var webClient = new WebClient())
            {
                NameValueCollection nameValueCollection = new NameValueCollection();
                var data = webClient.UploadValues(_api + "SamplePost", "POST", nameValueCollection);
                var responseString = UnicodeEncoding.UTF8.GetString(data);
            }
        }
       
        [Benchmark]
        public void Post_RestSharp()
        {
            var restClient = new RestClient(_api + "SamplePost");
            var getRequest = new RestRequest(Method.POST);
            var response = restClient.Execute(getRequest);
        }
        private void ExecuteWebRequest(string pUrl, string pReq, string method = "POST", string contentType = "application/json", int timeout = -1)
        {
            string result;

            WebRequest request = WebRequest.Create(pUrl);
            request.Method = method;
            request.ContentType = contentType;
            if (timeout > 0)
                request.Timeout = timeout;

            if (method == "POST")
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes(pReq);
                request.ContentLength = requestBytes.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    using (var writer = new StreamWriter(requestStream))
                    {
                        writer.Write(pReq);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                    reader.Close();
                }
                response.Close();
            }
        }

        private void ExecuteHttpClient(string pUrl, HttpMethod httpMethod)
        {
            using (var client = new HttpClient())
            {
                using (var req = new HttpRequestMessage(httpMethod, pUrl))
                {
                    var httpResponseMessage = client.SendAsync(req).Result;
                    var responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HttpBenchmark>();
        }
    }
}
