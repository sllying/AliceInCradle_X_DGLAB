using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using nel;
using UnityEngine;
using m2d;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using nel.mgm;


namespace AliceInCradle
{
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.27.0")]

    public class Main : BaseUnityPlugin
    {
        ////代码写的很乱将就这看吧qwq
        //public static void Log(string content)
        //{
        //    string path = "LOG.txt";
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
        private static readonly HttpClient _httpClient = new HttpClient();
        private M2Attackable _hpComponentAttackable;
        private PRNoel _prNoelComponent;
        private M2MoverPr _epComponent;
        private PR _prComponent;
        private float hpReductionMultiplier;
        private float mpReductionMultiplier;
        private float epReductionMultiplier;
        private int CheckIntervalMs;
        private int ReductionValue;
        private int FireMode;
        private DateTime _lastCheckTime;
        private DateTime _lastCheckTime2;
        private int lowest;
        private int Hero;
        private int holdMs;
        private int eroH;
        public void Start()
        {
            string jsonContent = File.ReadAllText("Config.json");
            //Log(jsonContent);
            // 解析JSON内容
            JObject config = JObject.Parse(jsonContent);
            hpReductionMultiplier = (float)config["hpReductionMultiplier"];
            mpReductionMultiplier = (float)config["mpReductionMultiplier"];
            epReductionMultiplier = (float)config["epReductionMultiplier"];
            CheckIntervalMs = (int)config["CheckIntervalMs"];
            ReductionValue = (int)config["ReductionValue"];
            FireMode = (int)config["FireMode"];
            lowest = (int)config["lowest"];
            Hero = (int)config["Hero"];
            holdMs = (int)config["holdMs"];
            eroH = (int)config["eroH"];
            //Log($"hpReductionMultiplier: {hpReductionMultiplier}");
            //Log($"hpCheckIntervalMs: {hpCheckIntervalMs}");
            //Log($"hpReductionValue: {hpReductionValue}");
            _lastCheckTime = DateTime.MinValue;
            _lastCheckTime2 = DateTime.MinValue;

            CacheGameComponents();
        }
        private void CacheGameComponents()
        {
            _hpComponentAttackable = FindObjectOfType<M2Attackable>();
            _prNoelComponent = FindObjectOfType<PRNoel>(); // PRNoel 同时用于 HP(模式1/2) 和 MP
            _epComponent = FindObjectOfType<M2MoverPr>();
            _prComponent = FindObjectOfType<PR>();
        }

        private int start = 0;
        private int end = 0;
        private int endCD = 0;
        private bool EpFlag = false;
        private bool OrFlag = false;
        private bool OrFlag2 = false;
        private int? _previousHp = null;
        private int? _previousMp = null;
        private int? _previousEp = null;
        private int? _previousOr = null;
        public void Update()
        {
            if (_prNoelComponent == null || _epComponent == null || _prComponent == null)
            {
                 CacheGameComponents();
                return;
            }

            var hpMax = default(object);
            var hp = default(object);
            var hpStart = default(int);
            var hpNow1 = default(int);
            var mp = _prNoelComponent;
            var mpMax = _prNoelComponent;
            var mpStart = default(int);
            var mpNow1 = default(int);
            var ep = _epComponent;
            //var orgasm = GameObject.FindObjectOfType<MgmEggRemove>();
            var prNoel = _prComponent;
            if (FireMode == 0)
            {
                hp = _hpComponentAttackable;
                hpMax = _hpComponentAttackable;
            }
            if (FireMode == 1)
            {
                hp = _prNoelComponent;
                hpMax = _prNoelComponent;
            }
            if (hp != null)
            {
                hpNow1 = Traverse.Create(hp).Field("hp").GetValue<int>();
                //Log(hpNow1.ToString());
            }
            if (hpMax != null)
            {
                hpStart = Traverse.Create(hpMax).Field("maxhp").GetValue<int>();
                //Log(hpStart.ToString());
            }
            if (mp != null)
            {
                mpNow1 = Traverse.Create(mp).Field("mp").GetValue<int>();
                //Log(mpNow1.ToString());
            }
            if (mpMax != null)
            {
                mpStart = Traverse.Create(mpMax).Field("maxmp").GetValue<int>();
                //Log(mpStart.ToString());
            }
            if (prNoel != null)
            {
                var epManager = prNoel.EpCon;
                if (epManager != null)
                {
                    int multiple = epManager.getOrgasmedTotal();
                    //Log($"值: {multiple}");

                    if (_previousOr == null)
                    {
                        _previousOr = multiple;
                    }
                    else
                    {
                        int difference = multiple - _previousOr.Value;
                        if (difference > 0)
                        {
                            int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * Hero * 1.0));
                            SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                            OrFlag = true;
                            _lastCheckTime2 = DateTime.UtcNow;
                        }

                        _previousOr = multiple;
                    }
                }
            }
            if (ep != null)
            {
                var epNow = Traverse.Create(ep).Field("ep").GetValue<int>();
                if (_previousEp == null)
                {
                    _previousEp = epNow;
                }
                else
                {
                    int difference = epNow - _previousEp.Value;
                    if (difference > 0 && difference < 100)
                    {
                        EpFlag = true;
                        int addDGLAB_middle = Math.Abs((int)Math.Round(difference * epReductionMultiplier / 10));
                        SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                    }

                }
                _previousEp = epNow;

            }
            if (OrFlag)
            {
                DateTime now1 = DateTime.UtcNow;
                if (now1 - _lastCheckTime2 > TimeSpan.FromMilliseconds(holdMs))
                {
                    OrFlag2 = true;
                }
                if (OrFlag2 == true)
                {
                    OrFlag = false;
                    OrFlag2 = false;
                    SendStrengthConfigAsync(0, 0, eroH).ConfigureAwait(false);
                }
            }
            start++;
            if (start >= end) 
            {
                // --- HP 变化处理 ---
                if (_previousHp == null)
                {
                    _previousHp = hpStart;
                }
                else
                {
                    int difference = hpNow1 - _previousHp.Value;
                    bool hpChanged = false; // 标记HP是否有触发事件

                    switch (FireMode)
                    {
                        case 0: // Mode 0 的 HP 逻辑
                        case 1://TODO以后不同的HP处理方法
                        case 2:
                            if (difference > 10 && difference < 200)
                            {
                                //TODO
                                //没有倍率
                                SendStrengthConfigAsync(0, 0, Math.Abs(difference)).ConfigureAwait(false);
                                hpChanged = true;
                            }
                            else if (difference < 0 && difference > -150)
                            {
                                int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * hpReductionMultiplier));
                                SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                                hpChanged = true;
                            }
                            break;

                    }

