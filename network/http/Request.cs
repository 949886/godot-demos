// Created by LunarEclipse on 2024-09-26 19:09.

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Luna.Network;

public class Request
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("http://127.0.0.1:8787/"),
        DefaultRequestHeaders = {
            Accept = { new("application/json") },
            Authorization = new("Bearer", "test"),
        },
    };
        
    public static Task<T> Get<T>(string path)
    {
        return _client.GetFromJsonAsync<T>(path);
        // var response = _client.GetAsync(path);
        // return response.ContinueWith(task =>
        // {
        //     var result = task.Result;
        //     if (result.IsSuccessStatusCode)
        //     {
        //         var content = result.Content.ReadAsStringAsync();
        //         return content.ContinueWith(task => JsonConvert.DeserializeObject<T>(task.Result)).Result;
        //     }
        //     else throw new Exception($"Request {path} failed: {result.StatusCode} {result.ReasonPhrase}");
        // });
    }
}
