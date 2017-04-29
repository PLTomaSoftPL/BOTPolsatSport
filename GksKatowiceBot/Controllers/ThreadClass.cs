using GksKatowiceBot.Helpers;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace GksKatowiceBot.Controllers
{
    public class ThreadClass
    {
        public async static void SendThreadMessage()
        {
            try
            {
                if (DateTime.UtcNow.Hour == 20 && (DateTime.UtcNow.Minute > 0 && DateTime.UtcNow.Minute <= 3))
                {
                    BaseDB.AddToLog("Wywołanie metody SendThreadMessage");

                    List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                    List<IGrouping<string, string>> hrefList2 = new List<IGrouping<string, string>>();
                    List<IGrouping<string, string>> hreflist3 = new List<IGrouping<string, string>>();
                    List<IGrouping<string, string>> hreflist4 = new List<IGrouping<string, string>>();
                    var items = BaseGETMethod.GetCardsAttachmentsAktualnosci(ref hrefList,true);
                    hreflist3 = hrefList;


                    string uzytkownik = "";
                    DataTable dt = BaseGETMethod.GetUser();

                    if (items.Count > 0)
                    {
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
                                    title = "Najważniejsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najwazniejsze",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Najnowsze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Najnowsze",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Popularne",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Popularne",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                                   }
                            });

                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            message.Attachments = items;
                            message.Text = "Podsumowanie dnia";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                try
                                {
                                    var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                    uzytkownik = userAccount.Name;
                                    var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                    var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "31a42302-5d77-403b-8636-de8bf91274e6", "aqiePPBrh9fc0YtetVmiojB");
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                    await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);
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


                        BaseDB.AddWiadomoscPilka(hreflist3);
                        BaseDB.AddWiadomoscSiatka(hrefList2);
                        BaseDB.AddWiadomoscHokej(hreflist4);
                    }
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }
    }
}