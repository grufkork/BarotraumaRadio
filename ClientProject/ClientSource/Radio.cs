using Barotrauma;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace BarotraumaRadio.ClientSource
{
    public class Radio : ItemComponent, IDisposable
    {
        private readonly ContentXElement contentXElement;

        private string stationsPath = "";
        private string lastPlayedPath = "";

        private RadioItem[] radiostations =
        [
            new("AniSonFM", "https://pool.anison.fm/AniSonFM(320)"),
            new("truckers.fm", "http://radio.truckers.fm"),
            new("radioparadise.com", "http://stream.radioparadise.com/flacm"),
            new("somafm", "http://ice1.somafm.com/groovesalad-256-mp3"),
            new("kexp", "http://kexp-mp3-128.streamguys1.com/kexp128.mp3"),
            new("srg-ssr", "http://stream.srg-ssr.ch/m/rsj/mp3_128"),
            new("ClassicFM", "http://media-ssl.musicradio.com/ClassicFM"),
            new("radiocaroline", "http://sc5.radiocaroline.net:8040/stream"),
        ];

        private int radioArrayIndex = 0;

        private float volume = 0.85f;

        private bool radioEnabled = false;

        private readonly Powered powered;

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

        public Radio(Item item, ContentXElement element) : base(item, element)
        {
            LoadFromFile();
            contentXElement = element;
            contentXElement.Element.SetAttributeValue("volume", 1.0f);
            contentXElement.Element.SetAttributeValue("range", 2400.0f);
            powered = item.GetComponent<Powered>()!;
        }

        private void UpdateLastPlayed()
        {
            File.WriteAllText(lastPlayedPath, radioArrayIndex.ToString());
        }

        private void LoadFromFile()
        {
            string contentDirectory = FindContentDirectory();
            if (string.IsNullOrEmpty(contentDirectory))
            {
                LuaCsSetup.PrintCsError("Could not find content directory");
                return;
            }

            stationsPath = Path.Combine(contentDirectory, "radiostations.json");
            lastPlayedPath = Path.Combine(contentDirectory, "lastPlayed.txt");

            try
            {
                if (File.Exists(stationsPath))
                {
                    LuaCsSetup.PrintCsMessage("Successfully found stations file");
                    string stationsText = File.ReadAllText(stationsPath);
                    radiostations = JsonSerializer.Deserialize<RadioItem[]>(stationsText)!;
                }

                if (File.Exists(lastPlayedPath))
                {
                    LuaCsSetup.PrintCsMessage("Successfully found last played file");
                    int index = int.Parse(File.ReadAllText(lastPlayedPath));
                    radioArrayIndex = Math.Min(index, radiostations.Length - 1);
                }
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }
        }

        private string FindContentDirectory()
        {
            try
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string modDirectory = Path.GetDirectoryName(assemblyLocation)!;
                string contentPath = Path.Combine(modDirectory, "..", "..", "..", "Content");

                LuaCsSetup.PrintCsMessage("Going to check contentPath - " + contentPath);

                if (Directory.Exists(contentPath))
                {
                    return Path.GetFullPath(contentPath);
                }

                LuaCsSetup.PrintCsMessage("Failed to find content directory at" + contentPath);
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }

            try
            {
                string contentPath = Path.Combine("RadioMod", "Content");

                LuaCsSetup.PrintCsMessage("Going to check contentPath - " + contentPath);

                if (Directory.Exists(contentPath))
                {
                    return contentPath;
                }

                LuaCsSetup.PrintCsError("Failed to find content directory at" + contentPath);
            }
            catch (Exception ex)
            {
                LuaCsSetup.PrintCsError(ex);
            }

            return "";
        }


        public async void PlayAsync()
        {
#if CLIENT
            await Task.Run(() =>
            {
                try
                {
                    BufferSound radioSound = new(
                        GameMain.SoundManager,
                        "RadioStream",
                        stream: true,
                        streamsReliably: true,
                        radiostations[radioArrayIndex].Url);
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
                        foreach (Character character in GameMain.GameSession.CrewManager.GetCharacters())
                        {
                            if (character.IsLocalPlayer)
                            {
                                PlaySound(itemSound.Type, character);
                                return;
                            }
                        }
                        LuaCsSetup.PrintCsError("Could not find controlled character");
                    }
                }
                catch (Exception e)
                {
                    LuaCsSetup.PrintCsError(e);
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

        public void CycleChannels()
        {
            radioArrayIndex = (radioArrayIndex + 1) % radiostations.Length;

            UpdateLastPlayed();
            DisplayMessage($"Now playing {radiostations[radioArrayIndex].Name}");

            if (loopingSound != null)
            {
                if (loopingSound.RoundSound.Sound is BufferSound bufferSound)
                {
                    bufferSound.SwitchStation(radiostations[radioArrayIndex].Url);
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

        public override void ReceiveSignal(Signal signal, Connection connection)
        {
            float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
            switch (connection.Name)
            {
                case "set_state":
                {
                    RadioEnabled = value != 0f;
                    break;
                }
                case "switch_channel":
                {
                    CycleChannels();
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

        public void Dispose()
        {
            Stop();
        }
    }
}
