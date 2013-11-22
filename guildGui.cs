using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading;

namespace guildchat.mod
{
    class guildGui
    {

        bool wantToPostEvent = false;
        int datechoosertarget = 0;
        bool datechooseractive = false;
        string dateChooser10hour="0";
        string dateChooser1hour = "0";
        string dateChooser10min = "0";
        string dateChooser1min = "0";

        float calendarhight = 10;
        float opacity = 1f;
        Rectomat recto;
        Guilddata gdata;
        Vector2 scrollPos, scrolll, scrolll2;
        public GUISkin cardListPopupSkin;
        public GUISkin cardListPopupGradientSkin;
        public GUISkin cardListPopupBigLabelSkin;
        public GUISkin cardListPopupLeftButtonSkin;

        string detailtitle="" ;
        string detaildescription ="";
        DateTime detailstart=DateTime.Now;
        DateTime detailend = DateTime.Now;
        string newEventEnd = "";
        string newEventStart = "";

        string DCDay = "";
        string DCMonth = "";
        string DCYear = "";
        string DCHour = "";
        string DCMin = "";
        DateTime DCdateTime = DateTime.Now;
        DateTime selected = DateTime.Now;


        private int gmenu = 0;

        public void reset()
        {
            this.datechooseractive = false;
            this.wantToPostEvent = false;
        }

        public guildGui(Rectomat r, Guilddata g)
        {
            gdata = g;
            recto = r;

            this.setskins((GUISkin)Resources.Load("_GUISkins/CardListPopup"), (GUISkin)Resources.Load("_GUISkins/CardListPopupGradient"), (GUISkin)Resources.Load("_GUISkins/CardListPopupBigLabel"), (GUISkin)Resources.Load("_GUISkins/CardListPopupLeftButton"));
           
        }

        public void setskins(GUISkin cllps, GUISkin clpgs, GUISkin clpbls, GUISkin clplbs)
        {
            this.cardListPopupSkin = cllps;
            this.cardListPopupGradientSkin = clpgs;
            this.cardListPopupBigLabelSkin = clpbls;
            this.cardListPopupLeftButtonSkin = clplbs;

        }

        public void drawgui(GUIStyle cls)
        {
            GUI.depth = 15;
            GUI.skin = this.cardListPopupSkin;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);

            GUI.Box(recto.position, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            GUI.Box(recto.position2, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);


            this.cardListPopupBigLabelSkin.label.fontSize = (int)(recto.fieldHeight / 2.5f);
            this.cardListPopupSkin.label.fontSize = (int)(recto.fieldHeight / 2.5f);

            if (datechooseractive) { this.dateChooser2000(cls); }
                if (gmenu == 0){this.drawNews();}

                if (gmenu == 1)
                {this.drawCalendar();}
                if (gmenu == 2)
                {newEvent(cls);}

                if (GUI.Button(recto.wtsbuttonrect, "News"))
                {
                    datechooseractive = false;
                    this.reset();
                    this.gmenu = 0;
                }
                if (GUI.Button(recto.wtbbuttonrect, "Calendar"))
                {
                    datechooseractive = false;
                    this.reset();
                    this.detailtitle = "";
                    this.detaildescription = "";
                    this.detailstart = DateTime.Now;
                    this.detailend = DateTime.Now;
                    this.gmenu = 1;

                }

                if (gdata.calendareditor != "")
                {
                    if (GUI.Button(recto.bothbuttonrect, "NewEvent"))
                    {
                        datechooseractive = false;
                        this.reset();
                        this.detailtitle = "new title";
                        this.detaildescription = "new description";
                        this.detailstart = DateTime.Now;
                        this.detailend = DateTime.Now;
                        this.newEventStart = detailstart.ToString("dd.MM.yyyy HH:mm");
                        this.newEventEnd = detailend.ToString("dd.MM.yyyy HH:mm");
                        this.gmenu = 2;

                    }
                }

                if (GUI.Button(recto.fillbuttonrect, "Refresh") && gdata.workthreadready)
                {
                    gdata.getGoogleDataKey();
                    new Thread(new ThreadStart(gdata.workthread)).Start();
                }
            
            // ende

            GUI.skin.button.normal.textColor = Color.white;
            GUI.skin.button.hover.textColor = Color.white;


            GUI.color = Color.white;
            GUI.contentColor = Color.white;

        }

