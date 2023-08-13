// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Diagnostics;
using System.Management;
using System.Text.Json;

namespace PacketLogistics.Tools
{
    internal class ComPortInfoManager
    {
        private static readonly TimeSpan MaxCacheAge = TimeSpan.FromDays(1);
        private static ComPortInfo[]? _portInfos;

        private static bool _wasCached;
        private readonly bool _ignoreBluetoothPorts;

        private static string CacheFilename => Path.Combine(Path.GetTempPath(), "PacketLogisticsComPortInfosCache.Json");

        private static ComPortInfo[]? CachedOnDisk
        {
            get
            {
                if (File.Exists(CacheFilename))
                {
                    var fileInfo = new FileInfo(CacheFilename);
                    var fileDate = fileInfo.LastWriteTimeUtc;
                    if (DateTime.UtcNow - fileDate < MaxCacheAge)
                    {
                        string jsonString = File.ReadAllText(CacheFilename);
                        var value = JsonSerializer.Deserialize<ComPortInfo[]>(jsonString);
                        if (value != null) return value;
                    }
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    if (File.Exists(CacheFilename))
                    {
                        File.Delete(CacheFilename);
                    }
                }
                else
                {
                    var jsonString = JsonSerializer.Serialize(value);
                    File.WriteAllText(CacheFilename, jsonString);
                }
            }
        }

        public ComPortInfo[] PortInfos
        {
            get
            {
                if (_portInfos == null)
                {
                    var cached = CachedOnDisk;
                    if (cached?.Any() == true)
                    {
                        _portInfos = cached;
                        _wasCached = true;
                    }
                    else
                    {
                        _portInfos = CalculatePorts();
                        _wasCached = false;
                    }
                }
                return _portInfos;
            }
        }

        public ComPortInfoManager(bool ignoreBluetoothPorts = true)
        {
            this._ignoreBluetoothPorts = ignoreBluetoothPorts;
        }

        private ComPortInfo[] CalculatePorts()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
            {
                var raw = searcher.Get().Cast<ManagementBaseObject>();
                var ports = raw
                    .Select(p => GetComPortInfoFromManagementBaseObject(p))
                    .Where(p =>
                    p != null &&
                    p.ComPort != null &&
                    p.ComPort.StartsWith("com", ignoreCase: true, null) &&
                    (_ignoreBluetoothPorts == false || !p.Caption.Contains("bluetooth", StringComparison.OrdinalIgnoreCase)))
                    .Select(p => p!)
                    .ToArray();
                CachedOnDisk = ports;
                return ports;
            }
        }
       
        public void ClearCache()
        {
            _portInfos = null;
            _wasCached = false;
            _portInfos = CalculatePorts();
        }

        public ComPortInfo GetComPortInfoByCaption(string caption)
        {
            var ports = GetComPortInfosByCaption(caption);
            if (ports.Length != 1 && _wasCached)
            {
                ClearCache();
                ports = GetComPortInfosByCaption(caption);
            }

            var allFound = string.Join(",\r\n", PortInfos.Select(p => $"Caption:'{p.CaptionCleaned}'/ComPort:'{p.ComPort}'"));

            switch (ports.Length)
            {
                case 0: throw new Exception($"Com Port '{caption}'not found!\r\nFound\r\n{allFound}");
                case 1: return ports[0];
                default: throw new Exception($"More then one Com Port '{caption}' found! Found \r\n{string.Join(",\r\n", ports.Select(p => $"Caption:'{p.CaptionCleaned}'/ComPort:'{p.ComPort}'"))}");
            }
        }

        public ComPortInfo? GetComPortInfoByName(string comPortName) =>
            PortInfos.SingleOrDefault(p => comPortName.Equals(p.ComPort, StringComparison.InvariantCultureIgnoreCase));

        public ComPortInfo[] GetComPortInfosByCaption(string caption) =>
            PortInfos.Where(p => caption.Equals(p.CaptionCleaned, StringComparison.InvariantCultureIgnoreCase)).ToArray();

        private static string? ComPortNameFromCaption(string caption)
        {
            if (string.IsNullOrWhiteSpace(caption)) return null;
            var parts = caption.Split(new char[] { ')', '(' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1) return parts.Last();
            return null;
        }

        private static ComPortInfo? GetComPortInfoFromManagementBaseObject(ManagementBaseObject p)
        {
            var deviceId = p["DeviceID"]?.ToString();
            var caption = p["Caption"]?.ToString();
            if (deviceId == null || caption == null) return null;

            var comPort = ComPortNameFromCaption(caption);
            if (comPort == null) return null;

            return new ComPortInfo(deviceId: deviceId, caption: caption, comPort: comPort);
        }
    }
}
