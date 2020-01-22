using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentExtensions.UI.Animations.Enums;

namespace FluentExtensions.UI
{
    public class Class1
    {
        public static async void Foo(Button control)
        {
            // Composition layer
            control
                .Animation()
                .Opacity(0)
                .Translation(Axis.X, 80, Easing.CircleEaseOut)
                .Duration(250)
                .Delay(100)
                .Start();

            // XAML too
            await control
                .Animation(FrameworkLayer.Xaml)
                .Opacity(0)
                .Scale(1.2, Easing.QuadraticEaseIn)
                .Duration(250)
                .StartAsync();
        }
    }
}
