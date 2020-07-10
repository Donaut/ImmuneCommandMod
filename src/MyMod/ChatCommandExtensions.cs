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
                        server.CreateFreeWorldItem(121, 1, player.GetPosition(), 100);   // Clothbox
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
                        server.SendMessageToPlayerLocal("BOUGHT TEST KIT!!!", player, msg); // Return success message!!!
                    }
                    break;
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
                case "/about":
                    server.SendMessageToPlayerLocal("Thanks for visiting the server. This is a test server(short-lived) The source code is available at. <color=lime>www.github.com/Donaut/ImmundeCommandMod</color>", player, msg);
                    break;
                case "/help":
                    server.SendMessageToPlayerLocal("Avalible commands are: <color=green>/kit-doc</color>, <color=purple>/weapon</color>, <color=purple>/food</color>, <color=purple>/medicine</color> and <color=red>/about</color>", player, msg);
                    break;
                case "/?":
                    server.SendMessageToPlayerLocal("Com-Mod: <color=purple>Made by</color> <color=green>Va1idUser</color> and <color=red>Donaut</color>.", player, msg);
                    break;
                default:
                    break;
            }
        }
    }
}
