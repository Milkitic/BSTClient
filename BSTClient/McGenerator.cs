using System;
using System.Linq;
using System.Management;

namespace BSTClient
{
    public static class McGenerator
    {
        private static readonly string TemporaryCode = Guid.NewGuid().ToString();
        public static string GetMachineCode()
        {
            try
            {
                string cpuId = GetCPU();
                cpuId = cpuId.Substring(cpuId.Length - 4);
                string hdId = GetHardDisk();
                hdId = hdId.Substring(hdId.Length - 4);
                var machineCode = cpuId + hdId;
                return machineCode == "00000000" ? TemporaryCode : machineCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching machine code: {ex.Message}");
                return TemporaryCode;
            }
        }

        private static string GetCPU()
        {
            string text = "0000";
            try
            {
                var instances = new ManagementClass("win32_Processor").GetInstances();
                foreach (var managementObject in instances)
                {
                    text = managementObject.Properties["Processorid"].Value?.ToString()?.Trim();
                    if (text != null) break;
                }

                return text?.Length >= 4 ? text.ToUpper() : "0000";
            }
            catch (Exception arg)
            {
                Console.WriteLine("Error while fetching CPU serial code:" + arg);
                return text;
            }
            finally
            {
                //Console.WriteLine($"CPU: {text}");
            }
        }

        private static string GetHardDisk()
        {
            string text = "0000";
            try
            {
                using (var managementClass = new ManagementClass("Win32_DiskDrive"))
                using (var managementClass2 = new ManagementClass("Win32_BootConfiguration"))
                using (var source2 = managementClass.GetInstances())
                using (var source = managementClass2.GetInstances())
                {
                    var bootConfigs = source.Cast<ManagementBaseObject>();
                    var diskDrivers = source2.Cast<ManagementBaseObject>();
                    var firstBootConfig = (string)bootConfigs.FirstOrDefault()?.Properties["Caption"].Value;
                    string[] firstBootConfigSplit =
                        firstBootConfig?.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

                    string value;
                    if (firstBootConfigSplit != null && firstBootConfigSplit.Length > 2)
                    {
                        string index = firstBootConfigSplit[1].Substring(firstBootConfigSplit[1].Length - 1);
                        var managementBaseObject = diskDrivers.First(k =>
                        {
                            string text3 = (string)k.Properties["DeviceID"].Value;
                            return text3.Substring(text3.Length - 1) == index;
                        });
                        value = (string)managementBaseObject.Properties["SerialNumber"].Value;
                    }
                    else
                    {
                        var managementBaseObject2 = diskDrivers.First(k =>
                            k.Properties["SerialNumber"].Value != null);
                        value = (string)managementBaseObject2.Properties["SerialNumber"].Value;
                    }

                    text = value.Replace("-", "").Trim();
                }

                text = text.Length < 4 ? "0000" : text.ToUpper();
                return text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching hard disk serial code: {ex.Message}");
                return text;
            }
            finally
            {
                //Console.WriteLine($"HardDisk: {text}");
            }
        }
    }
}
