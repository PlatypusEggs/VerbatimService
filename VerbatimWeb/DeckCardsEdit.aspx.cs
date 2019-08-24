﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI;
using System.Web.UI.WebControls;
using VerbatimService;

namespace VerbatimWeb
{
    public partial class DeckCardsEdit : System.Web.UI.Page
    { 
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Utilities.CheckForValidSteamSession(Request.Cookies["AccessToken"]))
            {
                HttpCookie myCookie = new HttpCookie("SteamUserData");
                myCookie.Expires = DateTime.Now.AddHours(-1);
                Response.Cookies.Add(myCookie); Response.Redirect("Default");
            }
            object DeckIdCookie = Request.Cookies["VerbatimDeckId"].Values["VerbatimDeckId"];
            if (DeckIdCookie == null || string.IsNullOrEmpty(DeckIdCookie.ToString()))
                Response.Redirect("Default");
            //DeckCardsGridView.DataBind();
            
            DropDownList DropDownCategory = (DropDownList)this.Master.FindControl("MainContent").FindControl("InsertCardFormView").Controls[0].Controls[1].Controls[0].FindControl("AddNewCategorySection").FindControl("DropDownListCategory");

            if (DropDownCategory.Items.Count < 2)
            {
                string QueryURL = Utilities.ServerDNS + "/GetDeckCategories/" + Request.Cookies["VerbatimDeckId"].Values["VerbatimDeckId"].ToString();
                List<string> Categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(Utilities.MakeGETRequest(QueryURL));

                foreach (string Category in Categories)
                    DropDownCategory.Items.Add(Category);
            }

        }
        protected void ButtonFilter_Click(object sender, EventArgs e)
        {

            string QueryURL = "DeckCardsEdit.aspx?filter=" + FilterInputBox.Text;

            Response.Redirect(QueryURL, false);
        }
        protected void DropDownListCategory_Changed(object sender, EventArgs e)
        {
            DropDownList DropDownList = (DropDownList)sender;
            Control control = this.Master.FindControl("MainContent").FindControl("InsertCardFormView").Controls[0].Controls[1].Controls[0].FindControl("AddNewCategorySection");
            if (DropDownList.SelectedIndex == 0)
            {
                control.Visible = true;
            }
            else
            {
                control.Visible = false;
            }

        }
        public IQueryable<Card> LoadDeckCards([QueryString("DeckPassword")] string DeckPassword, [QueryString("Filter")]string Filter)
        {
            object DeckIdCookie = Request.Cookies["VerbatimDeckId"].Values["VerbatimDeckId"];
            if (DeckIdCookie == null || string.IsNullOrEmpty(DeckIdCookie.ToString()))
                return null;

            string DeckId = DeckIdCookie.ToString();
            if (Filter == null)
                Filter = "";
            string QueryURL = Utilities.ServerDNS + "/GetDeckCards/" + DeckId + "?filter=" + Filter;

            List<Card> Cards = null;

            if (!string.IsNullOrEmpty(DeckId))
            {
                Cards = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Card>>(Utilities.MakeGETRequest(QueryURL));
                HiddenDeckId.Value = DeckId;

            }
            else
                return null;
            return Cards.AsQueryable();
        }

        public void EditCard(object sender, GridViewUpdateEventArgs e)
        {
            Card Card = new Card();
            Card.VerbatimDeckId = Int32.Parse(HiddenDeckId.Value.ToString());
            Card.VerbatimCardId = Int32.Parse(e.NewValues["VerbatimCardId"].ToString());
            Card.Title = e.NewValues["Title"].ToString();
            Card.Description = e.NewValues["Description"].ToString();
            Card.Category = e.NewValues["Category"].ToString();
            Card.PointValue = Int32.Parse(e.NewValues["PointValue"].ToString());

            string QueryURL = Utilities.ServerDNS + "/EditCard";

            using (var client = new System.Net.WebClient())
            {
                client.UploadData(QueryURL, "PUT", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Card)));
            }
        }
        public void DeleteCard(Card Card)
        {
            string QueryURL = Utilities.ServerDNS + "/DeleteCard";

            using (var client = new System.Net.WebClient())
            {
                client.UploadData(QueryURL, "PUT", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Card)));
            }
        }
        public void InsertCard(string Title, string Description, string Category, int PointValue, string CustomCategory)
        {
            if ((Category == "ADD NEW CATEGORY" && string.IsNullOrEmpty(CustomCategory)) || String.IsNullOrEmpty(Title) || String.IsNullOrEmpty(Description) || String.IsNullOrEmpty(Category))
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "Everything is required!" + "')", true);
                return;
            }

            Card Card = new Card();
            Card.VerbatimDeckId = Int32.Parse(HiddenDeckId.Value);
            Card.Title = Title;
            Card.Description = Description;
            Card.Category = Category;
            Card.PointValue = PointValue;

            if (Category == "ADD NEW CATEGORY")
                Card.Category = CustomCategory;

            string QueryURL = Utilities.ServerDNS + "/GetDeckCards/" + Int32.Parse(HiddenDeckId.Value);

            List<Card> Cards = null;

            Cards = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Card>>(Utilities.MakeGETRequest(QueryURL));

            foreach(Card CardFromDeck in Cards)
            {
                if(CardFromDeck.Title == Card.Title)
                {
                    ScriptManager.RegisterClientScriptBlock(this, GetType(),
                            "alertMessage", @"alert('" + "A card with the title: " + Card.Title + " is already in your deck!" + "')", true);
                    //Response.Redirect(HttpContext.Current.Request.Url.PathAndQuery, false);
                    return;
                }
            }

            QueryURL = Utilities.ServerDNS + "/InsertCard";

            using (var client = new System.Net.WebClient())
            {
                client.UploadData(QueryURL, "PUT", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Card)));
            }
            Response.Redirect("DeckCardsEdit.aspx?Filter=" + Title, false);

        }
        protected void DeckCardsGridView_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                // hides the Identity columns
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[3].Visible = false;
            }
        }
        protected void DeckCardsGridView_DataBound(object sender, GridViewRowEventArgs e)
        {
            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                ((TextBox)e.Row.Cells[5].Controls[0]).TextMode = TextBoxMode.MultiLine;
                ((TextBox)e.Row.Cells[5].Controls[0]).Rows = 10;
                ((TextBox)e.Row.Cells[5].Controls[0]).Columns = 100;

            }

        }

        public void TestDONOTDELETE(Card Card) { }

        //protected int GetRowIndexById(int VerbatimCardId)
        //{
        //    int Counter = 0;
        //    foreach(GridViewRow Row in DeckCardsGridView.Rows)
        //    {
        //        int RowCardId = Int32.Parse(((TextBox)Row.Cells[2].Controls[0]).Text);
        //        if (VerbatimCardId == RowCardId)
        //            return Counter;
        //        Counter++;
        //    }
        //    return -1;
        //}
    }
}