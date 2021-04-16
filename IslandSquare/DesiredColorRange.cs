using System.Windows.Media;

namespace IslandSquare
{
    public class DesiredColorRange
    {
        public Color MinColor { get; private set; }
        public Color MaxColor { get; private set; }

        public SolidColorBrush MinColorBrush
        {
            get { return new SolidColorBrush(MinColor); }
        }

        public SolidColorBrush MaxColorBrush
        {
            get { return new SolidColorBrush(MaxColor); }
        }

        public DesiredColorRange(Color min, Color max)
        {
            MinColor = min;
            MaxColor = max;
        }
    }
}
