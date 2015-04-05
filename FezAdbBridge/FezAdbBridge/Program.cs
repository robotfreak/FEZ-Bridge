using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.USBHost;
using GHIElectronics.NETMF.System;


namespace FezAdbBridge
{
  public class Program
  {
    // Servos
    static FEZ_Components.ServoMotor servo1 = new FEZ_Components.ServoMotor(FEZ_Pin.Digital.Di2);
    static FEZ_Components.ServoMotor servo2 = new FEZ_Components.ServoMotor(FEZ_Pin.Digital.Di3);
    static FEZ_Components.DistanceDetector myRanger = new FEZ_Components.DistanceDetector(FEZ_Pin.AnalogIn.An0, 
      FEZ_Components.DistanceDetector.SharpSensorType.GP2D120);


    static USBH_RawDevice usb;
    static USBH_RawDevice.Pipe adbInPipe;
    static USBH_RawDevice.Pipe adbOutPipe;
    static Thread adbInThread; // polls the adb for data


    const UInt32 A_SYNC = 0x434e5953;
    const UInt32 A_CNXN = 0x4e584e43;
    const UInt32 A_OPEN = 0x4e45504f;
    const UInt32 A_OKAY = 0x59414b4f;
    const UInt32 A_CLSE = 0x45534c43;
    const UInt32 A_WRTE = 0x45545257;

    private const ushort VALID_VID = 0x18D1;
    private const ushort VALID_PID = 0x4E12;
    private static bool initPhaseOneComplete;
    private static bool initPhaseTwoComplete;
    private static bool isReady;
    private static bool busyWrite;
    private static UInt32 localID = 1;
    private static UInt32 remoteID = 0;

    private static string hex = "0123456789ABCDEF";
    private static string hostName = "host::fezbridge";
    private static string portName = "tcp:4567";

    private UInt32[] AdbData = new UInt32[6];

    private struct sAdbMessage
    {
      public UInt32 command;
      public UInt32 arg0;
      public UInt32 arg1;
      public UInt32 dataLength;
      public UInt32 dataCheck;
      public UInt32 magic;

      public int ByteSize
      {
        get { return 24; }
      }

      public byte[] ToArray()
      {
        byte[] data = new byte[ByteSize];
        int offset = 0;

        data[offset++] = (byte)(command); data[offset++] = (byte)(command >> 8); data[offset++] = (byte)(command >> 16); data[offset++] = (byte)(command >> 24);
        data[offset++] = (byte)(arg0); data[offset++] = (byte)(arg0 >> 8); data[offset++] = (byte)(arg0 >> 16); data[offset++] = (byte)(arg0 >> 24);
        data[offset++] = (byte)(arg1); data[offset++] = (byte)(arg1 >> 8); data[offset++] = (byte)(arg1 >> 16); data[offset++] = (byte)(arg1 >> 24);
        data[offset++] = (byte)(dataLength); data[offset++] = (byte)(dataLength >> 8); data[offset++] = (byte)(dataLength >> 16); data[offset++] = (byte)(dataLength >> 24);
        data[offset++] = (byte)(dataCheck); data[offset++] = (byte)(dataCheck >> 8); data[offset++] = (byte)(dataCheck >> 16); data[offset++] = (byte)(dataCheck >> 24);
        data[offset++] = (byte)(magic); data[offset++] = (byte)(magic >> 8); data[offset++] = (byte)(magic >> 16); data[offset++] = (byte)(magic >> 24);
        /*
        string datastring = "Out>> ";

        for (int i = 0; i < data.Length; i++)
        {
          datastring += (ByteToHex(data[i]) + " ");
        }
        Debug.Print(datastring);
         */
        return data;
      }

      public static sAdbMessage Parse(byte[] data)
      {
        sAdbMessage msg = new sAdbMessage();
        int offset = 0;

        msg.command = (UInt32)(data[offset++] + (data[offset++] << 8) + (data[offset++] << 16) + (data[offset++] << 24));
        msg.arg0 = (UInt32)(data[offset++] + (data[offset++] << 8) + (data[offset++] << 16) + (data[offset++] << 24));
        msg.arg1 = (UInt32)(data[offset++] + (data[offset++] << 8) + (data[offset++] << 16) + (data[offset++] << 24));
        msg.dataLength = (UInt32)(data[offset++] + (data[offset++] << 8) + (data[offset++] << 16) + (data[offset++] << 24));
        msg.dataCheck = (UInt32)(data[offset++] + (data[offset++] << 8) + (data[offset++] << 16) + (data[offset++] << 24));
        msg.magic = (UInt32)(data[offset++] + (data[offset++] << 8) + (data[offset++] << 16) + (data[offset++] << 24));
        return msg;
      }
    };

