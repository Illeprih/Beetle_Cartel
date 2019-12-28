using Dragoon_Modifier;
using System;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;
using System.Globalization;
using System.Reflection;

public class BattleController {
    static bool FIRST_RUN = true;

    public static void Run(Emulator emulator) {
        if (FIRST_RUN == true) {
            Globals.DICTIONARY = new LoDDict();
            FIRST_RUN = false;
        }

        int encounterValue = emulator.ReadShort(Constants.GetAddress("BATTLE_VALUE"));
        if (Globals.IN_BATTLE && !Globals.STATS_CHANGED && encounterValue == 41215) {
            Constants.WriteOutput("Battle detected. Loading...");
            int slot1 = Globals.PARTY_SLOT[0];
            int slot2 = Globals.PARTY_SLOT[1];
            if (slot1 != 0)
            {
                Constants.WriteDebug("Party Slot 1:       " + Convert.ToString(Globals.PARTY_SLOT[0], 10));
                emulator.WriteByte(Constants.GetAddress("PARTY_SLOT") + 0x4, (byte)slot1);
            }
            Thread.Sleep(2000);
            if (Constants.REGION == Region.USA) {
                Globals.M_POINT = 0x1A439C + emulator.ReadShort(Constants.GetAddress("M_POINT")) + (int)Constants.OFFSET;
            } else {
                Globals.M_POINT = 0x1A43B4 + emulator.ReadShort(Constants.GetAddress("M_POINT"));

            }

            Globals.C_POINT = (int)(emulator.ReadInteger(Constants.GetAddress("C_POINT")) - 0x7F5A8558);
            Globals.MONSTER_SIZE = emulator.ReadByte(Constants.GetAddress("MONSTER_SIZE"));
            Globals.UNIQUE_MONSTERS = emulator.ReadByte(Constants.GetAddress("UNIQUE_MONSTERS"));

           
            Globals.STATS_CHANGED = true;
            LoDDictInIt(emulator);


            Constants.WriteDebug("Monster Size:        " + Globals.MONSTER_SIZE);
            Constants.WriteDebug("Unique Monsters:     " + Globals.UNIQUE_MONSTERS);
            Constants.WriteDebug("Monster Point:       " + Convert.ToString(Globals.M_POINT, 16).ToUpper());
            Constants.WriteDebug("Character Point:     " + Convert.ToString(Globals.C_POINT, 16).ToUpper());
            Constants.WriteDebug("Monster IDs:         " + String.Join(", ", Globals.MONSTER_IDS.ToArray()));
            Constants.WriteDebug("Unique Monster IDs:  " + String.Join(", ", Globals.UNIQUE_MONSTER_IDS.ToArray()));
            if (slot1 != 0) {
                Thread.Sleep(5000);
                Globals.CHARACTER_TABLE[1].Write("SP", 100);
                Globals.CHARACTER_TABLE[1].Write("Image", slot2);
                emulator.WriteByte(Constants.GetAddress("PARTY_SLOT") + 0x4, (byte)slot2);
                emulator.WriteShortU((long)0xB1493E, (ushort)slot1);
                Globals.MONSTER_TABLE[0].Write("HP", 50000);
                Globals.MONSTER_TABLE[0].Write("Max_HP", 50000);
            }
        } else {
            if (Globals.STATS_CHANGED && encounterValue < 9999) {
                Globals.STATS_CHANGED = false;
                Constants.WriteOutput("Exiting out of battle.");
            }
        }
    }

    public static int[] BitSplit2(int value) {
        int[] array = { value >> 6, (value >> 4) % 4, (value >> 2) % 4, value % 4 };
        return array;
    }

    public static int GetOffset() {
        int[] discOffset = { 0xD80, 0x0, 0x1458, 0x1B0 };
        int[] charOffset = { 0x0, 0x180, -0x180, 0x420, 0x540, 0x180, 0x350, 0x2F0, -0x180 };
        int partyOffset = 0;
        if (Globals.PARTY_SLOT[0] < 9 && Globals.PARTY_SLOT[1] < 9 && Globals.PARTY_SLOT[2] < 9) {
            partyOffset = charOffset[Globals.PARTY_SLOT[1]] + charOffset[Globals.PARTY_SLOT[2]];
        }
        return discOffset[Globals.DISC - 1] - partyOffset;
    }

