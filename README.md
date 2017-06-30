# UICompositionAnimations

A complete library that contains animations from the `UI.Composition` namespace (explicit and implicit animations, Win2D effects like blur, saturation, host backdrop blur effect and more) as well as XAML transform animations.

<a href="https://www.nuget.org/packages/UICompositionAnimations/"><img src="http://i.pi.gy/r8Wr.png" alt="Get it from NuGet" width='280' /></a>

## Used by

| [**OneLocker**](https://www.microsoft.com/store/apps/9nblggh3t7g3?cid=UICompositionAnimations) | [**Brainf\*ck#**](https://www.microsoft.com/store/apps/9nblgggzhvq5) |
| ------ | --- |
| <img src="http://i.pi.gy/Vo5k.png" alt="OneLocker screens" width='564'/> | <img src="http://i.pi.gy/B0go.png" alt="IDE" width='280'/> |


## Introduction

The library exposes three main classes: `CompositionExtensions`, `XAMLTransformExtensions` and `AttachedCompositionEffectsFactory`, along with other helper classes.
Most of the animation methods are available in two versions: a synchronous one that takes an optional callback `Action`, and an asynchronous one that returns a `Task` that completes when the animation ends.

## Examples

Synchronous fade animation
```C#
MyControl.StartCompositionFadeAnimation(
  null, // Using null will make the fade animation start from the current value
  1, // End opacity
  200, // Duration in ms
  null, // Optional delay in ms
  EasingFunctionNames.CircleEaseOut, // Easing function,
  () => Foo()); // Optional callback
```

Asynchronous fade and scale animation
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

Get an attached blur effect that can be animated (using composition and Win2D effects):
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

Get a custom acrylic brush effect:
```C#
AttachedStaticCompositionEffect<Border> attached = await BlurBorder.AttachCompositionInAppCustomAcrylicEffectAsync(
  BlurBorder, // The target host control for the effect visual (can be the same as the source)
  8, // The amount of blur to apply
  800, // The milliseconds to initially apply the blur effect with an automatic animation
  Color.FromArgb(byte.MaxValue, 0x1B, 0x1B, 0x1B), // The tint overlay color
  0.8f, // The ratio of tint overlay over the source effect (the strength of the tint effect)
  null, // Use the default saturation value for the effect (1)
  Win2DCanvas, // A CanvasControl in the current visual tree, used to render parts of the acrylic brush
  new Uri("ms-appx:///Assets/Misc/noise.png")); // A Uri to a custom noise texture to use to create the effect
```

ColorBrush animation
```C#
MyBrush.AnimateColor(
  #FFFF2B1C, // Target color
  250, // Duration in ms
  EasingFunctionNames.Linear); // Easing function
```

Many utility methods are also available. The `XAMLTransformToolkit` class for example, exposes methods to manually create, start and wait for `DoubleAnimation`(s) and `Storyboard`(s), as well as for quickly assigning a certain `RenderTransform` object to a `UIElement`.

## Requirements
At least Windows 10 Anniversary Update (14393.x)
