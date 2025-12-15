# CategoricalDebug

If you find yourself constantly commenting out Debug.Log calls, this package is for you.  CategoricalDebug is a logging and assertion system for Unity that allows you to organize, enable, and disable debug messages by categories. Each category can be individually controlled (via editor or code), enabling precise debugging without cluttering the console.

---

## Features

- **Category-based logging**: Assign log messages to specific categories.
- **Conditional compilation**: Logging and assertions only occur when the `EYE_DEBUG` define is active.
- **Dynamic enable/disable**: Enable or disable categories at runtime or via editor settings.
- **Prepend/append messages**: Temporarily prepend or append text to the next log call for contextual debugging.
- **File logging**: Log messages can be written to a file with optional stack traces.
- **Integrated editor UI**: Options to control global settings and category states directly from the Unity Editor.

---

## Installation

You can install this package in Unity via GitHub using the Unity Package Manager.

1. Open Unity and navigate to the **Package Manager** (Window > Package Manager).
2. In the Package Manager window, click on the `+` button in the top-left corner.
3. Select **Add package from Git URL...**.
4. Paste the following GitHub URL into the dialog:
 -  https://github.com/glurth/CategoricalDebug.git
5. Click **Add**. The package will be installed into your Unity project.

---

## Usage

### Basic Category Logging

```
class PhysicsDebug : CategoryLogBase<PhysicsDebug>
{
    protected override string CategoryName => "Physics";
}

// Example usage:
PhysicsDebug.Log("Impulse resolved");
PhysicsDebug.LogWarning("Potential collision issue");
PhysicsDebug.LogError("Physics simulation failure");
```

### General Logging

```
// Logs a message if the debug define is active
CatDebug.Log("This is a general log message");

// Logs to a specific category (by ID or name)
CatDebug.Log("Physics", "Collision detected");
CatDebug.Log(1, "Collision resolved");

// Prepend or append text to next log
CatDebug.PrependToNextLog(1, "Prepended text: ");
CatDebug.AppendToNextLog(1, "Appended text.");
```

### Assertions

```
PhysicsAssert.IsTrue(condition, "Condition failed");
PhysicsAssert.IsNotNull(someObject, "Object should not be null");
PhysicsAssert.AreNotNull("Multiple objects check", obj1, obj2, obj3);
```

### Editor Settings

Access global logging options via the editor:

- **Enabled via Compiler Directive**: Toggle logging for `EYE_DEBUG`.
- **Add Category Name To Log**: Prepend category name to all log messages.
- **Single-line category display**: Control whether category names are single-line.
- **Always show warnings**: Display warnings even for disabled categories.
- **Log to file with stack trace**: Enable writing stack traces to the log file.

You can also enable/disable categories directly in the editor through the `CategoricalDebugOptionsGUI`.

---

## Logging to File

By default, all logs are written to `CategoricalLog.txt` in the project root. Stack traces can optionally be included for non-console logs.

---

## Conditional Compilation

All logging and assertions use the `EYE_DEBUG` define. In Release builds, you can disable this define to remove all debug overhead.

---

## Adding New Categories

Use `DebugCategoryRegistrar.RegisterCategory(string categoryName)` in your code to create a new category. Once registered, you can log to it via `CatDebug` or a custom `CategoryLogBase` class.

---

## Notes

- Warnings can be displayed even if the category is disabled, controlled via the `alwaysShowWarnings` setting.
- Category settings are saved using `PlayerPrefs` and persist between editor sessions.
- Use `CatDebug.PrependToNextLog` and `AppendToNextLog` for temporary contextual messages without modifying the core log text.

---

## Example Category and Assert Classes

```
class PhysicsDebug : CategoryLogBase<PhysicsDebug>
{
    protected override string CategoryName => "Physics";
}

class PhysicsAssert : CategoryAssert<PhysicsAssert>
{
    protected override string CategoryName => "Physics";
}
```

---

## License

This package is licensed under the EyE Dual-Licensing Agreement.

It provides free, perpetual use for indie developers and non-commercial projects whose teams had Total Gross Receipts under $100,000 USD in the previous fiscal year.

Organizations exceeding this threshold must obtain a Perpetual Commercial License (PCL) for each named commercial project.

Please review the full terms in [LICENSE.md](LICENSE.md) before commercial use.

## Contributions

Contributions, issues, and feature requests are welcome! Please submit them via the GitHub repository. Note: Due to licensing, contributions can only be included with explicit written permission from the copyright holder.
