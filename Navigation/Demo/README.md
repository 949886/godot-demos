# UI Navigation Demo

This demo showcases the Godot 4 UI Navigation system, adapted from the Unity version. It demonstrates various navigation patterns and features.

## Features Demonstrated

### 1. Basic Navigation
- **Main Menu** → **Gameplay** → **Settings** → **Credits**
- Stack-based navigation with back functionality
- ESC key support for going back

### 2. Modal Dialogs
- **Pause Menu**: Modal dialog that blocks the background
- **Confirmation Dialogs**: Simple modal notifications
- Modal masks with click-to-dismiss functionality

### 3. Data Passing
- Score passing between gameplay and pause screens
- Player data sharing between screens
- Callback-based data initialization

### 4. Navigation Patterns
- `Push<T>()`: Add new screen to stack
- `Pop()`: Remove top screen
- `PopToRoot()`: Return to main menu
- `ShowModal<T>()`: Display modal dialog
- `PushReplacement<T>()`: Replace current screen

## How to Run the Demo

1. **Create a new scene** in Godot
2. **Add a Node** as the root
3. **Attach the `NavigationDemo.cs` script** to the root node
4. **Run the scene**

## Demo Screens

### Main Menu
- Play Game: Navigate to gameplay screen
- Settings: Open settings menu
- Credits: View credits
- Quit: Exit application

### Gameplay Screen
- Add Score: Increment score counter
- Pause Game: Open modal pause menu
- Open Inventory: Navigate to inventory
- Back to Menu: Return to main menu with score

### Settings Screen
- Volume slider
- Fullscreen toggle
- Graphics quality selection
- Save settings with confirmation modal

### Pause Menu (Modal)
- Resume: Close modal and continue
- Restart: Return to main menu and start new game
- Quit to Menu: Return to main menu

### Inventory Screen
- List of items
- Use selected item with confirmation
- Shows player score from gameplay

## Code Examples

### Basic Navigation
```csharp
// Navigate to a new screen
Navigator.Push<GameplayWidget>();

// Go back
Navigator.Pop();

// Return to main menu
Navigator.PopToRoot();
```

### Modal Dialogs
```csharp
// Show a modal dialog
Navigator.ShowModal<PauseWidget>(pauseWidget => 
{
    pauseWidget.SetScore(currentScore);
});
```

### Data Passing
```csharp
// Pass data when pushing a screen
Navigator.Push<InventoryWidget>(inventoryWidget => 
{
    inventoryWidget.SetPlayerScore(score);
});

// Pass data when popping
Navigator.PopToRoot(finalScore);
```

## Navigation System Features

- **Type-safe navigation** with generic constraints
- **Stack-based management** of UI screens
- **Modal dialog support** with background blocking
- **Data passing** between screens
- **Event system** for navigation callbacks
- **Multiple deactivation modes** (Visible, Process, Disabled)
- **Automatic cleanup** of removed screens

## Architecture

The navigation system consists of:

- `Navigator`: Main navigation controller
- `Widget`: Base class for all UI screens
- `Route`: Navigation route with data passing
- `ModalRoute`: Specialized route for modal dialogs
- `ModalMask`: Background overlay for modals

## Customization

You can customize the navigation behavior by:

- Changing `Navigator.deactivateMode` to control how inactive screens are handled
- Setting `escToPop` to enable/disable ESC key navigation
- Subscribing to `onPushed` and `onPopped` events for custom logic
- Creating custom widget classes that inherit from `Widget` 