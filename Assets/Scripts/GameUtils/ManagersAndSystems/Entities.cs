using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.UnitBar;

namespace GameUtils.ManagersAndSystems
{
    public class Entities
    {
        public static void AddStats(AbstractGameObject ent, Stats stats)
        {
            ent.EntStats.AddStats(stats);
            ent.UpgradedStats.AddStats(stats);

            if (ClickManager.IsChoosed(ent.gameObject))
                UnitBar_HTML.UpdateInfo(ent);
        }

        public static void SubStats(AbstractGameObject ent, Stats stats)
        {
            ent.EntStats.SubStats(stats);
            ent.UpgradedStats.SubStats(stats);

            if (ClickManager.IsChoosed(ent.gameObject))
                UnitBar_HTML.UpdateInfo(ent);
        }

        public static void HealUnit(AbstractGameObject ent, int countHp)
        {
            var maxHp = ent.EntStats.Hp;
            var endHp = ent.UpgradedStats.Hp + countHp;
            if (endHp > maxHp) endHp = maxHp;
            ent.UpgradedStats.Hp = endHp;

            if (ClickManager.IsChoosed(ent.gameObject))
                UnitBar_HTML.UpdateInfo(ent);
        }
    }
}