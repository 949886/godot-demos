using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace Navigation
{
    public partial class Navigator : Node
    {
        // Singleton instance for easy access
        public static Navigator Instance;
        
        private static readonly Stack<Navigator> _navigatorStack = new();
        
        // Reference to the root canvas. If not found, a new canvas will be created.
        public CanvasLayer canvasLayer;
        // The root widget of the navigator
        public Control rootWidget; 
        
        // Focus on last selected object when a new widget is popped
        public bool focusAutomatically = true; 
        // Pop the top widget when the escape key is pressed
        public bool escToPop = false; 
        
        public static DeactivateMode deactivateMode = DeactivateMode.Visible;
        
        private readonly Stack<Widget> _widgetStack = new();
        private readonly Stack<Route> _routeStack = new();
        
        private bool _isDontDestroyOnLoad = false;
        
        public static Control RootWidget
        {
            get => Instance.rootWidget;
            set
            {
                if (Instance.rootWidget == null)
                {
                    Instance.rootWidget = value;
                    var widget = value.GetNodeOrNull<Widget>("Widget") ?? value.GetNodeOrNull<Widget>(".");
                    if (widget == null)
                    {
                        widget = new Widget();
                        widget.Name = "Widget";
                        value.AddChild(widget);
                    }
                    Instance._widgetStack.Push(widget);
                }
                else GD.PrintErr("[Navigator] Root widget is already set. Use a different instance of Navigator if you need multiple root widgets.");
            }
        }
        
        public Widget TopWidget => _widgetStack.Count > 0 ? _widgetStack.Peek() : null;
        
        public Route PreviousRoute => _routeStack.Count > 1 ? _routeStack.Peek() : null;
        
        public event Action<Route> onPushed;
        public event Action<Route> onPopped;

        public static Navigator Create(Control rootWidget)
        {
            var navigator = new Navigator();
            navigator.Name = "UI Navigator";
            navigator.rootWidget = rootWidget;
            return navigator;
        }

        public override void _Ready()
        {
            foreach (var navigator in _navigatorStack)
                navigator.ProcessMode = ProcessModeEnum.Disabled;
            
            Instance = this;
            _navigatorStack.Push(this);
            
            if (canvasLayer == null)
            {
                // Create a new canvas layer if none is found
                canvasLayer = GetNodeOrNull<CanvasLayer>("CanvasLayer");
                if (canvasLayer == null)
                {
                    canvasLayer = new CanvasLayer();
                    canvasLayer.Name = "CanvasLayer";
                    AddChild(canvasLayer);
                }
            }
            
            // Load the rootWidget
            if (rootWidget != null)
            {
                if (rootWidget.Visible)
                {
                    var widget = rootWidget.GetNodeOrNull<Widget>("Widget") ?? rootWidget.GetNodeOrNull<Widget>(".");
                    if (widget == null)
                    {
                        widget = new Widget();
                        widget.Name = "Widget";
                        rootWidget.AddChild(widget);
                    }
                    _widgetStack.Push(widget);
                }
                else Push(rootWidget);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (escToPop && @event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                Pop();
            }
        }

        public override void _ExitTree()
        {
            _navigatorStack.Pop();
            if (_navigatorStack.Count > 0)
            {
                var navigator = _navigatorStack.Peek();
                navigator.ProcessMode = ProcessModeEnum.Inherit;
                Instance = navigator;
            }
        }
        
        public void Destroy()
        {
            QueueFree();
        }
        
        public static Task<dynamic> Push(Widget widget, Action callback = null)
        {
            return Instance._Push(widget, callback);
        }
        
        public static Task<dynamic> Push(Control widgetPrefab, Action<Control> callback = null)
        {
            return Instance._Push(widgetPrefab, callback);
        }
        
        /// Push a widget to the top of the stack.
        /// The widget will be instantiated and added to the canvas. <br/>
        ///
        ///  - [T] The type of the widget to be pushed.
        ///     Prefab with Widget component will be registered in the Widgets prefab database automatically.
        ///     Note: A widget type should only have one prefab that corresponds to it. <br/>
        ///
        ///  - [callback] A callback to be executed after the widget is instantiated.
        ///     You can use this callback to pass data to the widget.
        ///     Note: The callback will be executed before the widget is enabled.
        ///     Therefore, you should initialize the widget in _Ready method.
        public static Task<dynamic> Push<T>(Action<T> callback = null) where T : Widget, new()
        {
            return Instance._Push<T>(callback);
        }
        
        public static Task<dynamic> Push<T>(bool keepSelectionOnPop, Action<T> callback = null) where T : Widget, new()
        {
            return Instance._Push<T>(callback, keepSelectionOnPop);
        }
        
        /// Push a widget to replace the top widget in the stack. <br/>
        public static Task<dynamic> PushReplacement<T>(Action<T> callback = null) where T : Widget, new()
        {
            return Instance._PushReplacement(callback);
        }
        

        /// Pop the top widget from the stack.
        public static void Pop()
        {
            Instance._Pop(0);
        }
        
        /// Pop the top widget from the stack with a result. <br/>
        ///
        ///  - [T] The type of the result to be passed to the previous widget.
        /// 
        ///  - [result] The result to be passed to the previous widget.
        public static void Pop<T>(T result = default)
        {
            Instance._Pop(result);
        }
        
        /// Pop all widgets until the root widget. <br/>
        /// Returns the root widget.
        public static void PopToRoot()
        {
            Instance._PopToRoot(0);
        }
        
        /// Pop all widgets until the root widget. <br/>
        /// Returns the root widget.
        public static void PopToRoot<T>(T result = default)
        {
            Instance._PopToRoot(result);
        }
        
        /// Pop widgets until the top widget is of type T. <br/>
        ///
        ///  - [T] The type of the target widget.
        /// 
        public static void PopUntil<T>() where T : Widget, new()
        {
            Instance._PopUntil<T, int>(0);
        }
        
        
        /// Pop widgets until the top widget is of type T. <br/>
        /// Return the top widget of type T. <br/>
        public static T BackTo<T>() where T : Widget, new()
        {
            Instance._PopUntil<T, int>(0);
            return Instance.TopWidget as T;
        }
        
        /// Pop widgets until the top widget is of type T. <br/>
        /// Return the top widget of type T. <br/>
        public static void PopUntil<T, U>(U result = default) where T : Widget, new()
        {
            Instance._PopUntil<T, U>(result);
        }
        
        /// Pop widgets until the top widget is of type T. <br/>
        /// Return the top widget of type T. <br/>
        public static T BackTo<T, U>(U result = default) where T : Widget, new()
        {
            Instance._PopUntil<T, U>(result);
            return Instance.TopWidget as T;
        }
        
        /// Show a modal widget. <br/>
        public static Task<dynamic> ShowModal<T>(Action<T> builder = null, bool maskDismissible = true, Color? maskColor = null, Vector2 offset = default) where T : Widget, new()
        {
            return Instance._ShowModal<T>(builder, maskDismissible, maskColor, offset);
        }
        
        public static Widget GetPrevious()
        {
            return Instance._GetPrevious();
        }
        
        public static T GetPrevious<T>() where T : Widget, new()
        {
            return Instance._GetPrevious<T>();
        }
        
        internal Task<dynamic> _Push(Widget widget, Action callback = null)
        {
            var route = new Route(widget);
            _routeStack.Push(route);
            
            if (TopWidget != null)
            {
                TopWidget.SetNaviActive(false);
            }
            
            _widgetStack.Push(widget);
            widget.SetNaviActive(true);
            
            callback?.Invoke();
            onPushed?.Invoke(route);
            
            return route.Popped;
        }
        
        internal Task<dynamic> _Push(Control widgetPrefab, Action<Control> callback = null)
        {
            var instantiatedWidget = widgetPrefab.Duplicate() as Control;
            if (instantiatedWidget == null)
            {
                GD.PrintErr("[Navigator] Failed to instantiate widget prefab");
                return Task.FromResult<dynamic>(null);
            }
            
            canvasLayer.AddChild(instantiatedWidget);
            
            var widget = instantiatedWidget.GetNodeOrNull<Widget>("Widget") ?? instantiatedWidget.GetNodeOrNull<Widget>(".");
            if (widget == null)
            {
                widget = new Widget();
                widget.Name = "Widget";
                instantiatedWidget.AddChild(widget);
            }
            
            callback?.Invoke(instantiatedWidget);
            return _Push(widget);
        }
        
        internal async Task<dynamic> _Push<T>(Action<T> callback = null, bool keepSelectionOnPop = true, bool hidePrevious = true) where T : Widget, new()
        {
            var route = new Route<T>(callback);
            _routeStack.Push(route);
            
            if (TopWidget != null && hidePrevious)
            {
                TopWidget.SetNaviActive(false);
            }
            
            var widget = route.To;
            _widgetStack.Push(widget);
            widget.SetNaviActive(true);
            
            route.OnPush();
            onPushed?.Invoke(route);
            
            return await route.Popped;
        }
        
        internal Task<dynamic> _PushReplacement<T>(Action<T> callback = null) where T : Widget, new()
        {
            if (_widgetStack.Count > 0)
            {
                var topWidget = _widgetStack.Pop();
                topWidget.SetNaviActive(false);
                topWidget.QueueFree();
            }
            
            return _Push<T>(callback);
        }
        
        internal void _Pop<T>(T result = default)
        {
            if (_widgetStack.Count <= 1)
            {
                GD.PrintErr("[Navigator] Cannot pop root widget");
                return;
            }
            
            var widget = _widgetStack.Pop();
            widget.SetNaviActive(false);
            widget.QueueFree();
            
            var route = _routeStack.Pop();
            route.popCompleter.SetResult(result);
            route.OnPop();
            
            if (TopWidget != null)
            {
                TopWidget.SetNaviActive(true);
            }
            
            onPopped?.Invoke(route);
        }
        
        internal void _PopToRoot<T>(T result = default)
        {
            while (_widgetStack.Count > 1)
            {
                var widget = _widgetStack.Pop();
                widget.SetNaviActive(false);
                widget.QueueFree();
                
                var route = _routeStack.Pop();
                route.popCompleter.SetResult(result);
                route.OnPop();
            }
            
            if (TopWidget != null)
            {
                TopWidget.SetNaviActive(true);
            }
        }
        
        internal void _PopUntil<T, U>(U result = default) where T : Widget, new()
        {
            while (_widgetStack.Count > 1 && !(TopWidget is T))
            {
                var widget = _widgetStack.Pop();
                widget.SetNaviActive(false);
                widget.QueueFree();
                
                var route = _routeStack.Pop();
                route.popCompleter.SetResult(result);
                route.OnPop();
            }
            
            if (TopWidget != null)
            {
                TopWidget.SetNaviActive(true);
            }
        }
        
        internal Task<dynamic> _ShowModal<T>(Action<T> builder = null, bool maskDismissible = true, Color? maskColor = null, Vector2 offset = default) where T : Widget, new()
        {
            var route = new ModalRoute<T>(builder, maskDismissible, maskColor, offset);
            return _Push<T>(route.callback);
        }
        
        internal Widget _GetPrevious()
        {
            return _widgetStack.Count > 1 ? _widgetStack.ToArray()[_widgetStack.Count - 2] : null;
        }
        
        internal T _GetPrevious<T>() where T : Widget, new()
        {
            var previous = _GetPrevious();
            return previous as T;
        }
        
        public enum DeactivateMode
        {
            Visible,     // Use `Visible` to deactivate the widget
            Process,     // Use `ProcessMode` to deactivate the widget
            Disabled     // Use `Disabled` to deactivate the widget
        }
    }
    
    public static class NavigatorExtensions
    {
        public static Task<dynamic> Push(this Navigator navigator, Widget widget, Action callback = null) => navigator._Push(widget, callback);
        public static Task<dynamic> Push(this Navigator navigator, Control widgetPrefab, Action<Control> callback = null) => navigator._Push(widgetPrefab, callback);
        public static Task<dynamic> Push<T>(this Navigator navigator, Action<T> callback = null) where T : Widget, new() => navigator._Push<T>(callback);
        public static Task<dynamic> Push<T>(this Navigator navigator, bool keepSelectionOnPop, Action<T> callback = null) where T : Widget, new() => navigator._Push<T>(callback, keepSelectionOnPop);
        public static Task<dynamic> PushReplacement<T>(this Navigator navigator, Action<T> callback = null) where T : Widget, new() => navigator._PushReplacement(callback);
        public static void Pop(this Navigator navigator) => navigator._Pop(0);
        public static void Pop<T>(this Navigator navigator, T result = default) => navigator._Pop(result);
        public static void PopToRoot(this Navigator navigator) => navigator._PopToRoot(0);
        public static void PopToRoot<T>(this Navigator navigator, T result = default) => navigator._PopToRoot(result);
        public static void PopUntil<T>(this Navigator navigator) where T : Widget, new() => navigator._PopUntil<T, int>(0);
        public static void PopUntil<T, U>(this Navigator navigator, U result = default) where T : Widget, new() => navigator._PopUntil<T, U>(result);
    }
    
    public partial class Widget : Control
    {
        internal void SetNaviActive(bool active)
        {
            switch (Navigator.deactivateMode)
            {
                case Navigator.DeactivateMode.Visible:
                    Visible = active;
                    break;
                case Navigator.DeactivateMode.Process:
                    ProcessMode = active ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
                    break;
                case Navigator.DeactivateMode.Disabled:
                    SetProcess(active);
                    SetPhysicsProcess(active);
                    break;
            }
        }
        
        public virtual void OnPush() {}
        public virtual void OnPop() {}
        
        public static T New<T>(bool addToScene = true) where T : Widget, new()
        {
            var widget = new T();
            if (addToScene && Navigator.Instance != null)
            {
                Navigator.Instance.canvasLayer.AddChild(widget);
            }
            return widget;
        }
    }
} 