using Microsoft.AspNetCore.SignalR;
using SignalRChatServerExample.Models;
using SignalRChatServerExample.Data;

namespace SignalRChatServerExample.Hubs;

public class ChatHub:Hub
{
    public async Task GetNickName(string nickName)
    {
        var client = new Client
        {
            ConnectionId = Context.ConnectionId,
            NickName = nickName
        };
        
        ClientSource.Clients.Add(client);
        await Clients.Others.SendAsync("clientJoined", nickName);
        await Clients.All.SendAsync("clients", ClientSource.Clients);
    }

    public async Task SendMessageAsync(string message, string nickName)
    {
        nickName = nickName.Trim();
        Client senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        if (nickName == "Tümü")
        {
            await Clients.All.SendAsync("receiveMessage", message, senderClient.NickName);
        }
        else
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.NickName == nickName);
            await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderClient.NickName);
        }
    }

    public async Task AddGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        Group group = new Group { GroupName = groupName };
        group.Clients.Add(ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId));
        
        GroupSource.Groups.Add(group);
        
        
        await Clients.All.SendAsync("groups", GroupSource.Groups);
        
    }

    public async Task AddClientToGroup(IEnumerable<string> groupNames)
    {
        Client client = ClientSource.Clients.FirstOrDefault(c=>c.ConnectionId==Context.ConnectionId);
        foreach (var group  in groupNames)
        {
            Group _group = GroupSource.Groups.FirstOrDefault(g=>g.GroupName==group);
            
            var result = _group.Clients.Any(c => c.ConnectionId == client.ConnectionId);
            if (!result)
            {
                _group.Clients.Add(client);
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }
        }
    }

    public async Task GetClientToGroup(string groupName)
    {
        Group group = GroupSource.Groups.FirstOrDefault(g=>g.GroupName==groupName);
        
        await Clients.Caller.SendAsync("clients", group.Clients);
    }
    
    public async Task SendMessageToGroupAsync(string groupName, string message)
    {
        Client senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        await Clients.Group(groupName).SendAsync("receiveMessage", message, senderClient.NickName);
    }
}