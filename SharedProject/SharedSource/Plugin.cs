using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma;
using Barotrauma.Items.Components;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public static Plugin Instance;

        public void Initialize()
        {
            Instance = new Plugin();
            InjectRadioComponent();
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

        public void InjectRadioComponent()
        {
            GameMain.LuaCs.Hook.Add("radio.spawn", "radio.spawn", (object[] args) => {
                Item item = (Item)args[2];

                if (item.GetComponent<Radio>() != null) return null;

                try
                {
                    Radio radioComponent = new(item, new ContentXElement(null, new XElement("Radio")));
                    item.AddComponent(radioComponent);
                }
                catch (Exception ex)
                {
                    LuaCsSetup.PrintCsError($"[Radio spawn hook]: {ex.Message}");
                }

                return null;
            });
        }

        public void Dispose()
        {
        }
    }
}
