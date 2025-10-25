using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Reflection.Emit;
using static Barotrauma.Item;

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public void InitClient()
        {
            CreateStationHook("Radio");
            CreateVolumeHook("Radio");
            CreatePlayHook("Radio");

            harmony.Patch(
                original: typeof(ClientEntityEventManager).GetMethod("ReadEvent", BindingFlags.NonPublic | BindingFlags.Instance),
                prefix: new HarmonyMethod(typeof(Plugin).GetMethod(nameof(R_ReadEvent)))
            );
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

        private void CreatePlayHook(string name)
        {
            string hookName = name.ToLower() + ".play";
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
                        component.ChangeState();
                    }
                    catch (Exception e)
                    {
                        LuaCsSetup.PrintCsError("[VolumeHook]: " + e.Message);
                    }
                    return null;
                });
        }

        public static bool R_ReadEvent(IReadMessage buffer, IServerSerializable entity, float sendingTime)
        {
            try
            {
                entity.ClientEventRead(buffer, sendingTime);
            }
            catch
            {
                int newBitPosition = buffer.BitPosition + (int)buffer.ReadVariableUInt32() * 8;
                LuaCsSetup.PrintCsMessage($"[RadioMod]: Dropped exception, set new bit position = {newBitPosition}");
                buffer.BitPosition = newBitPosition;
            }
            return false;
        }

        private void CreateSyncHook(string name)
        {
            string hookName = name.ToLower() + ".sync";
        }
    }
}