    public static void LoDDictInIt(Emulator emulator) {
        Globals.UNIQUE_MONSTER_IDS = new List<int>();
        Globals.MONSTER_IDS = new List<int>();
        Globals.MONSTER_TABLE = new List<dynamic>();
        Globals.CHARACTER_TABLE = new List<dynamic>();

        for (int monster = 0; monster < Globals.UNIQUE_MONSTERS; monster++) {
            Globals.UNIQUE_MONSTER_IDS.Add(emulator.ReadShortU(Constants.GetAddress("UNIQUE_SLOT") + (int) Constants.OFFSET + (monster * 0x1A8)));
        }
        for (int i = 0; i < Globals.MONSTER_SIZE; i++) {
            Globals.MONSTER_IDS.Add(emulator.ReadShort(Constants.GetAddress("MONSTER_ID") + GetOffset() + (i * 0x8)));
        }
        for (int monster = 0; monster < Globals.MONSTER_SIZE; monster++) {
            Globals.MONSTER_TABLE.Add(new MonsterAddress(Globals.M_POINT, monster, Globals.MONSTER_IDS[monster], Globals.UNIQUE_MONSTER_IDS, emulator));
        }
        for (int character = 0; character < 3; character++) {
            if (Globals.PARTY_SLOT[character] < 9) {
                Globals.CHARACTER_TABLE.Add(new CharAddress(Globals.C_POINT, character, emulator));
            }
        }
        if (Globals.MONSTER_CHANGE == true) {
            Constants.WriteOutput("Changing stats...");
            for (int monster = 0; monster < Globals.MONSTER_SIZE; monster++) {
                int ID = Globals.MONSTER_IDS[monster];
                Globals.MONSTER_TABLE[monster].Write("HP", Globals.DICTIONARY.StatList[ID].HP);
                Globals.MONSTER_TABLE[monster].Write("Max_HP", Globals.DICTIONARY.StatList[ID].HP);
                Globals.MONSTER_TABLE[monster].Write("ATK", Globals.DICTIONARY.StatList[ID].ATK);
                Globals.MONSTER_TABLE[monster].Write("OG_ATK", Globals.DICTIONARY.StatList[ID].ATK);
                Globals.MONSTER_TABLE[monster].Write("MAT", Globals.DICTIONARY.StatList[ID].MAT);
                Globals.MONSTER_TABLE[monster].Write("OG_MAT", Globals.DICTIONARY.StatList[ID].MAT);
                Globals.MONSTER_TABLE[monster].Write("DEF", Globals.DICTIONARY.StatList[ID].DEF);
                Globals.MONSTER_TABLE[monster].Write("OG_DEF", Globals.DICTIONARY.StatList[ID].DEF);
                Globals.MONSTER_TABLE[monster].Write("MDEF", Globals.DICTIONARY.StatList[ID].MDEF);
                Globals.MONSTER_TABLE[monster].Write("OG_MDEF", Globals.DICTIONARY.StatList[ID].MDEF);
                Globals.MONSTER_TABLE[monster].Write("SPD", Globals.DICTIONARY.StatList[ID].SPD);
                Globals.MONSTER_TABLE[monster].Write("OG_SPD", Globals.DICTIONARY.StatList[ID].SPD);
                Globals.MONSTER_TABLE[monster].Write("A_AV", Globals.DICTIONARY.StatList[ID].A_AV);
                Globals.MONSTER_TABLE[monster].Write("M_AV", Globals.DICTIONARY.StatList[ID].M_AV);
                Globals.MONSTER_TABLE[monster].Write("P_Immune", Globals.DICTIONARY.StatList[ID].P_Immune);
                Globals.MONSTER_TABLE[monster].Write("M_Immune", Globals.DICTIONARY.StatList[ID].M_Immune);
                Globals.MONSTER_TABLE[monster].Write("P_Half", Globals.DICTIONARY.StatList[ID].P_Half);
                Globals.MONSTER_TABLE[monster].Write("M_Half", Globals.DICTIONARY.StatList[ID].M_Half);
                Globals.MONSTER_TABLE[monster].Write("E_Immune", Globals.DICTIONARY.StatList[ID].E_Immune);
                Globals.MONSTER_TABLE[monster].Write("E_Half", Globals.DICTIONARY.StatList[ID].E_Half);
                Globals.MONSTER_TABLE[monster].Write("Stat_Res", Globals.DICTIONARY.StatList[ID].Stat_Res);
                Globals.MONSTER_TABLE[monster].Write("Death_Res", Globals.DICTIONARY.StatList[ID].Death_Res);
            }
        }
        if (Globals.DROP_CHANGE == true) {
            Constants.WriteOutput("Changing drops...");
            for (int monster = 0; monster < Globals.UNIQUE_MONSTERS; monster++) {
                int ID = Globals.UNIQUE_MONSTER_IDS[monster];
                emulator.WriteShortU(Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + monster * 0x1A8, (ushort) Globals.DICTIONARY.StatList[ID].EXP);
                emulator.WriteShortU(Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + 0x2 + monster * 0x1A8, (ushort) Globals.DICTIONARY.StatList[ID].Gold);
                emulator.WriteByteU(Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + 0x4 + monster * 0x1A8, (byte) Globals.DICTIONARY.StatList[ID].Drop_Chance);
                emulator.WriteByteU(Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + 0x5 + monster * 0x1A8, (byte) Globals.DICTIONARY.StatList[ID].Drop_Item);
                Constants.WriteDebug(Convert.ToString(ID, 10) + " Drop: " + (int) Globals.DICTIONARY.StatList[ID].Drop_Item);
            }
        }
        if (Globals.DRAGOON_CHANGE == true) {
            for (int character = 0; character < 3; character++) {
                if (Globals.PARTY_SLOT[character] < 9) {
                    Globals.CHARACTER_TABLE[character].Write("DATK", Globals.DICTIONARY.DragoonStats[Globals.PARTY_SLOT[character]][1].DATK);
                    Globals.CHARACTER_TABLE[character].Write("DMAT", Globals.DICTIONARY.DragoonStats[Globals.PARTY_SLOT[character]][1].DMAT);
                    Globals.CHARACTER_TABLE[character].Write("DDEF", Globals.DICTIONARY.DragoonStats[Globals.PARTY_SLOT[character]][1].DDEF);
                    Globals.CHARACTER_TABLE[character].Write("DMDEF", Globals.DICTIONARY.DragoonStats[Globals.PARTY_SLOT[character]][1].DMDEF);
                }
            }
        }
    }

