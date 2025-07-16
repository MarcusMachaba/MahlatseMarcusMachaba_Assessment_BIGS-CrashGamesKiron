using System.Collections.Generic;

namespace Core.ApplicationModels.KironTestAPI
{
    public class NavNode
    {
        public string Text { get; set; }
        public List<NavNode> Children { get; set; } = new();
    }
}
