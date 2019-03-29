# UICompositionAnimations

The **UICompositionAnimations** library exposes classes and APIs to quickly implement animations and effects to a UWP application.
It also has a collection of helper methods to load Win2D images, dispatch code to the UI thread and more.

<a href="https://www.nuget.org/packages/UICompositionAnimations/"><img src="https://i.imgur.com/7WcDj71.png" alt="Get it from NuGet" width='280' /></a>

[![NuGet](https://img.shields.io/nuget/v/UICompositionAnimations.svg)](https://www.nuget.org/packages/UICompositionAnimations/) [![NuGet](https://img.shields.io/nuget/dt/UICompositionAnimations.svg)](https://www.nuget.org/stats/packages/UICompositionAnimations?groupby=Version)

## Used by

| [**OneLocker**](https://www.microsoft.com/store/apps/9nblggh3t7g3?cid=UICompositionAnimations) | [**myTube!**](https://www.microsoft.com/store/apps/9wzdncrcwf3l?cid=UICompositionAnimations) | [**Brainf\*ck#**](https://www.microsoft.com/store/apps/9nblgggzhvq5?cid=UICompositionAnimations) |
| --- | --- | --- |
| <img src="https://i.imgur.com/i2LgImE.png" alt="OneLocker screens" width='280'/> | <img src="https://i.imgur.com/Rjc3nlY.png" alt="IDE" width='280'/> | <img src="https://i.imgur.com/rdiVWEH.png" alt="IDE" width='280'/> |

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [Quick start](#quick-start)
  - [Animations](#animations) 
  - [`UI.Composition` effects](#uicomposition-effects)
- [Misc](#misc)
- [Requirements](#requirements)

# Installing from NuGet

To install **UICompositionAnimations**, run the following command in the **Package Manager Console**

```
Install-Package UICompositionAnimations
```

More details available [here](https://www.nuget.org/packages/UICompositionAnimations/).

# Quick start

## Animations

The available animation APIs use the fluent pattern and support combining multiple animations togetger. The main entry point is the ` UIElementExtensions.Animation` method, that returns an `IAnimationBuilder` object targeting the input `UIElement`. This object exposes all the available animation APIs.

You can use it like this:

```C#
MyControl.Animation()
         .Opacity(0)
         .Translation(Axis.X, 60)
         .Duration(250)
         .Start();
```

It is also possible to set an initial delay, and to wait for the animation to be completed. Also, should you need to do so in a particular situation, it is also possible to choose between the `Windows.UI.Composition` and `Windows.UI.Xaml.Media.Animation` APIs to run the animations. To toggle between the two, just pass a `FrameworkLayer` value to the `Animation` method. Furthermore, each animation API has two overloads: one that just takes the target value, and one that also sets the initial value for the animation. It is also possible to specify an easing function for each individual animation. Here is another, more complex example:

```C#
await MyControl.Animation(FrameworkLayer.Xaml)
               .Opacity(0, 1, Easing.CircleEaseOut)
               .Scale(1.2, 1, Easing.QuadratincEaseInOut)
               .Duration(500)
               .Delay(250)
               .StartAsync();
```

## `UI.Composition` effects

The library provides several ways to use `UI.Composition` effects: there are both ready to use XAML brushes (like a customizable acrylic brush), a `CompositionBrushBuilder` class to create complex composition effect pipelines, and more.

#### Declare an acrylic brush in XAML

```XAML
xmlns:brushes="using:UICompositionAnimations.Brushes">
  
<!--The acrylic brush to use in the app-->
<brushes:AcrylicBrush
    x:Key="InAppGrayAcrylicBrush"
    Source="HostBackdrop"
    BlurAmount="8"
    Tint="#FF222222"
    TintMix="0.6"
    NoiseTextureUri="/Assets/Misc/noise.png"/>
```

**Note**: the `NoiseTextureUri` parameter must be set to a .png image with a noise texture. It is up to the developer to create his own noise texture and to import it into the app. An easy plugin to create one is [NoiseChoice](https://forums.getpaint.net/topic/22500-red-ochre-plug-in-pack-v9-updated-30th-july-2014/) for [Paint.NET](https://www.getpaint.net/).

#### Create custom effects in XAML:

Using the APIs in `UICompositionAnimations.Brushes.Effects` it is also possible to build complex Composition/Win2D pipelines directly from XAML, in a declarative way. This is how to define a custom host backdrop acrylic brush:

```xml
xmlns:brushes="using:UICompositionAnimations.Brushes"
xmlns:effects="using:UICompositionAnimations.Brushes.Effects"

<brushes:PipelineBrush>
    <brushes:PipelineBrush.Effects>
        <effects:BackdropEffect Source="HostBackdrop"/>
        <effects:LuminanceEffect/>
        <effects:OpacityEffect Value="0.4"/>
        <effects:BlendEffect Mode="Multiply">
            <effects:BlendEffect.Input>
                <effects:BackdropEffect Source="HostBackdrop"/>
            </effects:BlendEffect.Input>
        </effects:BlendEffect>
        <effects:TintEffect Color="#FF1E90FF" Opacity="0.2"/>
        <effects:BlendEffect Mode="Overlay" Placement="Background">
            <effects:BlendEffect.Input>
                <effects:TileEffect Uri="/Assets/noise_high.png"/>
            </effects:BlendEffect.Input>
        </effects:BlendEffect>
    </brushes:PipelineBrush.Effects>
</brushes:PipelineBrush>
```

#### Create and assign an acrylic brush in C#

```C#
control.Background = PipelineBuilder.FromHostBackdropAcrylic(Colors.DarkOrange, 0.6f, new Uri("ms-appx:///Assets/noise.png"))
                                    .AsBrush();
```

#### Build an acrylic effect pipeline from scratch:

```C#
Brush brush = PipelineBuilder.FromHostBackdropBrush()
                             .Effect(source => new LuminanceToAlphaEffect { Source = source })
                             .Opacity(0.4f)
                             .Blend(CompositionBrushBuilder.FromHostBackdropBrush(), BlendEffectMode.Multiply)
                             .Tint(Color.FromArgb(0xFF, 0x14, 0x14, 0x14), 0.8f)
                             .Blend(CompositionBrushBuilder.FromTiles(new Uri("ms-appx:///Assets/noise.png")), BlendEffectMode.Overlay, Placement.Background)
                             .AsBrush();
```

The `PipelineBuilder` class can also be used to quickly implement custom XAML brushes with an arbitrary effects pipeline. To do so, just inherit from `XamlCompositionEffectBrushBase` and setup your own effects pipeline in the `OnBrushRequested` method.

#### Get a custom effect that can be animated:

```C#
// Build the effects pipeline
XamlCompositionBrush acrylic = PipelineBuilder.FromHostBackdropAcrylic(Colors.Orange, 0.6f, new Uri("ms-appx:///Assets/noise.png"))
                                              .Saturation(1, out EffectAnimation animation)
                                              .AsBrush();
acrylic.Bind(animation, out XamlEffectAnimation saturation); // Bind the effect animation to the target brush

// Later on, when needed
saturation(0.2f, 250); // Animate the opacity to 0.2 in 250ms
```

# Misc

Many utility methods are also available, here are some useful classes:
- `DispatcherHelper`: exposes methods to easily execute code on the UI thread or on a target `CoreDispatcher` object
- `Win2DImageHelper`: exposes APIs to quickly load a Win2D image on a `CompositionSurfaceBrush` object
- `PointerHelper`: exposes APIs to quickly setup pointer event handlers for `UIElement`s
- `AsyncMutex`: an async mutex included into `System.Threading.Tasks` that can be used to asynchronously acquire a lock with a `using` block.

# Requirements
At least Windows 10 April Update (17134.x)
