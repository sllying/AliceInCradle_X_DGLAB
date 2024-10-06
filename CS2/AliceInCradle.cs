using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using nel;
using UnityEngine;
using UnityEngine.UI;
using m2d;
using XX;
using BepInEx.Configuration;
using HarmonyLib;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.CSharp;
using System.Reflection;
using Newtonsoft.Json.Linq;
using static UnityEngine.UI.ContentSizeFitter;


namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.26.0")]

    public class Main : BaseUnityPlugin
    {
        //代码写的很乱将就这看吧qwq
        //public static void Log(string content)
        //{
        //    string path = "E:/bt/XJ00291/[als]coyote/LOG2.txt";
        //    FileStream fs;
        //    if (File.Exists(path))
        //    {
        //        fs = new FileStream(path, FileMode.Append, FileAccess.Write);
        //    }
        //    else
        //    {
        //        fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        //    }
        //    StreamWriter sw = new StreamWriter(fs);
        //    sw.WriteLine(content);
        //    sw.Close();
        //    fs.Close();
        //}

        private float hpReductionMultiplier;
        private float mpReductionMultiplier;
        private int CheckIntervalMs;
        private int ReductionValue;
        private int FireMode;
        private DateTime _lastCheckTime;
        public void Start()
        {
            string jsonContent = File.ReadAllText("Config.json");
            //Log(jsonContent);
            // 解析JSON内容
            JObject config = JObject.Parse(jsonContent);
            hpReductionMultiplier = (float)config["hpReductionMultiplier"];
            mpReductionMultiplier = (float)config["mpReductionMultiplier"];
            CheckIntervalMs = (int)config["CheckIntervalMs"];
            ReductionValue = (int)config["ReductionValue"];
            FireMode = (int)config["FireMode"];

            //Log($"hpReductionMultiplier: {hpReductionMultiplier}");
            //Log($"hpCheckIntervalMs: {hpCheckIntervalMs}");
            //Log($"hpReductionValue: {hpReductionValue}");
            _lastCheckTime = DateTime.MinValue;

        }


        private int start = 0;
        private int end = 20;
        private int startCD = 0;
        private int endCD = 0;

        private int? _previousHp = null;
        private int? _previousMp = null;
        public void Update()
        {
            if (FireMode == 1)
            {
            //var hp2 = GameObject.FindObjectOfType<PRNoel>();
            //var hpnow = Traverse.Create(hp2).Field("hp").GetValue<int>();

            //startCD++;
            start ++;
            if (start >= end)
            {

                var hp = GameObject.FindObjectOfType<PRNoel>();

                if (hp != null)
                {
                    var hpnow = Traverse.Create(hp).Field("hp").GetValue<int>();
                    //Log(hpnow.ToString());
                }

                var hpmax = GameObject.FindObjectOfType<PRNoel>();

                if (hpmax != null)
                {
                    var hpmaxnow = Traverse.Create(hpmax).Field("maxhp").GetValue<int>();
                    //Log(hpmaxnow.ToString());
                }

                var hpstart = Traverse.Create(hpmax).Field("maxhp").GetValue<int>();
                var hpnow1 = Traverse.Create(hp).Field("hp").GetValue<int>();

                if (_previousHp == null)
                {
                    _previousHp = hpstart;

                }
                else
                {
                    int difference = hpnow1 - _previousHp.Value;
                    //Log($"差距: {difference}");
                    if (difference > 20 && difference <60)
                    {
                        int subDGLAB_middle = Math.Abs((int)(difference));
                        SendStrengthConfigAsync(0,0, subDGLAB_middle).ConfigureAwait(false);
                        //Log("加血");
                        //Log($"hpReductionValue: {subDGLAB_middle}");
                    }
                    else if (difference < 0 && difference >-60)
                    {
                        //int addDGLAB_middle = Math.Abs((int)(difference));
                        int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * hpReductionMultiplier));
                        SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                        //Log("减血");
                        //startCD -= 100;


                    }
                    else
                    {
                        //Log("没变化");
                        SendStrengthConfigAsync(0, 0 ,0).ConfigureAwait(false);
                        DateTime now = DateTime.UtcNow;
                        if (now - _lastCheckTime > TimeSpan.FromMilliseconds(CheckIntervalMs))
                        {
                            _lastCheckTime = now;
                            endCD = 1;
                        }
                    }
                    _previousHp = hpnow1;
                }
                start = 0;
            }



            //if (startCD >= 1)
            //{
            //    endCD = 1;
            //    startCD = 0;
            //}
            }
            if (FireMode == 0)
            {
                //startCD++;
                start++;
                if (start >= end)
                {

                    var hp = GameObject.FindObjectOfType<M2Attackable>();

                    if (hp != null)
                    {
                        var hpnow = Traverse.Create(hp).Field("hp").GetValue<int>();
                        //Log(hpnow.ToString());
                    }

                    var hpmax = GameObject.FindObjectOfType<M2Attackable>();

                    if (hpmax != null)
                    {
                        var hpmaxnow = Traverse.Create(hpmax).Field("maxhp").GetValue<int>();
                        //Log(hpmaxnow.ToString());
                    }

                    var hpstart = Traverse.Create(hpmax).Field("maxhp").GetValue<int>();
                    var hpnow1 = Traverse.Create(hp).Field("hp").GetValue<int>();

                    if (_previousHp == null)
                    {
                        _previousHp = hpstart;

                    }
                    else
                    {
                        int difference = hpnow1 - _previousHp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 20 && difference < 60)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加血");
                            //Log($"hpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference < 0 && difference > -60)
                        {
                            //int addDGLAB_middle = Math.Abs((int)(difference));
                            int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * hpReductionMultiplier));
                            SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                            //Log("减血");
                            //startCD -= 100;


                        }
                        else
                        {
                            //Log("没变化");
                            SendStrengthConfigAsync(0, 0, 0).ConfigureAwait(false);
                            DateTime now = DateTime.UtcNow;
                            if (now - _lastCheckTime > TimeSpan.FromMilliseconds(CheckIntervalMs))
                            {
                                _lastCheckTime = now;
                                endCD = 1;
                            }
                        }
                        _previousHp = hpnow1;
                    }
                    start = 0;
                }



                //if (startCD >= 1)
                //{
                //    endCD = 1;
                //    startCD = 0;
                //}
            }
            if (FireMode == 2)
            {
                //startCD++;
                start++;
                if (start >= end)
                {

                    var hp = GameObject.FindObjectOfType<PRNoel>();

                    if (hp != null)
                    {
                        var hpnow = Traverse.Create(hp).Field("hp").GetValue<int>();
                        //Log(hpnow.ToString());
                    }

                    var hpmax = GameObject.FindObjectOfType<PRNoel>();

                    if (hpmax != null)
                    {
                        var hpmaxnow = Traverse.Create(hpmax).Field("maxhp").GetValue<int>();
                        //Log(hpmaxnow.ToString());
                    }

                    var hpstart = Traverse.Create(hpmax).Field("maxhp").GetValue<int>();
                    var hpnow1 = Traverse.Create(hp).Field("hp").GetValue<int>();

                    if (_previousHp == null)
                    {
                        _previousHp = hpstart;

                    }
                    else
                    {
                        int difference = hpnow1 - _previousHp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 20 && difference < 60)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加血");
                            //Log($"hpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference < 0 && difference > -60)
                        {
                            //int addDGLAB_middle = Math.Abs((int)(difference));
                            int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * hpReductionMultiplier));
                            SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                            //Log("减血");
                            //startCD -= 100;


                        }
                        else
                        {
                            //Log("没变化");
                            SendStrengthConfigAsync(0, 0, 0).ConfigureAwait(false);
                            DateTime now = DateTime.UtcNow;
                            if (now - _lastCheckTime > TimeSpan.FromMilliseconds(CheckIntervalMs))
                            {
                                _lastCheckTime = now;
                                endCD = 1;
                            }
                        }
                        _previousHp = hpnow1;
                    }


                    var mp = GameObject.FindObjectOfType<PRNoel>();

                    if (mp != null)
                    {
                        var mpnow = Traverse.Create(mp).Field("mp").GetValue<int>();
                        //Log(mpnow.ToString());
                    }

                    var mpmax = GameObject.FindObjectOfType<PRNoel>();

                    if (mpmax != null)
                    {
                        var mpmaxnow = Traverse.Create(mpmax).Field("maxmp").GetValue<int>();
                        //Log(mpmaxnow.ToString());
                    }

                    var mpstart = Traverse.Create(mpmax).Field("maxhp").GetValue<int>();
                    var mpnow1 = Traverse.Create(mp).Field("mp").GetValue<int>();

                    if (_previousMp == null)
                    {
                        _previousMp = mpstart;

                    }
                    else
                    {
                        int difference = mpnow1 - _previousMp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 20 && difference < 60)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加蓝");
                            //Log($"mpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference < 0 && difference > -60)
                        {
                            //int addDGLAB_middle = Math.Abs((int)(difference));
                            int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * mpReductionMultiplier));
                            SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                            //Log("减蓝");
                            //startCD -= 100;


                        }
                        else
                        {
                            //Log("没变化");
                            SendStrengthConfigAsync(0, 0, 0).ConfigureAwait(false);
                            DateTime now = DateTime.UtcNow;
                            if (now - _lastCheckTime > TimeSpan.FromMilliseconds(CheckIntervalMs))
                            {
                                _lastCheckTime = now;
                                endCD = 1;
                            }
                        }
                        _previousMp = mpnow1;
                    }



                    start = 0;
                }



                //if (startCD >= 1)
                //{
                //    endCD = 1;
                //    startCD = 0;
                //}
            }

        }
        private async Task SendStrengthConfigAsync(int setDGLAB, int addDGLAB, int subDGLAB)
        {
            string baseUrl = "http://127.0.0.1:8920/";
            string clientId = "all";
            string url = $"{baseUrl}api/game/{clientId}/strength_config";

            try
            {
                    // 构建请求体，根据参数包含不同的属性
                    string jsonContent;
                    if (setDGLAB == 0 && addDGLAB == 0 && subDGLAB == 0 && endCD == 1)
                    {
                    jsonContent = $@"
                    {{
                        ""strength"": {{
                        ""sub"": {ReductionValue}
                        }}
                    }}";

                    endCD = 0;
                }
                    else if (setDGLAB != 0 && addDGLAB == 0 && subDGLAB == 0)
                    {
                        jsonContent = $@"
                    {{
                        ""strength"": {{
                            ""set"": {setDGLAB}
                        }}
                    }}";
                    }
                    else if (setDGLAB == 0 && addDGLAB != 0 && subDGLAB == 0)
                    {
                        jsonContent = $@"
                    {{
                        ""strength"": {{
                            ""add"": {addDGLAB}
                        }}
                    }}";
                    }
                    else if (setDGLAB == 0 && addDGLAB == 0 && subDGLAB != 0) 
                    {
                        jsonContent = $@"
                    {{
                        ""strength"": {{
                            ""sub"": {subDGLAB}
                        }}
                    }}";
                    }
                    else
                {
                    jsonContent = $@"
                    {{
                        ""strength"": {{
                            ""add"": {addDGLAB}
                        }}
                    }}";
                }


                using (HttpClient client = new HttpClient())
                    {
                        HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync(url, content);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        //Log($"Response body:\n{responseBody}");
                    }
                }
                catch (Exception e)
                {
                    //Log("Exception Caught!");
                    //Log($"Message: {e.Message}");
                }
            }
        //static readonly HttpClient client = new HttpClient();

        //static async Task Main(string[] args)
        //{
        //    string url = "http://127.0.0.1:8921/api/game/all";

        //    try
        //    {
        //        // 发送GET请求
        //        HttpResponseMessage response = await client.GetAsync(url);
        //        response.EnsureSuccessStatusCode();

        //        // 读取响应内容
        //        string responseBody = await response.Content.ReadAsStringAsync();

        //        // 输出响应内容
        //        Log(responseBody);

        //        // 解析响应内容
        //        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
        //        int limit = data.clientStrength.limit;
        //        Log($"Limit: {limit}");
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        Console.WriteLine("\nException Caught!");
        //        Console.WriteLine("Message :{0} ", e.Message);
        //    }
        //}

        public void OnGUI()
        {
            /*GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in objects)
            {
                //Log(obj.name);

                Vector3 pos = obj.transform.position;
                Camera camera = Camera.main;
                Vector3 ScreenPos = camera.WorldToScreenPoint(pos);
                if (ScreenPos.z >= 0)
                {
                    UnityEngine.GUI.Label(new Rect(ScreenPos.x, Screen.height - ScreenPos.y, 999, 999), obj.name);
                }
            }
            */
        }
    }
}
