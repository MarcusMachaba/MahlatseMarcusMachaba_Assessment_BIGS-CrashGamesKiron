using System.Collections.Generic;

namespace Core.ApplicationModels.KironTestAPI
{
    public class DragonBallApiResponse
    {
        public List<DragonBallCharacter> Items { get; set; }
        public Meta Meta { get; set; }
        public Links Links { get; set; }
    }
}
