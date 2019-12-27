# CQGuguBot
使用CQ以及HTTP插件的QQ聊天机器人  
个人娱乐练手作，看心情填坑

### 目前支持的功能
- 色图[]
  - a
    - a

### 使用
- 环境
  - .net Framework 4.8
  - CQ 
  - CQ-Http插件
- 配置文件
  - loginAccont：登陆的QQ号
  - serverAddress：websocket地址
  - Is_setu_On：色图插件的开关
  - reg：正则
  - reg1：正则

        

### 预计加入的功能
- OCR
  - 百度ocr
  - 
- 翻译
  - google翻译
  - 阿里云机器翻译
  - 
- 搜图
  - SauceNAO
  - ascii2d
  - WhatAnime
- Base64加密解密与自定义码表
  - a
- 防撤回
- 语音消息
  - 音频转文字
  - 语音控制
- 资料卡点赞
- 二维码
  - 二维码识别
  - 字符串转二维码
- 单条消息允许触发的模块
  - 只能触发一个模块
    - 色图[setu]
  - 可以同时触发多个模块 [返回所有触发的模块的返回值]
    - a
  - 步进触发[后一模快的传入值为前一模块的返回值，返回单一值，如果有需要，返回多个值]
    [e.g：传入图片识别内容，然后翻译内容，再转换为Base64字符串，最终转换为二维码，流程：OCR->翻译->Base64加密->字符串转二维码]
    - a
- 消息可以触发的模块的优先级
  - 色图[setu] --
  - 