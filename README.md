# UICompositionAnimations

The **UICompositionAnimations** library exposes classes and APIs to quickly implement animations and effects to a UWP application.
It also has a collection of helper methods to load Win2D images, dispatch code to the UI thread and more.

<a href="https://www.nuget.org/packages/UICompositionAnimations/"><img src="http://i.pi.gy/r8Wr.png" alt="Get it from NuGet" width='280' /></a>

## Used by

| [**OneLocker**](https://www.microsoft.com/store/apps/9nblggh3t7g3?cid=UICompositionAnimations) | [**myTube!**](https://www.microsoft.com/store/apps/9wzdncrcwf3l?cid=UICompositionAnimations) | [**Brainf\*ck#**](https://www.microsoft.com/store/apps/9nblgggzhvq5?cid=UICompositionAnimations) |
| --- | --- | --- |
| <img src="http://i.pi.gy/7mn2.png" alt="OneLocker screens" width='280'/> | <img src="http://i.pi.gy/DVpK.png" alt="IDE" width='280'/> | <img src="http://i.pi.gy/k83G.png" alt="IDE" width='280'/> |

# Table of Contents

- [Installing from NuGet](#installing-from-nuget)
- [Quick start](#quick-start)
  - [Animations](#animations) 
  - [`UI.Composition` effects](#uicomposition-effects)
  - [Reveal highlight effect](#reveal-highlight-effect)
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

The package exposes synchronous and asynchronous APIs (from the `CompositionExtensions` and `XAMLTransformExtensions` classes) to quickly setup and start animations. There are different kinds of available animation types to choose from:
- **Fade** (Opacity)
- **Slide** (Translation)
- **Scale**
- **Color** (for a `SolidColorBrush` object)

The library also has APIs to automatically combine different kinds of animations, like Fade + Slide or Fade + Scale, and various helper methods to change UI-related parameters of a target object. Here are some animation exaples:

#### Synchronous fade animation
```C#
MyControl.StartCompositionFadeAnimation(
  null, // Using null will make the fade animation start from the current value
  1, // End opacity
  200, // Duration in ms
  null, // Optional delay in ms
  EasingFunctionNames.CircleEaseOut, // Easing function,
  () => Foo()); // Optional callback
```

#### Asynchronous fade and scale animation
```C#
await MyControl.StartCompositionFadeScaleAnimationAsync(
  null, // Initial opacity (use current value)
  1, // Target opacity
  1.1f, // Initial scale
  1, // End scale
  250, // Animation duration
  null, // Optional scale animation duration (if null, the base animation duration will be used)
  null, // Optional delay
  EasingFunctionNames.Linear); // Easing function
```

## `UI.Composition` effects

The library provides several ways to use `UI.Composition` effects. There are ready to use XAML brushes, a `CompositionBrushBuilder` class to create complex composition effect pipelines, an `AttachedCompositionEffectsFactory` class that provides an alternative way to attach commonly used effects to visual elements, and much more.

#### Declare a shared acrylic brush in XAML

```XAML
<ResourceDictionary
  ...
  xmlns:brushes="using:UICompositionAnimations.Brushes">
  
  <!--The acrylic brush to use in the app-->
  <brushes:CustomAcrylicBrush
      x:Key="InAppGrayAcrylicBrush"
      Mode="HostBackdrop"
      BlurAmount="8"
      Tint="#FF222222"
      TintMix="0.6"
      NoiseTextureUri="/Assets/Misc/noise.png"/>
  ...
</ResourceDictionary/>
```

**Note**: the `NoiseTextureUri` parameter must be set to a .png image with a noise texture. It is up to the developer to create his own noise texture and to import it into the app. An easy plugin to create one is [NoiseChoice](https://forums.getpaint.net/topic/22500-red-ochre-plug-in-pack-v9-updated-30th-july-2014/) for [Paint.NET](https://www.getpaint.net/).

#### Create and assign an acrylic brush in C#
```C#
control.Background = CompositionBrushBuilder.FromHostBackdropAcrylic(Colors.DarkOrange, 0.6f, new Uri("ms-appx:///Assets/noise.png")).AsBrush();
```

#### Build an acrylic effect pipeline from scratch:
```C#
Brush brush = CompositionBrushBuilder.FromHostBackdropBrush()
    .Effect(source => new LuminanceToAlphaEffect { Source = source })
    .Opacity(0.4f)
    .Blend(CompositionBrushBuilder.FromHostBackdropBrush(), BlendEffectMode.Multiply)
    .Tint(Color.FromArgb(0xFF, 0x14, 0x14, 0x14), 0.8f)
    .Blend(CompositionBrushBuilder.FromTiles(new Uri("ms-appx:///Assets/noise.png")), BlendEffectMode.Overlay, EffectPlacement.Background)
    .AsBrush();
```

The `CompositionBrushBuilder` class can also be used to quickly implement custom XAML brushes with an arbitrary effects pipeline. To do so, just inherit from `XamlCompositionEffectBrushBase` and setup your own effects pipeline in the `OnBrushRequested` method.

#### Get a custom effect that can be animated:
```C#
// Build the effects pipeline
XamlCompositionBrush acrylic = CompositionBrushBuilder.FromHostBackdropAcrylic(Colors.Orange, 0.6f, new Uri("ms-appx:///Assets/noise.png"))
    .Saturation(1, out EffectAnimation animation)
    .AsBrush();
acrylic.Bind(animation, out XamlEffectAnimation saturation); // Bind the effect animation to the target brush

// Later on, when needed
saturation(0.2f, 250); // Animate the opacity to 0.2 in 250ms
```

## Reveal highlight effect

Part of the Fluent Design System introduced with Windows 10 Fall Creators Update, this effect can actually already be used with Windows 10 Creators Update (build 15063.x). The library exposes APIs to easily use the effect in an application.

#### Setup the lights

```C#
// In App.xaml.cs, before loading the application UI
LightsSourceHelper.Initialize(
    () => new PointerPositionSpotLight { Shade = 0x60 }, // Example light
    () => new PointerPositionSpotLight
    {
        IdAppendage = "[Wide]", // This ID is used to specify the target brushes for the specific light
        Z = 30,
        Shade = 0x10
    }); // It is possible to add an arbitrary number of lights here
DefaultContent = new Shell();
LightsSourceHelper.SetIsLightsContainer(Window.Current.Content, true); // Assigns the lights to the app main UI
```

#### Setup the target brushes for the lights
````XAML
<ResourceDictionary
    ...
    xmlns:brushes="using:UICompositionAnimations.Brushes"
    xmlns:lights="using:UICompositionAnimations.Lights">
    <brushes:LightingBrush
            x:Key="BorderLightBrush"
            lights:PointerPositionSpotLight.IsTarget="True"/>
    <brushes:LightingBrush x:Key="BorderWideLightBrush"/>
    ...
</ResourceDictionary/>
````
```C#
// Since the second light has a special ID, it is necessary to manually set its target brush
LightingBrush brush = Application.Current.Resources["BorderWideLightBrush"] as LightingBrush;
XamlLight.AddTargetBrush($"{PointerPositionSpotLight.GetIdStatic()}[Wide]", brush);
```

At this point we have a series of lights, each targeting an arbitrary number of brushes. The library takes care of managing the pointer events for the lights as well. It is now possible to use those brushes in any `UIElement` to see the reveal highlight effect in action.

**Note**: the lights only work on `UIElement`s in their visual tree. This means that in order for the `LightingBrush` objects to work correctly in popups or flyouts, the lights must be added to their visual tree too. To do this, just call the `LightsSourceHelper.SetIsLightsContainer` method on the root element of the new object being displayed (for example, the root `Grid` inside a new `Popup`).

# Misc

Many utility methods are also available, here are some useful classes:
- `XAMLTransformToolkit`: exposes methods to manually create, start and wait for `DoubleAnimation`(s) and `Storyboard`(s), as well as for quickly assigning a certain `RenderTransform` object to a `UIElement`.
- `DispatcherHelper`: exposes methods to easily execute code on the UI thread or on a target `CoreDispatcher` object
- `Win2DImageHelper`: exposes APIs to quickly load a Win2D image on a `CompositionSurfaceBrush` object
- `PointerHelper`: exposes APIs to quickly setup pointer event handlers for `UIElement`s

# Requirements
At least Windows 10 April Update (17134.x)
