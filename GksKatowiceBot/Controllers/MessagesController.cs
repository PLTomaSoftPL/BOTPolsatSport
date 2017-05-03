using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using Parameters;
using GksKatowiceBot.Helpers;
using System.Json;

namespace GksKatowiceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    bool czyKarta = false;

                    if (BaseDB.czyAdministrator(activity.From.Id) != null && (((activity.Text != null && activity.Text.IndexOf("!!!") == 0) || (activity.Attachments != null && activity.Attachments.Count > 0))))
                    {
                        WebClient client = new WebClient();

                        if (activity.Attachments != null && activity.Attachments.Count>0 && !activity.Text.Contains("polsatsport.pl/"))
                        {
                            //Uri uri = new Uri(activity.Attachments[0].ContentUrl);
                            string filename = activity.Attachments[0].ContentUrl.Substring(activity.Attachments[0].ContentUrl.Length - 4, 3).Replace(".", "");


                            //  WebClient client = new WebClient();
                            client.Credentials = new NetworkCredential("serwer1606926", "Tomason1910");
                            client.BaseAddress = "ftp://serwer1606926.home.pl/public_html/pub/";


                            byte[] data;
                            using (WebClient client2 = new WebClient())
                            {
                                data = client2.DownloadData(activity.Attachments[0].ContentUrl);
                            }
                            if (activity.Attachments[0].ContentType.Contains("image")) client.UploadData(filename + ".png", data); //since the baseaddress
                            else if (activity.Attachments[0].ContentType.Contains("video")) client.UploadData(filename + ".mp4", data);
                        }

                        else if(activity.Text.Contains("polsatsport.pl/"))
                        {
                            activity.Attachments = BaseGETMethod.GetCardsAttachmentsExtra2(false, activity.Text);
                            activity.Text = "Z ostatniej chwili";
                            czyKarta = true;
                        }


                        CreateMessage(activity.Attachments, activity.Text == null ? "" : activity.Text.Replace("!!!", ""), activity.From.Id,czyKarta);

                    }
                    else
                    {
                        string komenda = "";
                        if (activity.ChannelData != null)
                        {
                            try
                            {
                                //   BaseDB.AddToLog("Przesylany Json " + activity.ChannelData.ToString());
                                dynamic stuff = JsonConvert.DeserializeObject(activity.ChannelData.ToString());
                                komenda = stuff.message.quick_reply.payload;
                                //    BaseDB.AddToLog("Komenda: " + komenda);
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Bład rozkładania Jsona " + ex.ToString());
                            }
                        }

                        MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);
                        if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia" || activity.Text == "Wydarzenia"
                            || activity.Text.ToUpper() == "WIADOMOŚCI" || activity.Text.ToUpper() == "AKTUALNOŚCI" || activity.Text.ToUpper() == "INFORMACJE" || activity.Text.ToUpper() == "WYDARZENIA")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Najważniejsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najwazniejsze",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Najnowsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najnowsze",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Popularne",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsAktualnosci(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Wideo" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Wideo" || activity.Text == "Wideo")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                //    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                // //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Więcej...",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Popularne",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsWideo(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                //    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                // //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Więcej...",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo2",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Popularne",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsNastepneWideo(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo2" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo2")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            //message.ChannelData = JObject.FromObject(new
                            //{
                            //    notification_type = "REGULAR",
                            //    //buttons = new dynamic[]
                            //    // {
                            //    //     new
                            //    //     {
                            //    //    type ="postback",
                            //    //    title="Tytul",
                            //    //    vslue = "tytul",
                            //    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                            //    //     }
                            //    // },
                            //    quick_replies = new dynamic[]
                            //           {
                            //    //new
                            //    //{
                            //    //    content_type = "text",
                            //    //    title = "Aktualności",
                            //    //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                            //    //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                            //    //},
                            //    //new
                            //    //{
                            //    //    content_type = "text",
                            //    //    title = "Aktualności",
                            //    //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                            //    //    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //    // //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                            //    //},
                            //    new
                            //    {
                            //        content_type = "text",
                            //        title = "Następne wideo",
                            //        payload = "DEVELOPER_DEFINED_PAYLOAD_NastepneWideo",
                            //       //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                            //    },
                            //    //new
                            //    //{
                            //    //    content_type = "text",
                            //    //    title = "Popularne",
                            //    //    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                            //    ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                            //    //},
                            //           }
                            //});
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsNastepneWideo2(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Popularne" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Popularne" || activity.Text == "Popularne")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Najważniejsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najwazniejsze",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Najnowsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najnowsze",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.DajCardsAttachmentsPopularne(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Najwazniejsze" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Najwazniejsze" || activity.Text == "Najważniejsze")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Najnowsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najnowsze",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Popularne",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.DajCardsAttachmentsNajwazniejsze(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Transmisje" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Transmisje" || activity.Text == "Transmisje")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",


                        //        buttons = new dynamic[]
                        //    {
                        //    new
                        //{
                        //        type = "web_url",
                        //        url = "https://petersfancyapparel.com/classic_white_tshirt",
                        //        title = "Wyniki",
                        //        webview_height_ratio = "compact"
                        //    }
                        //    },

                        //        quick_replies = new dynamic[]
                        //           {
                        //        //new
                        //        //{oh
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Polsat sport",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Polsat sport extra",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria",
                        //       //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Polsat Sport News",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                           }
                        //    });


                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                        //    //  message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Najnowsze" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Najnowsze" || activity.Text == "Najnowsze")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Najważniejsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najwazniejsze",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Popularne",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.DajCardsAttachmentsNajnowsze(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_POWIADOMIENIA" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_POWIADOMIENIA" || activity.Text == "Powiadomienia")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            byte czyPowiadomienia = BaseDB.czyPowiadomienia(userAccount.Id);
                            if (czyPowiadomienia == 0)
                            {
                                message.Text = "Opcja automatycznych, codziennych powiadomień o aktualnościach  jest włączona. Jeśli nie chcesz otrzymywać powiadomień  możesz je wyłączyć.";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                           {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Włącz",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},

                                           }
                                });
                            }
                            else if (czyPowiadomienia == 1)
                            {
                                message.Text = "Opcja automatycznych, codziennych  powiadomień o aktualnościach jest wyłączona. Jeśli chcesz otrzymywać powiadomienia możesz je włączyć.";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
           {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Wyłącz",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                //    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                // //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Włącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

           }
                                });
                            }
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //    message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            //     message.Text = "W kazdej chwili możesz włączyć lub wyłączyć otrzymywanie powiadomień na swojego Messengera. Co chcesz zrobić z powiadomieniami? ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zrozumiałem, wyłączyłem automatyczne, codzienne powiadomienia o aktualnościach.";
                            BaseDB.ChangeNotification(userAccount.Id, 1);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zrozumiałem, włączyłem automatyczne, codzienne powiadomienia o aktualnościach.";
                            BaseDB.ChangeNotification(userAccount.Id, 0);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_HIT" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_HIT")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    // BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",


                        //        buttons = new dynamic[]
                        //    {
                        //    new
                        //{
                        //        type = "web_url",
                        //        url = "https://petersfancyapparel.com/classic_white_tshirt",
                        //        title = "Wyniki",
                        //        webview_height_ratio = "compact"
                        //    }
                        //    },

                        //        quick_replies = new dynamic[]
                        //           {
                        //        //new
                        //        //{oh
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Polsat sport",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaAktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Polsat sport extra",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaGaleria",
                        //       //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Polsat Sport News",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_NoznaVideo",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                           }
                        //    });


                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                        //      message.Attachments = BaseGETMethod.GetCardsAttachmentsHIT(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PilkaNozna" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PilkaNozna")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    // BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",
                        //        //buttons = new dynamic[]
                        //        // {
                        //        //     new
                        //        //     {
                        //        //    type ="postback",
                        //        //    title="Tytul",
                        //        //    vslue = "tytul",
                        //        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //        //     }
                        //        // },
                        //        quick_replies = new dynamic[]
                        //               {
                        //        //new
                        //        //{
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Aktualności",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Siatkówka",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                        //           //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Sporty walki",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_SportyWalki",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Tenis",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Tenis",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                               }
                        //    });
                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                        //    message.Attachments = BaseGETMethod.GetCardsAttachmentsPilkaNozna(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PilkaNozna" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PilkaNozna")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    // BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",
                        //        //buttons = new dynamic[]
                        //        // {
                        //        //     new
                        //        //     {
                        //        //    type ="postback",
                        //        //    title="Tytul",
                        //        //    vslue = "tytul",
                        //        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //        //     }
                        //        // },
                        //        quick_replies = new dynamic[]
                        //               {
                        //        //new
                        //        //{
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Aktualności",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Siatkówka",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                        //           //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Sporty walki",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_SportyWalki",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                        new
                        //        {
                        //            content_type = "text",
                        //            title = "Tenis",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Tenis",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                               }
                        //    });
                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                        //    message.Attachments = BaseGETMethod.GetCardsAttachmentsPilkaNozna(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Siatkowka" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Siatkowka")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    // BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",
                        //        //buttons = new dynamic[]
                        //        // {
                        //        //     new
                        //        //     {
                        //        //    type ="postback",
                        //        //    title="Tytul",
                        //        //    vslue = "tytul",
                        //        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //        //     }
                        //        // },
                        //        quick_replies = new dynamic[]
                        //               {
                        //        //new
                        //        //{
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Aktualności",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Piłka nożna",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                        //           //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Sporty walki",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_SportyWalki",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                                                        new
                        //        {
                        //            content_type = "text",
                        //            title = "Tenis",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Tenis",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                               }
                        //    });
                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                        //    message.Attachments = BaseGETMethod.GetCardsAttachmentsSiatkowka(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_SportyWalki" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_SportyWalki")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    // BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",
                        //        //buttons = new dynamic[]
                        //        // {
                        //        //     new
                        //        //     {
                        //        //    type ="postback",
                        //        //    title="Tytul",
                        //        //    vslue = "tytul",
                        //        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //        //     }
                        //        // },
                        //        quick_replies = new dynamic[]
                        //               {
                        //        //new
                        //        //{
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Aktualności",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Piłka nożna",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_PilkaNozna",
                        //           //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Siatkówka",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                                                        new
                        //        {
                        //            content_type = "text",
                        //            title = "Tenis",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Tenis",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                               }
                        //    });
                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                        //    message.Attachments = BaseGETMethod.GetCardsAttachmentsSportyWalki(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        //else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Tenis" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Tenis")
                        //{
                        //    Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                        //    userStruct.userName = activity.From.Name;
                        //    userStruct.userId = activity.From.Id;
                        //    userStruct.botName = activity.Recipient.Name;
                        //    userStruct.botId = activity.Recipient.Id;
                        //    userStruct.ServiceUrl = activity.ServiceUrl;

                        //    // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                        //    // BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                        //    Parameters.Parameters.listaAdresow.Add(userStruct);
                        //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                        //    var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                        //    connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                        //    IMessageActivity message = Activity.CreateMessageActivity();
                        //    message.ChannelData = JObject.FromObject(new
                        //    {
                        //        notification_type = "REGULAR",
                        //        //buttons = new dynamic[]
                        //        // {
                        //        //     new
                        //        //     {
                        //        //    type ="postback",
                        //        //    title="Tytul",
                        //        //    vslue = "tytul",
                        //        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //        //     }
                        //        // },
                        //        quick_replies = new dynamic[]
                        //               {
                        //        //new
                        //        //{
                        //        //    content_type = "text",
                        //        //    title = "Aktualności",
                        //        //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                        //        //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                        //        //},
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Aktualności",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                        //            //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                        //         //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Piłka nożna",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_PilkaNozna",
                        //           //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                        //        },
                        //        new
                        //        {
                        //            content_type = "text",
                        //            title = "Siatkówka",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                                                        new
                        //        {
                        //            content_type = "text",
                        //            title = "Sporty walki",
                        //            payload = "DEVELOPER_DEFINED_PAYLOAD_SportyWalki",
                        //        //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                        //        },
                        //                                               }
                        //    });
                        //    message.From = botAccount;
                        //    message.Recipient = userAccount;
                        //    message.Conversation = new ConversationAccount(id: conversationId.Id);
                        //    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        //    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                        //    message.Attachments = BaseGETMethod.GetCardsAttachmentsTenis(ref hrefList, true);

                        //    await connector.Conversations.SendToConversationAsync((Activity)message);
                        //}
                        else if (activity.Text == "USER_DEFINED_PAYLOAD")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Transmisje",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Transmisje",
                                ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //},
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            if (userAccount.Name == "") userAccount.Name = " ";
                            
                            message.Text = @"Cześć " + userAccount.Name.Substring(0,userAccount.Name.IndexOf(' ')) + @",
