using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VerbatimService
{
    class ImageProcessing
    {
        private static Font StringFontDesc;
        private static Font StringFontCat;
        private static Font StringFontTitle;

        PrivateFontCollection pfcoll = new PrivateFontCollection();

        private static StringFormat StringFormat = new StringFormat();
        private static StringFormat StringFormatDesc = new StringFormat();
        private static Image TemplateImage1;
        private static Image TemplateImage2;
        private static Image TemplateImage3;
        private static Image TemplateImage4;
        private static Image TemplateImage5;
        private static List<Bitmap> CardImages = new List<Bitmap>();
        //2021 private static List<float> Distribution = new List<float> { 0.25f, 0.4f, 0.25f, 0.05f, 0.05f };
        private static List<float> Distribution = new List<float> { 0.15f, 0.45f, 0.35f, 0.03f, 0.02f };
        public SQLiteConnection Connection;
        private int UnixTimeStamp;
        public static string CurrentDirectory;

        public static List<string> NotConflictRedWords = new List<string> {"its","it's","at","her","what","who","your","we're", "you're", "they're", "i'm", "by", "their", "than", "it", "as", "they", "you", "his", "&", "from", "with", "in", "of", "on", "you", "too", "to", "that", "the", "and", "but", "for", "nor", "or", "so", "yet", "a", "an", "be", "am", "are", "is", "was", "were", "being", "been", "can", "could", "dare", "do", "does", "did", "have", "has", "had", "having", "may", "might", "must", "need", "ought", "shall", "should", "will", "would" };
        public static List<char> SpecialCharacters = new List<char>() { ',', '.', '-', ' ', '\t', '\n', '\r' };
        public static string SpecialCharactersForWeirdRegexThing = @"(?<=[\s.\-])";
        private static readonly int CardWidth = 663;
        private static readonly int CardHeight = 1001;

        public List<Card> CardObjects = new List<Card>();
        public Persistence Persistence = new Persistence();
        private int DeckId = -1;

        public string CreateDeck(int DeckSize, List<string> SteamIds, string Token)
        {
            Initialize(); 

            //DeleteOldFiles();

            if (string.IsNullOrEmpty(Token))
                DeckId = 1;
            else
                DeckId = Persistence.GetDeckIdByToken(Token);

            if (DeckId < 1)
                return "";

            Deck Deck = Persistence.GetDeck(DeckId.ToString());
            List<int> CardCounts = new List<int>();
            if (Deck.UseStandardDistribution)
                CardCounts = FixDistribution(Distribution, DeckSize);

            GenerateCards(CardCounts, SteamIds, DeckId, DeckSize);
            foreach (Card Card in CardObjects)
            {
                Card.Description = ProcessMarkupString(Card.Description);
                CardImages.Add(GenerateImage(Card));
            }

            Bitmap SheetBitMap = CreateSheetFromCards();
            string FileName = DateTime.Now.ToString("MM-dd-yyyy-HH：mm：ss") + ".jpg";
            string CurrentDirectory = "";
            if (DeckId == 1)
                CurrentDirectory = ImageProcessing.CurrentDirectory + @"\Verbatim\Sheets\Original\";
            else
            {
                if (!Directory.Exists(ImageProcessing.CurrentDirectory + @"\Verbatim\Sheets\" + Token + "\\"))
                    Directory.CreateDirectory(ImageProcessing.CurrentDirectory + @"\Verbatim\Sheets\" + Token + "\\");
                    
                CurrentDirectory = ImageProcessing.CurrentDirectory + @"\Verbatim\Sheets\" + Token + "\\";
            }
            string FullPictureFileName = CurrentDirectory + "\\" + FileName;

            SheetBitMap.Save(FullPictureFileName, ImageFormat.Jpeg);
            return FileName;
        }

        private void GenerateCards(List<int> CardCounts, List<string> SteamIDs, int DeckId, int DeckSize)
        {
            int count = 1;

            string SteamIDList = "";
            foreach (string SteamID in SteamIDs)
            {
                SteamIDList += "'" + SteamID + "',";
            }
            if (SteamIDs.Count > 0)
                SteamIDList = SteamIDList.Substring(0, SteamIDList.Length - 1);
            else
                SteamIDList = "'NOPE'";

            //SELECT VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue 
            //FROM VerbatimCard
            //    LEFT JOIN VerbatimCardPlayHistory 
            //        ON VerbatimCard.VerbatimCardId = VerbatimCardPlayHistory.VerbatimCardId
            //        AND VerbatimCardPlayHistory.SteamID IN ('test')
            //WHERE VerbatimDeckId = 1
            //AND PointValue = 1
            //AND VerbatimCardPlayHistory.VerbatimCardId IS NULL
            //ORDER BY RANDOM() LIMIT 10
            string InsertSql = "";
            string InsertSQLHead = @"INSERT INTO VerbatimCardPlayHistory (VerbatimCardId, SteamID, DateTimeUsed) VALUES ";

            string TemplateSQLRandom = @"		SELECT VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue, PictureURL, 0 as rowOrder, RANDOM() as Random
								FROM VerbatimCard 
								LEFT JOIN VerbatimCardPlayHistory
								ON VerbatimCardPlayHistory.VerbatimCardId = VerbatimCard.VerbatimCardId
								AND VerbatimCardPlayHistory.SteamID IN({0})
								WHERE VerbatimDeckId = {2}
								AND VerbatimCardPlayHistory.VerbatimCardId IS NULL
								UNION
								SELECT VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue, PictureURL, COUNT(*) as rowOrder, RANDOM() as Random
								FROM VerbatimCard 
								INNER JOIN VerbatimCardPlayHistory
								ON VerbatimCardPlayHistory.VerbatimCardId = VerbatimCard.VerbatimCardId
								WHERE VerbatimDeckId = {2}
								AND VerbatimCardPlayHistory.SteamID IN({0})    
								GROUP BY VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue
								ORDER BY rowOrder ASC, RANDOM()
                                LIMIT {1}";


            string TemplateSQL = @"		SELECT VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue, PictureURL, 0 as rowOrder, RANDOM() as Random
								FROM VerbatimCard 
								LEFT JOIN VerbatimCardPlayHistory
								ON VerbatimCardPlayHistory.VerbatimCardId = VerbatimCard.VerbatimCardId
								AND VerbatimCardPlayHistory.SteamID IN({0})
								WHERE VerbatimDeckId = {3}
								AND PointValue = {1}   
								AND VerbatimCardPlayHistory.VerbatimCardId IS NULL
								UNION
								SELECT VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue, PictureURL, COUNT(*) as rowOrder, RANDOM() as Random
								FROM VerbatimCard 
								INNER JOIN VerbatimCardPlayHistory
								ON VerbatimCardPlayHistory.VerbatimCardId = VerbatimCard.VerbatimCardId
								WHERE VerbatimDeckId = {3}
								AND VerbatimCardPlayHistory.SteamID IN({0})
								AND PointValue = {1}      
								GROUP BY VerbatimCard.VerbatimCardId, Title, Description, Category, PointValue
								ORDER BY rowOrder ASC, RANDOM()
								LIMIT {2}";
            if (CardCounts.Count > 0)
            {
                foreach (int CardCount in CardCounts)
                {
                    string SelectSQL = String.Format(TemplateSQL, SteamIDList, count, CardCount, DeckId);
                    //string sql = "SELECT VerbatimCardId, Title, Description, Category, PointValue FROM VerbatimCard WHERE VerbatimDeckId = 1 AND PointValue = " + count + " ORDER BY RANDOM() LIMIT " + CardCount;
                    SQLiteCommand Command = new SQLiteCommand(SelectSQL, Connection);
                    using (SQLiteDataReader SQLiteDataReader = Command.ExecuteReader())
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
                            CardObjects.Add(Card);

                            string UpsertSQLSelect = @"SELECT VerbatimCardId
								                FROM VerbatimCardPlayHistory
								                WHERE VerbatimCardId = {0}
								                AND SteamID = '{1}'";
                            string InsertSQLVal = "({0},'{1}',{2}),";
                            string UpdateSQL = @"UPDATE VerbatimCardPlayHistory
								                SET DateTimeUsed = {2}
								                WHERE VerbatimCardId = {0}
								                AND SteamID = '{1}'";
                            // UPSERT INTO VerbatimCardPlayHistory
                            foreach (string SteamID in SteamIDs)
                            {
                                SQLiteCommand UpsertSelectCommand = new SQLiteCommand(String.Format(UpsertSQLSelect, Card.VerbatimCardId, SteamID), Connection);
                                if (UpsertSelectCommand.ExecuteScalar() == null)
                                {
                                    InsertSql += String.Format(InsertSQLVal, Card.VerbatimCardId, SteamID, UnixTimeStamp);
                                }
                                else
                                {
                                    SQLiteCommand UpdateCommand = new SQLiteCommand(String.Format(UpdateSQL, Card.VerbatimCardId, SteamID, UnixTimeStamp), Connection);
                                    UpdateCommand.ExecuteNonQuery();
                                }
                            }
                        }

                    }
                    count++;
                }
                InsertSql = InsertSQLHead + InsertSql.TrimEnd(',') + ";";
                SQLiteCommand InsertCommand = new SQLiteCommand(InsertSql, Connection);
                InsertCommand.ExecuteNonQuery();
            }
            else
            {
                string SelectSQL = String.Format(TemplateSQLRandom, SteamIDList, DeckSize, DeckId);
                //string sql = "SELECT VerbatimCardId, Title, Description, Category, PointValue FROM VerbatimCard WHERE VerbatimDeckId = 1 AND PointValue = " + count + " ORDER BY RANDOM() LIMIT " + CardCount;
                SQLiteCommand Command = new SQLiteCommand(SelectSQL, Connection);
                using (SQLiteDataReader SQLiteDataReader = Command.ExecuteReader())
                {
                    while (SQLiteDataReader.Read())
                    {
                        Card Card = new Card();
                        Card.VerbatimCardId = SQLiteDataReader.GetInt32(0);
                        Card.Title = SQLiteDataReader.GetString(1);
                        Card.Description = SQLiteDataReader.GetString(2);
                        Card.Category = SQLiteDataReader.GetString(3);
                        Card.PointValue = SQLiteDataReader.GetInt32(4);
                        if (!SQLiteDataReader.IsDBNull(5))
                            Card.PictureURL = SQLiteDataReader.GetString(5);
                        CardObjects.Add(Card);

                        string UpsertSQLSelect = @"SELECT VerbatimCardId
								                FROM VerbatimCardPlayHistory
								                WHERE VerbatimCardId = {0}
								                AND SteamID = '{1}'";
                        string InsertSQL = @"INSERT INTO VerbatimCardPlayHistory (VerbatimCardId, SteamID, DateTimeUsed)
								                 VALUES ({0},'{1}',{2})";
                        string UpdateSQL = @"UPDATE VerbatimCardPlayHistory
								                SET DateTimeUsed = {2}
								                WHERE VerbatimCardId = {0}
								                AND SteamID = '{1}'";
                        // UPSERT INTO VerbatimCardPlayHistory
                        foreach (string SteamID in SteamIDs)
                        {
                            SQLiteCommand UpsertSelectCommand = new SQLiteCommand(String.Format(UpsertSQLSelect, Card.VerbatimCardId, SteamID), Connection);
                            if (UpsertSelectCommand.ExecuteScalar() == null)
                            {

                                SQLiteCommand InsertCommand = new SQLiteCommand(String.Format(InsertSQL, Card.VerbatimCardId, SteamID, UnixTimeStamp), Connection);
                                InsertCommand.ExecuteNonQuery();
                            }
                            else
                            {
                                SQLiteCommand UpdateCommand = new SQLiteCommand(String.Format(UpdateSQL, Card.VerbatimCardId, SteamID, UnixTimeStamp), Connection);
                                UpdateCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }


        }

        private static Bitmap CreateSheetFromCards()
        {
            Bitmap SheetBitMap = new Bitmap(CardWidth * 10, CardHeight * 7);
            using (Graphics Graphics = Graphics.FromImage(SheetBitMap))
            {
                for (int i = 0; i < 7; i++)
                    for (int j = 0; j < 10; j++)
                        if ((i * 10) + j != 69)
                        {
                            if (CardImages.Count - 1 < (i * 10) + j)
                                break;
                            Graphics.DrawImage(CardImages[(i * 10) + j], j * CardWidth, i * CardHeight);
                        }

            }

            return SheetBitMap;
        }

        public void Initialize()
        {
            TemplateImage1 = Image.FromFile(CurrentDirectory + @"\Template1.jpg");
            TemplateImage2 = Image.FromFile(CurrentDirectory + @"\Template2.jpg");
            TemplateImage3 = Image.FromFile(CurrentDirectory + @"\Template3.jpg");
            TemplateImage4 = Image.FromFile(CurrentDirectory + @"\Template4.jpg");
            TemplateImage5 = Image.FromFile(CurrentDirectory + @"\Template5.jpg");

            pfcoll.AddFontFile(@"C:\Verbatim\App_Data\proxima-nova-soft.otf");
            pfcoll.AddFontFile(@"C:\Verbatim\App_Data\Roboto-Medium.ttf");
            FontFamily ProximaFamily = pfcoll.Families[0];
            FontFamily RobotoFamily = pfcoll.Families[0];

            StringFontTitle = new Font(ProximaFamily, 40, FontStyle.Bold);
            StringFontCat = new Font(ProximaFamily, 25, FontStyle.Bold);
            StringFontDesc = new Font(RobotoFamily, 20, FontStyle.Regular);

            StringFormat.LineAlignment = StringAlignment.Center;
            StringFormat.Alignment = StringAlignment.Center;

            StringFormatDesc.LineAlignment = StringAlignment.Near;
            StringFormatDesc.Alignment = StringAlignment.Center;
            StringFormatDesc.Trimming = StringTrimming.None;
            StringFormatDesc.FormatFlags = StringFormatFlags.MeasureTrailingSpaces
            | StringFormatFlags.FitBlackBox;



            Connection = new SQLiteConnection("Data Source=" + CurrentDirectory + @"\Verbatim.sqlite;Version=3;");
            Connection.Open();
            CardImages = new List<Bitmap>();
            CardObjects = new List<Card>();

            UnixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            Persistence.Connection = Connection;
        }

        public static Bitmap GenerateImage(Card Card)
        {
            Bitmap OutPutImage;

            if (Card.PointValue == 1)
                OutPutImage = new Bitmap(TemplateImage1);
            else if (Card.PointValue == 2)
                OutPutImage = new Bitmap(TemplateImage2);
            else if (Card.PointValue == 3)
                OutPutImage = new Bitmap(TemplateImage3);
            else if (Card.PointValue == 4)
                OutPutImage = new Bitmap(TemplateImage4);
            else if (Card.PointValue == 5)
                OutPutImage = new Bitmap(TemplateImage5);
            else
                return null;

            using (Graphics graphics = Graphics.FromImage(OutPutImage))
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                //                graphics.DrawString("testtesttesttesttesttesttesttesttestt esttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttes ttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttes ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest ttesttesttesttest end", StringFontDesc, Brushes.Black,


                // add desc
                int x, y, width, height;
                x = y = width = height = 0;
                if (string.IsNullOrEmpty(Card.PictureURL))
                {
                    // add title
                    graphics.DrawString(Card.Title, StringFontTitle, Brushes.Black,
                                         new Rectangle(15, 115, OutPutImage.Size.Width - 50, 250), StringFormat);

                    x = 35; y = 445; width = OutPutImage.Size.Width - 70; height = 500;
                    List<string> DetectedWords = DetectWordInString(Card.Description, Card.Title);
                    if (DetectedWords.Count > 0)
                        DrawWithRedWords(Card.Description, graphics, OutPutImage, DetectedWords, x, y, width, height);
                    else
                        graphics.DrawString(Card.Description, StringFontDesc, Brushes.Black,
                         new Rectangle(x,y,width,height), StringFormatDesc);

                    // add category
                    graphics.DrawString(Card.Category, StringFontCat, Brushes.DarkBlue,
                         new Rectangle(35, 345, OutPutImage.Size.Width - 70, 70), StringFormat);
                }
                else
                {
                    // add title
                    graphics.DrawString(Card.Title, StringFontTitle, Brushes.Black,
                                         new Rectangle(15, 20, OutPutImage.Size.Width - 50, 250), StringFormat);

                    x = 35; y = 600; width = OutPutImage.Size.Width - 70; height = 500;

                    try
                    {
                        WebClient client = new WebClient();
                        Stream stream = client.OpenRead(Card.PictureURL);
                        Bitmap bitmap = new Bitmap(stream);
                        graphics.DrawImage(bitmap, 169, 210, 325, 325);
                    }
                    catch
                    {

                    }
                    List<string> DetectedWords = DetectWordInString(Card.Description, Card.Title);
                    if (DetectedWords.Count > 0)
                        DrawWithRedWords(Card.Description, graphics, OutPutImage, DetectedWords, x,y,width,height);
                    else
                        graphics.DrawString(Card.Description, StringFontDesc, Brushes.Black,
                         new Rectangle(x,y,width,height), StringFormatDesc);

                    graphics.DrawString(Card.Category, StringFontCat, Brushes.DarkBlue,
                         new Rectangle(35, 525, OutPutImage.Size.Width - 70, 70), StringFormat);
                }

            }
            return OutPutImage;
        }

        private static List<int> FixDistribution(List<float> Distribution, int DeckSize)
        {
 
            // 5 8 5 1 1

            // FLOOR ALL 
            // SAVE FLOORS
            // SUM THIS AND GET DELTA WITH N
            // DISTRUBTE ACROSS FLOORS

            List<RoundedValue> RoundedValues = new List<RoundedValue>();
            foreach (float Percentage in Distribution)
            {
                // ROUND ALL 

                RoundedValue RoundedValue = new RoundedValue();
                float FuzzyNumber = Percentage * DeckSize;
                RoundedValue.RoundedInt = (int)Math.Round(FuzzyNumber);

                // SAVE ROUNDS POLARITY
                if (FuzzyNumber != Math.Floor(FuzzyNumber))
                {
                    RoundedValue.WasRounded = true;
                    if ((int)FuzzyNumber == RoundedValue.RoundedInt)
                        RoundedValue.Polarity = -1;
                    else
                        RoundedValue.Polarity = 1;
                }
                else
                {
                    RoundedValue.WasRounded = false;
                }
                RoundedValues.Add(RoundedValue);
            }

            // SUM THIS AND GET DELTA WITH N
            int Sum = 0;
            foreach (RoundedValue RoundedValue in RoundedValues)
            {
                Sum += RoundedValue.RoundedInt;
            }
            List<int> FinalValues = new List<int>();
            if(Sum == DeckSize)
            {
                foreach (RoundedValue RoundedValue in RoundedValues)
                {
                    FinalValues.Add(RoundedValue.RoundedInt);
                }
            }
            // ADD OR SUB ACROSS ROUNDS
            else
            {
                int Delta = Sum - DeckSize;
                foreach (RoundedValue RoundedValue in RoundedValues)
                {
                    if(!RoundedValue.WasRounded)
                        FinalValues.Add(RoundedValue.RoundedInt);
                    else
                    {
                        if (RoundedValue.Polarity == -1 && Delta < 0)
                        {
                            FinalValues.Add(RoundedValue.RoundedInt + 1);
                            Delta++;
                        }
                        else if (RoundedValue.Polarity == 1 && Delta > 0)
                        {
                            FinalValues.Add(RoundedValue.RoundedInt - 1);
                            Delta--;
                        }
                        else
                            FinalValues.Add(RoundedValue.RoundedInt);

                    }
                }
            }
            return FinalValues;
        }

        private static void DeleteOldFiles()
        {
            List<string> Files = Directory.GetFiles(CurrentDirectory + @"\Verbatim\Sheets\Original\").ToList();
            foreach (string SheetFile in Files)
            {
                if (Directory.GetCreationTime(SheetFile) < DateTime.Now.AddMinutes(-300))
                    File.Delete(SheetFile);
            }

        }

        private static void DrawWithRedWords(string words, Graphics g, Image OutPutImage, List<string> RedWords, int x, int y, int width, int height)
        {
            TextFormatFlags flags = TextFormatFlags.Left
            | TextFormatFlags.NoPadding
            | TextFormatFlags.PreserveGraphicsClipping
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.GlyphOverhangPadding
            | TextFormatFlags.WordBreak;

            List<string> wordsList = Regex.Split(words, SpecialCharactersForWeirdRegexThing).ToList();
            var index = 0;

            List<List<string>> lists = splitList<string>(wordsList, 32).ToList();
            foreach(List<string> list in lists)
            {
                CharacterRange[] ranges = list.Select(m => {
                    var range = new CharacterRange(index, m.Length);
                    index += m.Length;
                    return range;
                }).ToArray();

                StringFormatDesc.SetMeasurableCharacterRanges(ranges);
                Region[] regions = g.MeasureCharacterRanges(words, StringFontDesc, new Rectangle(x, y, width, height), StringFormatDesc);
                for (int i = 0; i < ranges.Length; i++)
                {
                    Rectangle WordBounds = Rectangle.Round(regions[i].GetBounds(g));
                    //File.AppendAllText(@"C:\Verbatim\test.txt", WordBounds.X + " " + WordBounds.Y + "\n");
                    string word = words.Substring(ranges[i].First, ranges[i].Length);
                    if (RedWords.Contains(word.ToLower().Trim(SpecialCharacters.ToArray())))
                        TextRenderer.DrawText(g, word, StringFontDesc, WordBounds, Color.Red, flags);
                    else
                        TextRenderer.DrawText(g, word, StringFontDesc, WordBounds, Color.Black, flags);
                }
            }            
        }

        public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        private static List<string> DetectWordInString(string Description, string Title)
        {
            List<string> Detected = new List<string>();
            Description = Description.Replace(",", "");
            Description = Description.Replace("\"", "");

            List<string> DescriptionWords = Description.Split(' ').ToList();
            List<string> TitleWords = Title.Split(' ').ToList();

            foreach (string DescriptionWord in DescriptionWords)
                foreach(string TitleWord in TitleWords)
                    if (DescriptionWord.ToLower() == TitleWord.ToLower() && !NotConflictRedWords.Contains(TitleWord.ToLower()))
                        Detected.Add(TitleWord.ToLower());
            return Detected;

        }
        private static string ProcessMarkupString(string StringToMarkUp)
        {
            return StringToMarkUp.Replace("\\n", "\n");

        }
    }
}
