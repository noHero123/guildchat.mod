using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;


namespace guildchat.mod
{
    public struct calendarentry
    {
        public string title;
        public string description;
        public DateTime start;
        public DateTime end;
    }

    class Guilddata
    {

        public bool workthreadready = true;
        public string welcomeMessage = "";
        public string windowMessage = "";
        public string guildcolor = "<color=#00ff66>";
        public string guildroom = "";
        public string googledatakey = "";
        public List<string> calendarentrys = new List<string>();
        public string calendareditor = "";
        List<string> onlineorofflineentrys = new List<string>();
        public string onlineorofflineForm = "";
        public List<calendarentry> calendar = new List<calendarentry>();
        public List<string> members = new List<string>();
        Cryptomat cry;
        public string modfolder;

        public Guilddata(Cryptomat c, string m) 
        { 
            this.cry = c;
            this.modfolder = m;
            this.getGoogleDataKey();

        }


        public void getGoogleDataKey()
        {


            string[] files = Directory.GetFiles(this.modfolder, "guilddata.txt");
            if (files.Contains(this.modfolder + "guilddata.txt"))//File.Exists() was slower
            {
                string text = System.IO.File.ReadAllText(this.modfolder + "guilddata.txt");
                text.Replace("\r\n", "");
                this.googledatakey = text;
            }
            else { System.IO.File.WriteAllText(this.modfolder + "guilddata.txt", ""); }


        }

        public string getDataFromGoogleDocs(string googledatakey)
        {
            WebRequest myWebRequest;
            myWebRequest = WebRequest.Create("https://spreadsheets.google.com/feeds/list/" + googledatakey + "/od6/public/values?alt=json");
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            return ressi;
        }

        public void postDataToGoogleForm(List<string> entrys, List<string> keys, string form)
        {
            string txt = "";
            int i = 0;
            foreach (string t in entrys)
            {
                txt = txt + keys[i] + t + "&";
                i++;
            }
            string txt1 = txt.Replace(" ", "+");
            Console.WriteLine("##sendTogoogle: "+txt1);
            byte[] bytes = Encoding.ASCII.GetBytes(txt1 + "draftResponse=%5B%5D%0D%0A&pageHistory=0");

            HttpWebRequest webRequest = HttpWebRequest.Create(form + "formResponse") as HttpWebRequest;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            webRequest.Referer = form + "viewform";
            webRequest.ContentLength = bytes.Length;
            Stream requestStream = webRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            String data = readStream.ReadToEnd();
            Console.WriteLine(data);
            receiveStream.Close();
            readStream.Close();
            response.Close();
        }

