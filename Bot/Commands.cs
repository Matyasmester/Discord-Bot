using System;
using Discord;
using System.Threading.Tasks;
using Discord.Commands;
using System.Linq;

namespace Bot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly string[] commands = { "!Hali Gyurbi", "!cls <üzenetek_száma> (MINDEN6ÓK)", "!parancsok", "!list (MINDEN6ÓK)", "!getinfo <felhasználó> (MINDEN6ÓK)", "!say <üzenet>", "!report <felhasználó> <MINDEN6ó> <feljelentés_oka>" };

        private readonly long[] admins = { 432276529259741184, 485040164914069504 };

        [Command("Hali Gyurbi")]
        [Alias("Hello Gyurbi", "Szevasz Gyurbi")]
        public async Task Greet()
        {
            await ReplyAsync("Szevasz, alattvalóm!");
            return;
        }

        [Command("cls")]
        [Alias("clearscreen")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task ClearScreen(int nMessages)
        {
            if (Context.User.IsBot) return;

            if (nMessages <= 0)
            {
                await ReplyAsync("!cls <üzenetek_száma>");
                return;
            }

            var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, nMessages).FlattenAsync();

            var filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(filteredMessages);
            return;
        }

        [Command("parancsok")]
        [Alias("cmdlist")]
        public async Task ListCommands()
        {
            if (Context.User.IsBot) return; 

            string message = "Parancsaink: \n";

            foreach (string cmd in commands)                // We do this because we dont want to send too many messages,
            {                                               // lest it is incredibly slow, and annoying to the user.
                message += "-> " + cmd;                     // If we send only one message, then the tasks wont get blocked
                message += "\n";                            // by the 'MessageReceived' handler.
            }
            await ReplyAsync(message);
            return;
        }

        [Command("list")]
        [Alias("userlist")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task ListUsers()
        {
            if (Context.User.IsBot) return;

            short online = 0;
            short offline = 0;
            short all = 0;
            short bots = 0;

            string message = "Jelenleg itt levő (online) felhasználóink: \n";

            var users = Context.Guild.Users;

            foreach (var user in users)
            {
                if (user.IsBot)
                {
                    bots++;
                    continue;
                }

                if (user.Status == UserStatus.Online)
                {
                    message += "--- *" + user.Username + "*";
                    message += "\n";
                    online++;
                }
                if (user.Status == UserStatus.Offline) offline++;
                all++;
            }

            message += "**Összesen online: " + online + "**\n";
            message += "**Összesen offline: " + offline + "**\n";              // The *s are there for formatting, 
            message += "**Összes bot (nem számít tagnak): " + bots + "**\n";   // *text* is slanted, **text** is bold in Discord.
            message += "**Összes tag: " + all + "**\n";

            await ReplyAsync(message);
            return;
        }

        [Command("getinfo")]
        [Alias("getinfoabout")]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task GetInfoAbout(IUser User)
        {
            if (User.IsBot) return;

            string message;
 
            var users = Context.Guild.Users.ToArray();

            int UserIndex = Array.IndexOf(users, User);

            var user = users[UserIndex];

            message = "Infók **" + user.Username + "** tagról: \n";
            message += " - Ezóta Discord-felhasználó: " + "*" + user.CreatedAt + "*" + "\n";
            message += " - Ekkor csatlakozott: " + "*" + user.JoinedAt + "*" + "\n";
            message += " - ID: " + "*" + user.Id + "*" + "\n";
            message += " - Jelenleg: " + "*" + user.Status + "*" + "\n";
            message += user.Nickname == null ? " - Becenév: *nincs* \n" : " - Becenév: " + "*" + user.Nickname + "*" + "\n";

            await ReplyAsync(message);
            return;
            
        }

        [Command("say")]
        public async Task Say(params string[] strings)
        {
            if (Context.User.IsBot) return;

            string FinalMessage = "";

            foreach (string s in strings)
            {
                FinalMessage += s;
                FinalMessage += " ";
            }

            await ReplyAsync(FinalMessage);
            return;
        }

        [Command("report")]
        public async Task Report(IUser User, IUser Admin, params string[] Reason)
        {
            if (Context.User.IsBot) return;

            if (Array.IndexOf(admins, (long)Admin.Id) == -1) 
            {
                await ReplyAsync("A második paraméter egy MINDEN6ó kell legyen.");
                return;
            }

            string reason = "";

            foreach (string s in Reason)
            {
                reason += " ";
                reason += s;
            }

            string FinalMsg = "**" + Context.User.Username + "**" + " felhasználó feljelentette **" + User.Username + "** felhasználót a következő okból: " + reason;

            await Admin.SendMessageAsync(FinalMsg);
            return;
        }
    }
}
