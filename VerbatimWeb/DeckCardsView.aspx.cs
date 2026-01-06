using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI;
using System.Web.UI.WebControls;
using VerbatimService;

namespace VerbatimWeb
{
    public partial class DeckCardsView : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void ButtonFilter_Click(object sender, EventArgs e)
        {
            string QueryURL = "DeckCardsView.aspx?DeckId=" + HiddenDeckId.Value + "&filter=" + FilterInputBox.Text;

            Response.Redirect(QueryURL, false);
        }
        public IQueryable<Card> LoadDeckCards([QueryString("Filter")]string Filter)
        {
            string DeckId = Request.QueryString["DeckId"];

            if (DeckId == null)
                return null;
            
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
        protected void DeckCardsGridView_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count > 1)
            {
                // hides the Identity columns
                //e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
            }
        }
        public void LoadImage(Card Card)
        {
            string QueryURL = Utilities.ServerDNS + "/RenderCard/" + Card.VerbatimCardId;
            MemoryStream ms = Utilities.MakeGETRequestStream(QueryURL);
            string base64Data = Convert.ToBase64String(ms.ToArray());
            ImageHolder.Src = "data:image/gif;base64," + base64Data;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "RunCode", "hs.htmlExpand(document.getElementById('test'), { contentId: 'highslide-html' });", true);
        }
    }
}