# UICompositionAnimations

A complete library that contains animations from the UI.Composition namespace (explicit and implicit animations, Win2D effects and more) as well as XAML transform animations.

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
AttachedAnimatableCompositionEffect<Border> attached = await MyBorder.GetAttachedAnimatableBlurEffectAsync(
  14f, // The amount of blur to apply when the effect is enabled
  0f, // The default amount of blur
  false); // Indicates whether or not to immediately apply the effect to the target amount

// Later on, when needed
await attached.AnimateAsync(
  FixedAnimationType.In, // Indicates whether to fade the blur effect in or out
  TimeSpan.FromMilliseconds(500)); // The animation duration
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
