using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragoon_Modifier {
    public class Globals {
        public static ushort HOTKEY = 0;
        public static long CURRENT_TIME = 0;
        public static long LAST_HOTKEY = 0;
        public static byte DISC = 1;
        public static byte CHAPTER = 0;
        public static int BATTLE_VALUE = 0;
        public static ushort ENCOUNTER_ID = 0;
        public static ushort MAP = 0;
        public static byte[] PARTY_SLOT = new byte[3];
        public static byte DRAGOON_SPIRITS = 0;
        public static bool IN_BATTLE = false;
        public static bool STATS_CHANGED = false;
        public static int M_POINT = 0;
        public static int C_POINT = 0;
        public static int[] CHAR_ADDRESS = new int[3];
        public static int[] MONS_ADDRESS = new int[5];
        public static byte MONSTER_SIZE = 0;
        public static byte UNIQUE_MONSTERS = 0;
        public static List<int> MONSTER_IDS = new List<int>();
        public static List<int> UNIQUE_MONSTER_IDS = new List<int>();
        public static List<dynamic> MONSTER_TABLE = new List<dynamic>();
        public static List<dynamic> CHARACTER_TABLE = new List<dynamic>();
        public static dynamic DICTIONARY = new System.Dynamic.ExpandoObject();
        public static bool MONSTER_CHANGE = false;
        public static bool DROP_CHANGE = false;
        public static bool DRAGOON_CHANGE = false;
        public static string MOD = "US_Base";
    }
}
