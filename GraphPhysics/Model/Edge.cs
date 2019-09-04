using System.Globalization;
using System.Windows;
using System.Windows.Media;
using SystemPlus.Windows;
using SystemPlus.Windows.Media;

namespace GraphPhysics.Model
{
    public sealed class Edge : EdgeBase
    {
        #region Fields

        FormattedText formattedText;
        Pen pen;

        #endregion

        public Edge(NodeBase from, NodeBase to)
            : this(from, to, ColourTools.Average(from.Colour, to.Colour))
        { }

        public Edge(NodeBase from, NodeBase to, Color colour)
            : base(from, to, colour)
        {

            UpdateText();
        }

        protected override void OnColourChanged()
        {
            base.OnColourChanged();
            MakePen();
        }

        void MakePen()
        {
            SolidColorBrush brush = BrushCache.GetBrush(Colour);
            pen = new Pen(brush, Thickness);
            pen.Freeze();
        }

        private void UpdateText()
        {
            formattedText = new FormattedText("Friends", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Helvetica"), 12, BrushCache.GetBrush(Colors.White));
        }

        public override void Draw(DrawingContext dc, DrawParameters parameters)
        {
            dc.DrawLine(pen, From.Position, To.Position);

            if (parameters.ZoomLevel > DetailThreshold)
            {
                Point position = VectorTools.MidPoint(From.Position, To.Position);
                Point textPosition = new Point(position.X - formattedText.Width / 2, position.Y - formattedText.Height / 2);
                
                dc.DrawText(formattedText, textPosition);
            }
        }
    }
}