    public class MonsterAddress {
        int[] hp = { 0, 2 };
        int[] max_hp = { 0, 2 };
        int[] element = { 0, 2 };
        int[] display_element = { 0, 2 };
        int[] atk = { 0, 2 };
        int[] og_atk = { 0, 2 };
        int[] mat = { 0, 2 };
        int[] og_mat = { 0, 2 };
        int[] def = { 0, 2 };
        int[] og_def = { 0, 2 };
        int[] mdef = { 0, 2 };
        int[] og_mdef = { 0, 2 };
        int[] spd = { 0, 2 };
        int[] og_spd = { 0, 2 };
        int[] turn = { 0, 2 };
        int[] a_av = { 0, 1 };
        int[] m_av = { 0, 1 };
        int[] p_immune = { 0, 1 };
        int[] m_immune = { 0, 1 };
        int[] p_half = { 0, 1 };
        int[] m_half = { 0, 1 };
        int[] e_immune = { 0, 1 };
        int[] e_half = { 0, 1 };
        int[] stat_res = { 0, 1 };
        int[] death_res = { 0, 1 };
        int[] unique_index = { 0, 1 };
        int[] exp = { 0, 2 };
        int[] gold = { 0, 2 };
        int[] drop_chance = { 0, 1 };
        int[] drop_item = { 0, 1 };
        int[] special_effect = { 0, 1 };
        public Emulator emulator = null;

