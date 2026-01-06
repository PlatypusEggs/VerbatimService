using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace VerbatimService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IVerbatimService
    {

        //[OperationContract]
        //[WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, 
        //    BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "GetDeck/{DeckSize}")]
        //Deck GetDeck(string DeckSize);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "GetDeckWithSteamIds/{DeckSize}")]
        SpawnedDeck GetDeckWithSteamIds(string DeckSize, string SteamIDs);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "GetDeckWithSteamIds")]
        SpawnedDeck GetDeckWithSteamIdsAndToken(string DeckSize, string Token, List<string> SteamIDs);


        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "InsertDeck")]
        int InsertDeck(Deck Deck);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetDeck/{DeckId}")]
        Deck GetDeck(string DeckId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetDeckCategories/{DeckId}")]
        List<string> GetDeckCategories(string DeckId);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetAllDecks")]
        List<Deck> GetAllDecks();

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "EditDeck")]
        void EditDeck(Deck Deck);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "SearchForDeck?Query={Query}")]
        List<Deck> SearchForDeck(string Query);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetDeckCards/{DeckId}?filter={Filter}")]
        List<Card> GetDeckCards(string DeckId, string Filter);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "EditCard")]
        void EditCard(Card Card);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "InsertCard")]
        void InsertCard(Card Card);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "DeleteCard")]
        void DeleteCard(Card Card);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "DeleteDeck")]
        void DeleteDeck(Deck Deck);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "DeleteOneCardPlayHistory")]
        void DeleteOneCardPlayHistory(string SteamId, int CardId);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "DeleteCardPlayHistories")]
        void DeleteCardPlayHistories(List<string> SteamIds, List<int> CardIds);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "RenderCard/{CardId}")]
        Stream RenderCard(string CardId);
        
        #region Back Channel
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "CreateSession")]
        string CreateSession(string SteamId, string ServerPassword);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "VerfiySession?AccessToken={AccessToken}")]
        bool VerfiySession(string AccessToken);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "CheckDeckAccess?SteamId={SteamId}&DeckId={DeckId}")]
        bool CheckDeckAccess(string SteamId, int DeckId);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "RefreshAccessToken")]
        void RefreshAccessToken(string SteamId, string ServerPassword);

        #endregion

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "vip?s={s}")]
        string vip(string s);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetPlayDeck?Folder={Folder}&Name={Name}")]
        Stream GetPlayDeck(string Folder,string Name);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json,
    BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetSqlDB")]
        Stream GetSqlDB();
    }



}
