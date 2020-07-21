using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using System.Net;
using System.Threading;
using TechCraftEngine.WorldEngine;
using System.IO;
using System.IO.Compression;
using TechCraftEngine.WorldEngine.Generators;

namespace TechCraftEngine.Network
{
    public enum GameState
    {
        Ready,
        Connecting,
        Connected,
        Disconnected,
        Rejected,
        Loading,
        Loaded,
        Playing
    }

    public enum NetworkMessageType
    {
        PlayerMove = 1
    }

    public class GameClient
    {
        private TechCraftGame _game;
        private World _world;
        private GameState _gameState = GameState.Ready;

        private NetworkSession _networkSession;
        private PacketWriter _packetWriter = new PacketWriter();
        private PacketReader _packetReader = new PacketReader();

        public GameClient(TechCraftGame game)
        {
            _game = game;
            _world = new World(_game);
            _world.Initialize();
        }

        private void CreateNetworkSession()
        {
            _networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, WorldSettings.MAXGAMERS);
            _networkSession.GamerJoined += new EventHandler<GamerJoinedEventArgs>(_networkSession_GamerJoined);
            _networkSession.SessionEnded += new EventHandler<NetworkSessionEndedEventArgs>(_networkSession_SessionEnded);
        }

        void _networkSession_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            _networkSession.Dispose();
            _networkSession = null;
        }

        void _networkSession_GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public World World
        {
            get { return _world; }
        }

        public GameState GameState
        {
            get { return _gameState; }
        }

        private string _statusText = "INITIALIZING";
        public string StatusText
        {
            get { return _statusText; }
        }

        public void LoadMap()
        {
           /* _statusText = "LOADING";
            LandscapeMapGenerator mapGenerator = new LandscapeMapGenerator();
            //DualLayerTerrainWithMediumValleys mapGenerator = new DualLayerTerrainWithMediumValleys(); 
            _statusText = "GENERATING MAP";
            BlockType[, ,] mapData = mapGenerator.GenerateMap();
            _statusText = "BUILDING WORLD";
            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int y = 0; y < WorldSettings.MAPHEIGHT; y++)
                {
                    for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                    {
                        BlockType blockType = mapData[x, y, z];
                        if (blockType != BlockType.None)
                        {
                            _world.AddBlock(x, y, z, blockType);
                        }
                    }
                }
            }
            */
            _statusText = "BUILDING WORLD";
            //IRegionBuilder builder = new SimpleTerrain();
            IRegionBuilder builder = new TerrainWithCaves();
            //IRegionBuilder builder = new FlatReferenceTerrain();
            
            _world.BuildRegions(builder);

            _statusText = "INITIALIZING LIGHTING";
            _world.Lighting.Initialize();

            //_statusText = "BUILDING REGIONS";
            //_world.BuildRegions();
            _statusText = "LOADED";
            _gameState = GameState.Loaded;
        }

        public void UpdateNetworkSession()
        {
            foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers)
            {
                UpdateLocalGamer(gamer);
            }
            if (_networkSession.IsHost)
            {
                UpdateServer();
            }
            _networkSession.Update();
            if (_networkSession == null) return;
            foreach (LocalNetworkGamer gamer in _networkSession.LocalGamers)
            {
                if (gamer.IsHost)
                {
                    //ServerReadInputFromClients(gamer);
                }
                else
                {
                    //ClientReadStateFromServer(gamer);
                }
            }
        }

        private void UpdateLocalGamer(LocalNetworkGamer gamer)
        {
            // Only send if we are not the server. There is no point sending packets
            // to ourselves, because we already know what they will contain!
            if (!_networkSession.IsHost)
            {
                _packetWriter.Write((byte)NetworkMessageType.PlayerMove);             
            }
        }

        private void UpdateServer()
        {
            foreach (NetworkGamer gamer in _networkSession.AllGamers)
            {

            }
        }
    }
}
