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
                    server.SendMessageToPlayerLocal("Avalible commands are: <color=purple>/weapon</color>, <color=purple>/food</color>, <color=purple>/medicine</color> and <color=red>/about</color>", player, msg);
                    break;
                case "/?":
                    server.SendMessageToPlayerLocal("Avalible commands are: <color=purple>/weapon</color>, <color=purple>/food</color>, <color=purple>/medicine</color> and <color=red>/about</color>", player, msg);
                    break;
                default:
                    break;
            }
        }
    }
}
