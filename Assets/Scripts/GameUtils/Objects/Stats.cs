namespace GameUtils.Objects
{
    public class Stats
    {
        public int Hp { get;  set; }
        public int Dmg { get;  set; }
        public float MoveSpeed { get; set; }

        public Stats()
        {
            Hp = 0;
            Dmg = 0;
            MoveSpeed = 1f;
        }
        
        public Stats(int hp, int dmg, float movespeed)
        {
            Hp = hp;
            Dmg = dmg;
            MoveSpeed = movespeed;
        }

        public void AddStats(Stats stats)
        {
            Dmg += stats.Dmg;
            Hp += stats.Hp;
            MoveSpeed += stats.MoveSpeed;
        }

        public void SubStats(Stats stats)
        {
            Dmg -= stats.Dmg;
            Hp -= stats.Hp;
            MoveSpeed -= stats.MoveSpeed;

            if (Hp < 1) Hp = 1;
        }
    }
}