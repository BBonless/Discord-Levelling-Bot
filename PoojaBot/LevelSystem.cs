using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoojaBot
{ 

    class LevelSystem
    {
        private static PlayerData PlayerData;

        private static string DataPath;

        public LevelSystem(string DataPathIn)
        {
            PlayerData = new PlayerData(File.ReadAllLines(DataPathIn));

            DataPath = DataPathIn;
        }

        public void Register(ulong ID)
        {
            PlayerData.Data.Add(ID, new Data(0, 0, DateTimeOffset.FromUnixTimeSeconds(0)));
        }

        public void AdminAdd(ulong ID, int Amount, string Type)
        {
            if ( !PlayerData.Data.ContainsKey(ID) )
            {
                Register(ID);
            }

            switch ( Type )
            {
                case "levels":
                    PlayerData.Data[ID].Level += Amount;
                    break;
                case "points":
                    PlayerData.Data[ID].XP += Amount;
                    break;
                case "xp":
                    PlayerData.Data[ID].XP += Amount;
                    break;
                default:
                    break;
            }

            if ( PlayerData.Data[ID].XP >= Math.Pow(PlayerData.Data[ID].Level - 1, 2) * 10 )
            {
                LevelUp(ID);
            }

        }

        public void NaturalAdd(ulong ID)
        {
            if ( !PlayerData.Data.ContainsKey(ID) )
            {
                Register(ID);
            }

            DateTimeOffset Now = DateTimeOffset.Now;

            double SecondsSinceLastMessage = Now.Subtract(PlayerData.Data[ID].LastMessageTime).TotalSeconds;

            PlayerData.Data[ID].LastMessageTime = Now;

            if (SecondsSinceLastMessage < 2)
            {
                Util.Log("Did not grant XP for message to <@", ID, "> due to Spam Protection Timer");
                return;
            }

            if (++PlayerData.Data[ID].XP >= Math.Pow(PlayerData.Data[ID].Level - 1, 2) * 10)
            {
                LevelUp(ID);
            }

            PlayerData.Save(DataPath);
            Util.Log("Granted XP to <@", ID, ">");
        }

        public void QueryStatus(DiscordMessage MSG)
        {
            ulong ID = MSG.Author.Id;

            if (!PlayerData.Data.ContainsKey(ID))
            {
                Register(ID);
            }

            int LV = PlayerData.Data[ID].Level;
            int XP = PlayerData.Data[ID].XP;

            MSG.Channel.SendMessageAsync(string.Format("<@{0}>, Your current level is {1}, and you have {2} xp!", ID, LV, XP));
        }

        public void QueryToNext(DiscordMessage MSG)
        {
            ulong ID = MSG.Author.Id;

            int Max = (int)Math.Pow(PlayerData.Data[ID].Level - 1, 2) * 10;

            int Current = PlayerData.Data[ID].XP;

            if ( !PlayerData.Data.ContainsKey(ID) )
            {
                Register(ID);
            }

            MSG.Channel.SendMessageAsync(string.Format("<@{0}>, You need {1} xp to level up! ({2}/{3})", ID, Max - Current, Current, Max));
        }

        private void LevelUp(ulong ID)
        {
            PlayerData.Data[ID].Level++;
            PlayerData.Data[ID].XP = 0;

            Mainframe.Announce("<@", ID, ">", " Has levelled up to level ", PlayerData.Data[ID].Level, "!!");
        }
    }

    //Query, return the Level of a User
    //AdminAdd, Add N Levels to a User
    //AdminRemove, Remove N Levels from a User
    //Add, Qualify and Add Natural XP to a user
}