        public void readJsonfromGoogle(string txt)
        {
            JsonReader jsonReader = new JsonReader();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonReader.Read(txt);
            dictionary = (Dictionary<string, object>)dictionary["feed"];
            Dictionary<string, object>[] entrys = (Dictionary<string, object>[])dictionary["entry"];
            this.calendar.Clear();
            this.members.Clear();
            for (int i = 0; i < entrys.GetLength(0); i++)
            {
                /*for (int j = 0; j < 4; j++)
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r"+(j+1)];
                    Console.WriteLine((string)dictionary["$t"]);
                    
                }*/

                dictionary = (Dictionary<string, object>)entrys[i]["gsx$r1"];
                if (((string)dictionary["$t"]).ToLower() == "key")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    cry.setkey((string)dictionary["$t"]);
                    Console.WriteLine("found key: " + (string)dictionary["$t"]);
                }
                if (((string)dictionary["$t"]).ToLower() == "shortnews")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    this.welcomeMessage = (string)dictionary["$t"];
                    Console.WriteLine("found news: " + (string)dictionary["$t"]);
                }
                if (((string)dictionary["$t"]).ToLower() == "longnews")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    this.windowMessage = (string)dictionary["$t"];
                    Console.WriteLine("found news: " + (string)dictionary["$t"]);
                }
                if (((string)dictionary["$t"]).ToLower() == "room")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    this.guildroom = (string)dictionary["$t"];
                    Console.WriteLine("found room: " + (string)dictionary["$t"]);
                }
                if (((string)dictionary["$t"]).ToLower() == "calendaredit")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    this.calendareditor = ((string)dictionary["$t"]).Split(new String[] { "viewform" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    Console.WriteLine("found calendaredit: " + this.calendareditor);
                }
                if (((string)dictionary["$t"]).ToLower() == "onlineoroffline")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    this.onlineorofflineForm = ((string)dictionary["$t"]).Split(new String[] { "viewform" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    Console.WriteLine("found onlineoroffline: " + this.onlineorofflineForm);
                }
                if (((string)dictionary["$t"]).ToLower() == "calendar")
                {
                    calendarentry ce = new calendarentry();
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    ce.title = (string)dictionary["$t"];
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r3"];
                    ce.description = (string)dictionary["$t"];
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r4"];
                    ce.start = DateTime.ParseExact((string)dictionary["$t"], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r5"];
                    ce.end = DateTime.ParseExact((string)dictionary["$t"], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    this.calendar.Add(ce);
                }
                if (((string)dictionary["$t"]).ToLower() == "member")
                {
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r2"];
                    string mem = (string)dictionary["$t"];
                    dictionary = (Dictionary<string, object>)entrys[i]["gsx$r3"];
                    string status= (string)dictionary["$t"];
                    if (status == "online") mem = mem + " is online";
                    else {
                        if (status == "") { mem = mem + " never online"; }
                        else
                            mem = mem + " last online: " + status.Substring(0, status.Length - 3) ; 
                    }
                    this.members.Add(mem);
                }


            }

            if (this.calendareditor != "")
            { // get data for posting data to the calendar 
                this.getEntrysFromCalendarForm();
            }
            if (this.calendareditor != "")
            { // get data for posting data to the calendar 
                this.getOnlineOrOfflineForm();
            }
        }

        private void getEntrysFromCalendarForm()
        {
            Console.WriteLine("getCalendarEntrys");
            WebRequest myWebRequest;
            myWebRequest = WebRequest.Create(this.calendareditor);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            //Console.WriteLine(ressi);
            string[] splitter = ressi.Split(new string[] { "id=\"entry_" }, StringSplitOptions.RemoveEmptyEntries);
            this.calendarentrys.Clear();
            string entry1 = "entry." + splitter[1].Split('"')[0] + "=";
            string entry2 = "entry." + splitter[2].Split('"')[0] + "=";
            string entry3 = "entry." + splitter[3].Split('"')[0] + "=";
            string entry4 = "entry." + splitter[4].Split('"')[0] + "=";
            this.calendarentrys.Add(entry1);
            this.calendarentrys.Add(entry2);
            this.calendarentrys.Add(entry3);
            this.calendarentrys.Add(entry4);
            foreach (string s in this.calendarentrys)
            {
                Console.WriteLine(s);
            }

            // small test
            List<string> test = new List<string>();
            test.Add("testevent");
            test.Add("testdescription");
            test.Add("22.11.2013 11:00:12");
            test.Add("23.11.2013 11:00:12");
            //postDataToGoogleCalendarForm(test);

        }


        private void getOnlineOrOfflineForm()
        {
            Console.WriteLine("getonlineorofflineEntrys");
            WebRequest myWebRequest;
            myWebRequest = WebRequest.Create(this.onlineorofflineForm);
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;// or you get an exeption, because mono doesnt trust anyone
            myWebRequest.Timeout = 10000;
            WebResponse myWebResponse = myWebRequest.GetResponse();
            System.IO.Stream stream = myWebResponse.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
            string ressi = reader.ReadToEnd();
            //Console.WriteLine(ressi);
            string[] splitter = ressi.Split(new string[] { "id=\"entry_" }, StringSplitOptions.RemoveEmptyEntries);
            this.onlineorofflineentrys.Clear();
            string entry1 = "entry." + splitter[1].Split('"')[0] + "=";
            string entry2 = "entry." + splitter[2].Split('"')[0] + "=";
            this.onlineorofflineentrys.Add(entry1);
            this.onlineorofflineentrys.Add(entry2);
            foreach (string s in this.onlineorofflineentrys)
            {
                Console.WriteLine(s);
            }
        }


        public void sendOnline()
        {
            if (this.onlineorofflineForm != "")
            {
                List<string> test = new List<string>();
                test.Add(this.googledatakey);
                test.Add("online");
                postDataToGoogleForm(test,this.onlineorofflineentrys, this.onlineorofflineForm);
            }
        }

        public void sendOffline()
        {
            if (this.onlineorofflineForm != "")
            {
                List<string> test = new List<string>();
                test.Add(this.googledatakey);
                DateTime date = DateTime.Now;
                date = date.Add(DateTime.UtcNow - DateTime.Now);
                test.Add(date.ToString("dd.MM.yyyy HH:mm:01"));
                postDataToGoogleForm(test,this.onlineorofflineentrys, this.onlineorofflineForm);

            }
        }

        public void workthread()
        {
            this.workthreadready = false;
            this.readJsonfromGoogle(this.getDataFromGoogleDocs(this.googledatakey));
            this.workthreadready = true;
        }


    }
}
