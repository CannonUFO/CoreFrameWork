using ible.Foundation.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ible.GameModule
{
    /// <summary>
    /// 遊戲相關設定
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Config/Create Game Config")]
    public class GameConfig : ScriptableConfig<GameConfig>
    {
        [SerializeField]
        private List<ServerHost> _serverHosts = new List<ServerHost>();

        public ServerHost GetServerByType(ServerType serverType)
        {
            foreach (ServerHost host in _serverHosts) 
            { 
                if(host.serverType == serverType)
                    return host;
            }
            return null;
        }

        [SerializeField]
        private bool _showLog = false;
        public bool ShowLog
        {
            get { return _showLog; }
        }
    }


    [Serializable]
    public class ServerHost : IEquatable<ServerHost>
    {
        public string serverName;
        public ServerType serverType;
        public string host;
        public int port;

        public bool Equals(ServerHost other)
        {
            return serverType == other.serverType && port == other.port && host == other.host;
        }
    }

    public enum ServerType
    {
        None,
        Developer,
        InnerTest,
        QATest,
        ReleasePublic,
        Public,
        Custom,
    }
}
