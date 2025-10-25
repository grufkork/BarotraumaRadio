using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Steam;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public static Plugin Instance;
        public readonly Harmony harmony = new Harmony("plag.barotrauma.radio");

        public void Initialize()
        {
            Instance = new Plugin();
#if CLIENT
            LuaCsSetup.PrintCsMessage("init client started");
            InitClient();
#elif SERVER
            LuaCsSetup.PrintCsMessage("init server started");
            InitServer();
#endif
        }

        public void OnLoadCompleted()
        {
            // After all plugins have loaded
            // Put code that interacts with other plugins here.
        }

        public void PreInitPatching()
        {
            // Not yet supported: Called during the Barotrauma startup phase before vanilla content is loaded.
        }

        public void Dispose()
        {
        }
    }
}
