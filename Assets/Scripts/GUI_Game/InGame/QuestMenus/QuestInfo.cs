using Widgets;

namespace Widgets
{
    [Dom.TagName("QuestInfo")]
    public class QuestInfo : Widget
    {
        public override int Depth
        {
            get { return 1000; }
        }
    }
}