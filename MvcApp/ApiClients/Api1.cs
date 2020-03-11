using MvcApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MvcApp {
    public class Api1 {

        private readonly HttpClient _client;
        public Api1(IHttpClientFactory httpClientFactory) {
            _client = httpClientFactory.CreateClient("Api1");
        }

        public async Task<IEnumerable<ClaimViewModel>> GetClaimsFromApi1() {
            var response = await _client.GetAsync("identity");
            var content = await response.Content.ReadAsStringAsync();
            var claims = JsonSerializer.Deserialize<List<ClaimViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return claims;
        }


        public async Task<string> GetGetPolicy() {
            var response = await _client.GetAsync("identity/get/policy");
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                var content = await response.Content.ReadAsStringAsync();
                var action = JsonSerializer.Deserialize<ActionViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Action;
                return action;
            } else
                return response.StatusCode.ToString();
        }
        public async Task<string> GetEditPolicy() {
            var response = await _client.GetAsync("identity/edit/policy");
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                var content = await response.Content.ReadAsStringAsync();
                var action = JsonSerializer.Deserialize<ActionViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Action;
                return action;
            } else
                return response.StatusCode.ToString();
        }
        public async Task<string> GetDeletePolicy() {
            var response = await _client.GetAsync("identity/delete/policy");
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                var content = await response.Content.ReadAsStringAsync();
                var action = JsonSerializer.Deserialize<ActionViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Action;
                return action;
            } else
                return response.StatusCode.ToString();
        }

        public async Task<string> GetGetRole() {
            var response = await _client.GetAsync("identity/get/role");
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                var content = await response.Content.ReadAsStringAsync();
                var action = JsonSerializer.Deserialize<ActionViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Action;
                return action;
            } else
                return response.StatusCode.ToString();
        }
        public async Task<string> GetEditRole() {
            var response = await _client.GetAsync("identity/edit/role");
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                var content = await response.Content.ReadAsStringAsync();
                var action = JsonSerializer.Deserialize<ActionViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Action;
                return action;
            } else
                return response.StatusCode.ToString();
        }
        public async Task<string> GetDeleteRole() {
            var response = await _client.GetAsync("identity/delete/role");
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                var content = await response.Content.ReadAsStringAsync();
                var action = JsonSerializer.Deserialize<ActionViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Action;
                return action;
            } else
                return response.StatusCode.ToString();
        }

    }
}
