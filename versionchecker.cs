using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;

namespace guildchat.mod
{
    class versionchecker: ICommListener
    {
        bool getdata = false;
        int anzWarnings = 2;
        int warnings = 0;
        string currentversion = "1.0.0.1";
        string newestversion = "0.0.0.0";

        public  versionchecker()
        {
            try
            {
                App.Communicator.addListener(this);
            }
            catch { }
            
        }

            public string getDataFromGoogleDocs()
        {
            WebRequest myWebRequest;
            myWebRequest = WebRequest.Create("https://spreadsheets.google.com/feeds/list/0AhhxijYPL-BGdERzVGo2aVBTSHZTc0ZRMHFNNktqUWc/od6/public/values?alt=json");
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            return ressi;
        }


            public void readJsonfromGoogle(string txt)
            {
                JsonReader jsonReader = new JsonReader();
                Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(txt);
                dictionary = (Dictionary<string, object>)dictionary["feed"];
                Dictionary<string, object>[] entrys = (Dictionary<string, object>[])dictionary["entry"];
                for (int i = 0; i < entrys.GetLength(0); i++)
                {
                    /*for (int j = 0; j < 4; j++)
                    {
                        dictionary = (Dictionary<string, object>)entrys[i]["gsx$r"+(j+1)];
                        Console.WriteLine((string)dictionary["$t"]);
                    
                    }*/

                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r1"];
                    if (((string)dictionary["$t"]).ToLower() == "version")
                    {
                        dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                        this.newestversion=(string)dictionary["$t"];
                        Console.WriteLine("version string recived");
                    }

                }
            }

        public void handleMessage(Message msg)
        { // collect data for enchantments (or units who buff)
            
            if (msg is RoomChatMessageMessage)
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)msg;
                if (rcmm.text.StartsWith("You have joined"))
                {
                    if (this.getdata == false)
                    {
                        readJsonfromGoogle(getDataFromGoogleDocs());
                        this.getdata = true;
                    }
                    if (this.currentversion != this.newestversion)
                    {
                        RoomChatMessageMessage nrcmm = new RoomChatMessageMessage(rcmm.roomName, "your Guildchatmod is outdated, please install a new version");
                        nrcmm.from = "Version Checker";
                        App.ArenaChat.handleMessage(nrcmm);
                    }
                    warnings++;
                    if (warnings >= anzWarnings)
                    {
                        App.Communicator.removeListener(this);
                    }
                }
            }


            return;
        }
        public void onConnect(OnConnectData ocd)
        {
            return; // don't care
        }

       

    }
}
