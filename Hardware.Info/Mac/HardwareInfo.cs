﻿//using PListNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man3/sysctlbyname.3.html
// https://wiki.freepascal.org/Accessing_macOS_System_Information
// https://stackoverflow.com/questions/6592578/how-to-to-print-motherboard-and-display-card-info-on-mac
// https://stackoverflow.com/questions/53117107/cocoa-nstask-ouput-extraction
// https://docs.python.org/3/library/plistlib.html
// https://ss64.com/osx/system_profiler.html

namespace Hardware.Info.Mac
{
    internal class HardwareInfo : HardwareInfoBase, IHardwareInfo
    {
        [DllImport("libc")]
        static extern int sysctlbyname(string name, out IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);

        private readonly MemoryStatus memoryStatus = new MemoryStatus();

        /*
        public HardwareInfo()
        {
            //SystemProfiler();

            //SystemProfilerPList();
        }
        /**/

        /*
        private void SystemProfilerPList()
        {
            try
            {
                using MemoryStream stream = new MemoryStream();
                using StreamWriter output = new StreamWriter(stream);
                StringBuilder error = new StringBuilder();

                if (StartProcess("system_profiler", "-xml", standardOutput => output.Write(standardOutput), standardError => error.AppendLine(standardError)))
                {
                    output.Flush();
                    stream.Position = 0;

                    PNode system_profiler = PList.Load(stream);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /**/

        /*
        private void SystemProfiler()
        {
            string[] dataTypes = {
                "SPParallelATADataType",
                "SPUniversalAccessDataType",
                //"SPApplicationsDataType",
                "SPAudioDataType",
                "SPBluetoothDataType",
                "SPCameraDataType",
                "SPCardReaderDataType",
                //"SPComponentDataType",
                "SPDeveloperToolsDataType",
                "SPDiagnosticsDataType",
                "SPDisabledSoftwareDataType",
                "SPDiscBurningDataType",
                "SPEthernetDataType",
                //"SPExtensionsDataType",
                "SPFibreChannelDataType",
                "SPFireWireDataType",
                "SPFirewallDataType",
                //"SPFontsDataType",
                //"SPFrameworksDataType",
                "SPDisplaysDataType",
                "SPHardwareDataType",
                "SPHardwareRAIDDataType",
                //"SPInstallHistoryDataType",
                "SPNetworkLocationDataType",
                //"SPLogsDataType",
                "SPManagedClientDataType",
                "SPMemoryDataType",
                "SPNVMeDataType",
                "SPNetworkDataType",
                "SPPCIDataType",
                "SPParallelSCSIDataType",
                "SPPowerDataType",
                //"SPPrefPaneDataType",
                //"SPPrintersSoftwareDataType",
                "SPPrintersDataType",
                "SPConfigurationProfileDataType",
                //"SPRawCameraDataType",
                "SPSASDataType",
                "SPSerialATADataType",
                "SPSPIDataType",
                //"SPSmartCardsDataType",
                "SPSoftwareDataType",
                "SPStartupItemDataType",
                "SPStorageDataType",
                //"SPSyncServicesDataType",
                "SPThunderboltDataType",
                "SPUSBDataType",
                "SPNetworkVolumeDataType",
                "SPWWANDataType",
                "SPAirPortDataType",
                "SPiBridgeDataType"
            };

            foreach (string dataType in dataTypes)
            {
                Console.WriteLine(dataType);

                StartProcess("system_profiler", dataType, standardOutput => Console.WriteLine(standardOutput), standardError => Console.WriteLine(standardError));

                Console.WriteLine(dataType + " END");

                //Console.ReadLine();
            }
        }
        /**/

