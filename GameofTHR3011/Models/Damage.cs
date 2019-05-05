using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfTHR3011.Models
{

    public enum DamageType
    {
        Cold,
        Fire,
        Slashing,
        Piercing,
        Bludgeoning,
        Lightning,
        Thunder,
        Psychic,
        Force
    }

    public class Damage
    {
        public DamageType Type { get; set; }
        public int Magnitude { get; set; }
    }

}
