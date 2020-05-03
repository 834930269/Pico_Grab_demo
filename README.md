# Pico_Grab_demo
picovr sdk 魔改的一个可以抓取物体的demo

> 基于目前版本SDK做的
>> 单手操作测试中暂时没有问题

>> 双手操作没写,需要自己实现

场景里有demo.

实际使用时只需要将Interactable Object和ChildOf ControllerGrabAttach这两个组件拖到被抓取物体上.
注意被抓取物体需要有rigidbody()和boxcollider

Tip:

**Interactable Object**脚本配置:

Is Grabbale 设置为true

Hold Button To Grab 设置为true

Disable when idle 设置为true

**ChildOf ControllerGrabAttach**脚本配置:

Precision Grab设置为true