        public int[] HP { get { return hp; } }
        public int[] Max_HP { get { return max_hp; } }
        public int[] Element { get { return element; } }
        public int[] Display_Element { get { return display_element; } }
        public int[] ATK { get { return atk; } }
        public int[] OG_ATK { get { return og_atk; } }
        public int[] MAT { get { return mat; } }
        public int[] OG_MAT { get { return og_mat; } }
        public int[] DEF { get { return def; } }
        public int[] OG_DEF { get { return og_def; } }
        public int[] MDEF { get { return mdef; } }
        public int[] OG_MDEF { get { return og_mdef; } }
        public int[] SPD { get { return spd; } }
        public int[] OG_SPD { get { return og_spd; } }
        public int[] Turn { get { return turn; } }
        public int[] A_AV { get { return a_av; } }
        public int[] M_AV { get { return m_av; } }
        public int[] P_Immune { get { return p_immune; } }
        public int[] M_Immune { get { return m_immune; } }
        public int[] P_Half { get { return p_half; } }
        public int[] M_Half { get { return m_half; } }
        public int[] E_Immune { get { return e_immune; } }
        public int[] E_Half { get { return e_half; } }
        public int[] Stat_Res { get { return stat_res; } }
        public int[] Death_Res { get { return death_res; } }
        public int[] Unique_Index { get { return unique_index; } }
        public int[] EXP { get { return exp; } }
        public int[] Gold { get { return gold; } }
        public int[] Drop_Chance { get { return drop_chance; } }
        public int[] Drop_Item { get { return drop_item; } }
        public int[] Special_Effect { get { return special_effect; } }

        public MonsterAddress(int m_point, int monster, int ID, List<int> monsterUniqueIdList, Emulator emu) {
            emulator = emu;
            hp[0] = m_point - monster * 0x388;
            max_hp[0] = m_point + 0x8 - monster * 0x388;
            element[0] = m_point + 0x6a - monster * 0x388;
            display_element[0] = m_point + 0x14 - monster * 0x388;
            atk[0] = m_point + 0x2c - monster * 0x388;
            og_atk[0] = m_point + 0x58 - monster * 0x388;
            mat[0] = m_point + 0x2E - monster * 0x388;
            og_mat[0] = m_point + 0x5A - monster * 0x388;
            def[0] = m_point + 0x30 - monster * 0x388;
            og_def[0] = m_point + 0x5E - monster * 0x388;
            mdef[0] = m_point + 0x32 - monster * 0x388;
            og_mdef[0] = m_point + 0x60 - monster * 0x388;
            spd[0] = m_point + 0x2A - monster * 0x388;
            og_spd[0] = m_point + 0x5C - monster * 0x388;
            turn[0] = m_point + 0x44 - monster * 0x388;
            a_av[0] = m_point + 0x38 - monster * 0x388;
            m_av[0] = m_point + 0x3A - monster * 0x388;
            p_immune[0] = m_point + 0x108 - monster * 0x388;
            m_immune[0] = m_point + 0x10A - monster * 0x388;
            p_half[0] = m_point + 0x10C - monster * 0x388;
            m_half[0] = m_point + 0x10E - monster * 0x388;
            e_immune[0] = m_point + 0x1A - monster * 0x388;
            e_half[0] = m_point + 0x18 - monster * 0x388;
            stat_res[0] = m_point + 0x1C - monster * 0x388;
            death_res[0] = m_point + 0xC - monster * 0x388;
            unique_index[0] = m_point + 0x264 - monster * 0x388;
            exp[0] = Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + Globals.UNIQUE_MONSTER_IDS.IndexOf(ID) * 0x1A8;
            gold[0] = Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + 0x2 + Globals.UNIQUE_MONSTER_IDS.IndexOf(ID) * 0x1A8;
            drop_chance[0] = Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + 0x4 + Globals.UNIQUE_MONSTER_IDS.IndexOf(ID) * 0x1A8;
            drop_item[0] = Constants.GetAddress("MONSTER_REWARDS") + (int) Constants.OFFSET + 0x5 + Globals.UNIQUE_MONSTER_IDS.IndexOf(ID) * 0x1A8;
            special_effect[0] = Constants.GetAddress("UNIQUE_MONSTERS") + monster * 0x20;
        }

