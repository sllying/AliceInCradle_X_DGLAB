# AliceInCradle_X_DGLAB
//当厨圣是要付出代价的，你们有没有考虑过诺艾儿的感受，现在来体验下吧...这真的是惩罚吗（
## 爱丽丝的摇篮-X-郊狼
基于[BepInEx](https://github.com/BepInEx/BepInEx)开发的Unity MOD,使用[DG-Lab-Coyote-Game-Hub](https://github.com/hyperzlib/DG-Lab-Coyote-Game-Hub)提供的API实现爱丽丝的摇篮与郊狼联动MOD。
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
    //0 彩蛋，所有伤害都增加强度 注：包括造成的伤害，bug巨多！！
    //使用模式0和模式2时，注意强度上限与倍率，以免突然高强度（（ovo!
    "FireMode": "1",

    //虫子墙增强，怪物奸增强 
    //（有时被怪物奸时只扣2MP，进虫墙扣4MP，填5为每次小幅度蓝量变化增强5强度）
    //建议不要太高，毕竟是进一次虫墙将扣多次，！提升多次，注意强度上限！怪物奸也同理，尽量快速挣脱。
    //填0关闭 注：此项启用将开启所有开火模式的蓝量检测，但并不会和模式2一样使用魔法霰弹或圣光爆发等时增加强度，且不受倍率影响
    "lowest": "5",

    //每1滴血转化为强度的倍率（0.1 为 10滴血增加1强度）
    "hpReductionMultiplier": "0.5",
    //每1滴蓝转化为强度的倍率（0.1 为 10滴蓝增加1强度）
    "mpReductionMultiplier": "0.3",
    //强度自然下降时间（ms）
    "CheckIntervalMs": "1000",
    //强度下降速度（1 为每次下降1强度）
    "ReductionValue": "1"

    //小技巧：所有情况下使用软糖都能减少强度
