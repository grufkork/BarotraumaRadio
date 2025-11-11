using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Text.Json;

namespace BarotraumaRadio
{
    public partial class Radio : ItemComponent
    {
        public bool ServerSync
        {
            get
            {
                return serverSync;
            }
            set
            {
                if (GameMain.Client is not null && GameMain.IsMultiplayer)
                {
                    serverSync = value;
                }
                else
                {
                    serverSync = false;
                }
                UpdateConfig();
                if (serverSync)
                {
                    DisplayMessage("Server sync enabled");
                    RequestStationFromServer();
                }
                else
                {
                    DisplayMessage("Server sync disabled");
                    string newStation = radiostations[currentStationIndex].Url;
                    if (currentStationUrl != newStation)
                    {
                        currentStationUrl = newStation;
                        ChangeStation();
                    }
                }
            }
        }

        public bool RadioEnabled
        {
            get 
            { 
                return radioEnabled; 
            }
            set
            {
                if (powered == null)
                {
                    if (TryGetPoweredComponent(out Powered? component))
                    {
                        powered = component;
                    }
                    else
                    {
                        return;
                    }
                }
                if ((!powered!.HasPower || !value) && radioEnabled)
                {
                    radioEnabled = false;
                    DisplayMessage($"Turned Off");
                    Stop();
                }
                if (radioEnabled == value || !powered.HasPower)
                {
                    return;
                }
                radioEnabled = value;
                DisplayMessage($"Turned On");
                PlayAsync();
            }
        }

        [Serialize(1.0f, IsPropertySaveable.Yes, description: "The volume of the radio.")]
        public float Volume
        {
            get => volume;
            set => volume = MathHelper.Clamp(value, 0f, 1f);
        }

        private void RequestStationFromServer()
        {
            IWriteMessage message = GameMain.LuaCs.Networking.Start("RequestStationFromClient");

            INetSerializableStruct dataStruct = new RadioDataStruct(item.ID, "");

            dataStruct.Write(message);
            GameMain.LuaCs.Networking.Send(message);
        }

        private void UpdateConfig()
        {
            ClientRadioConfig config = new(currentStationIndex, Volume, ServerSync);
            string serializedConfig = JsonSerializer.Serialize(config);
            File.WriteAllText(clientConfigPath, serializedConfig);
        }

        private void SendStationToServer()
        {
            GameMain.LuaCs.PrintMessage("Sending station to server: " + currentStationUrl + " sync is " + ServerSync);
            if (ServerSync)
            {

                IWriteMessage message = GameMain.LuaCs.Networking.Start("ChangeStationFromClient");

                INetSerializableStruct dataStruct = new RadioDataStruct(item.ID, currentStationUrl);

                dataStruct.Write(message);
                GameMain.LuaCs.Networking.Send(message);
            }
        }

        public async void PlayAsync()
        {
#if CLIENT
            if (ServerSync && GameMain.Client is not null)
            {
                RequestStationFromServer();
            }
            await Task.Run(() =>
            {
                try
                {
                    BufferSound radioSound = new(
                        GameMain.SoundManager,
                        "RadioStream",
                        stream: true,
                        streamsReliably: true,
                        radiostations[currentStationIndex].Url);
                    RoundSound roundSound = new(contentXElement, radioSound);
                    ItemSound itemSound = new(
                        roundSound,
                        ActionType.Always,
                        loop: true,
                        onlyPlayInSameSub: false)
                    {
                        VolumeProperty = "Volume".ToIdentifier()
                    };
                    SetParentFields(itemSound);

                    if (GameMain.Client is not null)
                    {
                        PlaySound(itemSound.Type, GameMain.Client.Character);
                    }
                    else
                    {
                        Character? character = GameMain.GameSession.CrewManager!.GetCharacters().Where(c => c.IsLocalPlayer).FirstOrDefault();
                        if (character == null)
                        {
                            LuaCsSetup.PrintCsError("Could not find controlled character");
                            return;
                        }
                        PlaySound(itemSound.Type, character);
                    }
                }
                catch (Exception e)
                {
                    LuaCsSetup.PrintCsError("[PlayAsync]: " + e.Message);
                }
            });
#endif
        }

        private void SetParentFields(ItemSound itemSound)
        {
            if (hasSoundsOfType[(int)itemSound.Type])
            {
                return;
            }
            if (!sounds.ContainsKey(itemSound.Type))
            {
                sounds.Add(itemSound.Type, [itemSound]);
            }
            soundSelectionModes = new Dictionary<ActionType, SoundSelectionMode>
                {
                    { itemSound.Type, SoundSelectionMode.ItemSpecific }
                };
            hasSoundsOfType[(int)itemSound.Type] = true;
        }

        public void CycleStations()
        {
            currentStationIndex = GetNextStationIndex();
            currentStationUrl = radiostations[currentStationIndex].Url;
            UpdateConfig();
            DisplayMessage($"Now playing {radiostations[currentStationIndex].Name}");
            SendStationToServer();
            ChangeStation();
        }

        public void ChangeStation()
        {
            if (loopingSound != null)
            {
                if (loopingSound.RoundSound.Sound is BufferSound bufferSound)
                {
                    bufferSound.SwitchStation(currentStationUrl);
                }
            }
        }

        public void CycleVolume()
        {
            Volume = GetNextVolumeValue();
            UpdateConfig();
            DisplayMessage($"Current volume is {(int)(Volume * 100)}%");
        }

        public float GetNextVolumeValue()
        {
            return Volume == 1f ? 0f : Math.Min(1f, Volume + 0.15f);
        }

        public int GetNextStationIndex()
        {
            return (currentStationIndex + 1) % radiostations.Length;
        }

        private void DisplayMessage(string message)
        {
            GUI.AddMessage(message, Color.Orange, new Vector2(Item.WorldPositionX, Item.WorldPositionY + 15), Vector2.Zero);
        }

        public void ChangeState()
        {
            GameMain.LuaCs.PrintMessage("Toggling state from client");
            RadioEnabled = !RadioEnabled;
            GameMain.LuaCs.PrintMessage("Sync? " + ServerSync);
            if (ServerSync) 
            {
                GameMain.LuaCs.Networking.Start("SetStateFromClient");
                IWriteMessage message = GameMain.LuaCs.Networking.Start("SetStateFromClient");
                INetSerializableStruct dataStruct = new PlayDataStruct(item.ID, RadioEnabled);
                dataStruct.Write(message);
                GameMain.LuaCs.Networking.Send(message);
                GameMain.LuaCs.PrintMessage("State toggled to " + RadioEnabled);
            }
        }

        public override void ReceiveSignal(Signal signal, Connection connection)
        {
            float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
            switch (connection.Name)
            {
                case "set_state":
                {
                    ChangeState();
                    break;
                }
                case "switch_channel":
                {
                    CycleStations();
                    break;
                }
                case "change_volume":
                {
                    CycleVolume();
                    break;
                }
            }
        }

        private void ClearParentFields(ActionType type)
        {
            if (sounds is not null && sounds.ContainsKey(type))
            {
                sounds?.Remove(type);
            }
            if (soundSelectionModes is not null && soundSelectionModes.ContainsKey(type))
            { 
                soundSelectionModes?.Remove(type);
            }
            if (hasSoundsOfType is not null && hasSoundsOfType.Length >= (int)type)
            {
                hasSoundsOfType[(int)type] = false;
            }
        }

        public void Stop()
        {
#if CLIENT
            ActionType type = ActionType.Always;
            if (loopingSound != null)
            {
                type = loopingSound.Type;
            }
            StopSounds(type);
            ClearParentFields(type);
#endif
        }
    }
}
