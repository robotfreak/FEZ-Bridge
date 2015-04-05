#FezBridge

FezBridge is an [Android Debug Bridge (ADB)](http://developer.android.com/guide/developing/tools/adb.html) implementation in .NETMF for FEZ Devices.

[![FezBridge](https://farm8.staticflickr.com/7608/16904175826_a844a4b747_m.jpg)](https://github.com/robotfreak/robotfreak/tree/master/fez-bridge)

##Introduction

FezBridge is ported from the [http://code.google.com/p/microbridge MicroBridge] for Arduino. 
It is programmed in .NETMF and runs on FEZ devices from http://www.ghielectronics.com/ GHI electronics, like the [FEZ Domino](http://www.ghielectronics.com/catalog/product/133). Other devices like [FEZ Panda]http://www.ghielectronics.com/catalog/product/135) needs a small [hardware modification](http://ghielectronics.blogspot.com/2011/03/usb-host-support-is-added-on-fez-panda.html) to support USB host mode.


##Details

At the moment FezBridge is a work in progress. The basic communication and USB enuration has been done.

ADB works on almost every Android phone. That is important if you wan't to use [Android Open Accessory (ADK)](http://developer.android.com/guide/topics/usb/adk.html). ADK works only on Android phones with the newest firmware v2.3.4. But not all phones will support ADK, even with the newest firmware. This is where ADB can help. It has nearly the same functionality as ADK.

You can use ADB to forward TCP/IP ports via USB between phone and hardware. Simple data pipes can be used for communication. 

A description of the ADB protocol can be found [ADB Protocol](http://lxr.e2g.org/source/system/core/adb/protocol.txt) here.

##To do list

* build seperate driver class
* Demo programs 
* Demo apps (Android side)

##Videos

* [![Android Open Accessory DIY DemoKit](https://i.ytimg.com/vi/BqD5-chWdDY/2.jpg?time=1427315296596)](http://www.youtube.com/watch?v=sDUndL7bEic) Android Open Accessory DIY DemoKit
* [![FezBridge - Android Debug Bridge with Fez Domino](https://i.ytimg.com/vi/sDUndL7bEic/2.jpg?time=1427315527024)](https://www.youtube.com/watch?v=sDUndL7bEic) FezBridge - Android Debug Bridge with Fez Domino

##Weblinks
 * https://github.com/robotfreak/robotfreak/new/master/fez-bridge FezBridge Source repository
 * http://developer.android.com/guide/developing/tools/adb.html Android Debug Bridge (ADB)
 * http://www.ghielectronics.com/ GHI electronics
 * http://code.google.com/p/microbridge MicroBridge