Jestem BOTem, Twoim asystentem do kontaktu ze stronami internetowymi Polsat Sport. 
W każdej chwili umożliwię Ci dostęp do wiadomości sportowych i wideo, oraz powiadomię Cię
raz dziennie, wieczorem o wydarzeniach z całego dnia.";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Ponadto spodziewaj się również
innych powiadomień w formie komunikatów, bądź informacji, zdjęć lub filmów
przekazywanych przez moderatora, których nie znajdziesz na stronach internetowych.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Współpraca między nami jest bardzo prosta. Wydajesz mi polecenia, a ja za Ciebie wykonuje
robotę i czuwam, by najważniejsze informacje sportowe nie umknęły Twojej uwadze.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Polecenia możesz wydawać z rozwijanego menu, z moich podpowiedzi, a jeśli będziesz
zainteresowany konkretną dyscypliną, klubem, zawodnikiem itp. to poprzez wpisanie
nazwy, frazy i przesłaniu jej do mnie.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Jeśli polecenie będzie zrozumie dla mnie, natychmiast przystąpię do jego realizacji. Jeśli nie,

to poproszę o wydanie nowego.
W rozwijanym menu wybór opcji „ Powiadomienia” umożliwia odłączenie, bądź przyłączenie się do otrzymywania komunikatów i powiadomień.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);


                            message.Text = @"Na koniec jedna uwaga. Czasami na wykonanie zleconego zadania będziesz musiał trochę

