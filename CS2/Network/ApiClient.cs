using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AliceInCradle.Config;
using AliceInCradle.Network;
using Newtonsoft.Json;
using UnityEngine;

public class ApiClient : MonoBehaviour
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://127.0.0.1:8920/";
    private const string ClientId = "all";
    private readonly ConfigManager _config;

    private int _endCD = 0;

    public ApiClient(ConfigManager configManager)
    {
        _httpClient = new HttpClient();
        _config = configManager ?? throw new ArgumentNullException(nameof(configManager));
    }

    private int GetReductionValue()
    {
        return _config?.Settings?.ReductionValue ?? 0;
    }

    public void SetEndCD(int value)
    {
        _endCD = value;
    }

    public async Task SendStrengthConfigAsync(StrengthConfig config)
    {
        string url = $"{BaseUrl}api/game/{ClientId}/strength_config";
        try
        {
            string jsonContent;
            int setDGLAB = config.Set ?? 0;
            int addDGLAB = config.Add ?? 0;
            int subDGLAB = config.Sub ?? 0;

            if (setDGLAB == 0 && addDGLAB == 0 && subDGLAB == 0 && _endCD == 1)
            {
                jsonContent = JsonConvert.SerializeObject(new
                {
                    strength = new { sub = GetReductionValue() }
                });
                _endCD = 0;
            }
            else if (setDGLAB != 0 && addDGLAB == 0 && subDGLAB == 0)
            {
                jsonContent = JsonConvert.SerializeObject(new
                {
                    strength = new { set = setDGLAB }
                });
            }
            else if (setDGLAB == 0 && addDGLAB != 0 && subDGLAB == 0)
            {
                jsonContent = JsonConvert.SerializeObject(new
                {
                    strength = new { add = addDGLAB }
                });
            }
            else if (setDGLAB == 0 && addDGLAB == 0 && subDGLAB != 0)
            {
                jsonContent = JsonConvert.SerializeObject(new
                {
                    strength = new { sub = subDGLAB }
                });
            }
            else if (addDGLAB != 0)
            {
                jsonContent = JsonConvert.SerializeObject(new
                {
                    strength = new { add = addDGLAB }
                });
            }
            else
            {
                jsonContent = JsonConvert.SerializeObject(new { });
            }

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send strength config: {ex.Message}");
        }
    }
}