    static sAdbMessage adbMsg = new sAdbMessage();
    static sAdbMessage adbInMsg = new sAdbMessage();
    static byte[] adbData = new byte[65];

    public static void Main()
    {
      Debug.EnableGCMessages(false);

      // Subscribe to USBH event.
      USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
      USBHostController.DeviceDisconnectedEvent += DeviceDisconnectedEvent;
      initPhaseOneComplete = false;
      initPhaseTwoComplete = false;
      isReady = false;
      busyWrite = false;

      // Sleep forever
      Thread.Sleep(Timeout.Infinite);
    }

    static void DeviceConnectedEvent(USBH_Device device)
    {
      if (!initPhaseOneComplete)
      {
        usb = new USBH_RawDevice(device);
        USBH_Descriptors.Configuration cd = usb.GetConfigurationDescriptors(0);
        USBH_Descriptors.Endpoint adbEP = null;
        USBH_Descriptors.Interface adbIF = null;

        Debug.Print("[Device, Port " + usb.PORT_NUMBER + "]");
        Debug.Print("Interface: " + usb.INTERFACE_INDEX);
        Debug.Print("ID: " + usb.ID);
        Debug.Print("Type: " + usb.TYPE);
        Debug.Print("VID: " + usb.VENDOR_ID);
        Debug.Print("PID: " + usb.PRODUCT_ID);

        // look for HID class
        for (int i = 0; i < cd.bNumInterfaces; i++)
        {

          adbIF = cd.interfaces[i];
          if (adbIF.bInterfaceClass == 255)
          {

            Debug.Print("  === Interface ===");
            Debug.Print("  Class: " + cd.interfaces[i].bInterfaceClass);
            Debug.Print("  SubClass: " + cd.interfaces[i].bInterfaceSubclass);
            Debug.Print("  Number: " + cd.interfaces[i].bInterfaceNumber);
            Debug.Print("  Protocol: " + cd.interfaces[i].bInterfaceProtocol);
            Debug.Print("  Type: " + cd.interfaces[i].bDescriptorType);

            for (int ep = 0; ep < adbIF.bNumberEndpoints; ep++)
            {
              adbEP = adbIF.endpoints[ep];

              Debug.Print("   -- Endpoint --");
              Debug.Print("    Attributes: " + adbIF.endpoints[ep].bmAttributes);
              Debug.Print("    Address: " + adbIF.endpoints[ep].bEndpointAddress);
              Debug.Print("    Type: " + adbIF.endpoints[ep].bDescriptorType);
              Debug.Print("    Interval: " + adbIF.endpoints[ep].bInterval);
              Debug.Print(" ");
              switch (adbEP.bEndpointAddress)
              {
                case 133: // ADB data in
                  adbInPipe = usb.OpenPipe(adbEP);
                  adbInPipe.TransferTimeout = 0;              // recommended for interrupt transfers
                  break;
                case 5:   // ADB data out
                  adbOutPipe = usb.OpenPipe(adbEP);
                  break;
              }

            }
            initPhaseOneComplete = true;
          }
        }

      }
      else
      {
        initPhaseTwoComplete = true;
      }

      if (initPhaseTwoComplete)
      {
        initPhaseOneComplete = false;
        initPhaseTwoComplete = false;
        isReady = true;
      }

      if (isReady)
      {
        usb.SendSetupTransfer(0x00, 0x09, 0x0001, 0x0000);
        adbInThread = new Thread(AdbListening);      // create the polling thread
        adbInThread.Priority = ThreadPriority.Highest;
        adbInThread.Start();

        SendAdbMessage(A_CNXN, 16777216, 4096, hostName);
      }

    }

    static void DeviceDisconnectedEvent(USBH_Device device)
    {
      isReady = false;

    }


