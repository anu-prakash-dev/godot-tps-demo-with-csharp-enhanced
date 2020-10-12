using Godot;
using GodotTPSSharpServer.Level;
using System;

namespace GodotTPSSharpServer.Autoloads
{
    public class Main : Node
    {
        private readonly PackedScene Level = ResourceLoader.Load<PackedScene>("res://level/Level.tscn");

        public static Main Instance { get; private set; }

        private LevelMap _mainMap;

        public override void _Ready()
        {
            Instance = this;

        }

        public string AddPlayer(int id)
        {
            if (_mainMap == null)
            {
                _mainMap = CreateMap();
            }
            
            _mainMap.AddPlayer(id);

            return _mainMap.Name;
        }

        public void RemovePlayer(int id)
        {
            _mainMap?.RemovePlayer(id);
        }

        private LevelMap CreateMap()
        {
            var node = (LevelMap) Level.Instance();
            node.Name = "Prototype";

            GetTree().Root.AddChild(node);

            return node;
        }
    }
}
