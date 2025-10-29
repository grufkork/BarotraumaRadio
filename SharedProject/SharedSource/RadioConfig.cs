using System;
using System.Collections.Generic;
using System.Text;

namespace BarotraumaRadio
{
    public record ClientRadioConfig(int LastPlayedIndex, float Volume, bool ServerSync);
    public record ServerRadioConfig(string LastPlayedUrl);
}
