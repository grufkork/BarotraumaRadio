using Barotrauma;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace BarotraumaRadio
{
    public partial class Radio : ItemComponent, IServerSerializable, IClientSerializable
    {
        public bool RadioEnabled
        {
            get 
            { 
                return radioEnabled; 
            }
            private set
            {   
                if ((!powered.HasPower || !value) && radioEnabled)
                {
                    radioEnabled = false;
                    Stop();
                }
                if (radioEnabled == value || !powered.HasPower)
                {
                    return;
                }
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

        private void UpdateLastPlayed()
        {
            File.WriteAllText(lastPlayedPath, currentStationIndex.ToString());
        }

        public async void PlayAsync()
        {
#if CLIENT
            int count = 0;
            foreach (ItemComponent component in item.Components)
            {
                LuaCsSetup.PrintCsMessage($"[{count}]: " + component.ToString());
                count++;
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
            currentStationIndex = (currentStationIndex + 1) % radiostations.Length;

            UpdateLastPlayed();
            DisplayMessage($"Now playing {radiostations[currentStationIndex].Name}");

            if (loopingSound != null)
            {
                if (loopingSound.RoundSound.Sound is BufferSound bufferSound)
                {
                    bufferSound.SwitchStation(radiostations[currentStationIndex].Url);
                }
            }
        }

        public void CycleVolume()
        {
            Volume = Volume == 1f ? 0f : Math.Min(1f, Volume + 0.15f);

            DisplayMessage($"Current volume is {(int)(Volume * 100)}%");
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
            if (sounds.ContainsKey(type))
            {
                sounds?.Remove(type);
            }
            if (soundSelectionModes.ContainsKey(type))
            { 
                soundSelectionModes?.Remove(type);
            }
            hasSoundsOfType[(int)type] = false;
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
        }

        public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
        {
        }
    }
}
