using Dragoon_Modifier;
using System;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class Shop_Changer {
    static bool SHOP_CHANGED = true;
    public static void Run(Emulator emulator)
    {
        if (Globals.SHOP_CHANGE == true && SHOP_CHANGED == false)
        {
            int shop_og_size = ReadShop(0x11E13C, emulator);
            int shop = 0;
            switch (Globals.MAP)
            {
                case 16: // Hellena
                    shop = 0;
                    break;
                case 23: // Hellena 2nd spot
                    shop = 1;
                    break;
                case 83: // Bale Weapon Shop
                    shop = 2;
                    break;
                case 84: // Bale Item Shop
                    shop = 3;
                    break;
                case 122: // Volcano Villude
                    shop = 4;
                    break;
                case 145: // Lohan
                    if (ReadShop(0x11E0FC, emulator) == 229) //Item Shop
                    {
                        shop = 5;
                        break;
                    }
                    else //Weapon Shop
                    {
                        shop = 6;
                        break;
                    }
                case 175: // Kazas Weapon Shop
                    shop = 7;
                    break;
                case 180: // Kazas Item Shop
                    shop = 8;
                    break;
                case 193: // Black Castle
                    shop = 9;
                    break;
                case 204: // Fletz Weapon Shop
                    shop = 10;
                    break;
                case 211: // Fletz Jewelry Shop
                    shop = 11;
                    break;
                case 214: // Fletz Item Shop
                    shop = 12;
                    break;
                case 247: // Donau
                    if (ReadShop(0x11E0FC, emulator) == 229) //Item Shop
                    {
                        shop = 13;
                        break;
                    }
                    else //Weapon Shop
                    {
                        shop = 14;
                        break;
                    }
                case 267: // I'm not sure what shop this was supposed to be Home of Giganto???
                    break;
                case 287: // Phantom Ship
                    if (ReadShop(0x11E0FC, emulator) == 249) //Item Shop
                    {
                        shop = 15;
                        break;
                    }
                    else //Weapon Shop
                    {
                        shop = 16;
                        break;
                    }
                    break;
                case 309: // Lideria
                    shop = 17;
                    break;
                case 479: // Vellweb
                    shop = 18;
                    break;
                case 332: // Furni
                    shop = 19;
                    break;
                case 349: // Dennigrad
                    shop = 20;
                    break;
                case 357: // Dennigrad
                    break;
                case 384: // Wingly Forest
                    break;
                case 515: // Ulara
                    break;
                case 525: // Ulara
                    break;
                case 435: // Kashua
                    break;
                case 564: // Rouge
                    break;
                case 619: // Moon
                    if (ReadShop(0x11E0FA, emulator) == 30)
                    {
                        break;
                    }
                    else
                    {
                        break;
                    }
                case 530: // Law City
                    if (ReadShop(0x11E0FC, emulator) == 229)
                    {
                        break;
                    }
                    else
                    {
                        break;
                    }
            }
            // Should be in Dictionary. CSV.
            List<List<int[]>> ShopList = new List<List<int[]>>();
            List<int[]> arrayList = new List<int[]>
            {
                new int[] { 0, 1},
                new int[] { 1, 1},
                new int[] { 2, 1}
            };
            for (int i = 0; i < 15; i++)
            {
                ShopList.Add(arrayList);
            }
            // Actual shop writing
            int z = 0;
            foreach (int[] item in ShopList[shop])
            {
                WriteShop(0x11E0F8 + z * 4, (ushort)item[0], emulator);
                WriteShop(0x11E0F8 + 0x2 + z * 4, (ushort)item[1], emulator);
                z += 1;
            }
            int shop_size = ShopList[shop].Count;
            WriteShop(0x11E13C, (ushort)shop_size, emulator);
            if (shop_og_size > shop_size)
            {
                for (int i = shop_size; i < 17; i++)
                {
                    WriteShop(0x11E0F8 + i * 4, (ushort)255, emulator);
                    WriteShop(0x11E0F8 + 2 + i * 4, (ushort)0, emulator);
                }
                Constants.WriteOutput("Shop Changed");
                SHOP_CHANGED = true;
            }
        }
        else if ((int)emulator.ReadByteU(0xB155C8) == 3) // Only an ePSXe 1.9.0 address 
        {
            SHOP_CHANGED = false;
        }

    }

    public static void Open(Emulator emulator) { }
    public static void Close(Emulator emulator) { }
    public static void Click(Emulator emulator) { }

    public static int ReadShop(int address, Emulator emulator)
    {
        return emulator.ReadShort(address + ShopOffset());
    }

    public static void WriteShop(int address, ushort value, Emulator emulator)
    {
        emulator.WriteShort(address + ShopOffset(), value);
    }

    public static int ShopOffset()
    {
        int offset = 0x0;
        if (Constants.REGION == Region.JPN)
        {
            offset -= 0x4D90;
        }
        else if (Constants.REGION == Region.EUR_GER)
        {
            offset += 0x120;
        }
        return offset;
    }
}