        public int Read(string attribute) {
            PropertyInfo property = GetType().GetProperty(attribute);
            var address = (int[]) property.GetValue(this, null);
            if (address[1] == 2) {
                return this.emulator.ReadShortU(address[0]);
            } else {
                return (int) this.emulator.ReadByteU(address[0]);
            }
        }

        public void Write(string attribute, int value) {
            PropertyInfo property = GetType().GetProperty(attribute);
            var address = (int[]) property.GetValue(this, null);
            if (address[1] == 2) {
                this.emulator.WriteShortU(address[0], (ushort) value);
            } else {
                this.emulator.WriteByteU(address[0], (byte) value);
            }
        }
    }

    public class CharAddress {
        int[] level = { 0, 1 };
        int[] dlevel = { 0, 1 };
        int[] hp = { 0, 2 };
        int[] max_hp = { 0, 2 };
        int[] mp = { 0, 2 };
        int[] max_mp = { 0, 2 };
        int[] sp = { 0, 2 };
        int[] element = { 0, 2 };
        int[] display_element = { 0, 2 };
        int[] atk = { 0, 2 };
        int[] og_atk = { 0, 2 };
        int[] mat = { 0, 2 };
        int[] og_mat = { 0, 2 };
        int[] def = { 0, 2 };
        int[] og_def = { 0, 2 };
        int[] mdef = { 0, 2 };
        int[] og_mdef = { 0, 2 };
        int[] spd = { 0, 2 };
        int[] og_spd = { 0, 2 };
        int[] turn = { 0, 2 };
        int[] a_hit = { 0, 1 };
        int[] m_hit = { 0, 1 };
        int[] a_av = { 0, 1 };
        int[] m_av = { 0, 1 };
        int[] p_immune = { 0, 1 };
        int[] m_immune = { 0, 1 };
        int[] p_half = { 0, 1 };
        int[] m_half = { 0, 1 };
        int[] e_immune = { 0, 1 };
        int[] e_half = { 0, 1 };
        int[] stat_res = { 0, 1 };
        int[] death_res = { 0, 1 };
        int[] sp_p_hit = { 0, 1 };
        int[] sp_m_hit = { 0, 1 };
        int[] mp_p_hit = { 0, 1 };
        int[] mp_m_hit = { 0, 1 };
        int[] hp_reg = { 0, 1 };
        int[] mp_reg = { 0, 1 };
        int[] sp_reg = { 0, 1 };
        int[] sp_multi = { 0, 2 };
        int[] revive = { 0, 1 };
        int[] datk = { 0, 1 };
        int[] dmat = { 0, 1 };
        int[] ddef = { 0, 1 };
        int[] dmdef = { 0, 1 };
        int[] unique_index = { 0, 1 };
        int[] image = { 0, 1 };
        int[] special_effect = { 0, 1 };
        int[] guard = { 0, 1 };
        public Emulator emulator = null;

        public int[] Level { get { return level; } }
        public int[] DLevel { get { return dlevel; } }
        public int[] HP { get { return hp; } }
        public int[] Max_HP { get { return max_hp; } }
        public int[] MP { get { return mp; } }
        public int[] Max_MP { get { return max_mp; } }
        public int[] SP { get { return sp; } }
        public int[] Element { get { return element; } }
        public int[] Display_Element { get { return display_element; } }
        public int[] ATK { get { return atk; } }
        public int[] OG_ATK { get { return og_atk; } }
        public int[] MAT { get { return mat; } }
        public int[] OG_MAT { get { return og_mat; } }
        public int[] DEF { get { return def; } }
        public int[] OG_DEF { get { return og_def; } }
        public int[] MDEF { get { return mdef; } }
        public int[] OG_MDEF { get { return og_mdef; } }
        public int[] SPD { get { return spd; } }
        public int[] OG_SPD { get { return og_spd; } }
        public int[] Turn { get { return turn; } }
        public int[] A_Hit { get { return a_hit; } }
        public int[] M_Hit { get { return m_hit; } }
        public int[] A_AV { get { return a_av; } }
        public int[] M_AV { get { return m_av; } }
        public int[] P_Immune { get { return p_immune; } }
        public int[] M_Immune { get { return m_immune; } }
        public int[] P_Half { get { return p_half; } }
        public int[] M_Half { get { return m_half; } }
        public int[] E_Immune { get { return e_immune; } }
        public int[] E_Half { get { return e_half; } }
        public int[] Stat_Res { get { return stat_res; } }
        public int[] Death_Res { get { return death_res; } }
        public int[] SP_P_Hit { get { return sp_p_hit; } }
        public int[] SP_M_Hit { get { return sp_m_hit; } }
        public int[] MP_P_Hit { get { return mp_p_hit; } }
        public int[] MP_M_Hit { get { return mp_m_hit; } }
        public int[] HP_Reg { get { return hp_reg; } }
        public int[] MP_Reg { get { return mp_reg; } }
        public int[] SP_Reg { get { return sp_reg; } }
        public int[] SP_Multi { get { return sp_multi; } }
        public int[] Revive { get { return revive; } }
        public int[] Unique_Index { get { return unique_index; } }
        public int[] Image { get { return image; } }
        public int[] DATK { get { return datk; } }
        public int[] DMAT { get { return dmat; } }
        public int[] DDEF { get { return ddef; } }
        public int[] DMDEF { get { return dmdef; } }
        public int[] Special_Effect { get { return special_effect; } }
        public int[] Guard { get { return guard; } }

