# AliceInCradle X DGLAB

> 当厨圣是要付出代价的，你们有没有考虑过诺艾儿的感受？现在惩罚你来体验下吧！<br>
> （这真的是惩罚吗？）

⚠️ **注意**：请不要将游戏放在中文目录下。

---

## 项目简介

**爱丽丝的摇篮 X 郊狼** 是一个基于 [BepInEx](https://github.com/BepInEx/BepInEx) 开发的 Unity MOD。<br>使用了 [DG-Lab-Coyote-Game-Hub](https://github.com/hyperzlib/DG-Lab-Coyote-Game-Hub) 提供的 API完成游戏AliceInCradle与郊狼连接。

---

## 使用教程

### MOD 安装教程

1. 将压缩包内 **AliceInCradle_ver028** 文件夹的内容复制到本地游戏的 **AliceInCradle_ver028** 文件夹中。
2. [点击这里下载 MOD](https://github.com/sllying/AliceInCradle_X_DGLAB/releases)。

---

### 链接郊狼教程

1. 下载 [DG-Lab-Coyote-Game-Hub](https://github.com/hyperzlib/DG-Lab-Coyote-Game-Hub)。
2. 启动程序，并扫码与郊狼链接（注意需确保在同一网络环境下）。

---

### 启动游戏

点击启动输出即可。

🎥 **视频教程**：  
[哔哩哔哩教程](https://www.bilibili.com/video/BV1ud1iY3Ei6)

---

## 构建方法

### 开发环境

请确保已安装以下工具：
- **Microsoft Visual Studio** (版本 2022)；
- .NET Framework 4.7.2 开发工具包;

### 构建步骤

1. **加载解决方案**  
   使用 Visual Studio 打开项目根目录下的 [`AliceInCradleXDGLAB.sln`](https://github.com/sllying/AliceInCradle_X_DGLAB/blob/main/AliceInCradleXDGLAB.sln)。

2. **选择构建配置**  
   - 输出路径为 `（你的游戏位置）\AliceInCradle_ver028\BepInEx\plugins\`；

3. **依赖文件正确**  
   项目依赖BepInEx文件和AliceInCradle文件，请指定这些文件目录：
   - `0Harmony.dll`：`AliceInCradle_ver028/BepInEx/core/`
   - `Assembly-CSharp.dll`：`AliceInCradle_ver028/AliceInCradle_Data/Managed/`
   -  等等

4. **构建解决方案**  
   在 Visual Studio 中选择 **生成解决方案**，构建成功后可在目标路径中找到生成的文件。

---

## 配置文件说明
（配置文件在游戏根目录下config.json）
以下是默认的配置选项，可根据需要进行调整：

```code
{
    //开火模式
    //1 默认，受到伤害，增加兴奋度，高潮时增加强度
    //2 困难，同上默认但消耗mp时增加强度 注：包括使用魔法
    //0 彩蛋，同上默认但所有伤害都增加强度 注：包括造成的伤害
    //使用模式0和模式2时，注意强度上限与倍率，以免突然高强度（（ovo!
    "FireMode": "1",

    //以下可改0关闭，注意连续高潮会叠加
    //高潮增加强度
    "Hero": "10",
    //高潮持续时间（ms）
    "holdMs": "2000",
    //高潮后降低强度
    "eroH": "20",

    //每1滴血转化为强度的倍率（0.1 为 10滴血增加1强度）
    "hpReductionMultiplier": "0.3",
    //每1滴蓝转化为强度的倍率（0.1 为 10滴蓝增加1强度）
    "mpReductionMultiplier": "0.1",
    //每10兴奋度转化为强度的倍率（1 为 10兴奋度增加1强度，会与上叠加，建议设置在1以下）
    "epReductionMultiplier": "0.7",
    //以上数值最大变化强度生效上限，防止一次性变化过大（例如一次扣超过200血/蓝/兴奋度时不会提升强度）
    "maxChange": "200",

    //强度自然下降时间（ms）
    "CheckIntervalMs": "1000",
    //强度下降速率（1 为每次下降1强度）
    "ReductionValue": "1",

    //(实验性/已废弃)
    //虫子墙等增强，已经被兴奋度取代，如要使用会和兴奋度叠加。
    //虫墙会提升多次，注意强度上限！填0关闭，填1为每次变化最低增强1强度
    "lowest": "0"
}
```

💡 **小技巧**：  
在所有情况下使用软糖都能减少强度，高超同样有效哦！（（

---

## 贡献指南

如果您有新的想法，欢迎通过 [Issue](https://github.com/sllying/AliceInCradle_X_DGLAB/issues) 提交，或者直接发起 PR！  
