using System.Windows;
using System.Windows.Controls;
using GraphPhysics.Physics;

namespace GraphPhysics
{
    public class PhysicsTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                DataTemplate dt = null;

                if (item is ForceDirectedPhysics)
                    dt = element.FindResource("forceDirectedTemplate") as DataTemplate;
                else if (item is NewtonianPhysics)
                    dt = element.FindResource("gravityTemplate") as DataTemplate;
                else if (item is NoPhysics)
                    dt = element.FindResource("noPhysicsTemplate") as DataTemplate;

                return dt;
            }

            return base.SelectTemplate(item, container);
        }
    }
}