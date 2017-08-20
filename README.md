# UICompositionAnimations

The **UICompositionAnimations** library exposes classes and APIs to quickly implement animations and effects to a UWP application.
It also has a collection of helper methods to load Win2D images, dispatch code to the UI thread and more.

<a href="https://www.nuget.org/packages/UICompositionAnimations/"><img src="http://i.pi.gy/r8Wr.png" alt="Get it from NuGet" width='280' /></a>

## Used by

| [**OneLocker**](https://www.microsoft.com/store/apps/9nblggh3t7g3?cid=UICompositionAnimations) | [**Brainf\*ck#**](https://www.microsoft.com/store/apps/9nblgggzhvq5) |
| ------ | --- |
| <img src="http://i.pi.gy/Vo5k.png" alt="OneLocker screens" width='564'/> | <img src="http://i.pi.gy/B0go.png" alt="IDE" width='280'/> |

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

#### ColorBrush animation
```C#
MyBrush.AnimateColor(
  #FFFF2B1C, // Target color
  250, // Duration in ms
  EasingFunctionNames.Linear); // Easing function
```

## `UI.Composition` effects

The library provides several ways to use `UI.Composition` effects. There's a custom acrylic brush that can be used when running Windows 10 build 15063.x or greater, and other "attached" effects. An attached effect (created using the `AttachedCompositionEffectsFactory` class) is an effect that is loaded and then applied to the underlying `Visual` object behing a target `UIElement`. The main advantage of brushes is that they can be initialized and used in XAML and don't need any code-behind. Here are some examples:

#### Declare a shared acrylic brush in XAML

```XAML
<ResourceDictionary
  ...
  xmlns:brushes="using:UICompositionAnimations.Brushes">
  
  <!--The acrylic brush to use in the app-->
  <brushes:CustomAcrylicBrush
      x:Key="InAppGrayAcrylicBrush"
      Mode="InAppBlur"
      BlurAmount="8"
      Tint="#FF222222"
      TintMix="0.6"
      NoiseTextureUri="/Assets/Misc/noise.png"/>
  ...
</ResourceDictionary/>
```

**Note**: the `NoiseTextureUri` parameter must be set to a .png image with a noise texture. It is up to the developer to create his own noise texture and to import it into the app. An easy plugin to create a custom noise texture is [NoiseChoice](https://forums.getpaint.net/topic/22500-red-ochre-plug-in-pack-v9-updated-30th-july-2014/) for [Paint.NET](https://www.getpaint.net/).

#### Get a custom acrylic brush effect:
```C#
AttachedStaticCompositionEffect<Border> attached = await BlurBorder.AttachCompositionInAppCustomAcrylicEffectAsync(
  BlurBorder, // The target host control for the effect visual (can be the same as the source)
  8, // The amount of blur to apply
  800, // The milliseconds to initially apply the blur effect with an automatic animation
  Color.FromArgb(byte.MaxValue, 0x1B, 0x1B, 0x1B), // The tint overlay color
  0.8f, // The ratio of tint overlay over the source effect (the strength of the tint effect)
  null, // Use the default saturation value for the effect (1)
  Win2DCanvas, // A CanvasControl in the current visual tree, used to render parts of the acrylic brush
  new Uri("ms-appx:///Assets/Misc/noise.png"), // A Uri to a custom noise texture to use to create the effect
  BitmapCacheMode.EnableCaching, // The cache mode for the Win2D image to load
  false, // Indicates whether to fade the effect it or to display it as soon as possible
  true); // Indicates whether or not to automatically dispose the effect when the target `UIElement` is unloaded
```

**Note**: in order to remove the effect from the target `UIElement`, it is possible to call the `Dispose` method on the returned `AttachedStaticCompositionEffect<T>` object - calling that method will remove the effect from the object `Visual`.

#### Get an attached blur effect that can be animated (using composition and Win2D effects):
```C#
AttachedAnimatableCompositionEffect<Border> attached = await MyBorder.AttachCompositionAnimatableBlurEffectAsync(
  14f, // The amount of blur to apply when the effect is enabled
  0f, // The default amount of blur
  false); // Indicates whether or not to immediately apply the effect to the target amount

// Later on, when needed
await attached.AnimateAsync(
  FixedAnimationType.In, // Indicates whether to fade the blur effect in or out
  TimeSpan.FromMilliseconds(500)); // The animation duration
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

## Misc

Many utility methods are also available, here are some useful classes:
- `XAMLTransformToolkit`: exposes methods to manually create, start and wait for `DoubleAnimation`(s) and `Storyboard`(s), as well as for quickly assigning a certain `RenderTransform` object to a `UIElement`.
- `DispatcherHelper`: exposes methods to easily execute code on the UI thread or on a target `CoreDispatcher` object
- `Win2DImageHelper`: exposes APIs to quickly load a Win2D image on a `CompositionSurfaceBrush` object
- `ApiInformationHelper`: provides useful methods to check the capabilities of the current device
- `PointerHelper`: exposes APIs to quickly setup pointer event handlers for `UIElement`s

## Requirements
At least Windows 10 November Update (10586.x)
