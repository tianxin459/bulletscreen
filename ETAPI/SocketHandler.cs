using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETAPI
{
    public class SocketHandler
    {
        public const int BufferSize = 5*1024;

        WebSocket socket;

        public static List<WebSocket> wsl = new List<WebSocket>();
        public static List<SocketHandler> sl = new List<SocketHandler>();


        SocketHandler(WebSocket socket)
        {
            this.socket = socket;
            //wsl.Add(socket);
        }

        async Task EchoLoop()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);

            while (this.socket.State == WebSocketState.Open)
            {
                if (this.socket.State == WebSocketState.CloseReceived)
                {
                    wsl.Remove(this.socket);
                    break;
                }


                try
                {
                    var incoming = await this.socket.ReceiveAsync(seg, CancellationToken.None);

                    var msgBuffur = new ArraySegment<byte>(buffer, 0, incoming.Count);
                    var msg = System.Text.Encoding.UTF8.GetString(msgBuffur.ToArray());
                    var message = DateTime.Now.ToLongTimeString();
                    var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);

                    for (var i = wsl.Count()-1;i>0;i--)
                    {
                        if (wsl[i].State != WebSocketState.Open)
                        {
                            wsl.RemoveAt(i);
                            continue;
                        }
                        await wsl[i].SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);

                    }
                    //foreach (var s in wsl)
                    //{
                    //    if(s.State != WebSocketState.Open)
                    //    {
                    //        wsl.Remove(s);
                    //        continue;
                    //    }
                    //    await s.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
                    //}
                    //await this.socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
                    //incoming = await this.socket.ReceiveAsync(seg, CancellationToken.None);
                }
                catch(ObjectDisposedException eo)
                {
                    wsl.Remove(this.socket);
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }

        static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest)
                return;

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var h = new SocketHandler(socket);
            wsl.Add(socket);
            await h.EchoLoop();
        }

        /// <summary>
        /// branches the request pipeline for this SocketHandler usage
        /// </summary>
        /// <param name="app"></param>
        public static void Map(IApplicationBuilder app)
        {
            var wsOptions = new WebSocketOptions() {
                KeepAliveInterval = TimeSpan.FromMinutes(5),
                ReceiveBufferSize = 5 * 1024
            };
            app.UseWebSockets(wsOptions);
            app.Use(SocketHandler.Acceptor);
        }
    }
}
