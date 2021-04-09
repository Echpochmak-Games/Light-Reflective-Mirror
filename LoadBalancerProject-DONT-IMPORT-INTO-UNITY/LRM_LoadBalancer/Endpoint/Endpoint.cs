﻿using Grapevine;
using LightReflectiveMirror.Debug;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HttpStatusCode = Grapevine.HttpStatusCode;

namespace LightReflectiveMirror.LoadBalancing
{

    [RestResource]
    public partial class Endpoint
    {

        /// <summary>
        /// Sent from an LRM server node
        /// adds it to the list if authenticated.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/api/auth")]
        public async Task ReceiveAuthKey(IHttpContext context)
        {
            var req = context.Request;
            string receivedAuthKey = req.Headers["Authorization"];
            string endpointPort = req.Headers["x-EndpointPort"];
            string gamePort = req.Headers["x-GamePort"];
            string publicIP = req.Headers["x-PIP"];
            string region = req.Headers["x-Region"];

            string address = context.Request.RemoteEndPoint.Address.ToString();
            Logger.WriteLogMessage("Received auth req [" + receivedAuthKey + "] == [" + Program.conf.AuthKey + "]", ConsoleColor.Cyan);

            // if server is authenticated
            if (receivedAuthKey != null && region != null && int.TryParse(region, out int regionId) && 
                address != null && endpointPort != null && gamePort != null && receivedAuthKey == Program.conf.AuthKey)
            {
                Logger.WriteLogMessage($"Server accepted: {address}:{gamePort}");

                try
                {
                    var _gamePort = Convert.ToUInt16(gamePort);
                    var _endpointPort = Convert.ToUInt16(endpointPort);
                    await Program.instance.AddServer(address, _gamePort, _endpointPort, publicIP, regionId);
                }
                catch
                {
                    await context.Response.SendResponseAsync(HttpStatusCode.BadRequest);
                }

                await context.Response.SendResponseAsync(HttpStatusCode.Ok);
            }
            else
                await context.Response.SendResponseAsync(HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Called on the load balancer when a relay node had a change in their servers. This recompiles the cached values.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/api/roomsupdated")]
        public async Task ServerListUpdate(IHttpContext context)
        {
            // Dont allow unauthorizated access waste computing resources.
            string auth = context.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(auth) && auth == Program.conf.AuthKey)
            {
                var relays = Program.instance.availableRelayServers.ToList();
                ClearAllServersLists();
                List<Room> requestedRooms;

                for (int i = 0; i < relays.Count; i++)
                {
                    requestedRooms = await Program.instance.RequestServerListFromNode(relays[i].Key.address, relays[i].Key.endpointPort);
                    _allServers.AddRange(requestedRooms);

                    switch (relays[i].Key.serverRegion)
                    {
                        default:
                        case LRMRegions.NorthAmerica:
                            _northAmericaServers.AddRange(requestedRooms);
                            break;
                        case LRMRegions.SouthAmerica:
                            _southAmericaServers.AddRange(requestedRooms);
                            break;
                        case LRMRegions.Europe:
                            _europeServers.AddRange(requestedRooms);
                            break;
                        case LRMRegions.Africa:
                            _africaServers.AddRange(requestedRooms);
                            break;
                        case LRMRegions.Asia:
                            _asiaServers.AddRange(requestedRooms);
                            break;
                        case LRMRegions.Oceania:
                            _oceaniaServers.AddRange(requestedRooms);
                            break;
                    }
                }

                CacheAllServers();
            }
        }

        /// <summary>
        /// Hooks into from unity side, client will call this to 
        /// find the least populated server to join
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/api/join/")]
        public async Task JoinRelay(IHttpContext context)
        {
            // need to copy over in order to avoid
            // collection being modified while iterating.
            var servers = Program.instance.availableRelayServers.ToList();
            var low = lowest;

            if (servers.Count == 0)
            {
                await context.Response.SendResponseAsync(HttpStatusCode.RangeNotSatisfiable);
                return;
            }

            for (int i = 0; i < servers.Count; i++)
            {
                if (servers[i].Value.connectedClients < low.Value.connectedClients)
                {
                    low = servers[i];
                }
            }

            // respond with the server ip
            // if the string is still dummy then theres no servers
            await context.Response.SendResponseAsync(low.Key.address != "Dummy" ? JsonConvert.SerializeObject(low.Key) : HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Returns all the servers on all the relay nodes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/api/masterlist/")]
        public async Task GetMasterServerList(IHttpContext context)
        {
            string region = context.Request.Headers["x-Region"];

            if(int.TryParse(region, out int regionID))
            {
                switch ((LRMRegions)regionID)
                {
                    case LRMRegions.Any:
                        await context.Response.SendResponseAsync(allCachedServers);
                        break;
                    case LRMRegions.NorthAmerica:
                        await context.Response.SendResponseAsync(NorthAmericaCachedServers);
                        break;
                    case LRMRegions.SouthAmerica:
                        await context.Response.SendResponseAsync(SouthAmericaCachedServers);
                        break;
                    case LRMRegions.Europe:
                        await context.Response.SendResponseAsync(EuropeCachedServers);
                        break;
                    case LRMRegions.Africa:
                        await context.Response.SendResponseAsync(AfricaCachedServers);
                        break;
                    case LRMRegions.Asia:
                        await context.Response.SendResponseAsync(AsiaCachedServers);
                        break;
                    case LRMRegions.Oceania:
                        await context.Response.SendResponseAsync(OceaniaCachedServers);
                        break;
                }

                return;
            }

            // They didnt submit a region header, just give them all servers as they probably are viewing in browser.
            await context.Response.SendResponseAsync(allCachedServers);
        }

        /// <summary>
        /// Returns stats. you're welcome
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/api/stats/")]
        public async Task GetStats(IHttpContext context)
        {
            await context.Response.SendResponseAsync(JsonConvert.SerializeObject(_stats));
        }

        [RestRoute("Get", "/api/get/id")]
        public async Task GetServerID(IHttpContext context)
        {
            await context.Response.SendResponseAsync(Program.instance.GenerateServerID());
        }
    }

    #region Startup

    public partial class EndpointServer
    {
        public bool Start(ushort port = 7070)
        {
            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                var server = new RestServerBuilder(new ServiceCollection(), config,
                (services) =>
                {
                    services.AddLogging(configure => configure.AddConsole());
                    services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.None);
                }, (server) =>
                {
                    foreach (string ip in GetLocalIps())
                        server.Prefixes.Add($"http://{ip}:{port}/");
                }).Build();

                server.Router.Options.SendExceptionMessages = true;
                server.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        
        #endregion

    }

}