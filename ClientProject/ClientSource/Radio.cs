using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Text.Json;

namespace BarotraumaRadio // Client
{
<<<<<<< Updated upstream
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
                if (GameMain.Client is not null)
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
=======
    public partial class Radio : IServerSerializable
    {
>>>>>>> Stashed changes

        public bool RadioEnabled
        {
            get 
            { 
                return radioEnabled; 
            }
            private set
            {
                LuaCsSetup.PrintCsMessage("try start...");
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
<<<<<<< Updated upstream
                if ((!powered!.HasPower || !value) && radioEnabled)
=======
                LuaCsSetup.PrintCsMessage("next");
                if ((!powered.HasPower || !value) && radioEnabled)
>>>>>>> Stashed changes
                {
                    LuaCsSetup.PrintCsMessage("no power");
                    radioEnabled = false;
                    Stop();
                }
                if (radioEnabled == value || !powered.HasPower)
                {
                    return;
                }
                LuaCsSetup.PrintCsMessage("Set to " + value);
                radioEnabled = value;
                PlayAsync();
            }
        }

        [Serialize(1.0f, IsPropertySaveable.Yes, description: "The volume of the radio.")]
        public float Volume
        {
            get => volume;
            set => volume = MathHelper.Clamp(value, 0f, 1f);
        }

<<<<<<< Updated upstream
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
            if (ServerSync)
            {

                IWriteMessage message = GameMain.LuaCs.Networking.Start("ChangeStationFromClient");

                INetSerializableStruct dataStruct = new RadioDataStruct(item.ID, currentStationUrl);

                dataStruct.Write(message);
                GameMain.LuaCs.Networking.Send(message);
            }
        }

=======
>>>>>>> Stashed changes
        public async void PlayAsync()
        {
            LuaCsSetup.PrintCsMessage("Playing radio...");
#if CLIENT
<<<<<<< Updated upstream
            if (ServerSync && GameMain.Client is not null)
            {
                RequestStationFromServer();
            }
=======
            /*IWriteMessage message = GameMain.LuaCs.Networking.Start("RequestToServer");
            message.WriteString("hi");
            GameMain.LuaCs.Networking.Send(message);*/
>>>>>>> Stashed changes
            await Task.Run(() =>
            {
                try
                {
                    LuaCsSetup.PrintCsMessage("In start...");
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
                    LuaCsSetup.PrintCsMessage("Ok maybe...");
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
<<<<<<< Updated upstream
            currentStationIndex = GetNextStationIndex();
            currentStationUrl = radiostations[currentStationIndex].Url;
            UpdateConfig();
=======
            currentStationIndex = (currentStationIndex + 1) % radiostations.Length;

>>>>>>> Stashed changes
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
            RadioEnabled = !RadioEnabled;
        }

        public override void ReceiveSignal(Signal signal, Connection connection)
        {
            float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
            switch (connection.Name)
            {
                case "set_state":
                {
                    if (lastLeverValue != (value != 0f))
                    {
                        RadioEnabled = value != 0f;
                        lastLeverValue = value != 0f;
                    }
                    LuaCsSetup.PrintCsMessage("Radio enabled " + RadioEnabled);
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
            doSync = true;
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

        public void ClientEventRead(IReadMessage msg, float sendingTime)
        {
            RadioEnabled = msg.ReadBoolean();
            currentStationIndex = msg.ReadByte();
            volume = msg.ReadRangedSingle(0f, 1f, 8);
        }
    }
}