        private bool StartProcess(string fileName, string arguments, Action<string> output, Action<string> error, int millisecondsTimeout = 60 * 1000)
        {
            using Process process = new Process();

            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            using AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
            using AutoResetEvent errorWaitHandle = new AutoResetEvent(false);

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    outputWaitHandle.Set();
                }
                else
                {
                    output.Invoke(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    errorWaitHandle.Set();
                }
                else
                {
                    error.Invoke(e.Data);
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process.WaitForExit(millisecondsTimeout) && outputWaitHandle.WaitOne(millisecondsTimeout) && errorWaitHandle.WaitOne(millisecondsTimeout);
        }

        public MemoryStatus GetMemoryStatus()
        {
            IntPtr SizeOfLineSize = (IntPtr)IntPtr.Size;

            if (sysctlbyname("hw.memsize", out IntPtr lineSize, ref SizeOfLineSize, IntPtr.Zero, IntPtr.Zero) == 0)
            {
                memoryStatus.TotalPhysical = (ulong)lineSize.ToInt64();
            }

            return memoryStatus;
        }

        public List<Battery> GetBatteryList()
        {
            List<Battery> batteryList = new List<Battery>();

            Battery battery = new Battery();

            // https://stackoverflow.com/questions/29278961/check-mac-battery-percentage-in-swift

            // https://developer.apple.com/documentation/iokit/iopowersources_h

            /*
            SPPowerDataType
Power:

    System Power Settings:

      AC Power:
          System Sleep Timer (Minutes): 10
          Disk Sleep Timer (Minutes): 10
          Display Sleep Timer (Minutes): 10
          Sleep on Power Button: Yes
          Current Power Source: Yes
          Hibernate Mode: 0
          Standby Delay: 4200
          Standby Enabled: 1

    Hardware Configuration:

      UPS Installed: No
            /**/

            batteryList.Add(battery);

            return batteryList;
        }

        public List<BIOS> GetBiosList()
        {
            List<BIOS> biosList = new List<BIOS>();

            BIOS bios = new BIOS();

            biosList.Add(bios);

            return biosList;
        }

        /*
        SPHardwareDataType
Hardware:

    Hardware Overview:

      Model Name: iMac
      Model Identifier: iMac11,3
      Processor Speed: 3,29 GHz
      Number of Processors: 1
      Total Number of Cores: 4
      L2 Cache (per Core): 256 KB
      L3 Cache: 24 MB
      Memory: 4 GB
      Boot ROM Version: VirtualBox
      SMC Version (system): 2.3f35
      Serial Number (system): 0
      Hardware UUID: F6D9C340-725A-224A-8855-99AB8348F745
        /**/

        public List<CPU> GetCpuList()
        {
            List<CPU> cpuList = new List<CPU>();

            CPU cpu = new CPU();

            string processOutput = ReadProcessOutput("sysctl", "-n machdep.cpu.brand_string");
            string[] info = processOutput.Split('@');

            if (info.Length > 1)
            {
                string speedString = info[1].Trim();
                uint speed = 0;

                if (speedString.EndsWith("GHz"))
                {
                    string number = speedString.Replace("GHz", string.Empty).Trim();
                    if (uint.TryParse(number, out speed))
                        speed *= 1000;
                }
                else if (speedString.EndsWith("KHz"))
                {
                    string number = speedString.Replace("KHz", string.Empty).Trim();
                    if (uint.TryParse(number, out speed))
                        speed /= 1000;
                }
                else if (speedString.EndsWith("MHz"))
                {
                    string number = speedString.Replace("MHz", string.Empty).Trim();
                    uint.TryParse(number, out speed);
                }

                cpu.Name = info[0];
                cpu.CurrentClockSpeed = speed;
            }

            processOutput = ReadProcessOutput("sysctl", "-n hw.physicalcpu");

            if (uint.TryParse(processOutput, out uint numberOfCores))
                cpu.NumberOfCores = numberOfCores;

            processOutput = ReadProcessOutput("sysctl", "-n hw.logicalcpu");

            if (uint.TryParse(processOutput, out uint numberOfLogicalProcessors))
                cpu.NumberOfLogicalProcessors = numberOfLogicalProcessors;

            cpuList.Add(cpu);

            return cpuList;
        }

        public override List<Drive> GetDriveList()
        {
            /*
            SPSerialATADataType
SATA/SATA Express:

    Intel ICH8-M AHCI:

      Vendor: Intel
      Product: ICH8-M AHCI
      Link Speed: 3 Gigabit
      Negotiated Link Speed: 3 Gigabit
      Physical Interconnect: SATA
      Description: AHCI Version 1.10 Supported

        VBOX HARDDISK:

          Capacity: 536,87 GB (536.870.912.000 bytes)
          Model: VBOX HARDDISK                           
          Revision: 1,000000
          Serial Number: VBa308df62-62a2d2a0 
          Native Command Queuing: Yes
          Queue Depth: 32
          Removable Media: No
          Detachable Drive: No
          BSD Name: disk0
          Medium Type: Rotational
          Partition Map Type: GPT (GUID Partition Table)
          Volumes:
            EFI:
              Capacity: 209,7 MB (209.715.200 bytes)
              File System: MS-DOS FAT32
              BSD Name: disk0s1
              Content: EFI
              Volume UUID: 0E239BC6-F960-3107-89CF-1C97F78BB46B
            Macintosh HD:
              Capacity: 536,01 GB (536.011.153.408 bytes)
              Available: 513,49 GB (513.488.637.952 bytes)
              Writable: Yes
              File System: Journaled HFS+
              BSD Name: disk0s2
              Mount Point: /
              Content: Apple_HFS
              Volume UUID: 510DC06E-E3D7-36E9-9711-C8A209E8C61E
            Recovery HD:
              Capacity: 650 MB (650.002.432 bytes)
              File System: Journaled HFS+
              BSD Name: disk0s3
              Content: Apple_Boot
              Volume UUID: B822D1A0-3CE3-3BA2-852F-85F818623545

    Intel ICH8-M AHCI:

      Vendor: Intel
      Product: ICH8-M AHCI
      Link Speed: 3 Gigabit
      Negotiated Link Speed: 3 Gigabit
      Physical Interconnect: SATA
      Description: AHCI Version 1.10 Supported

        VBOX CD-ROM:

          Model: VBOX CD-ROM                             
          Revision: 1,000000
          Serial Number: VB1-1a2b3c4d        
          Native Command Queuing: No
          Detachable Drive: No
          Power Off: No
          Async Notification: No
            /**/

            /*
            SPStorageDataType
Storage:

    Macintosh HD:

      Available: 513,49 GB (513.488.637.952 bytes)
      Capacity: 536,01 GB (536.011.153.408 bytes)
      Mount Point: /
      File System: Journaled HFS+
      Writable: Yes
      Ignore Ownership: No
      BSD Name: disk0s2
      Volume UUID: 510DC06E-E3D7-36E9-9711-C8A209E8C61E
      Physical Drive:
          Device Name: VBOX HARDDISK
          Media Name: VBOX HARDDISK Media
          Medium Type: Rotational
          Protocol: SATA
          Internal: Yes
          Partition Map Type: GPT (GUID Partition Table)
            /**/

            return base.GetDriveList();
        }

        public List<Keyboard> GetKeyboardList()
        {
            List<Keyboard> keyboardList = new List<Keyboard>();

            Keyboard keyboard = new Keyboard();

            /*
            SPUSBDataType
USB:

    USB Bus:

      Host Controller Driver: AppleUSBOHCIPCI
      PCI Device ID: 0x003f 
      PCI Revision ID: 0x0000 
      PCI Vendor ID: 0x106b 

        USB Tablet:

          Product ID: 0x0021
          Vendor ID: 0x80ee
          Version: 1.00
          Speed: Up to 12 Mb/sec
          Manufacturer: VirtualBox
          Location ID: 0x06200000 / 2
          Current Available (mA): 500
          Current Required (mA): 100
          Extra Operating Current (mA): 0

        USB Keyboard:

          Product ID: 0x0010
          Vendor ID: 0x80ee
          Version: 1.00
          Speed: Up to 12 Mb/sec
          Manufacturer: VirtualBox
          Location ID: 0x06100000 / 1
          Current Available (mA): 500
          Current Required (mA): 100
          Extra Operating Current (mA): 0
            /**/

            keyboardList.Add(keyboard);

            return keyboardList;
        }

        public List<Memory> GetMemoryList()
        {
            List<Memory> memoryList = new List<Memory>();

            Memory memory = new Memory();

            /*
            SPMemoryDataType
Memory:

    Memory Slots:

      ECC: Disabled
      Upgradeable Memory: Yes

        Bank 0/DIMM 0:

          Size: 4 GB
          Type: DRAM
          Speed: 1600 MHz
          Status: OK
          Manufacturer: innotek GmbH
          Part Number: -
          Serial Number: -
            /**/

            memoryList.Add(memory);

            return memoryList;
        }

        public List<Monitor> GetMonitorList()
        {
            List<Monitor> monitorList = new List<Monitor>();

            Monitor monitor = new Monitor();

            // https://developer.apple.com/documentation/appkit/nsscreen

            // https://developer.apple.com/documentation/iokit/iographicslib_h

            // IODisplayConnect

            // IODisplayEDID

            //auto mainDisplayId = CGMainDisplayID();
            //width = CGDisplayPixelsWide(mainDisplayId);
            //height = CGDisplayPixelsHigh(mainDisplayId);

            /*
            SPDisplaysDataType
Graphics/Displays:

    Display:

      Type: GPU
      Bus: PCI
      VRAM (Total): 8 MB
      Device ID: 0xbeef
      Revision ID: 0x0000
      Kernel Extension Info: No Kext Loaded
      Displays:
        Display:
          Resolution: 1920 x 1200
          Framebuffer Depth: 24-Bit Color (ARGB8888)
          Main Display: Yes
          Mirror: Off
          Online: Yes
          Automatically Adjust Brightness: No
      Vendor ID: 0x80ee
            /**/

            monitorList.Add(monitor);

            return monitorList;
        }

        public List<Motherboard> GetMotherboardList()
        {
            List<Motherboard> motherboardList = new List<Motherboard>();

            Motherboard motherboard = new Motherboard();

            motherboardList.Add(motherboard);

            return motherboardList;
        }

        public List<Mouse> GetMouseList()
        {
            List<Mouse> mouseList = new List<Mouse>();

            Mouse mouse = new Mouse();

            /*
            SPUSBDataType
USB:

    USB Bus:

      Host Controller Driver: AppleUSBOHCIPCI
      PCI Device ID: 0x003f 
      PCI Revision ID: 0x0000 
      PCI Vendor ID: 0x106b 

        USB Tablet:

          Product ID: 0x0021
          Vendor ID: 0x80ee
          Version: 1.00
          Speed: Up to 12 Mb/sec
          Manufacturer: VirtualBox
          Location ID: 0x06200000 / 2
          Current Available (mA): 500
          Current Required (mA): 100
          Extra Operating Current (mA): 0

        USB Keyboard:

          Product ID: 0x0010
          Vendor ID: 0x80ee
          Version: 1.00
          Speed: Up to 12 Mb/sec
          Manufacturer: VirtualBox
          Location ID: 0x06100000 / 1
          Current Available (mA): 500
          Current Required (mA): 100
          Extra Operating Current (mA): 0
            /**/

            mouseList.Add(mouse);

            return mouseList;
        }

        public override List<NetworkAdapter> GetNetworkAdapterList()
        {
            /*
            SPNetworkDataType
Network:

    Ethernet:

      Type: Ethernet
      Hardware: Ethernet
      BSD Device Name: en0
      IPv4 Addresses: 10.0.2.15
      IPv4:
          AdditionalRoutes:
              DestinationAddress: 10.0.2.15
              SubnetMask: 255.255.255.255
              DestinationAddress: 169.254.0.0
              SubnetMask: 255.255.0.0
          Addresses: 10.0.2.15
          ARPResolvedHardwareAddress: 52:54:00:12:35:02
          ARPResolvedIPAddress: 10.0.2.2
          Configuration Method: DHCP
          ConfirmedInterfaceName: en0
          Interface Name: en0
          Network Signature: IPv4.Router=10.0.2.2;IPv4.RouterHardwareAddress=52:54:00:12:35:02
          Router: 10.0.2.2
          Subnet Masks: 255.255.255.0
      IPv6:
          Configuration Method: Automatic
      DNS:
          Server Addresses: 192.168.0.1
      DHCP Server Responses:
          Domain Name Servers: 192.168.0.1
          Lease Duration (seconds): 0
          DHCP Message Type: 0x05
          Routers: 10.0.2.2
          Server Identifier: 10.0.2.2
          Subnet Mask: 255.255.255.0
      Ethernet:
          MAC Address: 08:00:27:5f:7a:7e
          Media Options: Full Duplex
          Media Subtype: 1000baseT
      Proxies:
          Exceptions List: *.local, 169.254/16
          FTP Passive Mode: Yes
      Service Order: 0
            /**/

            return base.GetNetworkAdapterList();
        }

        public List<Printer> GetPrinterList()
        {
            List<Printer> printerList = new List<Printer>();

            Printer printer = new Printer();

            // https://stackoverflow.com/questions/57617998/how-to-retrieve-installed-printers-on-macos-in-c-sharp

            // https://developer.apple.com/documentation/appkit/nsprinter

            // https://developer.apple.com/documentation/iokit/1424817-printer_class_requests

            /*
            SPPrintersDataType
Printers:
            /**/

            printerList.Add(printer);

            return printerList;
        }

        public List<SoundDevice> GetSoundDeviceList()
        {
            List<SoundDevice> soundDeviceList = new List<SoundDevice>();

            SoundDevice soundDevice = new SoundDevice();

            /*
            SPAudioDataType
Audio:

    Devices:
            /**/

            soundDeviceList.Add(soundDevice);

            return soundDeviceList;
        }

        public List<VideoController> GetVideoControllerList()
        {
            List<VideoController> videoControllerList = new List<VideoController>();

            VideoController videoController = new VideoController();

            // https://stackoverflow.com/questions/18077639/getting-graphic-card-information-in-objective-c

            // https://developer.apple.com/documentation/iokit/iographicslib_h

            /*
            SPDisplaysDataType
Graphics/Displays:

    Display:

      Type: GPU
      Bus: PCI
      VRAM (Total): 8 MB
      Device ID: 0xbeef
      Revision ID: 0x0000
      Kernel Extension Info: No Kext Loaded
      Displays:
        Display:
          Resolution: 1920 x 1200
          Framebuffer Depth: 24-Bit Color (ARGB8888)
          Main Display: Yes
          Mirror: Off
          Online: Yes
          Automatically Adjust Brightness: No
      Vendor ID: 0x80ee
            /**/

            videoControllerList.Add(videoController);

            return videoControllerList;
        }
    }
}
