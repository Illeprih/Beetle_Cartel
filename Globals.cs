using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragoon_Modifier {
    public class Globals {
        public static ushort HOTKEY = 0;
        public static byte DISC = 1;
        public static byte CHAPTER = 0;
        public static int BATTLE_VALUE = 0;
        public static ushort ENCOUNTER_ID = 0;
        public static ushort MAP = 0;
        public static byte[] PARTY_SLOT = new byte[3];
        public static byte DRAGOON_SPIRITS = 0;
        public static bool IN_BATTLE = false;
        public static bool STATS_CHANGED = false;
        public static dynamic BATTLE = new System.Dynamic.ExpandoObject();
        public static dynamic DICTIONARY = new System.Dynamic.ExpandoObject();
        public static bool FIRST_RUN = true;
        public static bool MONSTER_CHANGE = false;
        public static bool DROP_CHANGE = false;
        public static bool DROP_DEFINED = true;
        public static string MOD = "US_Base";
    }
}
