using mapapi.Models;
using mapapi.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly GeosocdbContext _context;
        private List<long> groupNum;

        public ChatHub(GeosocdbContext context)
        {
            _context = context;
        }

        public async Task Enter(string username) //connect
        {
            if (String.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("Notify", "Для входа в чат введите логин");
            }
            else
            {
                var user = await _context.Users.FirstAsync(s => s.Username == username);
                var chatRoomsId = await _context.ChatUsers.Where(s => s.UserId == user.IdUser).Select(s => s.ChatId).ToListAsync();
                //List<string> id_str_arry = new List<string>();
                foreach(Guid chatRoomId in chatRoomsId)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
                    await Clients.Group(chatRoomId.ToString()).SendAsync("Notify", $"{username} онлайн");
                    //id_str_arry.Add(chatRoomId.ToString());
                };
            }
        }
        public async Task Send(string message, string userName, string chatId)
        {
            var user = await _context.Users.FirstAsync(s => s.Username == userName);
            var userId = user.IdUser;
            await Clients.Group(chatId).SendAsync("Receive", message, userName, userId, chatId);

            Guid chatIdGuid = Guid.Parse(chatId);
            //ADDNOTIFICATONS()
            Message msg = new Message{
                ChatId = chatIdGuid,
                UserId = user.IdUser,
                MessageText = message,
                CreationTime = DateTime.UtcNow
            };
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();
        }

        public void AddNotifications()
        {
            //get all users in this chat
            //find users who not online at the monent in cycle using chat_connection table
            //add notification row for these users
        }

        //public void SendChatMessage(string receiver, string message)
        //{
        //    var name = Context.User.Identity.Name;

        //    var user = _context.Users.FirstOrDefault(n => n.Username == receiver);
        //    if (user == null)
        //    {
        //        //Clients.Caller.showErrorMessage("Could not find that user.");
        //        Clients.Caller.SendAsync("Receive", "Could not find that user.", name);
        //    }
        //    else
        //    {
        //        _context.Entry(user)
        //            .Collection(u => u.Connections)
        //            .Query()
        //            .Where(c => c.Connected == true)
        //            .Load();

        //        if (user.Connections == null)
        //        {
        //            Clients.Caller.SendAsync("Receive", "The user is no longer connected.", name);
        //        }
        //        else
        //        {
        //            //Clients.Client(connectionId).SendAsync("Receive", message, name);
        //            foreach (var connection in user.Connections)
        //            {
        //                //Clients.Client(connection.IdConnection).
        //                //    .SendChatMessage(name + ": " + message);
        //                Clients.Client(connection.IdConnection).SendAsync("Receive", message, name); //Client(connectionId)
        //                Clients.Caller.SendAsync("Sent", message, name);
        //            }
        //        }
        //    }
        //}

        public override Task OnConnectedAsync()
        {
            var name = Context.User.Identity.Name;

            var user = _context.Users
                .Include(u => u.Connections)
                .SingleOrDefault(u => u.Username == name);

            if (user.Connections == null)
            {
                user.Connections = new List<ChatConnection>();
            }

            var httpCtx = Context.GetHttpContext();
            user.Connections.Add(new ChatConnection
            {
                IdConnection = Context.ConnectionId,
                UserAgent = httpCtx.Request.Headers["User-Agent"],
                Connected = true
            });
            _context.SaveChanges();

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connection = _context.ChatConnections.Find(Context.ConnectionId);
            connection.Connected = false;
            _context.ChatConnections.Remove(connection);
            _context.SaveChanges();
            return base.OnDisconnectedAsync(exception);
        }
    }
}