        private void drawNews()
        {
            GUI.skin = this.cardListPopupBigLabelSkin;
            string message =  gdata.windowMessage;
            GUI.skin.label.wordWrap = true;
            float msghigh = GUI.skin.label.CalcHeight(new GUIContent(message), recto.position2.width - 30f);
            scrolll = GUI.BeginScrollView(recto.position2, scrolll, new Rect(0f, 0f, recto.position2.width - 20f, msghigh));
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(5f, 5f, recto.position2.width - 30f, msghigh), message);

            //Console.WriteLine(message);

            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.EndScrollView();

            if (true)
            {
                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.filtermenurect, string.Empty);
                    GUI.skin = this.cardListPopupBigLabelSkin;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                    message = "Guild Members" + "\r\n";
                    foreach (string s in gdata.members)
                    {
                        if(s.EndsWith("online") && !s.EndsWith("never online"))
                        {
                        message = message + "\r\n" + s;
                        }
                    }
                    foreach (string s in gdata.members)
                    {
                        if(!s.EndsWith("online") || s.EndsWith("never online"))
                        {
                        message = message + "\r\n" + s;
                        }
                    }
                    GUI.skin.label.wordWrap = true;
                    msghigh = GUI.skin.label.CalcHeight(new GUIContent(message), recto.tbmessagescroll.width - 30f);
                    GUI.skin = this.cardListPopupSkin;
                    scrolll2 = GUI.BeginScrollView(recto.tbmessagescroll, scrolll2, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
                    GUI.skin = this.cardListPopupBigLabelSkin;

                    GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), message);

