using Barotrauma;
using Barotrauma.Items.Components;
using BarotraumaRadio.ClientSource;
using System.Runtime.CompilerServices;

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public void InitClient()
        {
            InjectComponent<Radio>("Radio");
            CreateStationHook("Radio");
            CreateVolumeHook("Radio");
        }

        private void CreateStationHook(string name)
        {
            string hookName = name.ToLower() + ".station";
            GameMain.LuaCs.Hook.Add(hookName, hookName,
                (object[] args) => {
                    Item item = (Item)args[2];
                    try
                    {
                        Radio component = item.GetComponent<Radio>();
                        if (component == null)
                        {
                            return null;
                        }
                        component.CycleStations();
                    }
                    catch (Exception e)
                    {
                        LuaCsSetup.PrintCsError("[StationHook]: " + e.Message);
                    }
                    return null;
                });
        }

        private void CreateVolumeHook(string name)
        {
            string hookName = name.ToLower() + ".volume";
            GameMain.LuaCs.Hook.Add(hookName, hookName,
                (object[] args) => {
                    Item item = (Item)args[2];
                    try
                    {
                        Radio component = item.GetComponent<Radio>();
                        if (component == null)
                        {
                            return null;
                        }
                        component.CycleVolume();
                    }
                    catch (Exception e)
                    {
                        LuaCsSetup.PrintCsError("[VolumeHook]: " + e.Message);
                    }
                    return null;
                });
        }

        private void CreateSyncHook(string name)
        {
            string hookName = name.ToLower() + ".sync";
        }
    }
}
