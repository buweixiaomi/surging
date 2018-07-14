﻿using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.WS;
using Surging.Core.Protocol.WS.Runtime;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketCore;

namespace Surging.Modules.Common.Domain
{
    public class ChatService : WSServiceBase, IChatService
    {
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> _clients = new ConcurrentDictionary<string, string>();
        private string _name;
        private  int _number = 0;
     

        private  int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            //Sessions.Broadcast(String.Format("{0} got logged off...", _name));
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (_clients.ContainsKey(ID))
            {
                Dictionary<string, object> model = new Dictionary<string, object>();
                model.Add("name", _clients[ID]);
                model.Add("data", e.Data);
               var result=  ServiceLocator.GetService<IServiceProxyProvider>()
                    .Invoke<object>(model, "api/chat/SendMessage").Result;

                //this.CreateProxy<IChatService>()
                //    .SendMessage(_clients[ID], e.Data);
            }
        }

        protected override void OnOpen()
        {
            _name = Context.QueryString["name"];
            if (!string.IsNullOrEmpty(_name))
            {
                _clients[ID] = _name;
                _users[_name] = ID;
            }
        }
        public Task SendMessage(string name, string data)
        {
            if (_users.ContainsKey(name))
            { 
                
                this.GetClient().SendTo($"hello,{name},{data}", _users[name]);
            }
            return Task.CompletedTask;
        }
    }
}
