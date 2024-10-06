namespace backend.Models.Entity.Proxy
{
    public class ProxyPlayerImage : IGameAsset
    {
        private RealPlayerImage realImage;
        private Player player;

        public ProxyPlayerImage(Player player)
        {
            this.player = player;
        }

        public void GetImage()
        {
            if(realImage == null)
            {
                realImage = new RealPlayerImage(player);
            }
            realImage.GetImage();
        }
    }
}
