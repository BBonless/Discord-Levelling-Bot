using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PoojaBot
{
    class Mainframe
    {
        const long BOTID = 843883099116142622;

        static ulong AnnouncementChannelID = 0;

        static LevelSystem Levels;

        static DiscordClient Discord;

        static void Main( string[] args )
        {
            Levels = new LevelSystem("C:\\Users\\quent\\Desktop\\POOJAPLAYERDATA.txt");

            StartBot().GetAwaiter().GetResult();
        }

        static async Task StartBot()
        {
            Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "ODQzODgzMDk5MTE2MTQyNjIy.YKKVYQ.AQvT6W5ApOlh3PVSxolwOZWMZP4",
                TokenType = TokenType.Bot
            });

            Discord.MessageCreated += async (Client, MSG) =>
            {
                Task.Run(() => HandleMessageCreated(MSG));
            };

            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }

        static async Task HandleMessageCreated(MessageCreateEventArgs MSG)
        {
            string Content = MSG.Message.Content;

            if (MSG.Author.Id == BOTID)
            {
                MSG.Handled = true; return;
            }

            if (!(Content.StartsWith("Pooja,") || Content.StartsWith("P,")))
            {
                HandleNoCommand(MSG.Message);

                MSG.Handled = true; return;
            }

            HandleCommand(MSG.Message);

            MSG.Handled = true; return;
        }


        static async Task HandleNoCommand(DiscordMessage MSG)
        {
            Levels.NaturalAdd(MSG.Author.Id);
        }

        static async Task HandleCommand(DiscordMessage MSG)
        {
            string[] Command = Util.ExtractCommand(MSG.Content.ToLower()).Split(' ');

            switch ( Command[0] )
            {
                case "rank":
                    Levels.QueryStatus(MSG);
                    break;
                case "next":
                    Levels.QueryToNext(MSG);
                    break;
                case "add":
                    // 1 = Amount
                    // 2 = Type
                    // 3 = To (OPT)
                    // 4 = User
                    
                    if (Command.Length <= 2)
                    {
                        Levels.AdminAdd(Util.GetIDFromTag(Command[4]), int.Parse(Command[1]), Command[2]);
                    }
                    else
                    {
                        Levels.AdminAdd(MSG.Author.Id, int.Parse(Command[1]), Command[2]);
                    }

                    Util.SendMessageThenDelete(MSG.Channel, "Done!", 1000);
                    break;
                case "set":
                    switch ( Command[1] )
                    {
                        case "announcements":
                            AnnouncementChannelID = MSG.ChannelId;
                            Util.SendMessageThenDelete(MSG.Channel, "Set Announcment channel to current channel!", 1000);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public static async Task Announce(params object[] O)
        {
            StringBuilder SB = new StringBuilder();

            foreach ( object O1 in O )
            {
                SB.Append(O1);
            }

            if ( AnnouncementChannelID == 0 )
            {
                Util.Log("Tried to announce the following: '", SB.ToString(), "' but no announcement channel was set :(");
                return;
            }

            (await Discord.GetChannelAsync(AnnouncementChannelID)).SendMessageAsync(SB.ToString());
        }
    }
}
