# Barotrauma Radio Mod

[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/gitempERROR/BarotraumaRadio)
[![Steam Workshop](https://img.shields.io/badge/Steam-Workshop-000000?style=for-the-badge&logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3559932434)

An immersive radio mod for Barotrauma that adds craftable radios to submarines, fully operable through the game's wiring system.

## Features

- **Fabricator Crafting**: Create radios at the fabricator and install them anywhere on your submarine
- **Wiring Integration**: Full control through in-game wiring connections
- **Pre-configured Stations**: Includes popular internet radio stations
- **Customizable**: Easily add your own radio stations via config file

## Installation

1. Subscribe to the mod on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3559932434)
2. Enable the mod in Barotrauma's mod menu
3. The mod will be available in your next game session

## Adding Custom Radio Stations

To add your own radio stations:

1. Navigate to the mod folder: `Steam/steamapps/workshop/content/602960/3559932434/Content/`
2. Open `radioStations.json` in a text editor
3. Add new stations in JSON format:
```json
{
  "Name": "Your Station Name",
  "Url": "http://your.radio.stream/url"
}
```
4. Save the file and launch the game

## Wiring Setup

The radio requires the following connections:
- **Power line**: Connect to submarine's power grid
- **Switch**: To turn the radio on/off
- **Button**: For switching between stations
- **Button**: For volume control

## Pre-installed Radio Stations

- **AniSonFM**: Japanese anime music
- **Truckers FM**: Music and talk for truckers
- **Radio Paradise**: Eclectic music mix
- **SomaFM**: Listener-supported radio
- **KEXP**: Seattle-based music station
- **SRG SSR**: Swiss broadcasting
- **Classic FM**: Classical music
- **Radio Caroline**: Legendary pirate radio
- **Jaz24**: Jazz music
- **NTS Radio**: East London radio station

## Compatibility

This mod uses Barotrauma's standard wiring and sound systems without modifying game code. It's compatible with most other mods, though there's a minor incompatibility with RealSonar that may cause console errors (doesn't affect functionality).

## Support

If you encounter issues or have suggestions:
1. Check the [GitHub Issues](https://github.com/gitempERROR/BarotraumaRadio/issues) for existing reports
2. Create a new issue with details about the problem
3. Include your game version and mod list for better support