        public CharAddress(int c_point, int character, Emulator emu) {
            emulator = emu;
            level[0] = c_point - 0x04 - character * 0x388;
            dlevel[0] = c_point - 0x02 - character * 0x388;
            hp[0] = c_point - character * 0x388;
            max_hp[0] = c_point + 0x8 - character * 0x388;
            mp[0] = c_point + 0x4 - character * 0x388;
            max_mp[0] = c_point + 0xA - character * 0x388;
            sp[0] = c_point + 0x2 - character * 0x388;
            element[0] = c_point + 0x6a - character * 0x388;
            display_element[0] = c_point + 0x14 - character * 0x388;
            atk[0] = c_point + 0x2c - character * 0x388;
            og_atk[0] = c_point + 0x58 - character * 0x388;
            mat[0] = c_point + 0x2E - character * 0x388;
            og_mat[0] = c_point + 0x5A - character * 0x388;
            def[0] = c_point + 0x30 - character * 0x388;
            og_def[0] = c_point + 0x5E - character * 0x388;
            mdef[0] = c_point + 0x32 - character * 0x388;
            og_mdef[0] = c_point + 0x60 - character * 0x388;
            spd[0] = c_point + 0x2A - character * 0x388;
            og_spd[0] = c_point + 0x5C - character * 0x388;
            turn[0] = c_point + 0x44 - character * 0x388;
            a_hit[0] = c_point + 0x34 - character * 0x388;
            m_hit[0] = c_point + 0x3 - character * 0x388;
            a_av[0] = c_point + 0x38 - character * 0x388;
            m_av[0] = c_point + 0x3A - character * 0x388;
            p_immune[0] = c_point + 0x108 - character * 0x388;
            m_immune[0] = c_point + 0x10A - character * 0x388;
            p_half[0] = c_point + 0x10C - character * 0x388;
            m_half[0] = c_point + 0x10E - character * 0x388;
            e_immune[0] = c_point + 0x1A - character * 0x388;
            e_half[0] = c_point + 0x18 - character * 0x388;
            stat_res[0] = c_point + 0x1C - character * 0x388;
            death_res[0] = c_point + 0xC - character * 0x388;
            sp_p_hit[0] = c_point + 0x112 - character * 0x388;
            sp_m_hit[0] = c_point + 0x116 - character * 0x388;
            mp_p_hit[0] = c_point + 0x114 - character * 0x388;
            mp_m_hit[0] = c_point + 0x118 - character * 0x388;
            hp_reg[0] = c_point + 0x11C - character * 0x388;
            mp_reg[0] = c_point + 0x11E - character * 0x388;
            sp_reg[0] = c_point + 0x120 - character * 0x388;
            sp_multi[0] = c_point + 0x102 - character * 0x388;
            revive[0] = c_point + 0x132 - character * 0x388;
            datk[0] = c_point + 0xA4 - character * 0x388;
            dmat[0] = c_point + 0xA6 - character * 0x388;
            ddef[0] = c_point + 0xA8 - character * 0x388;
            dmdef[0] = c_point + 0xAA - character * 0x388;
            unique_index[0] = c_point + 0x264 - character * 0x388;
            image[0] = c_point + 0x26A - character * 0x388;
            special_effect[0] = Constants.GetAddress("UNIQUE_MONSTERS") + (character + Globals.MONSTER_SIZE) * 0x20;
            guard[0] = c_point + 0x4C - character * 0x388;
        }

