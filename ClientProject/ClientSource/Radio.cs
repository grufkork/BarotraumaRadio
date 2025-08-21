using Barotrauma;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace BarotraumaRadio.ClientSource
{
    public class Radio : ItemComponent, IDisposable
    {
        private readonly ContentXElement contentXElement;

        private readonly RadioItem[] radioChannels =
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
            contentXElement = element;
            contentXElement.Element.SetAttributeValue("volume", 1.0f);
            contentXElement.Element.SetAttributeValue("range", 2400.0f);
            powered = item.GetComponent<Powered>()!;
        }

        public async void PlayAsync()
        {
#if CLIENT
            await Task.Run(() =>
            {
                try
                {
                    BufferSound radioSound = new(GameMain.SoundManager, "RadioStream",
                        stream: true, streamsReliably: true, radioChannels[radioArrayIndex].Url);
                    RoundSound roundSound = new(contentXElement, radioSound);
                    ItemSound itemSound = new(roundSound, ActionType.Always,
                        loop: true, onlyPlayInSameSub: false)
                    {
                        VolumeProperty = "Volume".ToIdentifier()
                    };
                    SetParentFields(itemSound);
                    PlaySound(itemSound.Type, GameMain.Client.Character);
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
            sounds.Add(itemSound.Type, [itemSound]);
            soundSelectionModes = new Dictionary<ActionType, SoundSelectionMode>
                {
                    { itemSound.Type, SoundSelectionMode.ItemSpecific }
                };
            hasSoundsOfType[(int)itemSound.Type] = true;
        }

        public void CycleChannels()
        {
            radioArrayIndex = (radioArrayIndex + 1) % radioChannels.Length;

            DisplayMessage($"Now playing {radioChannels[radioArrayIndex].Name}");

            if (loopingSound != null)
            {
                if (loopingSound.RoundSound.Sound is BufferSound bufferSound)
                {
                    bufferSound.SwitchStation(radioChannels[radioArrayIndex].Url);
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
            sounds?.Remove(type);
            soundSelectionModes?.Remove(type);
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
