using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.USBHost;
using GHIElectronics.NETMF.System;


namespace AndroidAdbTest
{
    #region ADB defines
    enum eAdbConnectionState
    {
        ADB_UNUSED = 0,
        ADB_CLOSED,
        ADB_OPEN,
        ADB_OPENING,
        ADB_RECEIVING,
        ADB_WRITING
    };

    enum eAdbEventtype
    {
        ADB_CONNECT,
        ADB_DISCONNECT,
        ADB_CONNECTION_OPEN,
        ADB_CONNECTION_CLOSE,
        ADB_CONNECTION_FAILED,
        ADB_CONNECTION_RECEIVE
    };

    struct sAdbUsbConfiguration
    {
        byte address;
        byte configuration;
        byte iface;
        byte inputEndPointAddress;
        byte outputEndPointAddress;
    };

    struct sAdbMessage
    {
        UInt32 command;
        UInt32 arg0;
        UInt32 arg1;
        UInt32 dataLength;
        UInt32 dataCheck;
        UInt32 magic;
    };

    struct sAdbConnection
    {
        string connectionString;
        UInt32 localID, remoteID;
        UInt32 lastConnectionAttempt;
        UInt16 dataSize;
        UInt16 dataRead;
        eAdbConnectionState status;
        bool reconnect;
    };


            
    #endregion

    class AndroidAdb
    {
        const byte ADB_CLASS = 0xff;
        const byte ADB_SUBCLASS = 0x42;
        const byte ADB_PROTOCOL = 0x1;

        const byte ADB_USB_PACKETSIZE = 0x40;
        const byte ADB_CONNECTSTRING_LENGTH = 64;
        const byte ADB_MAX_CONNECTIONS = 4;
        const int ADB_CONNECTION_RETRY_TIME = 1000;

        const int A_SYNC = 0x434e5953;
        const int A_CNXN = 0x4e584e43;
        const int A_OPEN = 0x4e45504f;
        const int A_OKAY = 0x59414b4f;
        const int A_CLSE = 0x45534c43;
        const int A_WRTE = 0x45545257;

        const int MAX_PAYLOAD = 4096;

        private const ushort VALID_VID = 0x18D1;
        private const ushort VALID_PID = 0x4E12;
        private USBH_RawDevice usb;
        private USBH_RawDevice.Pipe statusPipe;

        public static bool IsValidDevice(USBH_Device device, USBH_Descriptors.Configuration cd)
        {
            bool ret = true;
            if (cd.bNumInterfaces != 2) ret = false;
            return ret;
        }

        public AndroidAdb(USBH_Device device)
        {

            usb = new USBH_RawDevice(device);
            USBH_Descriptors.Configuration cd = usb.GetConfigurationDescriptors(0);

            if (!IsValidDevice(device, cd))
                throw new Exception("Invalid device!");

            DeviceInfo(device);


            // set configuration
            //            usb.SendSetupTransfer(0x00, 0x09, 0x0001, 0x0000);

            //            statusPipe = usb.OpenPipe(usb.GetConfigurationDescriptors(0).interfaces[0].endpoints[0]);
            //            statusPipe.TransferTimeout = 0;
        }

        public void DeviceInfo(USBH_Device device)
        {
            Debug.Print("[Device, Port " + device.PORT_NUMBER + "]");
            Debug.Print("Interface: " + device.INTERFACE_INDEX);
            Debug.Print("ID: " + device.ID);
            Debug.Print("Type: " + device.TYPE);
            Debug.Print("VID: " + device.VENDOR_ID);
            Debug.Print("PID: " + device.PRODUCT_ID);
        }



    }
}