        public int Read(string attribute) {
            PropertyInfo property = GetType().GetProperty(attribute);
            var address = (int[])property.GetValue(this, null);
            if (address[1] == 2) {
                return this.emulator.ReadShortU(address[0]);
            } else {
                return (int)this.emulator.ReadByteU(address[0]);
            }
        }

        public void Write(string attribute, int value) {
            PropertyInfo property = GetType().GetProperty(attribute);
            var address = (int[])property.GetValue(this, null);
            if (address[1] == 2) {
                this.emulator.WriteShortU(address[0], (ushort)value);
            } else {
                this.emulator.WriteByteU(address[0], (byte)value);
            }
        }
    }
    public static void Open(Emulator emulator) { }
    public static void Close(Emulator emulator) { }
    public static void Click(Emulator emulator) { }
}

public class LoDDict {
    IDictionary<int, dynamic> statList = new Dictionary<int, dynamic>();
    IDictionary<int, Dictionary<int, dynamic>> dragoonStats = new Dictionary<int, Dictionary<int, dynamic>>();
    IDictionary<int, string> num2item = new Dictionary<int, string>();
    IDictionary<string, int> item2num = new Dictionary<string, int>();
    IDictionary<int, string> num2element = new Dictionary<int, string>() {
        {0, "None" },
        {1, "Water" },
        {2, "Earth" },
        {4, "Dark" },
        {8, "Non-Elemental" },
        {16, "Thunder" },
        {32, "Light" },
        {64, "Wind" },
        {128, "Fire" }
    };
    IDictionary<string, int> element2num = new Dictionary<string, int>() {
        {"None", 0 },
        {"Water", 1 },
        {"Earth", 2 },
        {"Dark", 4 },
        {"Non-Elemental", 8 },
        {"Thunder", 16 },
        {"Light", 32 },
        {"Wind", 64 },
        {"Fire", 128 }
    };

    public IDictionary<int, dynamic> StatList { get { return statList; } }
    public IDictionary<int, string> Num2Item { get { return num2item; } }
    public IDictionary<string, int> Item2Num { get { return item2num; } }
    public IDictionary<int, string> Num2Element { get { return num2element; } }
    public IDictionary<string, int> Element2Num { get { return element2num; } }
    public IDictionary<int, Dictionary<int, dynamic>> DragoonStats { get { return dragoonStats; } }

    public LoDDict() {
        string cwd = AppDomain.CurrentDomain.BaseDirectory;
        string[] lines = File.ReadAllLines(cwd + "Mods/" + Globals.MOD + "/Item_List.txt");
        var i = 0;
        foreach (string row in lines) {
            if (row != "") {
                item2num.Add(row, i);
                num2item.Add(i, row);
            }
            i++;
        }
        using (var monsterData = new StreamReader(cwd + "Mods/" + Globals.MOD + "/Monster_Data.csv")) {
            bool firstline = true;
            while (!monsterData.EndOfStream) {
                var line = monsterData.ReadLine();
                if (firstline == false) {
                    var values = line.Split(',').ToArray();
                    statList.Add(Int32.Parse(values[0]), new StatList(values, element2num, item2num));
                } else {
                    firstline = false;
                }
            }
        }
        using (var dragoon = new StreamReader(cwd + "Mods/" + Globals.MOD + "/Dragoon_Stats.csv")) {
            bool firstline = true;
            i = 0;
            while (!dragoon.EndOfStream) {
                var line = dragoon.ReadLine();
                if (firstline == false) {
                    var values = line.Split(',').ToArray();
                    Dictionary<int, dynamic> level = new Dictionary<int, dynamic>();
                    level.Add(1, new DragoonStats(Int32.Parse(values[1]), Int32.Parse(values[2]), Int32.Parse(values[3]), Int32.Parse(values[4])));
                    level.Add(2, new DragoonStats(Int32.Parse(values[5]), Int32.Parse(values[6]), Int32.Parse(values[7]), Int32.Parse(values[8])));
                    level.Add(3, new DragoonStats(Int32.Parse(values[9]), Int32.Parse(values[10]), Int32.Parse(values[11]), Int32.Parse(values[12])));
                    level.Add(4, new DragoonStats(Int32.Parse(values[13]), Int32.Parse(values[14]), Int32.Parse(values[15]), Int32.Parse(values[16])));
                    level.Add(5, new DragoonStats(Int32.Parse(values[17]), Int32.Parse(values[18]), Int32.Parse(values[19]), Int32.Parse(values[20])));
                    dragoonStats.Add(i - 1, level);
                } else {
                    firstline = false;
                }
                i++;
            }
        }
    }
}