                    //Console.WriteLine(message);

                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.EndScrollView();
                    GUI.skin.label.wordWrap = false;
                    GUI.skin = this.cardListPopupLeftButtonSkin;

                
            }


        }

        private void drawCalendar()
        {
            bool clickableItems = true;

            if (true)
            {


                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.filtermenurect, string.Empty);
                if (this.detailtitle != "")
                {
                    GUI.skin = this.cardListPopupBigLabelSkin;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                    string message = this.detailtitle + "\r\n\r\n" + this.detaildescription + "\r\n\r\n" + "start: " + this.detailstart.ToString("dddd, dd. MMM yyyy HH:mm") + "\r\n" + "end: " + this.detailend.ToString("dddd, dd. MMM yyyy HH:mm");
                    GUI.skin.label.wordWrap = true;
                    float msghigh = GUI.skin.label.CalcHeight(new GUIContent(message), recto.tbmessagescroll.width - 30f);
                    GUI.skin = this.cardListPopupSkin;
                    scrolll = GUI.BeginScrollView(recto.tbmessagescroll, scrolll, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
                    GUI.skin = this.cardListPopupBigLabelSkin;

                    GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), message);

                    //Console.WriteLine(message);

                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.EndScrollView();
                    GUI.skin.label.wordWrap = false;
                    GUI.skin = this.cardListPopupLeftButtonSkin;

                    if (GUI.Button(recto.sbclearrect, "Edit"))
                    {
                        this.newEventStart = detailstart.ToString("dd.MM.yyyy HH:mm");
                        this.newEventEnd = detailend.ToString("dd.MM.yyyy HH:mm");
                        this.gmenu = 2;
                        
                    }
                }
            }



            int num = 0;
            // draw auctimes################################################
            float anzcards = anzcards = (float)gdata.calendar.Count();
            GUI.skin = this.cardListPopupSkin;
            this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * anzcards));

            GUI.skin = this.cardListPopupBigLabelSkin;


            float testy = this.scrollPos.y;
            foreach (calendarentry current in gdata.calendar)
            {

                GUI.skin = this.cardListPopupGradientSkin;
                //draw boxes
                Rect position7 = recto.position7(num);
                if (position7.yMax < testy || position7.y > testy + recto.position3.height)
                {
                    num++;
                    GUI.color = Color.white;
                }
                else
                {
                    if (clickableItems)
                    {
                        if (GUI.Button(position7, string.Empty))
                        {
                            this.detailtitle = current.title;
                            this.detaildescription = current.description;
                            this.detailstart = DateTime.Now;
                            this.detailend = DateTime.Now;
                            this.detailstart = current.start;
                            this.detailend = current.end;
                        }
                    }
                    else
                    {
                        GUI.Box(position7, string.Empty);
                    }
                    string name = current.title;
                    GUI.skin = this.cardListPopupBigLabelSkin;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
                    // draw text
                    Rect position8 = recto.position8(num);
                    GUI.Label(position8, (vector.x >= position8.width) ? (name.Substring(0, Mathf.Min(name.Length, recto.maxCharsName)) + "...") : name);
                    GUI.skin = this.cardListPopupSkin;
                    Rect position9 = recto.position9(num);
                    Rect restyperect = recto.restyperect(num);

                    //draw seller name

                    string sellername = current.start.Day + " " + current.start.ToString("MMMM");
                    if ((current.start - DateTime.Now) <= new TimeSpan(7, 0, 0, 0, 0))
                    {
                        sellername = current.start.DayOfWeek.ToString();
                    }
                    if (DateTime.Now.Day == current.start.Day && DateTime.Now.Month == current.start.Month && DateTime.Now.Year == current.start.Year)
                    {
                        sellername = "today"; 
                    }
                    if (DateTime.Now.Day == current.start.Day-1 && DateTime.Now.Month == current.start.Month && DateTime.Now.Year == current.start.Year)
                    {
                        sellername = "tomorrow";
                    }
                    
                    GUI.skin = this.cardListPopupBigLabelSkin;

                    vector = GUI.skin.label.CalcSize(new GUIContent(sellername));
                    //(this.fieldHeight-this.cardListPopupBigLabelSkin.label.fontSize)/2f
                    //Rect position11 = new Rect(restyperect.xMax + 2f, (float)num * this.fieldHeight, this.labelsWidth, this.fieldHeight);
                    Rect position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.fieldHeight);
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(position11, (vector.x >= position11.width) ? (sellername.Substring(0, Mathf.Min(sellername.Length, recto.maxCharsName)) + "...") : sellername);
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                    GUI.skin = this.cardListPopupGradientSkin;
                            if (GUI.Button(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth*2, recto.fieldHeight), ""))
                            {
                                this.detailtitle = current.title;
                                this.detaildescription = current.description;
                                this.detailstart = current.start;
                                this.detailend = current.end;
                            }
                        GUI.skin = this.cardListPopupBigLabelSkin;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.Label(new Rect(position7.xMax + 2, (float)num * recto.fieldHeight, recto.costIconWidth*2, recto.fieldHeight), "Details");
                    
                    
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.color = Color.white;
                    num++;
                }
            }

            GUI.EndScrollView();


            
        }

        private void newEvent(GUIStyle cls)
        {

            if (this.wantToPostEvent)
            {


                GUI.color = Color.white;

                // draw filter menue
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(recto.filtermenurect, string.Empty);
                if (this.detailtitle != "")
                {
                    GUI.skin = this.cardListPopupBigLabelSkin;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                    string message = "You want post this calendar entry :\r\n" + this.detailtitle + "\r\n\r\n" + this.detaildescription + "\r\n\r\n" + "start: " + this.detailstart.ToString("dddd, dd. MMM yyyy HH:mm") + "\r\n" + "end: " + this.detailend.ToString("dddd, dd. MMM yyyy HH:mm");
                    GUI.skin.label.wordWrap = true;
                    float msghigh = GUI.skin.label.CalcHeight(new GUIContent(message), recto.tbmessagescroll.width - 30f);
                    GUI.skin = this.cardListPopupSkin;
                    scrolll = GUI.BeginScrollView(recto.tbmessagescroll, scrolll, new Rect(0f, 0f, recto.tbmessagescroll.width - 20f, msghigh));
                    GUI.skin = this.cardListPopupBigLabelSkin;

                    GUI.Label(new Rect(5f, 5f, recto.tbmessagescroll.width - 30f, msghigh), message);

                    //Console.WriteLine(message);

                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.EndScrollView();
                    GUI.skin.label.wordWrap = false;
                    GUI.skin = this.cardListPopupLeftButtonSkin;

                    if (GUI.Button(recto.sbclearrect, "Yes"))
                    {
                        this.wantToPostEvent = false;
                        List<string> data = new List<string>();
                        data.Add(this.detailtitle);
                        data.Add(this.detaildescription);
                        this.detailstart = this.detailstart.Add(DateTime.UtcNow - DateTime.Now);
                        this.detailend = this.detailend.Add(DateTime.UtcNow - DateTime.Now);
                        data.Add(this.detailstart.ToString(" dd.MM.yyyy HH:mm:01"));
                        data.Add(this.detailend.ToString(" dd.MM.yyyy HH:mm:01"));
                        gdata.postDataToGoogleForm(data,gdata.calendarentrys,gdata.calendareditor);

                    }
                    if (GUI.Button(recto.sbclearrect2, "No"))
                    {
                        this.wantToPostEvent = false;
                    }
                }
            }

            GUI.skin = this.cardListPopupSkin;
            this.scrollPos = GUI.BeginScrollView(recto.position3, this.scrollPos, new Rect(0f, 0f, recto.innerRect.width - 20f, recto.fieldHeight * 6));
            GUI.skin = this.cardListPopupGradientSkin;
            Rect position7 = recto.position7(0);
            GUI.Box(position7, string.Empty);
            string name = "Title";
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            Vector2 vector = GUI.skin.label.CalcSize(new GUIContent(name));
            // draw text
            Rect position8 = recto.position8(0);
            GUI.Label(position8, name);
            Rect restyperect = recto.restyperect(0);
            Rect position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.fieldHeight);
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(position11, string.Empty);
            cls.alignment = TextAnchor.MiddleCenter;
            this.detailtitle=GUI.TextField(position11, this.detailtitle, cls);
            cls.alignment = TextAnchor.MiddleLeft;


            GUI.skin = this.cardListPopupGradientSkin;
            position7 = recto.position7(1);
            GUI.Box(position7, string.Empty);
            name = "Start";
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            vector = GUI.skin.label.CalcSize(new GUIContent(name));
            // draw text
            position8 = recto.position8(1);
            GUI.Label(position8, name);
            restyperect = recto.restyperect(1);
            position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.fieldHeight);
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(position11, string.Empty);

            if (GUI.Button(position11, this.newEventStart))
            {
                if (!this.datechooseractive)
                {
                    DCdateTime = this.detailstart;
                    selected = DCdateTime;
                    this.datechooseractive = true; this.datechoosertarget = 1;
                    string hours = this.detailstart.ToString("HH");
                    this.dateChooser10hour = Convert.ToInt32(hours.Substring(0,1)).ToString();
                    this.dateChooser1hour = Convert.ToInt32(hours.Substring(1, 1)).ToString();
                    string mins = this.detailstart.ToString("mm");
                    this.dateChooser10min = Convert.ToInt32(mins.Substring(0, 1)).ToString();
                    this.dateChooser1min = Convert.ToInt32(mins.Substring(1, 1)).ToString();

                }
            }



            GUI.skin = this.cardListPopupGradientSkin;
            position7 = recto.position7(2);
            GUI.Box(position7, string.Empty);
            name = "End";
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            vector = GUI.skin.label.CalcSize(new GUIContent(name));
            // draw text
            position8 = recto.position8(2);
            GUI.Label(position8, name);
            restyperect = recto.restyperect(2);
            position11 = new Rect(restyperect.xMax + 2f, position8.yMin, recto.labelsWidth, recto.fieldHeight);
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(position11, string.Empty);
            if (GUI.Button(position11, this.newEventEnd))
            {
                if (!this.datechooseractive)
                {
                    DCdateTime = this.detailend;
                    selected = DCdateTime;
                    this.datechooseractive = true; this.datechoosertarget = 2;
                    string hours = this.detailend.ToString("HH");
                    this.dateChooser10hour = Convert.ToInt32(hours.Substring(0, 1)).ToString();
                    this.dateChooser1hour = Convert.ToInt32(hours.Substring(1, 1)).ToString();
                    string mins = this.detailend.ToString("mm");
                    this.dateChooser10min = Convert.ToInt32(mins.Substring(0, 1)).ToString();
                    this.dateChooser1min = Convert.ToInt32(mins.Substring(1, 1)).ToString();
                }
            }

            GUI.skin = this.cardListPopupGradientSkin;
            position7 = recto.position7(3);
            GUI.Box(position7, string.Empty);
            name = "Description";
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            vector = GUI.skin.label.CalcSize(new GUIContent(name));
            // draw text
            position8 = recto.position8(3);
            GUI.Label(position8, name);
            restyperect = recto.restyperect(3);
            position11 = recto.position7(4);
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(position11, string.Empty);
            cls.alignment = TextAnchor.MiddleCenter;
            this.detaildescription = GUI.TextField(position11, this.detaildescription, cls);
            cls.alignment = TextAnchor.MiddleLeft;
            
            // ok button
            GUI.skin = this.cardListPopupGradientSkin;
            position7 = recto.position7(5);
            if (GUI.Button(position7, "OK")) 
            {
                this.wantToPostEvent = true;
            }

            GUI.EndScrollView();

        }


        private string dateChooser2000(GUIStyle cls)
        {
            GUI.skin = this.cardListPopupSkin;
            Rect pos1 = new Rect(recto.screenRect.xMax + 8, (float)Screen.height * 0.18f, Screen.width*0.98f - recto.screenRect.xMax - 8, this.calendarhight);
            GUI.Box(pos1, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity * 0.3f);
            Vector4 margins = new Vector4(12f, 12f, 12f, 12f + recto.BOTTOM_MARGIN_EXTRA);
            Rect pos2 = new Rect(pos1.x + margins.x, pos1.y + margins.y, pos1.width - (margins.x + margins.z), pos1.height - (2*margins.y));
            GUI.Box(pos2, string.Empty);
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, this.opacity);
            string str1 = this.DCdateTime.ToString("MMMM") + " "+ this.DCdateTime.Year;

            GUI.skin = this.cardListPopupBigLabelSkin;
            float stringhigh = GUI.skin.label.CalcSize(new GUIContent("Jg")).y;
            float stringleng = GUI.skin.label.CalcSize(new GUIContent(str1)).x;
            Rect posTitle = new Rect(pos2.xMax - ((pos2.width + stringleng) / 2), pos2.y + 10, stringleng + 8, stringhigh);
            float buttonLeng= GUI.skin.label.CalcSize(new GUIContent("Jg")).x;
            

            stringleng = GUI.skin.label.CalcSize(new GUIContent(" Mon ")).x;
            float placebetween = 4f;
            float meshleng = 7 * stringleng + 6 * placebetween;
            float xstart = pos2.xMax - ((pos2.width + meshleng) / 2);
            Rect prevMonth = new Rect(xstart, posTitle.y, buttonLeng, posTitle.height);
            Rect nextMonth = new Rect(xstart+meshleng-buttonLeng, posTitle.y, buttonLeng, posTitle.height);
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(posTitle, "");
            if (GUI.Button(prevMonth, "")) { this.DCdateTime=this.DCdateTime.AddDays(-this.DCdateTime.Day +1); this.DCdateTime = this.DCdateTime.AddMonths(-1); };
            if (GUI.Button(nextMonth, "")) { this.DCdateTime = this.DCdateTime.AddDays(-this.DCdateTime.Day +1); this.DCdateTime = this.DCdateTime.AddMonths(1); };
            
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(posTitle, new GUIContent(str1));
            GUI.Label(prevMonth, new GUIContent("<"));
            GUI.Label(nextMonth, new GUIContent(">"));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;



            
            float ystart = posTitle.yMax + 8;
            DateTime calcDT = new DateTime(this.DCdateTime.Year, this.DCdateTime.Month, 1, 1, 0, 0);
            float tempystart = ystart;

            for (int i = 0; i < 7; i++)
            {
                float tempxsstart = xstart + i * (stringleng + placebetween);
                Rect daybutton = new Rect(tempxsstart, tempystart, stringleng, stringhigh);
                GUI.skin = this.cardListPopupSkin;
                GUI.Box(daybutton, "");
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(daybutton, new GUIContent(((DayOfWeek)((i+8) % 7)).ToString().Substring(0, 3)));
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            }
            tempystart = tempystart + stringhigh + placebetween;
            int DaysinMon = DateTime.DaysInMonth(calcDT.Year, calcDT.Month);

            DateTime highlighter = selected;

            for (int i = 0; i < DaysinMon; i++)
            {
                int dayofweek = (int)(calcDT.DayOfWeek + 6) % 7; // monday==0;
                float tempxsstart = xstart + dayofweek * (stringleng + placebetween);
                
                Rect daybutton = new Rect(tempxsstart, tempystart, stringleng, stringhigh);
                GUI.skin = this.cardListPopupSkin;
                GUI.color = Color.white;
                Texture2D temp= GUI.skin.button.normal.background;

                if (highlighter.Year == calcDT.Year && highlighter.Month == calcDT.Month && highlighter.Day == calcDT.Day)
                {
                    GUI.skin.button.normal.background = GUI.skin.button.hover.background;
                }
                if (GUI.Button(daybutton, ""))
                {
                    this.selected = calcDT;
                    Console.WriteLine("selected Day yyyy mm dd: " + calcDT.Year + " " + calcDT.Month +" " + calcDT.Day );
                    /*if (datechoosertarget == 1)
                    {
                        this.detailstart = this.DCdateTime;
                        if (this.detailend < this.detailstart)
                        {
                            this.detailend = this.DCdateTime;
                        }
                    }
                    if (datechoosertarget == 2)
                    {
                        this.detailend = this.DCdateTime;

                        if (this.detailend < this.detailstart)
                        {
                            this.detailstart = this.DCdateTime;
                        }
                    }
                      */
                }
                GUI.skin.button.normal.background = temp;
                GUI.skin = this.cardListPopupBigLabelSkin;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(daybutton, new GUIContent(calcDT.Day.ToString()));
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                if (i < DaysinMon)
                {
                    if (dayofweek == 6) tempystart = tempystart + stringhigh + placebetween;
                    calcDT = calcDT.AddDays(1);
                }
            }

            float clockhight = ystart + 7 * (stringhigh + placebetween) + 4;
            GUI.skin = this.cardListPopupBigLabelSkin;
            float digitlen = GUI.skin.label.CalcSize(new GUIContent("88")).x;
            float digithi = GUI.skin.label.CalcSize(new GUIContent("88")).y;
            float dotlen = GUI.skin.label.CalcSize(new GUIContent("#:#")).x;
            float clocklen = 4 * digitlen + dotlen + 2 * 4;
            float clockstart = pos2.xMax - ((pos2.width + clocklen) / 2);

            Rect clorect1 = new Rect(clockstart, clockhight, digitlen, digithi);
            Rect clorect2 = new Rect(clorect1.xMax + 4, clockhight, digitlen, digithi);
            Rect ddotrect = new Rect(clorect2.xMax , clockhight, dotlen, digithi);
            Rect clorect3 = new Rect(ddotrect.xMax, clockhight, digitlen, digithi);
            Rect clorect4 = new Rect(clorect3.xMax + 4, clockhight, digitlen, digithi);
            GUI.skin = this.cardListPopupBigLabelSkin;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(ddotrect, new GUIContent(":"));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin = this.cardListPopupSkin;
            GUI.Box(clorect1, string.Empty);
            GUI.Box(clorect2, string.Empty);
            GUI.Box(clorect3, string.Empty);
            GUI.Box(clorect4, string.Empty);
            this.dateChooser10hour = Regex.Replace(GUI.TextField(clorect1, this.dateChooser10hour, cls), @"[^0-9]", "");
            this.dateChooser1hour = Regex.Replace(GUI.TextField(clorect2, this.dateChooser1hour, cls), @"[^0-9]", "");
            this.dateChooser10min = Regex.Replace(GUI.TextField(clorect3, this.dateChooser10min, cls), @"[^0-9]", "");
            this.dateChooser1min = Regex.Replace(GUI.TextField(clorect4, this.dateChooser1min, cls), @"[^0-9]", "");
            //correction
            int tempint = 0;
            if (dateChooser10hour != "")
            {
                tempint = Convert.ToInt32(dateChooser10hour);
                if (tempint < 0) { dateChooser10hour = ""; };
                if (tempint > 2) { dateChooser10hour = "2"; };
            }
            if (dateChooser1hour != "")
            {
                tempint = Convert.ToInt32(dateChooser1hour);
                if (tempint < 0) { dateChooser1hour = ""; };
                if (tempint > 9) { dateChooser1hour = "9"; };
            }
            if (dateChooser10min != "")
            {
                tempint = Convert.ToInt32(dateChooser10min);
                if (tempint < 0) { dateChooser10min = ""; };
                if (tempint > 5) { dateChooser10min = "5"; };
            }
            if (dateChooser10min != "")
            {
                tempint = Convert.ToInt32(dateChooser1min);
                if (tempint < 0) { dateChooser1min = ""; };
                if (tempint > 9) { dateChooser1min = "9"; };
            }

            if (GUI.Button(new Rect(clorect4.xMax + 8, clockhight, 2 * dotlen, clorect1.height), new GUIContent("OK")))
            {
                int hour10=0, hour1=0, min10=0, min1=0;
                if (dateChooser10hour != "") hour10 = Convert.ToInt32(dateChooser10hour);
                if (dateChooser1hour != "") hour1 = Convert.ToInt32(dateChooser1hour);
                if (dateChooser10min != "") min10 = Convert.ToInt32(dateChooser10min);
                if (dateChooser1min != "") min1 = Convert.ToInt32(dateChooser1min);

                this.DCdateTime = new DateTime(this.selected.Year, this.selected.Month, this.selected.Day, 10 * hour10 + hour1, 10 * min10 + min1, 1);
                if (datechoosertarget == 1)
                {
                    this.detailstart = this.DCdateTime;
                    this.newEventStart = detailstart.ToString("dd.MM.yyyy HH:mm");
                    this.newEventEnd = detailend.ToString("dd.MM.yyyy HH:mm");
                    if (this.detailend < this.detailstart)
                    {
                        this.detailend = this.DCdateTime;
                        this.newEventEnd = detailend.ToString("dd.MM.yyyy HH:mm");
                    }
                }
                if (datechoosertarget == 2)
                {
                    this.detailend = this.DCdateTime;
                    this.newEventEnd = detailend.ToString("dd.MM.yyyy HH:mm");
                    this.newEventStart = detailstart.ToString("dd.MM.yyyy HH:mm");

                    if (this.detailend < this.detailstart)
                    {
                        this.detailstart = this.DCdateTime;
                        this.newEventStart = detailstart.ToString("dd.MM.yyyy HH:mm");
                    }
                }
                this.datechooseractive = false;
            }

            this.calendarhight = clockhight + 2*digithi + 4 + margins.y - pos1.y;

            
            return "";
        }

    }
}