poczekać. Nie jestem leniwy, ale rozumiesz, nie wszystko zależy tylko ode mnie.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                    if (activity.Text == "DEVELOPER_DEFINED_PAYLOAD_HELP" || activity.Text == "O mnie")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                                            new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Transmisje",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Transmisje",
                                ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //},
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            if (userAccount.Name == "") userAccount.Name = " ";


                            message.Text = @"Cześć " + userAccount.Name.Substring(0, userAccount.Name.IndexOf(' ')) + @",
Jestem BOTem, Twoim asystentem do kontaktu ze stronami internetowymi Polsat Sport. 
W każdej chwili umożliwię Ci dostęp do wiadomości sportowych i wideo, oraz powiadomię Cię
raz dziennie, wieczorem o wydarzeniach z całego dnia.";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Ponadto spodziewaj się również
innych powiadomień w formie komunikatów, bądź informacji, zdjęć lub filmów
przekazywanych przez moderatora, których nie znajdziesz na stronach internetowych.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Współpraca między nami jest bardzo prosta. Wydajesz mi polecenia, a ja za Ciebie wykonuje
robotę i czuwam, by najważniejsze informacje sportowe nie umknęły Twojej uwadze.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Polecenia możesz wydawać z rozwijanego menu, z moich podpowiedzi, a jeśli będziesz
zainteresowany konkretną dyscypliną, klubem, zawodnikiem itp. to poprzez wpisanie
nazwy, frazy i przesłaniu jej do mnie.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Jeśli polecenie będzie zrozumie dla mnie, natychmiast przystąpię do jego realizacji. Jeśli nie,

