using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.USBHost;
using Ascended.Helpers;

 

namespace AndroidAdbTest
{
    public class Program
    {
        public static void Main()
        {
            // Subscribe to USBH event.
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;

            // Sleep forever
            Thread.Sleep(Timeout.Infinite);
        }
        static void DeviceConnectedEvent(USBH_Device device)
        {
                AndroidAdb android = new AndroidAdb(device);


        }
    }
}
