using Barotrauma;
using Barotrauma.Networking;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public void InitClient()
        {
            CreateStationHook("Radio");
            CreateVolumeHook("Radio");
            CreatePlayHook("Radio");
        }

        private static Radio? FindClosestRadio(Item remote)
        {
            Item? RadioItem = Item.ItemList
                .Where(it => it.Name == "Radio" && Vector2.Distance(it.WorldPosition, remote.WorldPosition) <= 500)
                .OrderBy(it => Vector2.Distance(it.WorldPosition, remote.WorldPosition))
                .FirstOrDefault();

            if (RadioItem == null)
            {
                GUI.AddMessage("No Radio nearby", Color.Orange, new Vector2(remote.WorldPositionX, remote.WorldPositionY + 15), Vector2.Zero);
                return null;
            }

            Radio component = RadioItem.GetComponent<Radio>();

            return component;
        }

        private void CreateStationHook(string name)
        {
            string hookName = name.ToLower() + ".station";
            GameMain.LuaCs.Hook.Add(hookName, hookName,
                (object[] args) => {
                    Item item = (Item)args[2];
                    try
                    {
                        Radio? component = FindClosestRadio(item);

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
                        Radio? component = FindClosestRadio(item);
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
                        Radio? component = FindClosestRadio(item);
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

        private void CreateSyncHook(string name)
        {
            string hookName = name.ToLower() + ".sync";
        }
    }
}
