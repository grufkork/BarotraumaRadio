# Barotrauma Radio Mod

[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/gitempERROR/BarotraumaRadio)
[![Steam Workshop](https://img.shields.io/badge/Steam-Workshop-000000?style=for-the-badge&logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3559932434)

An immersive radio mod for Barotrauma that adds craftable radios to submarines. Control your radio through the game's wiring system or toss the remote to a crewmate - perfect for battling the deep-sea blues during those long delivery missions.

## Features

- **Fabricator Crafting**: Create radios at the fabricator and install them anywhere on your submarine
- **Dual Control Options**: Operate via in-game wiring system OR use a dedicated remote control
- **Built-in Remote Storage**: The radio has a dedicated slot to store the remote
- **Server Synchronization**: Optional server-side sync for consistent multiplayer experience
- **Pre-configured Stations**: Includes popular internet radio stations
- **Easy Customization**: Add your own radio stations via simple config file

## Installation

1. Subscribe to the mod on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3559932434)
2. Enable the mod in Barotrauma's mod menu
3. The mod will be available in your next game session

## Adding Custom Radio Stations

To add your own radio stations:

1. Navigate to the mod folder: `C:\Users\User\AppData\Local\Daedalic Entertainment GmbH\Barotrauma\WorkshopMods\Installed\3559932434\Content`
2. Open `radioStations.json` in a text editor
3. Add new stations in JSON format:
```json
{
  "Name": "Your Station Name",
  "Url": "http://your.radio.stream/url"
}
```
4. Save the file and launch the game

## Control Options

### Wiring Control
The radio supports full wiring integration:
- **Power line**: Connect to submarine's power grid
- **Switch**: To turn the radio on/off
- **Button**: For switching between stations
- **Button**: For volume control

### Remote Control
- Use the dedicated remote control for wireless operation
- Remote can be stored directly in the radio unit
- Perfect for passing control to other crew members

## Pre-installed Radio Stations

- **AniSonFM**: Japanese anime music and soundtracks
- **Truckers FM**: Music and talk for the open road
- **Radio Paradise**: Carefully curated eclectic music mix
- **SomaFM**: Listener-supported, commercial-free radio
- **KEXP**: Seattle-based music discovery station
- **SRG SSR**: Swiss public broadcasting
- **Classic FM**: Classical music for everyone
- **Radio Caroline**: Legendary British pirate radio
- **Jazz24**: Non-stop jazz streaming
- **NTS Radio**: Eclectic music from East London

## Compatibility

This mod uses Barotrauma's standard wiring and sound systems without modifying core game code, making it compatible with most other mods.

There's one known minor incompatibility with RealSonar that may display a console error, but this doesn't affect gameplay or the functionality of either mod.

## Support

If you encounter issues or have suggestions:
1. Check the [GitHub Issues](https://github.com/gitempERROR/BarotraumaRadio/issues) for existing reports
2. Create a new issue with details about the problem
3. Include your game version and mod list for better support

---

*Keep the crew entertained even when surrounded by crushing depths and monstrous creatures. Because sometimes, what you really need isn't more ammunition - it's better music.*
