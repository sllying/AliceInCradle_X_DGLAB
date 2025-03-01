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
            var hpMax = default(object);
            var hp = default(object);
            var hpStart = default(int);
            var hpNow1 = default(int);
            var mp = GameObject.FindObjectOfType<PRNoel>();
            var mpMax = GameObject.FindObjectOfType<PRNoel>();
            var mpStart = default(int);
            var mpNow1 = default(int);
            var ep = GameObject.FindObjectOfType<M2MoverPr>();
            var orgasm = GameObject.FindObjectOfType<MgmEggRemove>();
            var prNoel = GameObject.FindObjectOfType<PR>();
            if (FireMode == 0)
            {
                hp = GameObject.FindObjectOfType<M2Attackable>();
                hpMax = GameObject.FindObjectOfType<M2Attackable>();
            }
            if (FireMode == 1)
            {
                hp = GameObject.FindObjectOfType<PRNoel>();
                hpMax = GameObject.FindObjectOfType<PRNoel>();
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
            if (FireMode == 1)
            {
                start++;
                if (start >= end)
                {
                    if (_previousHp == null)
                    {
                        _previousHp = hpStart;
                    }
                    else
                    {
                        int difference = hpNow1 - _previousHp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 20 && difference < 100)
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
                        _previousHp = hpNow1;
                    }
                    if (_previousMp == null)
                    {
                        _previousMp = mpStart;

                    }
                    else
                    {
                        int difference = mpNow1 - _previousMp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 50 && difference < 80)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加蓝");
                            //Log($"mpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference <= -1 && difference > -10 && lowest != 0 && EpFlag == true)
                        {
                            SendStrengthConfigAsync(0, lowest, 0).ConfigureAwait(false);
                            EpFlag = false;
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
                        _previousMp = mpNow1;
                    }
                    start = 0;
                }
            }
            if (FireMode == 0)
            {
                start++;
                if (start >= end)
                {
                    if (_previousHp == null)
                    {
                        _previousHp = hpStart;

                    }
                    else
                    {
                        int difference = hpNow1 - _previousHp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 20 && difference < 100)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加血");
                            //Log($"hpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference < 0 && difference > -100)
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
                        _previousHp = hpNow1;
                    }

                    if (_previousMp == null)
                    {
                        _previousMp = mpStart;

                    }
                    else
                    {
                        int difference = mpNow1 - _previousMp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 20 && difference < 100)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加蓝");
                            //Log($"mpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference <= -1 && difference > -10 && lowest != 0 && EpFlag == true)
                        {
                            SendStrengthConfigAsync(0, lowest, 0).ConfigureAwait(false);
                            EpFlag = false;
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
                        _previousMp = mpNow1;
                    }
                    start = 0;
                }
            }
            if (FireMode == 2)
            {
                start++;
                if (start >= end)
                {
                    if (_previousHp == null)
                    {
                        _previousHp = hpStart;

                    }
                    else
                    {
                        int difference = hpNow1 - _previousHp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 50 && difference < 80)
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
                        _previousHp = hpNow1;
                    }
                    if (_previousMp == null)
                    {
                        _previousMp = mpStart;
                    }
                    else
                    {
                        int difference = mpNow1 - _previousMp.Value;
                        //Log($"差距: {difference}");
                        if (difference > 50 && difference < 80)
                        {
                            int subDGLAB_middle = Math.Abs((int)(difference));
                            SendStrengthConfigAsync(0, 0, subDGLAB_middle).ConfigureAwait(false);
                            //Log("加蓝");
                            //Log($"mpReductionValue: {subDGLAB_middle}");
                        }
                        else if (difference < 0 && difference > -70)
                        {
                            if (difference <= -1 && difference > -10 && lowest != 0 && EpFlag == true)
                            {
                                SendStrengthConfigAsync(0, lowest, 0).ConfigureAwait(false);
                                EpFlag = false;
                            }
                            else if (difference <= -10)
                            {
                                int addDGLAB_middle = Math.Abs((int)Math.Ceiling(difference * mpReductionMultiplier));
                                SendStrengthConfigAsync(0, addDGLAB_middle, 0).ConfigureAwait(false);
                            }
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
                        _previousMp = mpNow1;
                    }



                    start = 0;
                }
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


                using (HttpClient client = new HttpClient())
                {
                    HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Log($"Response body:\n{responseBody}");
                }
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
