using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
//using Mono.Cecil;
//using ScrollsModLoader.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using JsonFx.Json;
using System.Text.RegularExpressions;
using System.Threading;



namespace guildchat.mod
{
   
    public class guildchat : BaseMod, ICommListener
	{
        
        bool justontimeonline = false;
        string oldtyping = "";

        string modfolder = "";

        bool chatisshown = false;

        float opacity = 1f;
        ChatRooms chatrooms=null;
        //MethodInfo addroom;
        FieldInfo chatmsg;
        List<ChatUser> guildmembers=new List<ChatUser>();
        //ArenaChat ac;
        Cryptomat cry;
        Guilddata gdata;
        Rect guildbutton;
        GUISkin lobbyskin;
        bool inGuildmenu = false;
        GUIStyle chatLogStyle;
        guildGui ggui;
        versionchecker vs;
        Rectomat recto;
        FieldInfo chatLogStyleinfo;
        FieldInfo targetchathightinfo;
        

        public void handleMessage(Message msg)
        { // collect data for enchantments (or units who buff)

            if (msg is RoomChatMessageMessage)
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                if (rcmm.text == "You have joined \"" + gdata.guildroom + "\"")
                {
                    if (!this.justontimeonline)
                    {
                        gdata.sendOnline();
                        this.justontimeonline = true;
                    }
                    RoomChatMessageMessage nrcmm = new RoomChatMessageMessage(gdata.guildroom, gdata.guildcolor + gdata.welcomeMessage + "</color>");
                    nrcmm.from = "Guildnews";
                    App.ArenaChat.handleMessage(nrcmm);
                }
            }

            return;
        }
        public void onConnect(OnConnectData ocd)
        {
            return; // don't care
        }

       

