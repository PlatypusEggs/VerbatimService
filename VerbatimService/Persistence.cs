using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace VerbatimService
{
    public class Persistence
    {
        public SQLiteConnection Connection { get; set; }
        private SQLiteCommand SQLiteCommand;

        public void EditCard(Card Card)
        {
            if (Card.Description == null)
                Card.Description = "";
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"UPDATE VerbatimCard
                           SET Title = :Title,
                               Description = :Description,
                               Category = :Category,
                               PointValue = :PointValue,
                               PictureURL = :PictureURL
                         WHERE VerbatimCardId = :VerbatimCardId";

            SQLiteCommand.Parameters.Add("Title", DbType.String).Value = Card.Title;
            SQLiteCommand.Parameters.Add("Description", DbType.String).Value = Card.Description;
            SQLiteCommand.Parameters.Add("Category", DbType.String).Value = Card.Category;
            SQLiteCommand.Parameters.Add("PointValue", DbType.String).Value = Card.PointValue;
            SQLiteCommand.Parameters.Add("VerbatimCardId", DbType.String).Value = Card.VerbatimCardId;
            SQLiteCommand.Parameters.Add("PictureURL", DbType.String).Value = Card.PictureURL;
            
            SQLiteCommand.ExecuteNonQuery();
        }

        public void EditDeck(Deck Deck)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"
                           UPDATE VerbatimDeck
                           SET Name = :Name,
                               Description = :Description,
                               Author = :Author,
                               IdentifiyngToken = :IdentifiyngToken,
                               UseStandardDistribution = :UseStandardDistribution,
                               Language = :Language
                         WHERE VerbatimDeckId = :VerbatimDeckId";
            SQLiteCommand.Parameters.Add("Name", DbType.String).Value = Deck.Name;
            SQLiteCommand.Parameters.Add("Description", DbType.String).Value = Deck.Description;
            SQLiteCommand.Parameters.Add("Author", DbType.String).Value = Deck.Author;
            SQLiteCommand.Parameters.Add("IdentifiyngToken", DbType.String).Value = Deck.IdentifiyngToken;
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = Deck.VerbatimDeckId;
            SQLiteCommand.Parameters.Add("UseStandardDistribution", DbType.String).Value = Deck.UseStandardDistribution;
            SQLiteCommand.Parameters.Add("Language", DbType.String).Value = Deck.Language;
            SQLiteCommand.ExecuteNonQuery();
        }

        public Deck GetDeck(string DeckId)
        {
            try
            {
                SQLiteCommand = new SQLiteCommand(Connection);
            

            Deck Deck = new Deck();
            SQLiteCommand.CommandText = @"SELECT VerbatimDeck.VerbatimDeckId, Name, VerbatimDeck.Description, Author, IdentifiyngToken, UseStandardDistribution, COUNT(*), sum(case when pointvalue = 1 then 1 else 0 end), sum(case when pointvalue = 2 then 1 else 0 end), sum(case when pointvalue = 3 then 1 else 0 end), sum(case when pointvalue = 4 then 1 else 0 end), sum(case when pointvalue = 5 then 1 else 0 end), Language
                        FROM VerbatimDeck  
                        LEFT JOIN VerbatimCard ON VerbatimCard.VerbatimDeckId = VerbatimDeck.VerbatimDeckId 					                        
                        WHERE VerbatimDeck.VerbatimDeckId = :VerbatimDeckId";
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = DeckId;
            using (SQLiteDataReader SQLiteDataReader = SQLiteCommand.ExecuteReader())
            {
                while (SQLiteDataReader.Read())
                {
                    Deck.VerbatimDeckId = SQLiteDataReader.GetInt32(0);
                    Deck.Name = SQLiteDataReader.GetString(1);
                    Deck.Description = SQLiteDataReader.GetString(2);
                    Deck.Author = SQLiteDataReader.GetString(3);
                    Deck.IdentifiyngToken = SQLiteDataReader.GetString(4);
                    Deck.UseStandardDistribution = bool.Parse(SQLiteDataReader.GetString(5));
                    Deck.TotalCards = SQLiteDataReader.GetInt32(6);
                    Deck.OnePointTotalCards = SQLiteDataReader.GetInt32(7);
                    Deck.TwoPointTotalCards = SQLiteDataReader.GetInt32(8);
                    Deck.ThreePointTotalCards = SQLiteDataReader.GetInt32(9);
                    Deck.FourPointTotalCards = SQLiteDataReader.GetInt32(10);
                    Deck.FivePointTotalCards = SQLiteDataReader.GetInt32(11);
                    Deck.Language = SQLiteDataReader.GetString(12);
                }
            }
            return Deck;
            }
            catch (Exception e)
            {
                Deck d = new Deck();
                d.Description = "in" + e.Message;
                return new Deck();
            }
        }

        public Card GetCard(string CardId)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            Card Card = new Card();
            SQLiteCommand.CommandText = @"SELECT Title, Description, Category, PointValue, PictureURL
                        FROM VerbatimCard
                        WHERE VerbatimCardId = :VerbatimCardId";
            SQLiteCommand.Parameters.Add("VerbatimCardId", DbType.String).Value = CardId;
            using (SQLiteDataReader SQLiteDataReader = SQLiteCommand.ExecuteReader())
            {
                while (SQLiteDataReader.Read())
                {
                    Card.Title = SQLiteDataReader.GetString(0);
                    Card.Description = SQLiteDataReader.GetString(1);
                    Card.Category = SQLiteDataReader.GetString(2);
                    Card.PointValue = SQLiteDataReader.GetInt32(3);
                    if (!SQLiteDataReader.IsDBNull(4))
                        Card.PictureURL = SQLiteDataReader.GetString(4);
                }
            }
            return Card;
        }

        public List<Deck> GetAllDecks()
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            List<Deck> Decks = new List<Deck>();
            SQLiteCommand.CommandText = @"SELECT VerbatimDeck.Name, VerbatimDeck.Description, VerbatimDeck.Author, VerbatimDeck.IdentifiyngToken, VerbatimDeck.UseStandardDistribution, VerbatimDeck.VerbatimDeckId, VerbatimDeck.Language, COUNT(*)
                        FROM VerbatimDeck
                        LEFT JOIN VerbatimCard ON VerbatimCard.VerbatimDeckId = VerbatimDeck.VerbatimDeckId
                        GROUP BY VerbatimDeck.Name, VerbatimDeck.Description, VerbatimDeck.Author, VerbatimDeck.IdentifiyngToken,VerbatimDeck.UseStandardDistribution, VerbatimDeck.VerbatimDeckId, VerbatimDeck.Language";
            using (SQLiteDataReader SQLiteDataReader = SQLiteCommand.ExecuteReader())
            {
                while (SQLiteDataReader.Read())
                {
                    Deck Deck = new Deck();
                    Deck.Name = SQLiteDataReader.GetString(0);
                    Deck.Description = SQLiteDataReader.GetString(1);
                    Deck.Author = SQLiteDataReader.GetString(2);
                    Deck.IdentifiyngToken = SQLiteDataReader.GetString(3);
                    Deck.UseStandardDistribution = bool.Parse(SQLiteDataReader.GetString(4));
                    Deck.VerbatimDeckId = SQLiteDataReader.GetInt32(5);
                    Deck.Language = SQLiteDataReader.GetString(6);
                    Deck.TotalCards = SQLiteDataReader.GetInt32(7);
                    Decks.Add(Deck);
                }
            }
            return Decks;
        }

        public List<Card> GetDeckCards(string DeckId, string Filter)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            List<Card> DeckCards = new List<Card>();
            SQLiteCommand.CommandText = @"SELECT VerbatimCardId, Title, Description, Category, PointValue, PictureURL
                        FROM VerbatimCard
                        WHERE VerbatimDeckId = :VerbatimDeckId
                        AND (
                            Title LIKE @Filter
                            OR Description LIKE @Filter
                            )
                        ORDER BY Title";
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = DeckId;
            SQLiteCommand.Parameters.Add("@Filter", DbType.String).Value = "%" + Filter + "%";

            using (SQLiteDataReader SQLiteDataReader = SQLiteCommand.ExecuteReader())
            {
                while (SQLiteDataReader.Read())
                {
                    Card Card = new Card();
                    Card.VerbatimCardId = SQLiteDataReader.GetInt32(0);
                    Card.Title = SQLiteDataReader.GetString(1);
                    Card.Description = SQLiteDataReader.GetString(2);
                    Card.Category = SQLiteDataReader.GetString(3);
                    Card.PointValue = SQLiteDataReader.GetInt32(4);
                    if(!SQLiteDataReader.IsDBNull(5))
                        Card.PictureURL = SQLiteDataReader.GetString(5);

                    DeckCards.Add(Card);
                }
            }
            return DeckCards;
        }

        public List<Deck> SearchForDeck(string Query)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            List<Deck> FoundDecks = new List<Deck>();
            SQLiteCommand.CommandText = @"SELECT VerbatimDeckId, Name, Description, Author, IdentifiyngToken, UseStandardDistribution, Language
                        FROM VerbatimDeck
                        WHERE Name like @Query
                        ORDER BY Name";
            SQLiteCommand.Parameters.Add("@Query", DbType.String).Value = "%" + Query + "%";
            using (SQLiteDataReader SQLiteDataReader = SQLiteCommand.ExecuteReader())
            {
                while (SQLiteDataReader.Read())
                {
                    Deck Deck = new Deck();
                    Deck.VerbatimDeckId = SQLiteDataReader.GetInt32(0);
                    Deck.Name = SQLiteDataReader.GetString(1);
                    Deck.Description = SQLiteDataReader.GetString(2);
                    Deck.Author = SQLiteDataReader.GetString(3);
                    Deck.IdentifiyngToken = SQLiteDataReader.GetString(4);
                    Deck.UseStandardDistribution = bool.Parse(SQLiteDataReader.GetString(5));
                    Deck.Language = SQLiteDataReader.GetString(6);
                    FoundDecks.Add(Deck);

                }
            }
            return FoundDecks;
        }
        public void InsertCard(Card Card)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"INSERT INTO VerbatimCard (VerbatimDeckId, Title, Description, Category, PointValue, PictureURL)
                        VALUES (:VerbatimDeckId,:Title,:Description,:Category,:PointValue,:PictureURL)";

            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = Card.VerbatimDeckId;
            SQLiteCommand.Parameters.Add("Title", DbType.String).Value = Card.Title;
            SQLiteCommand.Parameters.Add("Description", DbType.String).Value = Card.Description;
            SQLiteCommand.Parameters.Add("Category", DbType.String).Value = Card.Category;
            SQLiteCommand.Parameters.Add("PointValue", DbType.String).Value = Card.PointValue;
            SQLiteCommand.Parameters.Add("PictureURL", DbType.String).Value = Card.PictureURL;

            SQLiteCommand.ExecuteNonQuery();
        }
        public int InsertDeck(Deck Deck)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"INSERT INTO VerbatimDeck (Name, Description, Author, IdentifiyngToken, UseStandardDistribution, Language)
                        VALUES (:Name,:Description,:Author,:IdentifiyngToken,:UseStandardDistribution,:Language)";

            SQLiteCommand.Parameters.Add("Name", DbType.String).Value = Deck.Name;
            SQLiteCommand.Parameters.Add("Description", DbType.String).Value = Deck.Description;
            SQLiteCommand.Parameters.Add("Author", DbType.String).Value = Deck.Author;
            SQLiteCommand.Parameters.Add("IdentifiyngToken", DbType.String).Value = Deck.IdentifiyngToken;
            SQLiteCommand.Parameters.Add("UseStandardDistribution", DbType.String).Value = Deck.UseStandardDistribution;
            SQLiteCommand.Parameters.Add("Language", DbType.String).Value = Deck.Language;

            SQLiteCommand.ExecuteNonQuery();

            SQLiteCommand.CommandText = "select last_insert_rowid()";
            int DeckId = Int32.Parse(SQLiteCommand.ExecuteScalar().ToString());

            SQLiteCommand.CommandText = "INSERT INTO DeckAccess (SteamID, VerbatimDeckId) VALUES (:SteamID, :VerbatimDeckId)";
            SQLiteCommand.Parameters.Add("SteamID", DbType.String).Value = Deck.SteamId;
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = DeckId;

            SQLiteCommand.ExecuteNonQuery();

            return DeckId;
        }
        public void DeleteCard(Card Card)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"DELETE FROM VerbatimCardPlayHistory
                                           WHERE  VerbatimCardId = :VerbatimCardId";
            SQLiteCommand.Parameters.Add("VerbatimCardId", DbType.String).Value = Card.VerbatimCardId;
            SQLiteCommand.ExecuteNonQuery();

            SQLiteCommand.CommandText = @"DELETE FROM VerbatimCard
                                           WHERE  VerbatimCardId = :VerbatimCardId";
            SQLiteCommand.Parameters.Add("VerbatimCardId", DbType.String).Value = Card.VerbatimCardId;
            SQLiteCommand.ExecuteNonQuery();
        }

        public void DeleteDeck(Deck Deck)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"DELETE FROM VerbatimCard
                                           WHERE  VerbatimDeckId = :VerbatimDeckId";
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = Deck.VerbatimDeckId;
            SQLiteCommand.ExecuteNonQuery();

            SQLiteCommand.CommandText = @"DELETE FROM DeckAccess
                                           WHERE  VerbatimDeckId = :VerbatimDeckId";
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = Deck.VerbatimDeckId;
            SQLiteCommand.ExecuteNonQuery();

            SQLiteCommand.CommandText = @"DELETE FROM VerbatimDeck
                                           WHERE  VerbatimDeckId = :VerbatimDeckId";
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = Deck.VerbatimDeckId;
            SQLiteCommand.ExecuteNonQuery();
        }

        public int GetDeckIdByToken(string IdentifiyngToken)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"SELECT VerbatimDeckId FROM VerbatimDeck
                                           WHERE IdentifiyngToken  = :IdentifiyngToken COLLATE NOCASE";
            SQLiteCommand.Parameters.Add("IdentifiyngToken", DbType.String).Value = IdentifiyngToken;
            
            return Int32.Parse(SQLiteCommand.ExecuteScalar().ToString());
        }

        public List<string> GetDeckCategories(string DeckId)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"SELECT DISTINCT Category FROM VerbatimCard
                                           WHERE VerbatimDeckId  = :VerbatimDeckId";
            SQLiteCommand.Parameters.Add("VerbatimDeckId", DbType.String).Value = DeckId;
            List<string> Categories = new List<string>();
            using (SQLiteDataReader SQLiteDataReader = SQLiteCommand.ExecuteReader())
            {
                while (SQLiteDataReader.Read())
                {
                    Categories.Add(SQLiteDataReader.GetString(0));
                }
            }
            return Categories;
        }

        public void DeleteOneCardPlayHistory(int CardID, string SteamID)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"DELETE FROM VerbatimCardPlayHistory
                         WHERE VerbatimCardId = :VerbatimCardId
                         AND SteamID = :SteamID";

            SQLiteCommand.Parameters.Add("VerbatimCardId", DbType.Int32).Value = CardID;
            SQLiteCommand.Parameters.Add("SteamID", DbType.String).Value = SteamID;
        
            SQLiteCommand.ExecuteNonQuery();
        }
        public void DeleteCardPlayHistories(List<string> SteamIDs, List<int> CardIDs)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            foreach(string SteamID in SteamIDs)
                foreach(int CardID in CardIDs)
                {
                    SQLiteCommand.CommandText = @"DELETE FROM VerbatimCardPlayHistory
                         WHERE VerbatimCardId = :VerbatimCardId
                         AND SteamID = :SteamID";

                    SQLiteCommand.Parameters.Add("VerbatimCardId", DbType.Int32).Value = CardID;
                    SQLiteCommand.Parameters.Add("SteamID", DbType.String).Value = SteamID;

                    SQLiteCommand.ExecuteNonQuery();
                }
            
        }

        public string CreateSession(string SteamId)
        {
            SQLiteCommand = new SQLiteCommand(Connection);
            int UnixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            int ExpiryUnixTimeStamp = UnixTimeStamp + 300; // 5 minutes!
            string Token = GenerateToken();

            SQLiteCommand.CommandText = @"SELECT SessionId
                                        FROM session
                                        WHERE SteamId = :SteamId";
            SQLiteCommand.Parameters.Add("SteamId", DbType.String).Value = SteamId;
            object SessionId = SQLiteCommand.ExecuteScalar();

            SQLiteCommand.Parameters.Clear();
            if (SessionId == null)
            {
                SQLiteCommand.CommandText = @"INSERT INTO Session (SteamId, AccessToken, CreatedDate, ExpiryDate)
                                            VALUES (:SteamId,:AccessToken,:CreatedDate,:ExpiryDate)";
                SQLiteCommand.Parameters.Add("SteamId", DbType.String).Value = SteamId;
                SQLiteCommand.Parameters.Add("AccessToken", DbType.String).Value = Token;
                SQLiteCommand.Parameters.Add("CreatedDate", DbType.Int32).Value = UnixTimeStamp;
                SQLiteCommand.Parameters.Add("ExpiryDate", DbType.Int32).Value = ExpiryUnixTimeStamp;
                SQLiteCommand.ExecuteNonQuery();
            }
            else
            {
                SQLiteCommand.CommandText = @"UPDATE Session 
                                            SET AccessToken = :AccessToken,
                                            CreatedDate = :CreatedDate,
                                            ExpiryDate = :ExpiryDate
                                            WHERE SteamId = :SteamId";
                SQLiteCommand.Parameters.Add("SteamId", DbType.String).Value = SteamId;
                SQLiteCommand.Parameters.Add("AccessToken", DbType.String).Value = Token;
                SQLiteCommand.Parameters.Add("CreatedDate", DbType.Int32).Value = UnixTimeStamp;
                SQLiteCommand.Parameters.Add("ExpiryDate", DbType.Int32).Value = ExpiryUnixTimeStamp;
                SQLiteCommand.ExecuteNonQuery();
            }
            return Token;
        }

        public void RefreshAccessToken(string SteamId)
        {
            SQLiteCommand = new SQLiteCommand(Connection);
            int UnixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            int ExpiryUnixTimeStamp = UnixTimeStamp + (3600 * 12); // 12 hours!

            SQLiteCommand.CommandText = @"SELECT SessionId
                                        FROM session
                                        WHERE SteamId = :SteamId";
            SQLiteCommand.Parameters.Add("SteamId", DbType.String).Value = SteamId;
            object SessionId = SQLiteCommand.ExecuteScalar();

            SQLiteCommand.Parameters.Clear();
            if (SessionId != null)
            {
                SQLiteCommand.CommandText = @"UPDATE Session 
                                            SET ExpiryDate = :ExpiryDate
                                            WHERE SteamId = :SteamId";
                SQLiteCommand.Parameters.Add("SteamId", DbType.String).Value = SteamId;
                SQLiteCommand.Parameters.Add("ExpiryDate", DbType.Int32).Value = ExpiryUnixTimeStamp;
                SQLiteCommand.ExecuteNonQuery();
            }
            return;
        }

        

        public bool VerfiySession(string AccessToken)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            int UnixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


            SQLiteCommand.CommandText = @"SELECT SessionId
                                        FROM Session
                                        WHERE AccessToken =:AccessToken
                                        AND ExpiryDate > :ExpiryDate";
            SQLiteCommand.Parameters.Add("AccessToken", DbType.String).Value = AccessToken;
            SQLiteCommand.Parameters.Add("ExpiryDate", DbType.Int32).Value = UnixTimeStamp;
            return SQLiteCommand.ExecuteScalar() != null;

        }

        public bool CheckDeckAccess(string SteamId, int DeckId)
        {
            SQLiteCommand = new SQLiteCommand(Connection);

            SQLiteCommand.CommandText = @"SELECT DeckAccessId
                                        FROM DeckAccess
                                        WHERE SteamId =:SteamId
                                        AND VerbatimDeckId = :DeckId";
            SQLiteCommand.Parameters.Add("SteamId", DbType.String).Value = SteamId;
            SQLiteCommand.Parameters.Add("DeckId", DbType.Int32).Value = DeckId;
            return SQLiteCommand.ExecuteScalar() != null;

        }

        private static string GenerateToken()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);

                string token = Convert.ToBase64String(tokenData);
                return token;

            }
        }
    }
}