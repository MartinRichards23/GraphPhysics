using System.Windows;
using System.Windows.Media;
using SystemPlus.ComponentModel;
using SystemPlus.Windows.Media;

namespace GraphPhysics.Model
{
    public abstract class GraphItem : NotifyPropertyChanged
    {
        #region Fields

        bool isSelected;

        Color colour;

        protected static readonly Brush highlightBrush;

        public static double DetailThreshold = 0.6;

        #endregion

        static GraphItem()
        {
            highlightBrush = BrushCache.GetBrush(Color.FromArgb(150, 255, 255, 0));
        }

        protected GraphItem(Color colour)
        {
            Colour = colour;
        }

        #region Properties

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        public Color Colour
        {
            get { return colour; }
            set
            {
                colour = value;
                OnColourChanged();
            }
        }

        #endregion

        protected virtual void OnColourChanged()
        {
        }

        public virtual void Draw(DrawingContext dc, DrawParameters parameters)
        {
        }

        public virtual bool HitTest(Point p)
        {
            return false;
        }
    }
}