# AliceInCradle X DGLAB
*当厨圣是要付出代价的，你们有没有考虑过诺艾儿的感受，现在来体验下吧...这真的是惩罚吗（
\
*请不要将游戏放在中文目录下
## 爱丽丝的摇篮 X 郊狼
\
基于[BepInEx](https://github.com/BepInEx/BepInEx)开发的Unity MOD,使用[DG-Lab-Coyote-Game-Hub](https://github.com/hyperzlib/DG-Lab-Coyote-Game-Hub)提供的API实现爱丽丝的摇篮与郊狼联动MOD。
\
## 食用教程

### MOD安装教程


将压缩包文件中的第一个AliceInCradle_ver026文件夹的内容复制到本地游戏中的AliceInCradle_ver026文件夹中即可
[点击前往下载MOD](https://github.com/sllying/AliceInCradle_X_DGLAB/releases)
### 链接郊狼教程

下载[DG-Lab-Coyote-Game-Hub](https://github.com/hyperzlib/DG-Lab-Coyote-Game-Hub)，启动即可，扫码与郊狼链接，注意需要同一网络下。
### Play

点击启动输出即可（推荐选快速按捏((）
 \
 b站视频教程：https://www.bilibili.com/video/BV1ud1iY3Ei6
 \
 \
 \
 \
如有什么新想法可发Issue或直接PR哦qwq

### 目前配置文件
    //开火模式
    //1 默认，受到伤害增加强度
    //2 困难，受到伤害和损失mp时增加强度 注：包括使用魔法
    //0 彩蛋，所有伤害都增加强度 注：包括造成的伤害
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
    "hpReductionMultiplier": "0.5",
    //每1滴蓝转化为强度的倍率（0.1 为 10滴蓝增加1强度）
    "mpReductionMultiplier": "0.2",
    //每10兴奋度转化为强度的倍率（1 为 10兴奋度增加1强度，会与上叠加，建议设置在1以下）
    "epReductionMultiplier": "0.7",
    //强度自然下降时间（ms）
    "CheckIntervalMs": "1000",
    //强度下降速率（1 为每次下降1强度）
    "ReductionValue": "1",

    //(实验性/已废弃)
    //虫子墙等增强，已经被兴奋度取代，如要使用会和兴奋度叠加。
    //虫墙会提升多次，注意强度上限！填0关闭，填1为每次变化最低增强1强度
    "lowest": "0"


    //小技巧：所有情况下使用软糖都能减少强度，还有高超也行（（
