namespace GameUtils.Objects
{
    public class Quest
    {
        public bool completed;

        public int n { get; set; }
        public string questName { get; set; }
        public string questTitle { get; set; }
        public string questObjectivies { get; set; }
        public string questDescription { get; set; }
        
        
        public string reward { get; set; }
        public int Counter { get; set; }

      
        

        public Quest(int n, string questName, string questTitle, string questObjectivies, string questDescription,
            string reward)
        {
            this.n = n;
            this.questName = questName;
            this.questTitle = questTitle;
            this.questDescription = questDescription;
            this.reward = reward;
            this.questObjectivies = questObjectivies;
            completed = false;
        }

        public static Quest GetQuestTemplate()
        {
            return new Quest(
                -1,
                "Template quest",
                "Most useful quest(not)",
                "Test this and close",
                "Description starts here",
                "No revvard yet"
            );
        }
    }
}