using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;

namespace AoShinhoServ_Monitor
{
    public class IAnimation
    {
        public static ThicknessAnimation F_Thickness(Thickness From, Thickness To, double Duration = 0.1)
        {
            ThicknessAnimation marginAnimation = new ThicknessAnimation();
            marginAnimation.GetAnimationBaseValue(FrameworkElement.MarginProperty);
            marginAnimation.From = From;
            marginAnimation.To = To;
            marginAnimation.Duration = TimeSpan.FromSeconds(Duration);
            return marginAnimation;
        }

        public static Thickness F_Thickness_Pressed(Thickness thickness) => new Thickness(thickness.Left + 1, thickness.Top + 1, thickness.Right, thickness.Bottom);

        public static void F_Grid(Grid grid, Thickness thickness, bool enter = false) => grid.BeginAnimation(FrameworkElement.MarginProperty, enter ? F_Thickness(thickness, F_Thickness_Pressed(thickness)) : F_Thickness(F_Thickness_Pressed(thickness), thickness));

    }
}
