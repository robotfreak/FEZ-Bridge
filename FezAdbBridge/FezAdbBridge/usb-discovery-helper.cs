using System;
using System.Threading;
using Microsoft.SPOT;
using GHIElectronics.NETMF.USBHost;
using GHIElectronics.NETMF.System;


namespace Ascended.Helpers
{
    class UsbDiscoveryHelper
    {
        USBH_RawDevice controller;
        USBH_RawDevice.Pipe controlPipe;
        Thread writeThread;

        public UsbDiscoveryHelper()
        {
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
            USBHostController.DeviceDisconnectedEvent += DeviceDisconnectedEvent;

        }


        void DeviceConnectedEvent(USBH_Device device)
        {
            
            Debug.Print("[Device, Port " + device.PORT_NUMBER + "]");
            controller = new USBH_RawDevice(device);
            Debug.Print("Type: " + DeviceTypeToString(device.TYPE));
            Debug.Print("ProductID: " + controller.PRODUCT_ID);
            Debug.Print("VendorID: " + controller.VENDOR_ID);

            // Get descriptors
            USBH_Descriptors.Configuration cd = controller.GetConfigurationDescriptors(0);
            
            // look for HID class
            for (int i = 0; i < cd.interfaces.Length; i++)
            {

                Debug.Print("  === Interface ===");
                Debug.Print("  Class: " + ClassToString(cd.interfaces[i].bInterfaceClass));
                Debug.Print("  SubClass: " + cd.interfaces[i].bInterfaceSubclass);
                Debug.Print("  Number: " + cd.interfaces[i].bInterfaceNumber);
                Debug.Print("  Protocol: " + cd.interfaces[i].bInterfaceProtocol);
                Debug.Print("  Type: " + cd.interfaces[i].bDescriptorType);

                for (int ep = 0; ep < cd.interfaces[i].endpoints.Length; ep++)
                {
                    Debug.Print("   -- Endpoint --");
                    Debug.Print("    Attributes: " + cd.interfaces[i].endpoints[ep].bmAttributes);
                    Debug.Print("    Address: " + cd.interfaces[i].endpoints[ep].bEndpointAddress);
                    Debug.Print("    Type: " + cd.interfaces[i].endpoints[ep].bDescriptorType);
                    Debug.Print("    Interval: " + cd.interfaces[i].endpoints[ep].bInterval);
                    Debug.Print(" ");
                }

                Debug.Print(" ");
                Debug.Print(" ");
            }
        }

        void DeviceDisconnectedEvent(USBH_Device device)
        {
            Debug.Print("Device disconnected...");
            Debug.Print("ID: " + device.ID + ", Interface: " +
                        device.INTERFACE_INDEX + ", Type: " + device.TYPE);
        }


        private static string ClassToString(byte interfaceClass)
        {
            switch (interfaceClass)
            {
                case 0x0:
                    return "Base Class";
                case 0x01:
                    return "Audio";
                case 0x02:
                    return "Communications/CDC";
                case 0x03:
                    return "HID";
                case 0x05:
                    return "Physical";
                case 0x06:
                    return "Image";
                case 0x07:
                    return "Printer";
                case 0x08:
                    return "Mass Storage";
                case 0x09:
                    return "Hub";
                case 0x0A:
                    return "CDC-Data";
                case 0x0B:
                    return "Smart Card";
                case 0x0D:
                    return "Content Security";
                case 0x0E:
                    return "Video";
                case 0x0F:
                    return "Personal Healthcare";
                case 0xDC:
                    return "Diagnostic Device";
                case 0xE0:
                    return "Wireless Controller";
                case 0xEF:
                    return "Misc";
                case 0xFE:
                    return "Application Specific";
                case 0xFF:
                    return "Vendor Specific";
                default:
                    return "unknown";
            }
        }

        private static string DeviceTypeToString(USBH_DeviceType usbhDeviceType)
        {
            switch (usbhDeviceType)
            {
                case USBH_DeviceType.Unknown:
                    return "Unknown";
                case USBH_DeviceType.Hub:
                    return "Hub";
                case USBH_DeviceType.HID:
                    return "HID";
                case USBH_DeviceType.Mouse:
                    return "Mouse";
                case USBH_DeviceType.Keyboard:
                    return "Keyboard";
                case USBH_DeviceType.Joystick:
                    return "Joystick";
                case USBH_DeviceType.MassStorage:
                    return "MassStorage";
                case USBH_DeviceType.Printer:
                    return "Printer";
                case USBH_DeviceType.Serial_FTDI:
                    return "Serial_FTDI";
                case USBH_DeviceType.Serial_Prolific:
                    return "Serial_Prolific";
                case USBH_DeviceType.Serial_Prolific2:
                    return "Serial_Prolific2";
                case USBH_DeviceType.Serial_SiLabs:
                    return "Serial_SiLabs";
                case USBH_DeviceType.Serial_CDC:
                    return "Serial_CDC";
                case USBH_DeviceType.Serial_Sierra_C885:
                    return "Serial_Sierra_C885";
                case USBH_DeviceType.Sierra_Installer:
                    return "Sierra_Installer";
            }

            return null;
        }
    }
}