                    if (!hpChanged) // 如果HP没有发生需要处理的变化，则执行通用无变化逻辑
                    {
                        SendStrengthConfigAsync(0, 0, 0).ConfigureAwait(false);
                        DateTime now = DateTime.UtcNow;
                        if (now - _lastCheckTime > TimeSpan.FromMilliseconds(CheckIntervalMs))
                        {
                            _lastCheckTime = now;
                            endCD = 1;
                        }
                    }
                    _previousHp = hpNow1;
                }

                // --- MP 变化处理 ---
                if (_previousMp == null)
                {
                    _previousMp = mpStart;
                }
                else
                {
                    int difference = mpNow1 - _previousMp.Value;
                    bool mpChanged = false; // 标记MP是否有触发事件

                    switch (FireMode)
                    {

                        case 0: // Mode 0 的 MP 逻辑
                        case 1: //mp增加时减少强度
                            if (difference > 20 && difference < 100)
                            {
                                SendStrengthConfigAsync(0, 0, Math.Abs(difference)).ConfigureAwait(false);
                                mpChanged = true;
                            }
                            else if (difference <= -1 && difference > -10 && lowest != 0 && EpFlag)
                            {
                                SendStrengthConfigAsync(0, lowest, 0).ConfigureAwait(false);
                                EpFlag = false;
                                mpChanged = true;
                            }
                            break;

                        case 2:
                            if (difference > 20 && difference < 100)
                            {
                                SendStrengthConfigAsync(0, 0, Math.Abs(difference)).ConfigureAwait(false);
                                mpChanged = true;
                            }
                            else if (difference < 0 && difference > -70)
                            {
                                if (difference <= -1 && difference > -10 && lowest != 0 && EpFlag)
                                {
                                    SendStrengthConfigAsync(0, lowest, 0).ConfigureAwait(false);
                                    EpFlag = false;
                                }
                                else if (difference <= -10) // 注意这里是 <= -10
                                {
                                    int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * mpReductionMultiplier));
                                    SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                                }
                                mpChanged = true;
                            }
                            break;
                    }

                    if (!mpChanged) // 如果MP没有发生需要处理的变化
                    {
                        SendStrengthConfigAsync(0, 0, 0).ConfigureAwait(false);
                        DateTime now = DateTime.UtcNow;
                        if (now - _lastCheckTime > TimeSpan.FromMilliseconds(CheckIntervalMs))
                        {
                            _lastCheckTime = now;
                            endCD = 1;
                        }
                    }
                    _previousMp = mpNow1;
                }

                start = 0; // 在处理完所有逻辑后重置计时器
            }

        }

        private async Task SendStrengthConfigAsync(int setDGLAB, int addDGLAB, int subDGLAB)
        {
            string baseUrl = "http://127.0.0.1:8920/";
            string clientId = "all";
            string url = $"{baseUrl}api/game/{clientId}/strength_config";

            try
            {
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


                
                    HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Log($"Response body:\n{responseBody}");

            }
            catch (Exception)
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