to poproszę o wydanie nowego.
W rozwijanym menu wybór opcji „ Powiadomienia” umożliwia odłączenie, bądź przyłączenie się do otrzymywania komunikatów i powiadomień.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);


                            message.Text = @"Na koniec jedna uwaga. Czasami na wykonanie zleconego zadania będziesz musiał trochę

poczekać. Nie jestem leniwy, ale rozumiesz, nie wszystko zależy tylko ode mnie.";

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (activity.Text.Length <= 40)
                        {

                            if (activity.Text.ToUpper().Contains("NA ŻYWO") || activity.Text.ToUpper().Contains("PROGRAM") || activity.Text.ToUpper().Contains("TELEWIZJA")
                              || activity.Text.ToUpper() == "TV" || activity.Text.ToUpper().Contains("POLSAT") || activity.Text.ToUpper().Contains("EUROSPORT"))
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                //     message.ChannelData = JObject.FromObject(new
                                //     {
                                //         notification_type = "REGULAR",
                                //         //buttons = new dynamic[]
                                //         // {
                                //         //     new
                                //         //     {
                                //         //    type ="postback",
                                //         //    title="Tytul",
                                //         //    vslue = "tytul",
                                //         //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //         //     }
                                //         // },
                                //         quick_replies = new dynamic[]
                                //     {
                                //     //new
                                //     //{
                                //     //    content_type = "text",
                                //     //    title = "Aktualności",
                                //     //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //     //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //     //},
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Piłka nożna",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Pilka_Nozna",
                                //         //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //        // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //     },
                                //     new
                                //     {
                                //         content_type = "text",
                                //         title = "Siatkówka",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Siatkowka",
                                ////         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //     },                                new
                                //     {
                                //         content_type = "text",
                                //         title = "Hokej",
                                //         payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                //     //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //     },
                                //                                    }
                                //     });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Text = "";
                                message.Attachments = Helpers.BaseGETMethod.GetCardsAttachmentsExtra(ref hrefList, true, "http://www.polsatsport.pl/program-telewizyjny/", "Program TV", "http://www.polsatsport.pl/templates/psport2014/gfx/logo-polsat-sport.png");

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else
                            {
                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                //BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                                //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();

                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                             //   new
                                                               //{
                                                               //    content_type = "text",
                                                               //    title = "Transmisje",
                                                               //    payload = "DEVELOPER_DEFINED_PAYLOAD_Transmisje",
                                                               ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                                               //},
                                                               //new
                                                               //{
                                                               //    content_type = "text",
                                                               //    title = "Hokej",
                                                               //    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                                               ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                                               //},
                                }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                                message.Attachments = BaseGETMethod.GetCardsAttachmentsInne(ref hrefList, true, activity.Text.ToString());
                                if (message.Attachments.Count > 0)
                                {
                                    // message.Text = "Wybierz jedną z opcji";
                                }
                                else
                                {
                                    message.Text = "Niestety na nie znaleziono artykułów pasujących do wpisanego hasła na stronach PolsatSport.pl. Spóbuj ponownie.";
                                }
                                // message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true);

                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        else if (activity.Text.Length > 20)
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                             //   new
                                //{
                                //    content_type = "text",
                                //    title = "Transmisje",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Transmisje",
                                ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Hokej",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //},
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Niestety wpisany zwrot jest za długi. Spróbuj jeszcze raz lub wybierz jedną z opcji";


                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                             //   new
                                //{
                                //    content_type = "text",
                                //    title = "Transmisje",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Transmisje",
                                ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Hokej",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Hokej",
                                ////       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                //},
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Wybierz jedną z opcji";
                            // message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                    }
                }

                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Wysylanie wiadomosci: " + ex.ToString());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        internal async static void CreateMessage(IList<Attachment> foto, string wiadomosc, string fromId, bool czyKarta = false)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateMessage");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        quick_replies = new dynamic[]
                            {
                                                    new
                                {
                                    content_type = "text",
                                    title = "Wydarzenia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wydarzenia",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Wideo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Wideo",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                                           }
                    });

                    message.AttachmentLayout = null;
                    
                    message.Text = wiadomosc;

                    if (foto != null && foto.Count > 0)
                    {
                        if (!czyKarta)
                        {
                            string filename = foto[0].ContentUrl.Substring(foto[0].ContentUrl.Length - 4, 3).Replace(".", "");

                            if (foto[0].ContentType.Contains("image")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";//since the baseaddress
                            else if (foto[0].ContentType.Contains("video")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".mp4";
                        }
                        //foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";
                                                
                    }
                    message.Attachments = foto;

                    //var list = new List<Attachment>();
                    //if (foto != null)
                    //{
                    //    for (int i = 0; i < foto.Count; i++)
                    //    {
                    //        list.Add(GetHeroCard(
                    //       foto[i].ContentUrl, "", "",
                    //       new CardImage(url: foto[i].ContentUrl),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                    //    }
                    //}


                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        BaseDB.AddToLog("Ilość użytkowników "+dt.Rows.Count);
                        try
                        {
                            if (fromId != dt.Rows[i]["UserId"].ToString())
                            {
                                BaseDB.AddToLog("Wysyłam do " + dt.Rows[i]["UserId"].ToString());
                                var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "31a42302-5d77-403b-8636-de8bf91274e6", "aqiePPBrh9fc0YtetVmiojB");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }






        public static void CallToChildThread()
        {
            try
            {
                Thread.Sleep(5000);
            }

            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Couldn't catch the Thread Exception");
            }
        }






        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                BaseDB.DeleteUser(message.From.Id);
            }
            else
                if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else
                    if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else
                        if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else
                            if (message.Type == ActivityTypes.Ping)
            {
            }
            else
                                if (message.Type == ActivityTypes.Typing)
            {
            }
            return null;
        }







    }
}