    static void AdbListening()
    {
      int count = 0;
      string datastring = "";

      // Read every bInterval
      while (true)
      {
        Thread.Sleep(adbInPipe.PipeEndpoint.bInterval);

        try
        {
          count = adbInPipe.TransferData(adbData, 0, adbData.Length);
        }
        catch (Exception ex)
        {
          Debug.Print(ex.ToString());
        }

        if (count == 24)
        {
          adbInMsg = sAdbMessage.Parse(adbData);
          switch (adbInMsg.command)
          {
            case A_CNXN:
              Debug.Print("In << CNXN " + adbInMsg.arg0.ToString() + "," + adbInMsg.arg1.ToString());
              break;
            case A_OPEN:
              Debug.Print("In << OPEN " + adbInMsg.arg0.ToString() + "," + adbInMsg.arg1.ToString());
              break;
            case A_OKAY:
              Debug.Print("In << OKAY " + adbInMsg.arg0.ToString() + "," + adbInMsg.arg1.ToString());
              remoteID = adbInMsg.arg0;
              busyWrite = false;
              break;
            case A_CLSE:
              Debug.Print("In << CLSE " + adbInMsg.arg0.ToString() + "," + adbInMsg.arg1.ToString());
              SendAdbMessage(A_OKAY, adbInMsg.arg1, adbInMsg.arg0);
              break;
            case A_WRTE:
              Debug.Print("In << WRTE " + adbInMsg.arg0.ToString() + "," + adbInMsg.arg1.ToString());
              SendAdbMessage(A_OKAY, adbInMsg.arg1, adbInMsg.arg0);
              break;
            default:
              datastring = ByteArrayToString(adbData);
              Debug.Print("In << (" + count.ToString() + ") " + datastring);
              break;
          }
        }
        else if (count > 0)
        {
          adbData[count] = 0;
          datastring = ByteArrayToString(adbData);
          Debug.Print("In << (" + count.ToString() + ") " + datastring);
          switch (datastring)
          {
            case "device::":
              SendAdbMessage(A_OPEN, 1, 0, portName);
              break;
            default:
              string[] cmds = datastring.Split(';');
              for (int i = 0; i < cmds.Length; i++)
              {
                string[] cmd = cmds[i].Split(':');
                if (cmd.Length == 2)
                {
                  switch (cmd[0])
                  {
                    case "x":
                      servo1.SetPosition((byte)int.Parse(cmd[1]));
                      break;
                    case "y":
                      servo2.SetPosition((byte)int.Parse(cmd[1]));
                      break;
                  }
                }

              }
              break;
          }
        }
        else if (count == 0)
        {
          float value = myRanger.GetDistance_cm();
          Debug.Print("myRanger reading is: " + value.ToString());
          SendAdbMessage(A_WRTE, localID, remoteID, "a0:" + value.ToString() + ";");
          Thread.Sleep(10);
        }

      }
    }

    public static void SendAdbMessage(UInt32 st, UInt32 arg0, UInt32 arg1)
    {
      SendAdbMessage(st, arg0, arg1, (byte[])null);
    }

    public static void SendAdbMessage(UInt32 st, UInt32 arg0, UInt32 arg1, string str)
    {
      SendAdbMessage(st, arg0, arg1, StringToByteArray(str));
    }

    public static void SendAdbMessage(UInt32 st, UInt32 arg0, UInt32 arg1, byte[] data)
    {
      UInt32 crc = 0;
      UInt32 cnt = 0;

      if (data != null)
      {
        while (cnt < data.Length)
        {
          crc += data[cnt++];
        }
        cnt++;
      }

      Debug.Print("Out >> " + st.ToString() + "," + arg0.ToString() + "," + arg1.ToString());
      adbMsg.command = st;
      adbMsg.arg0 = arg0;
      adbMsg.arg1 = arg1;
      adbMsg.dataLength = cnt;
      adbMsg.dataCheck = crc;
      adbMsg.magic = st ^ 0xffffffff;
      try
      {
        adbOutPipe.TransferData(adbMsg.ToArray(), 0, adbMsg.ByteSize);

        if (data != null)
        {
          byte[] zdata = new byte[data.Length + 1];
          data.CopyTo(zdata, 0);
          adbOutPipe.TransferData(zdata, 0, zdata.Length);
        }
      }
      catch (Exception ex)
      {
        Debug.Print(ex.ToString());
      }

    }

    private static byte[] StringToByteArray(string str)
    {
      System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
      return enc.GetBytes(str);
    }

    private static string ByteArrayToString(byte[] data)
    {
      return new string(System.Text.Encoding.UTF8.GetChars(data));
    }

    public static string UIn16tToHex(UInt16 number)
    {
      return new string(new char[] { hex[(number & 0xF000) >> 12], hex[(number & 0xF00) >> 8], hex[(number & 0xF0) >> 4], hex[number & 0x0F] });
    }

    public static string ByteToHex(byte number)
    {
      return new string(new char[] { hex[(number & 0xF0) >> 4], hex[number & 0x0F] });
    }

  }
}
