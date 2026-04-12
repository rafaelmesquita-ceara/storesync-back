using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public interface IApiService
{
    void SetApiKey(string apiKey);
    Task<Response> GetAsync(string? url);
    Task<Response> PostAsync(string? url, JsonContent content);
    Task<Response> PutAsync(string? url, JsonContent content);
    Task<Response> PatchAsync(string? url);
    Task<Response> DeleteAsync(string? url);
    Task<byte[]?> DownloadAsync(string? url);
    event Action OnUnauthorized;
}

public class ApiService(HttpClient httpClient) : IApiService
{
    private string _apiKey;
    
    public event Action OnUnauthorized;

    public void SetApiKey(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    private void AddAuthHeader(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_apiKey))
        {
            request.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    private void CheckAuthorization(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            OnUnauthorized?.Invoke();
        }
    }
    
    public async Task<Response> GetAsync(string? url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(request);
        
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        CheckAuthorization(response);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return new Response((int)response.StatusCode, jsonResponse);
    }

    public async Task<Response> PostAsync(string? url, JsonContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeader(request);
        request.Content = content;
        
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        CheckAuthorization(response);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return new Response((int)response.StatusCode, jsonResponse);
    }

    public async Task<Response> PutAsync(string? url, JsonContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        AddAuthHeader(request);
        request.Content = content;
        
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        CheckAuthorization(response);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return new Response((int)response.StatusCode, jsonResponse);
    }

    public Task<Response> PatchAsync(string? url)
    {
        throw new System.NotImplementedException();
    }

    public async Task<Response> DeleteAsync(string? url)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        AddAuthHeader(request);
        
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        CheckAuthorization(response);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return new Response((int)response.StatusCode, jsonResponse);
    }

    public async Task<byte[]?> DownloadAsync(string? url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(request);
        
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        CheckAuthorization(response);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
        return null;
    }
}