		//initialize everything here, Game is loaded at this point
        public guildchat()
        {
            cry = new Cryptomat();
            this.modfolder = this.OwnFolder() + Path.DirectorySeparatorChar;
            gdata = new Guilddata(cry, this.modfolder);


            
            recto = Rectomat.Instance;
            ggui = new guildGui(recto, gdata);
            this.targetchathightinfo = typeof(ChatUI).GetField("targetChatHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            chatmsg = typeof(ChatUI).GetField("chatMsg", BindingFlags.Instance | BindingFlags.NonPublic);
            //addroom = typeof(ChatRooms).GetMethod("AddActiveRoom", BindingFlags.Instance | BindingFlags.NonPublic);
            vs = new versionchecker();
            try
            {
                App.Communicator.addListener(this);
            }
            catch { }

            if (gdata.googledatakey != "")
            {
                new Thread(new ThreadStart(gdata.workthread)).Start();
                
            }
            

            
            this.lobbyskin=(GUISkin)Resources.Load("_GUISkins/Lobby");
             chatLogStyleinfo = typeof(ChatUI).GetField("chatMsgStyle", BindingFlags.Instance | BindingFlags.NonPublic);

            Console.WriteLine("loadet guildchat");
    
		}

        

		public static string GetName ()
		{
            return "Guildchat";
		}

		public static int GetVersion ()
		{
			return 1;
		}

       

       

		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
                    scrollsTypes["BattleMode"].Methods.GetMethod("sendBattleRequest", new Type[]{typeof(Message)}),
                    scrollsTypes["ChatUI"].Methods.GetMethod("Initiate")[0],
                    scrollsTypes["ChatUI"].Methods.GetMethod("Awake")[0],
                    scrollsTypes["ChatUI"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["ChatUI"].Methods.GetMethod("Show", new Type[]{typeof(bool)}),
                    scrollsTypes["ArenaChat"].Methods.GetMethod("handleMessage", new Type[]{typeof(Message)}),
                    scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}), 
                    scrollsTypes["ChatUI"].Methods.GetMethod("OnSendMessage",new Type[]{typeof(Room), typeof(string)}),
                    scrollsTypes["MainMenu"].Methods.GetMethod("OnGUI")[0],
                    scrollsTypes["GameSocket"].Methods.GetMethod("OnDestroy")[0],
             };
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}

       



       






        public override bool WantsToReplace(InvocationInfo info)
        {
            if (info.target is MainMenu && info.targetMethod.Equals("OnGUI"))
            {
                if (this.inGuildmenu)
                {
                    return true;
                }
            }

            if (info.target is Communicator && info.targetMethod.Equals("sendRequest"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                    if (rcmm.text.StartsWith("/g") || rcmm.text.StartsWith("\\g"))
                    {
                        return true;

                    }
                    if (gdata.autoencrypt && rcmm.roomName == gdata.guildroom)
                    {
                        return true;

                    }
                    if (rcmm.text.StartsWith("/autoencrypt"))
                    {
                        return true;

                    }
                }
            }
            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage wmsg = (RoomChatMessageMessage)msg;
                    if ((wmsg.text).StartsWith(".g") || (wmsg.text).StartsWith(",g ")) return true;

                }

            }

            return false;
        }

        public override void ReplaceMethod(InvocationInfo info, out object returnValue)
        {

            returnValue = null;

            if (info.target is Communicator && info.targetMethod.Equals("sendRequest"))
            {
                Message msg = (Message)info.arguments[0];
                if (msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                    Console.WriteLine("###roomchatmessage" + rcmm.text);

                    if (rcmm.text.StartsWith("/autoencrypt"))
                    {
                        gdata.autoencrypt = !gdata.autoencrypt;
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage(rcmm.roomName, "autoencryption in guildchat is disabled");
                        nrcmm.from = "guildmod";

                        if (gdata.autoencrypt) nrcmm.text = "autoencryption in guildchat is enabled";
                        App.ArenaChat.handleMessage(nrcmm);
                        gdata.saveEncryptSettings();
                        returnValue = true;
                        return;
                    }

                    if (rcmm.roomName == gdata.guildroom && !(rcmm.text.StartsWith("/g") || rcmm.text.StartsWith("\\g")))
                    {
                        rcmm.text = "/g " + rcmm.text;
                    }

                    if (rcmm.text.StartsWith("/g") || rcmm.text.StartsWith("\\g"))
                    {
                        Console.WriteLine("###" + rcmm.text);
                        string chiffre = cry.Encrypt(rcmm.text);
                        Console.WriteLine("###" + chiffre);
                        if (chiffre.Length >= 512)
                        {

                            returnValue = true;
                            return;
                        }

                        rcmm.text = chiffre;
                        rcmm.roomName = gdata.guildroom;
                        //App.Communicator.sendRequest(rcmm);

                        List<Room> lrooms = chatrooms.GetAllRooms();
                        bool isinguildroom = false;
                        foreach (Room room in lrooms)
                        {
                            if (room.name == gdata.guildroom)
                                isinguildroom = true;
                        }

                        if (!isinguildroom)
                        {
                            openGuildChatWindow();
                        }
                        returnValue = App.Communicator.send(rcmm);

                    }
                    

                }
            }


            if (info.target is ArenaChat && info.targetMethod.Equals("handleMessage"))
            {
                Message msg = (Message)info.arguments[0];
                if ( msg is RoomChatMessageMessage)
                {
                    RoomChatMessageMessage wmsg = (RoomChatMessageMessage)msg;
                    if ((wmsg.text).StartsWith(".g ") || (wmsg.text).StartsWith(",g "))
                    {
                        string txt = wmsg.text;
                        txt = cry.Decrypt(txt);
                        ArenaChat ac = (ArenaChat)info.target;
                        txt = gdata.guildcolor + txt + "</color>";
                        RoomChatMessageMessage rcmm = new RoomChatMessageMessage(gdata.guildroom, txt);
                        rcmm.from = wmsg.from;
                        ac.handleMessage(rcmm);

                    }

                }

            }



        }

        public override void BeforeInvoke(InvocationInfo info)
        {

            return;

        }


        public override void AfterInvoke (InvocationInfo info, ref object returnValue)
        
        {
            if (info.target is GameSocket && info.targetMethod.Equals("OnDestroy")) {
                gdata.sendOffline(); 
            }

            if (info.target is ChatUI && info.targetMethod.Equals("Show")) { this.chatisshown = (bool)info.arguments[0]; }

            if (info.target is MainMenu && info.targetMethod.Equals("OnGUI"))
            {
                GUI.depth = 21;
                GUIPositioner subMenuPositioner = App.LobbyMenu.getSubMenuPositioner(1f, 5);
                this.guildbutton = new Rect(subMenuPositioner.getButtonRect(0f));
                if (LobbyMenu.drawButton(this.guildbutton, "Guild", this.lobbyskin))
                {
                    this.inGuildmenu = !this.inGuildmenu;
                    ggui.reset();
                    targetchathightinfo.SetValue(App.ChatUI, (float)Screen.height * 0.25f);
                }
                if (this.inGuildmenu)
                {
                    //ggui.cardListPopupBigLabelSkin.label.fontSize = (int)(this.fieldHeight / 1.7f);
                    //ggui.cardListPopupSkin.label.fontSize = (int)(this.fieldHeight / 2.5f);
                    recto.setupPositions(this.chatisshown, 1.0f, this.chatLogStyle, ggui.cardListPopupSkin);
                    ggui.drawgui(this.chatLogStyle);
                }

            }
            
            if (info.target is ChatUI && (info.targetMethod.Equals("Initiate") || info.targetMethod.Equals("Awake")))
            {
                
                this.chatrooms = App.ArenaChat.ChatRooms;
                this.chatLogStyle = (GUIStyle)chatLogStyleinfo.GetValue(info.target);
                if (gdata.guildroom != "")
                {
                    openGuildChatWindow(); 
                }
            }

            if (info.target is ChatUI && info.targetMethod.Equals("OnGUI") )
            {
                string typingmessage = (string)chatmsg.GetValue(info.target);
                if (chatrooms.GetCurrentRoomName() == null) return;
                if ((this.chatrooms.GetCurrentRoomName()).Equals(gdata.guildroom) || typingmessage.StartsWith("/g") || typingmessage.StartsWith("\\g"))
                {
                    if (!(typingmessage.StartsWith("/g") || typingmessage.StartsWith("\\g")))
                    {
                        typingmessage = "/g " + typingmessage;
                    }

                    if (typingmessage != this.oldtyping && typingmessage.Length>=50)
                    {
                        string result = cry.Encrypt(typingmessage);
                        if (result.Length >= 512)
                        {
                            chatmsg.SetValue(info.target,oldtyping);
                        }
                        else
                        {
                            this.oldtyping = typingmessage;
                        }
                    }
                }
            }

            return;
        }



        private void openGuildChatWindow()
        {
            App.Communicator.sendRequest(new RoomEnterMessage(gdata.guildroom));
        }
	}
}

