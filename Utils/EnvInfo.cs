using Microsoft.Win32;
using System;
using System.Linq;
using System.Net;
using System.Windows;

namespace NanoTools2.Utils
{
    public static class EnvInfo
    {
        private static string getRegistryValue(string keyname, string valuename)
                                        => Registry.GetValue(keyname, valuename, "").ToString();

        public static string os_version { get; }
            = Environment.OSVersion.VersionString;

        public static string os_product_name { get; }
            = getRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");

        public static string os_release { get; }
            = getRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId");

        public static string os_build { get; }
            = getRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuild");

        public static string os_edition { get; }
            = getRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "EditionID");

        public static string os_bit { get; }
            = Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit";

        public static string process_bit { get; }
            = Environment.Is64BitProcess ? "64 bit" : "32 bit";

        public static string framework_version { get; }
            = Environment.Version.ToString();

        public static string registry_framework_version { get; }
            = getRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Version");

        public static string registry_framework_release { get; }
            = getRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release");

        public static string host_name { get; }
            = Dns.GetHostName();

        public static string machine_name { get; }
            = Environment.MachineName;

        public static string first_address { get; }
            = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().First().GetPhysicalAddress().ToString();

        public static bool IsDupliStart()
        {
            // var minePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var mine = System.Diagnostics.Process.GetCurrentProcess();
            var mineName = mine.ProcessName;

            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process p in ps)
            {
                try
                {
                    // if (p.MainModule.FileName.Equals(minePath) && mine.Id != p.Id)
                    if (p.ProcessName.Equals(mineName) && mine.Id != p.Id)
                    {
                        Console.WriteLine("ファイル名: {0}", p.MainModule.FileName);
                        return true;
                    }
                }
                catch (Exception ex)
                {   // エラー: アクセスが拒否されました。
                    Console.WriteLine("エラー: {0}", ex.Message);
                }
            }
            return false;
        }

        public static bool IsLicenceInvalid()
        {
            // Check Lic
            if (Approbate.IsNonApprobate(string.Empty))
            {
                //System.Threading.Thread.Sleep(3000);
                // var isMessageOk = MessageBox.Show("Demo License is expired.\nPlease inform MAC address to my support team.\n your MAC address: " + EnvInfo.first_address,
                var isMessageOk = MessageBox.Show("License is expired.",
                                             "Invalid license.", 
                                             MessageBoxButton.OK, 
                                             MessageBoxImage.Exclamation);

                // Display message box
                isMessageOk = MessageBox.Show(
                                                "License is expired.\nPlease inform your MAC address to my support team.\n\n MAC address: " + EnvInfo.first_address,
                                                "Invalid license.",
                                                MessageBoxButton.YesNo, 
                                                MessageBoxImage.Question);
                if (isMessageOk.Equals(MessageBoxResult.Yes))
                    System.Diagnostics.Process.Start("https://www.w-fusion.co.jp/J/contactus.php");

                // Application.Current.Shutdown();
                System.Threading.Thread.Sleep(2000);
                return true;
            }
            return false;
        }

        public static string tempPath = System.IO.Path.Combine(
                                        Environment.GetEnvironmentVariable("temp"),
                                        Approbate.LicenceFileName);

        public static void CreateDammyLicense()
        {
            var mes = string.Empty;
            var err = string.Empty;
            var words = new string[] { machine_name, first_address };
            WfComponent.Utils.FileUtils.WriteFile(tempPath, words, ref err);
        }

    }
}
