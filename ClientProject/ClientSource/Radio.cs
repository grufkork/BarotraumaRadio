using Barotrauma;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace BarotraumaRadio.ClientSource
{
    public class Radio : ItemComponent, IDisposable
    {
        private readonly ContentXElement contentXElement;

        private readonly string[] radioArray =
        {
            "https://pool.anison.fm/AniSonFM(320)",
            "http://radio.truckers.fm",
            "http://stream.radioparadise.com/flacm",
            "http://ice1.somafm.com/groovesalad-256-mp3",
            "http://kexp-mp3-128.streamguys1.com/kexp128.mp3",
            "http://stream.srg-ssr.ch/m/rsj/mp3_128",
            "http://media-ssl.musicradio.com/ClassicFM",
            "http://sc5.radiocaroline.net:8040/stream"
        };
        private readonly string[] radioNamesArray =
{
            "AniSonFM",
            "truckers.fm",
            "radioparadise.com",
            "somafm",
            "kexp",
            "srg-ssr",
            "ClassicFM",
            "radiocaroline"
        };

        private int radioArrayIndex = 0;

        private float volume = 0.85f;

        private bool radioRestarting = false;

        private bool radioEnabled = false;

        private Powered? powered;

        public bool RadioEnabled
        {
            get 
            { 
                return radioEnabled; 
            }
            private set
            {
                powered = item.GetComponent<Powered>();
                if (powered == null)
                {
                    LuaCsSetup.PrintCsError($"Error Getting power component");
                    return;
                }
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
                new Thread(Play).Start();
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
        }

        public void Play()
        {
#if CLIENT
            try
            {
                PlayItemSound();
            }
            catch (Exception e)
            {
                LuaCsSetup.PrintCsError(e);
            }
#endif
        }

        private void PlayItemSound()
        {
            BufferSound radioSound = new(GameMain.SoundManager, "RadioStream", stream: true, streamsReliably: true, radioArray[radioArrayIndex]);
            RoundSound  roundSound = new(contentXElement, radioSound);
            ItemSound   itemSound  = new(roundSound, ActionType.Always, loop: true, onlyPlayInSameSub: false)
            {
                VolumeProperty = "Volume".ToIdentifier()
            };
            SetParentFields(itemSound);
            PlaySound(itemSound.Type, GameMain.Client.Character);
        }

        private void SetParentFields(ItemSound itemSound)
        {
            sounds.Add(itemSound.Type, [itemSound]);
            loopingSound = itemSound;
            soundSelectionModes = new Dictionary<ActionType, SoundSelectionMode>
            {
                { itemSound.Type, SoundSelectionMode.ItemSpecific }
            };
            hasSoundsOfType[(int)itemSound.Type] = true;
        }

        private void ClearParentFields(ActionType type)
        {
            sounds?.Remove(type);
            soundSelectionModes?.Remove(type);
            hasSoundsOfType[(int)type] = false;
        }

        public void CycleChannels()
        {
            if (radioRestarting)
            {
                return;
            }
            if (++radioArrayIndex == radioArray.Length)
            {
                radioArrayIndex = 0;
            }
            string message = $"Now playing {radioNamesArray[radioArrayIndex]}";
            new Thread(delegate() { RestartRadio(message); }).Start();
        }

        public void CycleVolume()
        {
            if (radioRestarting)
            {
                return;
            }
            if (Volume == 1f)
            {
                Volume = 0f;
            }
            else
            {
                Volume += 0.15f;
            }
            string message = $"Current volume is {(int)(Volume * 100)}%";
            new Thread(delegate () { RestartRadio(message); }).Start();
        }

        public void RestartRadio(string restartMessage)
        {
            if (loopingSoundChannel != null && !radioRestarting)
            {
                GUI.AddMessage(restartMessage, Color.Orange, new Vector2(Item.WorldPositionX, Item.WorldPositionY + 15), Vector2.Zero);
                radioRestarting = true;

                Thread.Sleep(500);
                RadioEnabled = false;
                Thread.Sleep(500);
                RadioEnabled = true;

                radioRestarting = false;
            }
        }

        public override void ReceiveSignal(Signal signal, Connection connection)
        {
            float value;
            float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
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
