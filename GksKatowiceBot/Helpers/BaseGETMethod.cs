using HtmlAgilityPack;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseGETMethod
    {


        public static IList<Attachment> GetCardsAttachmentsAktualnosci(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "	cycle-slideshow";
                string xpath = String.Format("//section[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//figcaption").Select(p => p.ChildNodes).ToList();

                response.Close();
                readStream.Close();

                int index = hrefListNew.Count;

                //DataTable dt = GetWiadomosciHokej();
                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = hrefListNew.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[0][i].InnerHtml.Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[0][i].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: "http:" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[0][i].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: "http:" + imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        if (imgList[i].Contains("http"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                            new CardImage(url: imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsInne(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string haslo = "")
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/search/search?cfg=7641ac7820b0312bc8f1055e7218d305&resultsPerPage=20&text=" + '"' + haslo + '+' + "'+&pagedType=event";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "newsRes fl-left";
                string xpath = String.Format("//article[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";

                if (people != null)
                {
                    foreach (var person in people)
                    {
                        text += person.InnerHtml;
                    }

                    HtmlDocument doc2 = new HtmlDocument();

                    doc2.LoadHtml(text);
                    var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                      .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("wiadomosc")).Distinct()
                                      .ToList();

                    var imgList = doc2.DocumentNode.SelectNodes("//img")
                                      .Select(p => p.GetAttributeValue("src", "not found"))
                                      .ToList();



                    //var titleList = doc2.DocumentNode.SelectNodes("//a").Select(p => p.ChildNodes).ToList();

                    var titleList = doc2.DocumentNode.SelectNodes(".//*[contains(@class,'title')]");

                    response.Close();
                    readStream.Close();

                    int index = hrefListNew.Count >= 5 ? 5 : hrefListNew.Count;

                    //DataTable dt = GetWiadomosciHokej();
                    DataTable dt = new DataTable();

                    if (newUser == true)
                    {
                        index = hrefListNew.Count >= 5 ? 5 : hrefListNew.Count; ;
                        if (dt.Rows.Count == 0)
                        {
                            //    AddWiadomosc(hrefList);
                        }
                    }

                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            List<int> deleteList = new List<int>();
                            var listTemp = new List<System.Linq.IGrouping<string, string>>();
                            var imageListTemp = new List<string>();
                            var titleListTemp = new List<string>();

                            for (int i = 0; i < hrefList.Count; i++)
                            {
                                if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                    dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                                {
                                    listTemp.Add(hrefList[i]);
                                    imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                    titleListTemp.Add(titleList[i].InnerHtml.Replace("&quot;", ""));
                                }
                                listTemp2.Add(hrefList[i]);
                            }
                            hrefList = listTemp;
                            index = hrefList.Count;
                            imgList = imageListTemp;
                            //  titleList = titleListTemp;
                            //   AddWiadomosc(listTemp2);
                        }
                        else
                        {
                            index = hrefList.Count;
                            //   AddWiadomosc(hrefList);
                        }
                    }

                    for (int i = 0; i < index; i++)
                    {
                        string link = "";
                        if (hrefListNew[i].Contains("http"))
                        {
                            link = hrefListNew[i];
                        }
                        else
                        {
                            link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                            //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                        }

                        if (link.Contains("video"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i].InnerHtml.Replace("&quot;", ""), "", "",
                            new CardImage(url: "http:" + imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                        else
                            if (link.Contains("gallery"))
                        {
                            list.Add(GetHeroCard(
                            titleList[0].InnerHtml.Replace("&quot;", ""), "", "",
                            new CardImage(url: "http:" + imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                        else
                        {
                            if (imgList[i].Contains("http"))
                            {
                                list.Add(GetHeroCard(
                                titleList[i].InnerHtml.Replace("&quot;", ""), "", "",
                                new CardImage(url: imgList[i]),
                                new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                                new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                                );
                            }
                        }

                        //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                    }
                }
                if (listTemp2.Count > 0)
                {
                    hrefList = listTemp2;
                }
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsWideo(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/wideo/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "cycle-slideshow fl-left wideo-slider";
                string xpath = String.Format("//section[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//figcaption").Select(p => p.ChildNodes).ToList();

                response.Close();
                readStream.Close();

                int index = hrefListNew.Count;

                //DataTable dt = GetWiadomosciHokej();
                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = hrefListNew.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[0][i].InnerHtml.Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[0][i].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[0][i].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        if (imgList[i].Contains("http"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                            new CardImage(url: imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Zobacz wideo", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }
        public static IList<Attachment> GetCardsAttachmentsNastepneWideo(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/wideo/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "wideo-news-medium-2 fl-left";
                string xpath = String.Format("//article[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Distinct().Where(p => p.Contains("film"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 10;

                //DataTable dt = GetWiadomosciHokej();
                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = 10;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        if (imgList[i].Contains("http"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i].Replace("&quot;", ""), "", "",
                            new CardImage(url: imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Zobacz wideo", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsNastepneWideo2(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/wideo/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "wideo-news-medium-2 fl-left";
                string xpath = String.Format("//article[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Distinct().Where(p => p.Contains("film"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 20;

                //DataTable dt = GetWiadomosciHokej();
                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = 20;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 10; i < 20; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        if (imgList[i].Contains("http"))
                        {
                            list.Add(GetHeroCard(
                            titleList[i].Replace("&quot;", ""), "", "",
                            new CardImage(url: imgList[i]),
                            new CardAction(ActionTypes.OpenUrl, "Zobacz wideo", value: link),
                            new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                            );
                        }
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }



        public static void GetCardsAttachmentsPopularne()
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/ajax-aside/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "najpopularniejsze";
                string xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found").Replace("'","''"))
                                  .ToList();



                for (int i = 1; i < hrefListNew.Count; i++)
                {
                    urlAddress = hrefListNew[i];
                    // string urlAddress = "http://www.orlenliga.pl/";

                    request = (HttpWebRequest)WebRequest.Create(urlAddress);
                    response = (HttpWebResponse)request.GetResponse();

                    listTemp2 = new List<System.Linq.IGrouping<string, string>>();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        receiveStream = response.GetResponseStream();
                        readStream = null;

                        if (response.CharacterSet == null)
                        {
                            readStream = new StreamReader(receiveStream);
                        }
                        else
                        {
                            readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        }

                        data = readStream.ReadToEnd();

                        doc = new HtmlDocument();
                        doc.LoadHtml(data);

                        matchResultDivId = "news";
                        xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                        people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                        text = "";
                        foreach (var person in people)
                        {
                            text += person.InnerHtml;
                        }
                        doc2.LoadHtml(text);
                        List<string> value = new List<string>();
                        try
                        {

                            value = doc2.DocumentNode.SelectNodes("//img")
                                      .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("http")).ToList();

                        }
                        catch
                        {

                        }
                        if (value!=null&&value.Count() > 0)
                        {
                            imgList.Add(value[0]);
                        }
                        else
                        {
                            imgList.Add("");
                            //hrefListNew.RemoveAt(i);
                            //titleList.RemoveAt(i);
                            //i--;
                        }


                    }

                }

                BaseDB.AddWiadomoscPopularne(hrefListNew);
                BaseDB.AddWiadomoscPopularneImg(imgList);
                BaseDB.AddWiadomoscPopularneTytul(titleList);

            }

        }



        public static void GetCardsAttachmentsNajwazniejsze()
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/ajax-aside/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "najwazniejsze";
                string xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found").Replace("'","''"))
                                  .ToList();

                

                for (int i = 1; i < hrefListNew.Count; i++)
                {
                    urlAddress = hrefListNew[i];
                    // string urlAddress = "http://www.orlenliga.pl/";

                    request = (HttpWebRequest)WebRequest.Create(urlAddress);
                    response = (HttpWebResponse)request.GetResponse();

                    listTemp2 = new List<System.Linq.IGrouping<string, string>>();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        receiveStream = response.GetResponseStream();
                        readStream = null;

                        if (response.CharacterSet == null)
                        {
                            readStream = new StreamReader(receiveStream);
                        }
                        else
                        {
                            readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        }

                        data = readStream.ReadToEnd();

                        doc = new HtmlDocument();
                        doc.LoadHtml(data);

                        matchResultDivId = "news";
                        xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                        people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                        text = "";
                        foreach (var person in people)
                        {
                            text += person.InnerHtml;
                        }
                        doc2.LoadHtml(text);

                        List<string> value = new List<string>();
                        try
                        {

                            value = doc2.DocumentNode.SelectNodes("//img")
                                      .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("http")).ToList();

                        }
                        catch
                        {

                        }
                        if (value!=null&&value.Count() > 0)
                        {
                            imgList.Add(value[0]);
                        }
                        else
                        {
                            imgList.Add("");
                            //hrefListNew.RemoveAt(i);
                            //titleList.RemoveAt(i);
                            //i--;
                        }


                    }

                }

                BaseDB.AddWiadomoscNajwazniejsze(hrefListNew);
                BaseDB.AddWiadomoscNajwazniejszeImg(imgList);
                BaseDB.AddWiadomoscNajwazniejszeTytul(titleList);

            }
        }


        public static IList<Attachment> DajCardsAttachmentsNajwazniejsze(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/ajax-aside/";
            // string urlAddress = "http://www.orlenliga.pl/";

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();


            List<string> hrefListNew = new List<string>();
            List<string> imgList = new List<string>();
            List<string> titleList = new List<string>();
            DataTable dtArt = BaseDB.GetWiadomoscNajwazniejsze();
            DataTable dtImg = BaseDB.GetWiadomoscNajwazniejszeImg();
            DataTable dtTitle = BaseDB.GetWiadomoscNajwazniejszeTytul();
            for (int i = 0; i < 10; i++)
            {
                hrefListNew.Add(dtArt.Rows[0][i].ToString());
                imgList.Add(dtImg.Rows[0][i].ToString());
                titleList.Add(dtTitle.Rows[0][i].ToString());

            }

            for (int i = 0; i < 10; i++)
            {
                string link = "";
                if (hrefListNew[i].Contains("http"))
                {
                    link = hrefListNew[i];
                }
                else
                {
                    link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                    //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                }

                if (link.Contains("video"))
                {
                    list.Add(GetHeroCard(
                    titleList[i].Replace("&quot;", ""), "", "",
                    new CardImage(url: imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );
                }
                else
                    if (link.Contains("gallery"))
                {
                    list.Add(GetHeroCard(
                    titleList[i].Replace("&quot;", ""), "", "",
                    new CardImage(url: imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );
                }
                else
                {
                    if (imgList[i].Contains("http"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                }

            }


            return list;

        }
        public static IList<Attachment> DajCardsAttachmentsPopularne(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/ajax-aside/";
            // string urlAddress = "http://www.orlenliga.pl/";

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();


            List<string> hrefListNew = new List<string>();
            List<string> imgList = new List<string>();
            List<string> titleList = new List<string>();
            DataTable dtArt = BaseDB.GetWiadomoscPopularne();
            DataTable dtImg = BaseDB.GetWiadomoscPopularneImg();
            DataTable dtTitle = BaseDB.GetWiadomoscPopularneTytul();
            for (int i = 0; i < 10; i++)
            {
                hrefListNew.Add(dtArt.Rows[0][i].ToString());
                imgList.Add(dtImg.Rows[0][i].ToString());
                titleList.Add(dtTitle.Rows[0][i].ToString());

            }

            for (int i = 0; i < 10; i++)
            {
                string link = "";
                if (hrefListNew[i].Contains("http"))
                {
                    link = hrefListNew[i];
                }
                else
                {
                    link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                    //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                }

                if (link.Contains("video"))
                {
                    list.Add(GetHeroCard(
                    titleList[i].Replace("&quot;", ""), "", "",
                    new CardImage(url: imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );
                }
                else
                    if (link.Contains("gallery"))
                {
                    list.Add(GetHeroCard(
                    titleList[i].Replace("&quot;", ""), "", "",
                    new CardImage(url: imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );
                }
                else
                {
                    if (imgList[i].Contains("http"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                }

            }


            return list;

        }


        public static IList<Attachment> DajCardsAttachmentsNajnowsze(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/ajax-aside/";
            // string urlAddress = "http://www.orlenliga.pl/";

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();


            List<string> hrefListNew = new List<string>();
            List<string> imgList = new List<string>();
            List<string> titleList = new List<string>();
            DataTable dtArt = BaseDB.GetWiadomoscNajnowsze();
            DataTable dtImg = BaseDB.GetWiadomoscNajnowszeImg();
            DataTable dtTitle = BaseDB.GetWiadomoscNajnowszeTytul();
            for (int i = 0; i < 10; i++)
            {
                hrefListNew.Add(dtArt.Rows[0][i].ToString());
                imgList.Add(dtImg.Rows[0][i].ToString());
                titleList.Add(dtTitle.Rows[0][i].ToString());

            }

            for (int i = 0; i < 10; i++)
            {
                string link = "";
                if (hrefListNew[i].Contains("http"))
                {
                    link = hrefListNew[i];
                }
                else
                {
                    link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                    //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                }

                if (link.Contains("video"))
                {
                    list.Add(GetHeroCard(
                    titleList[i].Replace("&quot;", ""), "", "",
                    new CardImage(url: imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );
                }
                else
                    if (link.Contains("gallery"))
                {
                    list.Add(GetHeroCard(
                    titleList[i].Replace("&quot;", ""), "", "",
                    new CardImage(url: imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );
                }
                else
                {
                    if (imgList[i].Contains("http"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                }

            }


            return list;

        }

        public static void GetCardsAttachmentsNajnowsze()
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/ajax-aside/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "najnowsze";
                string xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found").Replace("'","''"))
                                  .ToList();



                for (int i = 1; i < hrefListNew.Count; i++)
                {
                    urlAddress = hrefListNew[i];
                    // string urlAddress = "http://www.orlenliga.pl/";

                    request = (HttpWebRequest)WebRequest.Create(urlAddress);
                    response = (HttpWebResponse)request.GetResponse();

                    listTemp2 = new List<System.Linq.IGrouping<string, string>>();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        receiveStream = response.GetResponseStream();
                        readStream = null;

                        if (response.CharacterSet == null)
                        {
                            readStream = new StreamReader(receiveStream);
                        }
                        else
                        {
                            readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        }

                        data = readStream.ReadToEnd();

                        doc = new HtmlDocument();
                        doc.LoadHtml(data);

                        matchResultDivId = "news";
                        xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                        people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                        text = "";
                        foreach (var person in people)
                        {
                            text += person.InnerHtml;
                        }
                        doc2.LoadHtml(text);
                        List<string> value = new List<string>();
                        try
                        {

                            value = doc2.DocumentNode.SelectNodes("//img")
                                      .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("http")).ToList();

                        }
                        catch
                        {

                        }
                        if (value!=null&&value.Count() > 0)
                        {
                            imgList.Add(value[0]);
                        }
                        else
                        {
                            imgList.Add("");
                            //hrefListNew.RemoveAt(i);
                            //titleList.RemoveAt(i);
                            //i--;
                        }


                    }

                }

                BaseDB.AddWiadomoscNajnowsze(hrefListNew);
                BaseDB.AddWiadomoscNajnowszeImg(imgList);
                BaseDB.AddWiadomoscNajnowszeTytul(titleList);

            }
        }

        public static IList<Attachment> GetCardsAttachmentsPilkaNozna(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/pilka-nozna/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "fl-left main";
                string xpath = String.Format("//main[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();
                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found"))
                                  .ToList();


                response.Close();
                readStream.Close();

                int index = 10;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = 10;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[0].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsSiatkowka(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/siatkowka/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "fl-left main";
                string xpath = String.Format("//main[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();
                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found"))
                                  .ToList();


                response.Close();
                readStream.Close();

                int index = 10;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = 10;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[0].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsHIT(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "article-news-big margin-box2 fl-left";
                string xpath = String.Format("//article[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();
                var titleList = doc2.DocumentNode.SelectNodes("//div")
                                  .Select(p => p.GetAttributeValue("news-preview", "not found"))
                                  .ToList();


                response.Close();
                readStream.Close();

                int index = 10;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = 10;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[0].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsSportyWalki(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/sporty-walki/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "fl-left main";
                string xpath = String.Format("//main[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();
                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found"))
                                  .ToList();


                response.Close();
                readStream.Close();

                int index = 10;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = 10;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[0].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsTenis(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl/tenis/";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "fl-left main";
                string xpath = String.Format("//main[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();
                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found"))
                                  .ToList();


                response.Close();
                readStream.Close();

                int index = 10;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = 10;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[i].Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[0].Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetProgramTV(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.polsatsport.pl";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "	cycle-slideshow";
                string xpath = String.Format("//section[@class='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefListNew = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found"))
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//figcaption").Select(p => p.ChildNodes).ToList();

                response.Close();
                readStream.Close();

                int index = hrefListNew.Count;

                DataTable dt = GetWiadomosciHokej();

                if (newUser == true)
                {
                    index = hrefListNew.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.hokej.gkskatowice.eu" + imgList[i]);
                                titleListTemp.Add(titleList[0][i].InnerHtml.Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //  titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefListNew[i].Contains("http"))
                    {
                        link = hrefListNew[i];
                    }
                    else
                    {
                        link = "http://www.hokej.gkskatowice.eu" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[0][i].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[0][i].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction, CardAction cardAction2)
        {
            if (cardAction2 != null)
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction, cardAction2 },
                };

                return heroCard.ToAttachment();
            }
            else
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction },
                };

                return heroCard.ToAttachment();
            }
        }


        public static DataTable GetWiadomosciPilka()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciGKSKatowicePilka]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }


        public static DataTable GetWiadomosciHokej()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciGKSKatowiceHokej]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }
        public static DataTable GetUser()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[UserPolsatSport] where flgDeleted=0";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania użytkowników");
                return null;
            }
        }

        public static DataTable GetWiadomosciSiatka()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciGKSKatowiceSiatka]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości Orlen");
                return null;
            }
        }

        public static IList<Attachment> GetCardsAttachmentsGallery(ref List<IGrouping<string, string>> hrefList, bool newUser = false, byte rodzajStrony = 0)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.gkskatowice.eu/index";

            switch (rodzajStrony)
            {
                case 1:
                    urlAddress = "http://siatkowka.gkskatowice.eu/index";
                    break;
                case 2:
                    urlAddress = "http://hokej.gkskatowice.eu/index";
                    break;
            }

            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "box-href";
                string xpath = String.Format("//li[@class='{0}']", matchResultDivId);

                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml); ;
                string text = "";
                foreach (var person in people)
                {
                    if (person.Contains("camera-icon"))
                    {
                        text += person;

                    }
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);

                hrefList = doc2.DocumentNode.SelectNodes("//a")
               .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/media/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
               .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/uploads/"))
                                  .ToList();



                var titleList = doc2.DocumentNode.SelectNodes("//h4").Select(p => p.ChildNodes)
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;


                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        if (rodzajStrony == 0)
                        {
                            link = "http://www.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 1)
                        {
                            link = "http://siatkowka.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 2)
                        {
                            link = "http://hokej.gkskatowice.eu" + hrefList[i].Key;
                        }
                    }

                    list.Add(GetHeroCard(
                    titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                    new CardImage(url: urlAddress.Replace("/index", "") + imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );

                }
            }
            return list;

        }
        public static IList<Attachment> GetCardsAttachmentsVideo(ref List<IGrouping<string, string>> hrefList, bool newUser = false, byte rodzajStrony = 0)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.gkskatowice.eu/index";

            switch (rodzajStrony)
            {
                case 1:
                    urlAddress = "http://siatkowka.gkskatowice.eu/index";
                    break;
                case 2:
                    urlAddress = "http://hokej.gkskatowice.eu/index";
                    break;
            }

            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "box-href";
                string xpath = String.Format("//li[@class='{0}']", matchResultDivId);

                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml); ;
                string text = "";
                foreach (var person in people)
                {
                    if (person.Contains("video-icon"))
                    {
                        text += person;

                    }
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);

                hrefList = doc2.DocumentNode.SelectNodes("//a")
               .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/media/") || p.Contains("/video/") || p.Contains("/gallery/") || p.Contains("/blog/")).GroupBy(p => p.ToString())
               .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/uploads/"))
                                  .ToList();



                var titleList = doc2.DocumentNode.SelectNodes("//h4").Select(p => p.ChildNodes)
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = 5;


                for (int i = 0; i < index; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        if (rodzajStrony == 0)
                        {
                            link = "http://www.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 1)
                        {
                            link = "http://siatkowka.gkskatowice.eu" + hrefList[i].Key;
                        }
                        else if (rodzajStrony == 2)
                        {
                            link = "http://hokej.gkskatowice.eu" + hrefList[i].Key;
                        }
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    list.Add(GetHeroCard(
                    titleList[i][0].InnerHtml.Replace("&quot;", ""), "", "",
                    new CardImage(url: urlAddress.Replace("/index", "") + imgList[i]),
                    new CardAction(ActionTypes.OpenUrl, "Oglądaj", value: link),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                    );

                }
            }
            return list;

        }
        public static IList<Attachment> GetCardsAttachmentsExtra(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string urlAddress = "", string tytul = "", string imgLink = "")
        {
            List<Attachment> list = new List<Attachment>();

            //  string urlAddress = "http://www.gkskatowice.eu/index";
            //    // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                list.Add(GetHeroCard(
                tytul, "", "",
                new CardImage(url: imgLink),
                new CardAction(ActionTypes.OpenUrl, "Zobacz", value: urlAddress),
                new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + urlAddress))
                );

            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsExtra2( bool newUser = false, string urlAddress = "")
        {
            List<Attachment> list = new List<Attachment>();


            urlAddress = urlAddress.Replace("!!!", ""); 
            

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();


            
            // string urlAddress = "http://www.orlenliga.pl/";

            request = (HttpWebRequest)WebRequest.Create(urlAddress);
            response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {

                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                receiveStream = response.GetResponseStream();
                readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                var data = readStream.ReadToEnd();

                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var matchResultDivId = "news";
                var xpath = String.Format("//section[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath);//.Select(p => p.InnerHtml);
                var text = "";
                foreach (var person in people)
                {
                    text += person.InnerHtml;
                }
                var doc2 = new HtmlDocument();
                doc2.LoadHtml(text);

                var value = doc2.DocumentNode.SelectNodes("//img")
                          .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("http")).ToList();

                var value2 = doc2.DocumentNode.SelectNodes("//img")
                          .Select(p => p.GetAttributeValue("alt", "not found")).ToList();
                string imgLink = "";
                string tytul = "";
                if (value.Count() > 0)
                {
                    imgLink = value[0];
                    tytul = value2[0];
                }



                list.Add(GetHeroCard(
                    tytul, "", "",
                    new CardImage(url: imgLink),
                    new CardAction(ActionTypes.OpenUrl, "Zobacz", value: urlAddress),
                    new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + urlAddress))
                    );

            }

            return list;

        }
    }
}