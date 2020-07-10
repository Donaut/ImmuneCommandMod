using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                case "/kit-doc":
                    int IA = player.m_inventory.GetItemAmountByType(254); // Start currency Check
                    if (IA <= 4999) // Value to check against.
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg); // Return error message if plater des not have enough currency!
                    }
                    else if (IA >= 5000) // If players currency is higher then the value given here
                    {
                        int num = 500; // Kit Price
                        num = Math.Min(IA, num); // Price math logic
                        player.m_inventory.DeclineItemAmountByType(254, num); // Define item used as currency
                        // Clothing
                        // server.CreateFreeWorldItem(ID, amount, player.GetPosition(), Durability); <-- Structure to follow.
                        server.CreateFreeWorldItem(153, 1, player.GetPosition(), 100);   // Leather-Vest
                        server.CreateFreeWorldItem(171, 1, player.GetPosition(), 100);   // Shoes
                        // Weapons
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);    // Knife
                        server.CreateFreeWorldItem(107, 1, player.GetPosition(), 100);   // Torch
                        // Food
                        server.CreateFreeWorldItem(3, 6, player.GetPosition(), 100);     // Coocked Potatoes
                        server.CreateFreeWorldItem(8, 1, player.GetPosition(), 100);     // Energy Bar
                        server.CreateFreeWorldItem(17, 2, player.GetPosition(), 100);    // Water
                        // ITEMS
                        server.CreateFreeWorldItem(140, 3, player.GetPosition(), 100);   // Bandages
                        server.CreateFreeWorldItem(141, 1, player.GetPosition(), 100);   // Antibiotics
                        server.CreateFreeWorldItem(142, 2, player.GetPosition(), 100);   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition(), 100);   // Medpack
                        server.SendMoneyUpdate(player); // Request money update from server.
                        server.SendMessageToPlayerLocal("BOUGHT DOCTOR KIT!!!", player, msg); // Return success message!!!
                    }
                    break;
                case "/kit-scav":
                    int IA_1 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_1 <= 4999)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_1 >= 5000)
                    {
                        int num = 500;
                        num = Math.Min(IA_1, num);
                        player.m_inventory.DeclineItemAmountByType(254, num);
                        // Clothing
                        server.CreateFreeWorldItem(151, 1, player.GetPosition(), 100);   // Scrap-Vest
                        server.CreateFreeWorldItem(171, 1, player.GetPosition(), 100);   // C_Shoes
                        // Weapons
                        server.CreateFreeWorldItem(111, 1, player.GetPosition(), 100);    // Machete
                        server.CreateFreeWorldItem(93, 1, player.GetPosition(), 100);   // Knife
                        server.CreateFreeWorldItem(61, 1, player.GetPosition(), 100);   // Revolver
                        // AMMO
                        server.CreateFreeWorldItem(40, 20, player.GetPosition(), 100);   // 45mm
                        // Food
                        server.CreateFreeWorldItem(4, 2, player.GetPosition(), 100);     // Raw Meat
                        server.CreateFreeWorldItem(8, 1, player.GetPosition(), 100);     // Energy Bar
                        server.CreateFreeWorldItem(10, 2, player.GetPosition(), 100);    // Canned Food
                        server.CreateFreeWorldItem(18, 2, player.GetPosition(), 100);    // Beer
                        // ITEMS
                        server.CreateFreeWorldItem(140, 3, player.GetPosition(), 100);   // Bandages
                        server.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT SCAV KIT!!!", player, msg); 
                    }
                    break;
                case "/kit-scav2":
                    int IA_2 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_2 <= 4999)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_2 >= 5000)
                    {
                        int num = 500;
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
                        server.CreateFreeWorldItem(4, 2, player.GetPosition(), 100);     // Raw Meat
                        server.CreateFreeWorldItem(9, 4, player.GetPosition(), 100);     // Mushrooms
                        server.CreateFreeWorldItem(10, 1, player.GetPosition(), 100);    // Canned Food
                        server.CreateFreeWorldItem(15, 1, player.GetPosition(), 100);    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition(), 100);   // Bandages
                        server.CreateFreeWorldItem(140, 1, player.GetPosition(), 100);   // Painkillers
                        server.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT SCAV-2 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-bandit":
                    int IA_3 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_3 <= 4999)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_3 >= 5000)
                    {
                        int num = 500;
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
                        server.CreateFreeWorldItem(12, 2, player.GetPosition(), 100);     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition(), 100);     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition(), 100);    // Wine
                        server.CreateFreeWorldItem(15, 1, player.GetPosition(), 100);    // Rum-Bottle
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition(), 100);   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition(), 100);   // Painkillers
                        server.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT BANDIT KIT!!!", player, msg);
                    }
                    break;
                case "/kit-guard1":
                    int IA_4 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_4 <= 4999)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_4 >= 5000)
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
                        server.CreateFreeWorldItem(12, 2, player.GetPosition(), 100);     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition(), 100);     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition(), 100);    // Wine
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition(), 100);   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition(), 100);   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition(), 100);   // Medkit
                        server.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT GUARD-1 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-guard2":
                    int IA_5 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_5 <= 4999)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_5 >= 5000)
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
                        server.CreateFreeWorldItem(12, 2, player.GetPosition(), 100);     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition(), 100);     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition(), 100);    // Wine
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition(), 100);   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition(), 100);   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition(), 100);   // Medkit
                        server.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT GUARD-2 KIT!!!", player, msg);
                    }
                    break;
                case "/kit-guard3":
                    int IA_6 = player.m_inventory.GetItemAmountByType(254);
                    if (IA_6 <= 4999)
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg);
                    }
                    else if (IA_6 >= 5000)
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
                        server.CreateFreeWorldItem(12, 2, player.GetPosition(), 100);     // Cooked_fish
                        server.CreateFreeWorldItem(8, 4, player.GetPosition(), 100);     // Energy-Bar
                        server.CreateFreeWorldItem(16, 1, player.GetPosition(), 100);    // Wine
                        // ITEMS
                        server.CreateFreeWorldItem(140, 2, player.GetPosition(), 100);   // Bandages
                        server.CreateFreeWorldItem(142, 2, player.GetPosition(), 100);   // Painkillers
                        server.CreateFreeWorldItem(143, 1, player.GetPosition(), 100);   // Medkit
                        server.SendMoneyUpdate(player);
                        server.SendMessageToPlayerLocal("BOUGHT GUARD-3 KIT!!!", player, msg);
                    }
                    break;
                case "/about":
                    server.SendMessageToPlayerLocal("Com-Mod is a project to help expand the Immune-Dedicated software capabilities. For more info please use /commands and /help commandname.", 
                        player, msg);
                    break;
                case "/shout":
                    int IA_C_SHOUT = player.m_inventory.GetItemAmountByType(254); // Start currency Check
                    if (IA_C_SHOUT <= 499) // Value to check against.
                    {
                        server.SendMessageToPlayerLocal("Not Enough Gold!", player, msg); // Return error message if plater des not have enough currency!
                    }
                    else if (IA_C_SHOUT >= 500) // If players currency is higher then the value given here
                    {
                        string text_shout = msg.ReadString();
                        server.SendNotification(text_shout);
                    }  
                    break;

                case "/commands":
                    server.SendMessageToPlayerLocal("<color=red>COMMANDS</color> <color=green>ARE</color> <color=purple>COLOR</color> <color=purple>CODED</color>!/nEach command is <color=red>color coded</color> within /help-commandname./nColors represent permission level needed to use them./n<color=red>RED COMMANDS ARE ADMIN ONLY!!!</color>/n<color=yellow>YELLOW COMMANDS REQUIRE GOLD TO EXECUTE!</color>/n<color=green>GREEN COMMANDS ARE ALL LEVEL ACCESS!</color>", player, msg);
                    break;

                case "/help":
                    server.SendMessageToPlayerLocal("Avalible commands are: <color=green>/kit-doc</color>, <color=purple>/weapon</color>, <color=purple>/food</color>, <color=purple>/medicine</color> and <color=red>/about</color>", player, msg);
                    break;
                case "/?":
                    server.SendMessageToPlayerLocal("Com-Mod: <color=purple>Made by</color> <color=green>Va1idUser</color> and <color=red>Donaut</color>.", player, msg);
                    break;

 // Useless/Outdated/Wrong method for safe item dispersal/execution. "Potential spam/abuse to cause crashes." Left for educational purposes.
 /*
               case "/weapon":
                     //Shotgun
                   server.CreateFreeWorldItem(63, 1, player.GetPosition(), 100);
                     //Ammo
                   server.CreateFreeWorldItem(44, 50, player.GetPosition());
                     //Sneakers
                   server.CreateFreeWorldItem(170, 1, player.GetPosition(), 100);
                   break;
               case "/food":
                     //Canned Food's
                   server.CreateFreeWorldItem(10, 2, player.GetPosition());
                    break;
               case "/medicine":
                   server.CreateFreeWorldItem(143, 2, player.GetPosition());
                    break;
 */
                default:
                    break;
            }
        }
    }
}
