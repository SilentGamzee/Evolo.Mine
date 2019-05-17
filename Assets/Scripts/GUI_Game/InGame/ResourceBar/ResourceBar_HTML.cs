using PowerUI;

namespace GUI_Game.InGame.ResourceBar
{
    public class ResourceBar_HTML
    {
        public static void SetFoodCount(int count)
        {
            if (UI.document != null)
                UI.document.Run("setFoodCount", count);
        }

        public static void SetFoodMax(int max)
        {
            if (UI.document != null)
                UI.document.Run("setFoodMax", max);
        }

        public static void SetGoldCount(int count)
        {
            if (UI.document != null)
                UI.document.Run("setGoldCount", count);
        }
    }
}