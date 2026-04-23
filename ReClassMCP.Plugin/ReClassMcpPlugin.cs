using System.Threading;
using ReClassNET.Plugins.V2;

namespace ReClassMCP
{
    [Plugin("net.reclass.mcp",
        DisplayName = "ReClass MCP",
        Version = "1.0.0",
        Description = "Exposes ReClass.NET to Claude Code via TCP JSON-RPC / MCP",
        Stage = PluginLoadStage.AfterMainForm)]
    public sealed class ReClassMcpPlugin : IReClassPlugin
    {
        private IPluginContext _context;
        private SynchronizationContext _uiThread;
        private McpServer _server;
        private Thread _serverThread;
        private const int Port = 27015;

        public void OnLoad(IPluginContext context)
        {
            _context = context;
        }

        public void OnActivate()
        {
            _uiThread = SynchronizationContext.Current;

            var handler = new CommandHandler(_context, _uiThread);
            _server = new McpServer(Port, handler);
            _serverThread = new Thread(() => _server.Start())
            {
                IsBackground = true,
                Name = "ReClassMCP Server"
            };
            _serverThread.Start();
            _context.Log.Info($"[ReClassMCP] MCP Server started on port {Port}");
        }

        public void OnDeactivate()
        {
            _server?.Stop();
            if (_serverThread?.IsAlive == true)
                _serverThread.Join(1000);
            _context.Log.Info("[ReClassMCP] MCP Server stopped");
        }

        public void OnUnload() { }
    }
}
