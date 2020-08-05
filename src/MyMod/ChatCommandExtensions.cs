using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MyMod
{
    public class ChatCommandExtensions
    {
        internal static void HandleChatCommand(string text, ServerPlayer player, NetIncomingMessage msg)
        {
            var server = (LidServer)UnityEngine.Object.FindObjectOfType(typeof(LidServer));
            
            string[] commands = text.Split(' ');
            switch (commands[0])
            {
                case "/kit-doc1":
                    int IA = player.m_inventory.GetItemAmountByType(254); // Start currency Check
                    if (IA <= 349) // Value to check against.
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg); // Return error message if plater des not have enough currency!
                    }
                    else if (IA >= 350) // If players currency is higher then the value given here
                    {
                        int num = 350; // Kit Price
                        num = Math.Min(IA, num); // Price math logic
                        player.m_inventory.DeclineItemAmountByType(254, num); // Define item used as currency
                        server.CreateFreeWorldItem(153, 1, player.GetPosition(), 100);   // Leather-Vest
                        server.CreateFreeWorldItem(171, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);    // Knife
                        server.CreateFreeWorldItem(107, 1, player.GetPosition(), 100);   // Torch
                        // Food
                        server.CreateFreeWorldItem(3, 6, player.GetPosition());     // Coocked Potatoes
                        server.CreateFreeWorldItem(8, 1, player.GetPosition());     // Energy Bar
                        server.CreateFreeWorldItem(17, 2, player.GetPosition());    // Water
                        // ITEMS
                        server.CreateFreeWorldItem(140, 3, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(141, 1, player.GetPosition());   // Antibiotics
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition());   // Medpack
                        LidServer.SendMoneyUpdate(player); // Request money update from server.
                        server.SendMessageToPlayerLocal("BOUGHT DOCTOR KIT!!!", player, msg); // Return success message!!!
                    }
                    break;
                case "/kit-scav1":
                    int IA_1 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_1 <= 349)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_1 >= 350)
                    {
                        int num = 350;
                        num = Math.Min(IA_1, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        // Weapons
                        server.CreateFreeWorldItem(111, 1, player.GetPosition(), 100);    // Machete
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(61, 1, player.GetPosition(), 100);   // Revolver
                        // AMMO
                        server.CreateFreeWorldItem(40, 20, player.GetPosition(), 100);   // 45mm
                        // Food
                        server.CreateFreeWorldItem(4, 2, player.GetPosition());     // Raw Meat
                        server.CreateFreeWorldItem(8, 1, player.GetPosition());     // Energy Bar
                        server.CreateFreeWorldItem(10, 2, player.GetPosition());    // Canned Food
                        server.CreateFreeWorldItem(18, 2, player.GetPosition());    // Beer
                        // ITEMS
                        server.CreateFreeWorldItem(140, 3, player.GetPosition());   // Bandages
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT SCAV KIT!!!", player, msg);
                    }
                    break;
                case "/kit-scav2":
                    int IA_2 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_2 <= 349)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_2 >= 350)
                    {
                        int num = 350;
                        num = Math.Min(IA_2, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(151, 1, player.GetPosition(), 100);   // Scrap-Vest
                        server.CreateFreeWorldItem(171, 1, player.GetPosition(), 100);   // C_Shoes
                        // Weapons
                        server.CreateFreeWorldItem(104, 1, player.GetPosition(), 100);    // Mutant Claw
                        server.CreateFreeWorldItem(106, 1, player.GetPosition(), 100);   // C_Knife
                        server.CreateFreeWorldItem(79, 1, player.GetPosition(), 100);   // Bow
                        // AMMO
                        server.CreateFreeWorldItem(50, 35, player.GetPosition(), 100);   // Arrows
                        // Food
                        server.CreateFreeWorldItem(4, 2, player.GetPosition());     // Raw Meat
                        server.CreateFreeWorldItem(15, 1, player.GetPosition());    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(140, 1, player.GetPosition());   // Painkillers
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT SCAV-2 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-scav3":
                    int IA_7 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_7 <= 349)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_7 >= 350)
                    {
                        int num = 350;
                        num = Math.Min(IA_7, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(151, 1, player.GetPosition(), 100);   // Scrap-Vest
                        server.CreateFreeWorldItem(171, 1, player.GetPosition(), 100);   // C_Shoes
                        // Weapons
                        server.CreateFreeWorldItem(79, 1, player.GetPosition(), 100);   // Bow
                        // AMMO
                        server.CreateFreeWorldItem(50, 35, player.GetPosition(), 100);   // Arrows
                        // Food
                        server.CreateFreeWorldItem(4, 2, player.GetPosition());     // Raw Meat
                        server.CreateFreeWorldItem(9, 4, player.GetPosition());     // Mushrooms
                        server.CreateFreeWorldItem(10, 1, player.GetPosition());    // Canned Food
                        server.CreateFreeWorldItem(15, 1, player.GetPosition());    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(140, 1, player.GetPosition());   // Painkillers
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT SCAV-3 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-bandit1":
                    int IA_3 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_3 <= 349)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_3 >= 350)
                    {
                        int num = 350;
                        num = Math.Min(IA_3, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(152, 1, player.GetPosition(), 100);   // Metal-Vest
                        server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(95, 1, player.GetPosition(), 100);    // Mutant Claw
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(62, 1, player.GetPosition(), 100);   // SMG
                        // AMMO
                        server.CreateFreeWorldItem(42, 35, player.GetPosition(), 100);   // 556
                        // Food
                        server.CreateFreeWorldItem(12, 2, player.GetPosition());     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition());     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition());    // Wine
                        server.CreateFreeWorldItem(15, 1, player.GetPosition());    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT BANDIT KIT!!!", player, msg);
                    }
                    break;
                case "/kit-bandit2":
                    int IA_8 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_8 <= 349)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_8 >= 350)
                    {
                        int num = 350;
                        num = Math.Min(IA_8, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(68, 1, player.GetPosition(), 100);   // Tommy-Gun
                        // AMMO
                        server.CreateFreeWorldItem(40, 35, player.GetPosition(), 100);   // 45mm
                        // Food
                        server.CreateFreeWorldItem(12, 2, player.GetPosition());     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition());     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition());    // Wine
                        server.CreateFreeWorldItem(15, 1, player.GetPosition());    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT BANDIT KIT!!!", player, msg);
                    }
                    break;
                case "/kit-bandit3":
                    int IA_9 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_9 <= 349)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_9 >= 350)
                    {
                        int num = 350;
                        num = Math.Min(IA_9, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(152, 1, player.GetPosition(), 100);   // Metal-Vest
                        server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(95, 1, player.GetPosition(), 100);    // Mutant Claw
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        // AMMO
                        // Food
                        server.CreateFreeWorldItem(12, 2, player.GetPosition());     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition());     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition());    // Wine
                        server.CreateFreeWorldItem(15, 1, player.GetPosition());    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT BANDIT KIT!!!", player, msg);
                    }
                    break;
                case "/kit-guard1":
                    int IA_4 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_4 <= 499)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_4 >= 500)
                    {
                        int num = 500;
                        num = Math.Min(IA_4, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(154, 1, player.GetPosition(), 100);   // Guardian-Vest
                        server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(99, 1, player.GetPosition(), 100);    // Giant Sword
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(65, 1, player.GetPosition(), 100);   // AK47
                        // AMMO
                        server.CreateFreeWorldItem(43, 35, player.GetPosition(), 100);   // 762
                        // Food
                        server.CreateFreeWorldItem(12, 2, player.GetPosition());     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition());     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition());    // Wine
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition());   // Medkit
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT GUARD-1 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-guard2":
                    int IA_5 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_5 <= 499)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_5 >= 500)
                    {
                        int num = 500;
                        num = Math.Min(IA_5, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(154, 1, player.GetPosition(), 100);   // Guardian-Vest
                        server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(99, 1, player.GetPosition(), 100);    // Giant Sword
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(64, 1, player.GetPosition(), 100);   // Sniper
                        // AMMO
                        server.CreateFreeWorldItem(43, 35, player.GetPosition(), 100);   // 762
                        // Food
                        server.CreateFreeWorldItem(12, 2, player.GetPosition());     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition());     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition());    // Wine
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition());   // Medkit
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT GUARD-2 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-guard3":
                    int IA_6 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_6 <= 499)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_6 >= 500)
                    {
                        int num = 500;
                        num = Math.Min(IA_6, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(154, 1, player.GetPosition(), 100);   // Guardian-Vest
                        server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(99, 1, player.GetPosition(), 100);    // Giant Sword
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(67, 1, player.GetPosition(), 100);   // Auto-SHotgun
                        // AMMO
                        server.CreateFreeWorldItem(44, 35, player.GetPosition(), 100);   // Shells
                        // Food
                        server.CreateFreeWorldItem(12, 2, player.GetPosition());     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition());     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition());    // Wine
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition());   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition());   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition());   // Medkit
                        LidServer.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT GUARD-3 KIT!!!", player, msg);
                    }
                    break;
                case "/players":
                case "/online":
                    server.SendMessageToPlayerLocal(LNG.Get("CMD_ONLINE_PLAYERS").Replace("{p_online}", server.GetPlayerCount().ToString()), player, msg);
                    break;

                case "/about":
                    server.SendMessageToPlayerLocal("I.C.E is a project to help expand the Immune-Dedicated software capabilities. For more info please use /commands and /help commandname.",
                        player, msg);
                    break;
                case "/shout":
                    server.SendNotification(text.Remove(0,6)); 
                    break;
                case "/commands":
                    server.SendMessageToPlayerLocal("<color=red>COMMANDS</color> <color=green>ARE</color> <color=purple>COLOR</color> <color=purple>CODED</color>! Each command is <color=red>color coded</color> within /help-commandname. Colors represent permission level needed to use them. <color=red>RED COMMANDS ARE ADMIN ONLY!!!</color> <color=yellow>YELLOW COMMANDS REQUIRE GOLD TO EXECUTE!</color> <color=green>GREEN COMMANDS ARE ALL LEVEL ACCESS!</color>", player, msg);
                    break;

                case "/help":
                    server.SendMessageToPlayerLocal("Avalible commands are: <color=green>/kit-doc</color>, <color=purple>/weapon</color>, <color=purple>/food</color>, <color=purple>/medicine</color> and <color=red>/about</color>", player, msg);
                    break;

                case "/help-kit":
                    server.SendMessageToPlayerLocal("Usage for /kit-xxx: To purchase a kit you must have the required amount of gold and or permissions. Example: Enter */kit-doc* without the ** to buy a doctors kit for 500gold.", player, msg);
                    break;
                case "/?":
                    server.SendMessageToPlayerLocal("I.C.E-Mod: <color=purple>Made by</color> <color=green>Va1idUser: Github.com/McSkinnerOG/ImmuneCommandMod</color> and <color=red>Donaut: Github.com/Donaut/ImmuneCommandMod</color>.", player, msg);
                    break;
             
                default:
                    break;
            }
            switch (commands[1])
            {
                case "kit-doc": 
                    server.SendMessageToPlayerLocal("<color=purple>Doctor-kit costs 500gold and recieves: </color> <color=yellow>Leather-Vest x1, Shoes x1, </color> <color=white>Torch x1, Knife x1,</color> <color=brown>Cooked-Potatoes x6, Energy-Bar x1,</color> <color=cyan>Water x2,</color> <color=red>Bandages x3, Anti-Biotics x1 , Painkillers x2, Medpack x1</color>", player, msg);
                    break;
                case "kit-scav1":
                    server.SendMessageToPlayerLocal("<color=purple>Scavenger-kit 1 costs 500gold and recieves: </color> <color=yellow>Scrap-Vest x1, Shoes x1, </color> <color=white>Machete x1, Knife x1, Revolver x1, 45mm Ammo x20</color> <color=brown> Raw-Meat x 1, Canned-Food x6, Energy-Bar x1,</color> <color=cyan>Beer x2,</color> <color=red>Bandages x3</color>", player, msg);
                    break;
                case "kit-scav2":
                    server.SendMessageToPlayerLocal("<color=purple>Scavenger-kit 2 costs 500gold and recieves: </color> <color=yellow>Scrap-Vest x1, Sneakers x1, </color> <color=white>Mutant-Claw x1, Crafted-Knife x1, Bow x1, Arrows x32 </color> <color=brown>Cooked-Potatoes x6, Mushrooms x3,</color> <color=cyan>Rum-Bottle x2,</color> <color=red>Bandages x3, Anti-Biotics x1 , Painkillers x2, Medpack x1</color>", player, msg);
                    break;
                case "kit-bandit":
                    server.SendMessageToPlayerLocal("<color=purple>Bandit-kit costs 500gold and recieves: </color> <color=yellow>Metal-Vest x1, Sneakers x1, </color> <color=white>Katana x1, Knife x1, SMG x1, 9mm Ammo x32 </color> <color=brown>Cooked-Fish x3, Energy-Bar x1,</color> <color=cyan>Rum-Bottle x2, Wine x1, </color> <color=red>Bandages x3, Medpack x1</color>", player, msg);
                    break;
                case "kit-guard1":
                    server.SendMessageToPlayerLocal("<color=purple>Guardian-kit 1 costs 5000gold and recieves: </color> <color=yellow>Guardian-Vest x1, Sneakers x1, </color> <color=white>Torch x1, Knife x1,</color> <color=brown>Cooked-Potatoes x6, Energy-Bar x1,</color> <color=cyan>Water x2,</color> <color=red>Bandages x3, Anti-Biotics x1 , Painkillers x2, Medpack x1</color>", player, msg);
                    break;
                case "kit-guard2":
                    server.SendMessageToPlayerLocal("<color=purple>Guardian-kit 2 costs 5000gold and recieves: </color> <color=yellow>Guardian-Vest x1, Sneakers x1, </color> <color=white>Torch x1, Knife x1,</color> <color=brown>Cooked-Potatoes x6, Energy-Bar x1,</color> <color=cyan>Water x2,</color> <color=red>Bandages x3, Anti-Biotics x1 , Painkillers x2, Medpack x1</color>", player, msg);
                    break;
                case "kit-guard3":
                    server.SendMessageToPlayerLocal("<color=purple>Guardian-kit 3 costs 5000gold and recieves: </color> <color=yellow>Guardian-Vest x1, Sneakers x1, </color> <color=white>Giant-Sword x1, Knife x1, Crowbar x1, AK47 x1, </color> <color=brown>Canned-Food x6, Energy-Bar x1, Soda x1, </color> <color=cyan>Water x2,</color> <color=red>Medpack x1.</color>", player, msg);
                    break;
                default:
                    break;
            }
        }
    }
}
