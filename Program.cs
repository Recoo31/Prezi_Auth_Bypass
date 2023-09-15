using System;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace PreziBypass
{
    class Program
    {
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
        
        static void Main()
        {
            var proxyServer = InitializeProxy();
            SetProxy("127.0.0.1:8000");
            Console.WriteLine(@"                                                  
                                                          
                          :     ..     :                  
                   .      -.    --    .-      .           
                    :.    .=.   ==   .=.    .:            
                     --.   ==. .==. .==   .--             
              .:.     -=-  -==.:==:.==-  -=-     .:.      
                .--:   -==::===-==-===::==-   :--.        
                  .-=-:.-================-.:-=-.          
           .::..    :========================:    ..::.   
              .:-==---======================---==-:.      
                  :-==========================-:          
          .:::::::---========================---:::::::.  
             ...::--==========================--::...     
                 .:-==========================-:.         
            .:-====--========================--====-:.    
          ..       .-========================-.       ..  
                 :===-:====================:-===:         
              .--:.  .-==-==============-==-.  .:--.      
            ..      :==-.:===-======-===:.-==:      ..    
                   --:   ==- :==::==: -==   :--           
                 .:.    -=:  :=-  -=:  :=-    .:.         
                       .=.   :=.  .=:   .=.               
                       -     --    --     -               
                      .      -      -      .              
                             .      .                     
                         
");
            Console.WriteLine("Waiting...");
            while (true) { }
        }

        private static ProxyServer InitializeProxy()
        {
            var proxyServer = new ProxyServer();
            proxyServer.BeforeResponse += OnResponse;

            var explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, 8000, true);
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();

            return proxyServer;
        }

        private static async Task OnResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == "https://prezi.com/api/desktop/license/json/")
            {
                var bodyString = await e.GetResponseBodyAsString();
                Console.WriteLine("Bypassing...");
                
                if (bodyString.Contains("false"))
                {
                    bodyString = bodyString.Replace("false", "true");
                    e.SetResponseBody(System.Text.Encoding.UTF8.GetBytes(bodyString));
                    Console.WriteLine("Done...");
                    DisableProxy();
                }
            }
        }

        private static void SetProxy(string proxyAddress)
        {
            UpdateRegistrySettings(1, proxyAddress);
            NotifyOSSettingsChanged();
        }

        private static void DisableProxy()
        {
            UpdateRegistrySettings(0);
            NotifyOSSettingsChanged();
        }

        private static void UpdateRegistrySettings(int enable, string proxyAddress = null)
        {
            using RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", writable: true);
            registry.SetValue("ProxyEnable", enable);
            
            if (proxyAddress != null)
            {
                registry.SetValue("ProxyServer", proxyAddress);
            }
        }

        private static void NotifyOSSettingsChanged()
        {
            _ = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            _ = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}
