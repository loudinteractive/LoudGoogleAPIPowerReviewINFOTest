using System;
using System.Collections.Generic;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Services;
using System.IO;
using System.Linq;
using SQLite;

namespace Google_Custom_Search_API
{

    public class SERPResult
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string ReviewURL { get; set; }
        public string reviewTextuery { get; set; }
        public int Rank { get; set; }
        public string Snippet { get; set; }
        public string Title { get; set; }
        public string RankingUrl { get; set; }

    }


    internal class Program
    {

        private static string DbPath = "SERPS_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".db";

        public static DateTime startTime;

        public static DateTime stopTime;

        public static int SaveItem<T>(T item) where T : SERPResult
        {
            using (var db = new SQLiteConnection(DbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache))
            {
                if (item.id != 0)
                {
                    db.Update(item);
                    return item.id;
                }
                else
                {
                    return db.Insert(item);
                }
            }
        }

        public static int DeleteItem<T>(int id) where T : SERPResult, new()
        {
            using (var db = new SQLiteConnection(DbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache))
            {

                return db.Delete<T>(new T() { id = id });
            }

        }

        public static SQLite.SQLiteConnection database;

        private static void Main(string[] args)
        {


            using (var db = new SQLiteConnection(DbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache))
            {
                db.CreateTable<SERPResult>();
                database = db;
            }

            // This text file should be setup with as tab delimited with reviewText first, reviewHeadline second and lastly the review url
            var search = File.ReadAllLines("search.txt");

            var count = 0;
            foreach(var ss in search)
            {
                if (!string.IsNullOrEmpty(ss))
                {
                    var lines = ss.Split('\t');
                    Uri uri;
                    try
                    {
                        var reviewUrl = lines[3];
                        uri = new Uri(reviewUrl.ToString());

                        var reviewText = lines[1];

                        var reviewHeadline = lines[2];


                        var currentUrl = reviewUrl;
                        
                        if (reviewText.IndexOf(@"""")<=-1)
                        {
                            reviewText = @"""" + reviewText.ToString() + @"""";
                        }
                        
                        GetFirst10ResultsPushToDatabase(reviewText.ToString(),reviewUrl.ToString());

                        if (reviewHeadline.IndexOf(@"""") <= -1)
                        {
                            reviewHeadline = @"""" + reviewHeadline.ToString() + @"""";
                        }

                        var reviewQuery = reviewText + " " + reviewHeadline;

                        GetFirst10ResultsPushToDatabase(reviewQuery.ToString(), reviewUrl.ToString());


                        count++;
                        System.Threading.Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                
            }
            

            Console.WriteLine("Done!");


        }


        public static void GetFirst10ResultsPushToDatabase(string reviewTextuery, string reviewUrl)
        {
            // TODO Fill in your API KEY and Search Engine Id
            const string apiKey = "";
            const string searchEngineId = "";
            var customSearchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = apiKey });
            var listRereviewTextuest = customSearchService.Cse.List(reviewTextuery);
            listRereviewTextuest.Cx = searchEngineId;

            IList<Result> paging = new List<Result>();
            var count = 0;
            var rank = 1;
            listRereviewTextuest.Start = count * 10 + 1;
            string finalStr = string.Empty + Environment.NewLine;
            try
            {
                paging = listRereviewTextuest.Execute().Items;
                if (paging != null)
                {
                    finalStr = string.Empty;
                    foreach (var item in paging)
                    {
                       

                        var sr = new SERPResult()
                        {
                            Rank = rank,
                            RankingUrl = item.Link,
                            Snippet = item.Snippet,
                            ReviewURL = reviewUrl.ToString(),
                            Title = item.Title,
                            reviewTextuery = reviewTextuery.ToString()
                        };
                        SaveItem(sr);
                        Console.WriteLine(reviewTextuery.ToString() + " " + rank + " " + item.Link);
                        count++;
                        rank++;
                    
                    }
                    

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());

            }
           
        }

    }
}