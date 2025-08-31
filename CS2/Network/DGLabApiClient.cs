// DGLabApiClient.cs
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AliceInCradle
{
    public class DGLabApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly BepInEx.Logging.ManualLogSource _logger;
        private const string BASE_URL = "http://127.0.0.1:8920/";
        private const string CLIENT_ID = "all";

        public DGLabApiClient(ManualLogSource logger)
        {
            _logger = logger;
        }

        public async Task SendStrengthUpdateAsync(int set = 0, int add = 0, int sub = 0)
        {
            // 如果没有任何操作，直接返回
            if (set == 0 && add == 0 && sub == 0) return;

            string url = $"{BASE_URL}api/game/{CLIENT_ID}/strength_config";

            JObject strengthBody = new JObject();
            if (set != 0) strengthBody["set"] = set;
            if (add != 0) strengthBody["add"] = add;
            if (sub != 0) strengthBody["sub"] = sub;

            JObject payload = new JObject
            {
                ["strength"] = strengthBody
            };

            string jsonContent = payload.ToString();

            try
            {
                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInfo($"成功向 DGLAB 发送强度更新: set={set}, add={add}, sub={sub}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DGLabApiClient] 发送强度更新失败: {ex.Message}");
            }
        }

        public async Task<int> QueryStrengthLimitAsync()
        {
            string url = $"{BASE_URL}api/game/{CLIENT_ID}";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                return data.clientStrength.limit;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DGLabApiClient] Failed to query strength limit: {ex.Message}");
                return 20; // 返回一个默认值
            }
        }
    }
}