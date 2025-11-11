using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace BarotraumaRadio
{
    public partial class Plugin : IAssemblyPlugin
    {
        public void InitClient()
        {
            CreateStationHook("Radio");
            CreateVolumeHook("Radio");
            CreatePlayHook("Radio");
            CreateSyncHook("Radio");
            SetClientSyncCallback();
        }

        private void SetClientSyncCallback()
        {
            GameMain.LuaCs.Networking.Receive("ChangeStationFromServer", (object[] args) =>
            {
                IReadMessage message = (IReadMessage)args[0];
                RadioDataStruct dataStruct = INetSerializableStruct.Read<RadioDataStruct>(message);
                Item? item = Item.ItemList.FirstOrDefault(serverItem => serverItem.ID == dataStruct.RadioID);
                if (item == null)
                    return;
                ItemComponent? component = item.Components.FirstOrDefault(c => c is Radio);
                if (component != null && component is Radio radioComponent && radioComponent.ServerSync)
                {
                    if (radioComponent.currentStationUrl == dataStruct.ParamValue)
                    {
                        return;
                    }
                    radioComponent.currentStationUrl = dataStruct.ParamValue!;
                    radioComponent.ChangeStation();
                }
            });

            GameMain.LuaCs.Networking.Receive("SetStateFromServer", (object[] args) =>
            {
                GameMain.LuaCs.PrintMessage("Received SetStateFromServer");
                IReadMessage message = (IReadMessage)args[0];
                PlayDataStruct dataStruct = INetSerializableStruct.Read<PlayDataStruct>(message);
                Item? item = Item.ItemList.FirstOrDefault(serverItem => serverItem.ID == dataStruct.RadioID);
                if (item == null)
                    return;
                ItemComponent? component = item.Components.FirstOrDefault(c => c is Radio);
                if (component != null && component is Radio radioComponent && radioComponent.ServerSync)
                {
                    if(radioComponent.RadioEnabled == dataStruct.Playing)
                    {
                        GameMain.LuaCs.PrintMessage("Radio state already set to " + dataStruct.Playing);
                        return;
                    }
                    radioComponent.RadioEnabled = dataStruct.Playing;
                }
                GameMain.LuaCs.PrintMessage("Radio state set to " + dataStruct.Playing);
            });

            GameMain.LuaCs.Networking.Receive("SendStringFromServer", (object[] args) =>
            {
                IReadMessage message = (IReadMessage)args[0];
                string data = message.ReadString();
                LuaCsSetup.PrintCsMessage(data);
            });
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
                        if (GameMain.Client is not null)
                        {
                            if (!GameMain.Client.Character.HeldItems.Contains(item))
                            {
                                return null;
                            }

                            component.CycleStations();
                        }
                        else
                        {
                            component.CycleStations();
                        }
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
                        if (GameMain.Client is not null && !GameMain.Client.Character.HeldItems.Contains(item))
                        {
                            return null;
                        }
                        else
                        {              
                            component.CycleVolume();
                        }
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
                        if (GameMain.Client is not null && !GameMain.Client.Character.HeldItems.Contains(item))
                        {
                            return null;
                        }
                        else
                        {
                            component.ServerSync = !component.ServerSync;
                        }
                    }
                    catch (Exception e)
                    {
                        LuaCsSetup.PrintCsError("[VolumeHook]: " + e.Message);
                    }
                    return null;
                });
        }
    }
}
