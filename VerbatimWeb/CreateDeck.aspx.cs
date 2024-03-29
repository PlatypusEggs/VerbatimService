﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VerbatimService;

namespace VerbatimWeb
{
    public partial class CreateDeck : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Utilities.CheckForValidSteamSession(Request.Cookies["AccessToken"]))
            {
                HttpCookie myCookie = new HttpCookie("SteamUserData");
                myCookie.Expires = DateTime.Now.AddHours(-1);
                Response.Cookies.Add(myCookie); Response.Redirect("Default");
            }
        }
        public void InsertDeck(Deck Deck)
        {
            if (!Utilities.CheckForValidSteamSession(Request.Cookies["AccessToken"]))
            {
                HttpCookie myCookie2 = new HttpCookie("SteamUserData");
                myCookie2.Expires = DateTime.Now.AddHours(-1);
                Response.Cookies.Add(myCookie2);
                Response.Redirect("Default");
            }
            HttpCookie myCookie = Request.Cookies["SteamUserData"];

            Deck.SteamId = JObject.Parse(myCookie.Values["SteamUserData"].ToString())["response"]["players"][0]["steamid"].ToString();

            if (string.IsNullOrEmpty(Deck.IdentifiyngToken))
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "TTS Token is required!" + "')", true);
                return;
            }
            if (string.IsNullOrEmpty(Deck.Author))
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Author is required!" + "')", true);
                return;
            }
            if (string.IsNullOrEmpty(Deck.Description))
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Description is required!" + "')", true);
                return;
            }
            if (string.IsNullOrEmpty(Deck.Name))
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Name is required!" + "')", true);
                return;
            }
            if (string.IsNullOrEmpty(Deck.Language))
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Language is required!" + "')", true);
                return;
            }
            string QueryURL = Utilities.ServerDNS + "/GetAllDecks";

            List<Deck> Decks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Deck>>(Utilities.MakeGETRequest(QueryURL));

            foreach(Deck DeckFromDB in Decks)
            {
                if(Deck.Name == DeckFromDB.Name)
                {
                    ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Name is already taken!" + "')", true);
                    return;
                }
                else if (Deck.IdentifiyngToken == DeckFromDB.IdentifiyngToken)
                {
                    ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Token is already taken!" + "')", true);
                    return;
                }
            }

            QueryURL = Utilities.ServerDNS + "/InsertDeck";
            string DeckId = "";
            using (var client = new System.Net.WebClient())
            {
                byte[] response = client.UploadData(QueryURL, "PUT", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Deck)));
                DeckId = client.Encoding.GetString(response);
            }

            Response.Redirect("DeckCardsEdit.aspx?DeckId=" + DeckId, false);


        }
    }
}