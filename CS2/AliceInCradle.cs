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
    [BepInPlugin("AliceInCradle.DGLAB", "Main", "0.28.0")]

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
        private static readonly HttpClient _httpClient = new HttpClient();//HttpClient实例
        private M2Attackable _hpComponentAttackable;//hp组件
        private PRNoel _prNoelComponent;//hp和mp组件
        private M2MoverPr _epComponent;//体力组件
        private PR _prComponent;//高潮组件
        private float hpReductionMultiplier;//hp减少时增加强度倍率
        private float mpReductionMultiplier;//mp减少时增加强度倍率
        private float epReductionMultiplier;//ep减少时增加强度倍率
        private int CheckIntervalMs;//间隔检测
        private int ReductionValue;//每次减少强度
        private int FireMode;//开火模式
        private DateTime _strengthReductionTimer;//间隔检测
        private DateTime _orgasmDurationTimer;//高潮持续时间检测
        private int lowest;//mp减少时增加强度最低值
        private int Hero;//每次高潮增加强度
        private int holdMs;//高潮持续时间
        private int eroH;//高潮恢复强度
        private int maxChange;//变化过大不处理
        private int DGLabLimit = 20;//强度默认20
        private const int addChangeLimit = -1000;//增益最大值
        private FileSystemWatcher _configWatcher;//配置文件监视器
        private string _configPath = "Config.json";//配置文件路径
        private object _configLock = new object();//配置文件锁
        public void Start()
        {
            // 初始化配置
            LoadConfig();

            // 设置文件监视器
            SetupConfigWatcher();

            // 缓存游戏必须组件
            CacheGameComponents();
        }
        // 缓存游戏组件
        private void CacheGameComponents()
        {
            _hpComponentAttackable = FindObjectOfType<M2Attackable>();
            _prNoelComponent = FindObjectOfType<PRNoel>(); // PRNoel 同时用于 HP(模式1/2) 和 MP
            _epComponent = FindObjectOfType<M2MoverPr>();
            _prComponent = FindObjectOfType<PR>();
        }
        // 设置配置文件监视器
        private void SetupConfigWatcher()
        {
            _configWatcher = new FileSystemWatcher();
            _configWatcher.Path = Path.GetDirectoryName(Path.GetFullPath(_configPath));
            _configWatcher.Filter = Path.GetFileName(_configPath);
            _configWatcher.NotifyFilter = NotifyFilters.LastWrite;

            _configWatcher.Changed += OnConfigFileChanged;
            _configWatcher.EnableRaisingEvents = true;
        }
        // 加载配置文件
        private void LoadConfig()
        {
            lock (_configLock)
            {
                string jsonContent = File.ReadAllText(_configPath);
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
                maxChange = (int)config["maxChange"];
            }
        }
        // 配置文件变更处理
        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            // 添加短暂延迟确保文件写入完成
            Task.Delay(100).ContinueWith(_ =>
            {
                try
                {
                    LoadConfig();
                    Debug.Log("Configuration reloaded successfully");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reloading configuration: {ex.Message}");
                }
            });
        }
        // 清理资源
        private void OnDestroy()
        {
            if (_configWatcher != null)
            {
                _configWatcher.EnableRaisingEvents = false;
                _configWatcher.Dispose();
            }
        }


        
        private int endCD = 0;//是否进入冷却
        private bool EpFlag = false;//体力增加标记
        private bool OrFlag = false;//高潮标记
        private bool OrFlag2 = false;//高潮标记2
        private int? _previousHp = null;//上次hp
        private int? _previousMp = null;//上次mp
        private int? _previousEp = null;//上次ep
        private int? _previousOr = null;//上次高潮次数
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
            if (FireMode == 1 || FireMode == 2)
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
                            _orgasmDurationTimer = DateTime.UtcNow;
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
                    if (difference > 0 && difference < maxChange)
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
                if (now1 - _orgasmDurationTimer > TimeSpan.FromMilliseconds(holdMs))
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
                        case 1: //TODO以后不同的HP处理方法
                        case 2:
                            if (difference > 10 && difference < maxChange)
                            {
                                //TODO
                                //增加血量是根据增量减少强度 没有倍率
                                SendStrengthConfigAsync(0, 0, Math.Abs(difference)).ConfigureAwait(false);
                                hpChanged = true;
                            }
                            else if (difference < 0 && difference >  addChangeLimit)
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
                        if (now - _strengthReductionTimer > TimeSpan.FromMilliseconds(CheckIntervalMs))
                        {
                            _strengthReductionTimer = now;
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
                        if (difference > 20 && difference < maxChange)
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
                            if (difference > 20 && difference < maxChange)
                            {
                                SendStrengthConfigAsync(0, 0, Math.Abs(difference)).ConfigureAwait(false);
                                mpChanged = true;
                            }
                            else if (difference < 0 && difference >  addChangeLimit)
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
                        if (now - _strengthReductionTimer > TimeSpan.FromMilliseconds(CheckIntervalMs))
                        {
                            _strengthReductionTimer = now;
                            endCD = 1;
                        }
                    }
                    _previousMp = mpNow1;
                }
            }

        const string baseUrl = "http://127.0.0.1:8920/";
        const string clientId = "all";

        private async Task SendStrengthConfigAsync(int setDGLAB, int addDGLAB, int subDGLAB)
        {
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
        static readonly HttpClient client = new HttpClient();

        //TODO 查询当前强度上限
        private async Task QueryStrengthConfig(string[] args)
        {
            string url = baseUrl + "api/game/" + clientId;

            try
            {
                // 发送GET请求
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // 读取响应内容
                string responseBody = await response.Content.ReadAsStringAsync();
                //Log(responseBody);

                // 解析响应内容 获取范围
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                DGLabLimit = data.clientStrength.limit;
                //Log($"Limit: {limit}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

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