public class StatList {
    string name = "Monster";
    int element = 128;
    int hp = 0;
    int atk = 0;
    int mat = 0;
    int def = 0;
    int mdef = 0;
    int spd = 0;
    int a_av = 0;
    int m_av = 0;
    int p_immune = 0;
    int m_immune = 0;
    int p_half = 0;
    int m_half = 0;
    int e_immune = 0;
    int e_half = 0;
    int stat_res = 0;
    int death_res = 0;
    int exp = 0;
    int gold = 0;
    int drop_item = 255;
    int drop_chance = 0;

    public string Name { get { return name; } }
    public int Element { get { return element; } }
    public int HP { get { return hp; } }
    public int ATK { get { return atk; } }
    public int MAT { get { return mat; } }
    public int DEF { get { return def; } }
    public int MDEF { get { return mdef; } }
    public int SPD { get { return spd; } }
    public int A_AV { get { return a_av; } }
    public int M_AV { get { return m_av; } }
    public int P_Immune { get { return p_immune; } }
    public int M_Immune { get { return m_immune; } }
    public int P_Half { get { return p_half; } }
    public int M_Half { get { return m_half; } }
    public int E_Immune { get { return e_immune; } }
    public int E_Half { get { return e_half; } }
    public int Stat_Res { get { return stat_res; } }
    public int Death_Res { get { return death_res; } }
    public int EXP { get { return exp; } }
    public int Gold { get { return gold; } }
    public int Drop_Item { get { return drop_item; } }
    public int Drop_Chance { get { return drop_chance; } }

    public StatList(string[] monster, IDictionary<string, int> element2num, IDictionary<string, int> item2num) {
        name = monster[1];
        element = element2num[monster[2]];
        hp = Int32.Parse(monster[3]);
        atk = Int32.Parse(monster[4]);
        mat = Int32.Parse(monster[5]);
        def = Int32.Parse(monster[6]);
        mdef = Int32.Parse(monster[7]);
        spd = Int32.Parse(monster[8]);
        a_av = Int32.Parse(monster[9]);
        m_av = Int32.Parse(monster[10]);
        p_immune = Int32.Parse(monster[11]);
        m_immune = Int32.Parse(monster[12]);
        p_half = Int32.Parse(monster[13]);
        m_half = Int32.Parse(monster[14]);
        e_immune = element2num[monster[15]];
        e_half = element2num[monster[16]];
        stat_res = Int32.Parse(monster[17]);
        death_res = Int32.Parse(monster[18]);
        exp = Int32.Parse(monster[19]);
        gold = Int32.Parse(monster[20]);
        drop_item = item2num[monster[21]];
        drop_chance = Int32.Parse(monster[22]);
    }
}

public class DragoonStats {
    int datk = 0;
    int dmat = 0;
    int ddef = 0;
    int dmdef = 0;

    public int DATK { get { return datk; } }
    public int DMAT { get { return dmat; } }
    public int DDEF { get { return ddef; } }
    public int DMDEF { get { return dmdef; } }

    public DragoonStats(int ndatk, int ndmat, int nddef, int ndmdef) {
        datk = ndatk;
        dmat = ndmat;
        ddef = nddef;
        dmdef = ndmdef;
    }
}
