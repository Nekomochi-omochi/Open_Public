using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using Boxelle.Services;
using System.Windows.Controls.Primitives;
using Controls.Resizer;

namespace Boxelle;

public partial class MainWindow : Window
{
    private double _previousWidth;
    private double _previousHeight;
    private double _previousLeft;
    private double _previousTop;
    private bool _isFullWindow = false;
    private readonly Dictionary<ResizableWidgetContainer, Action> _resizeSavers;

    public MainWindow()
    {
        InitializeComponent();

        _positionSavers = new()
        {
            [ClockContainer] = (left, top) =>
                SaveJson.SaveClockWidget(
                    GetSafeLeft(ClockContainer),
                    GetSafeTop(ClockContainer),
                    ClockContainer.ActualWidth,
                    ClockContainer.ActualHeight),

            [TimerContainer] = (left, top) =>
                SaveJson.SaveTimerWidget(
                    GetSafeLeft(TimerContainer),
                    GetSafeTop(TimerContainer),
                    TimerContainer.ActualWidth,
                    TimerContainer.ActualHeight),

            [MemoContainer] = (left, top) =>
                SaveJson.SaveMemoWidget(
                    GetSafeLeft(MemoContainer),
                    GetSafeTop(MemoContainer),
                    MemoContainer.ActualWidth,
                    MemoContainer.ActualHeight),

            [TodoContainer] = (left, top) =>
                SaveJson.SaveTodoWidget(
                    GetSafeLeft(TodoContainer),
                    GetSafeTop(TodoContainer),
                    TodoContainer.ActualWidth,
                    TodoContainer.ActualHeight)
        };

        _resizeSavers = new()
        {
            [ClockContainer] = () =>
                SaveJson.SaveClockWidget(
                    Canvas.GetLeft(ClockContainer),
                    Canvas.GetTop(ClockContainer),
                    ClockContainer.Width,
                    ClockContainer.Height),

            [TimerContainer] = () =>
                SaveJson.SaveTimerWidget(
                    Canvas.GetLeft(TimerContainer),
                    Canvas.GetTop(TimerContainer),
                    TimerContainer.Width,
                    TimerContainer.Height),

            [MemoContainer] = () =>
                SaveJson.SaveMemoWidget(
                    Canvas.GetLeft(MemoContainer),
                    Canvas.GetTop(MemoContainer),
                    MemoContainer.ActualWidth,
                    MemoContainer.ActualHeight),

            [TodoContainer] = () =>
                SaveJson.SaveTodoWidget(
                    Canvas.GetLeft(TodoContainer),
                    Canvas.GetTop(TodoContainer),
                    TodoContainer.ActualWidth,
                    TodoContainer.ActualHeight)
        };

        // JSON読み込み
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
    
    // raised when mouse cursor enters the area occupied by the element
    void OnMouseEnterHandler(object sender, MouseEventArgs e)
    {
        border2.Opacity = 1;
        border2.IsHitTestVisible = true;
    }

    // raised when mouse cursor leaves the area occupied by the element
    void OnMouseLeaveHandler(object sender, MouseEventArgs e)
    {
        border2.Opacity = 0;
        border2.IsHitTestVisible = false;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isFullWindow)
        {
            // 現在の状態を記憶
            _previousWidth = this.Width;
            _previousHeight = this.Height;
            _previousLeft = this.Left;
            _previousTop = this.Top;

            // タスクバーを除いた作業領域のサイズを取得して設定
            this.Left = SystemParameters.WorkArea.Left;
            this.Top = SystemParameters.WorkArea.Top;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;

            _isFullWindow = true;
        }
        else
        {
            // 元のサイズに戻す
            this.Left = _previousLeft;
            this.Top = _previousTop;
            this.Width = _previousWidth;
            this.Height = _previousHeight;

            _isFullWindow = false;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close(); // ウィンドウを閉じる
    }

    private bool WouldOverlap(
    FrameworkElement movingElement,
    double proposedLeft,
    double proposedTop)
    {
        Rect proposedRect = new Rect(
            proposedLeft,
            proposedTop,
            movingElement.ActualWidth,
            movingElement.ActualHeight);

        FrameworkElement[] elements =
        {
                    ClockContainer,
            TimerContainer,
            MemoContainer,
            TodoContainer
        };

        foreach (FrameworkElement element in elements)
        {
            if (element == movingElement)
            {
                continue;
            }

            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);

            Rect otherRect = new Rect(
                left,
                top,
                element.ActualWidth,
                element.ActualHeight);

            if (proposedRect.IntersectsWith(otherRect))
            {
                return true;
            }
        }

        return false;
    }

    private Point ClampToCanvas(
        FrameworkElement element,
        double proposedLeft,
        double proposedTop)
    {
        double minLeft = MarginThreshold;
        double minTop = MarginThreshold;

        double maxLeft = Math.Max(
            minLeft,
            DragCanvas.ActualWidth
            - element.ActualWidth
            - MarginThreshold);

        double maxTop = Math.Max(
            minTop,
            DragCanvas.ActualHeight
            - element.ActualHeight
            - MarginThreshold);

        double left = Math.Clamp(
            proposedLeft,
            minLeft,
            maxLeft);

        double top = Math.Clamp(
            proposedTop,
            minTop,
            maxTop);

        return new Point(left, top);
    }


    private void WidgetContainer_ResizeCompleted(
    object? sender,
    EventArgs e)
    {
        if (sender is not ResizableWidgetContainer container)
        {
            return;
        }

        if (IsOverlapping(container))
        {
            container.RestorePreviousSize();
            return;
        }

        if (_resizeSavers.TryGetValue(container, out Action? save))
        {
            save();
        }
    }


    private bool IsOverlapping(FrameworkElement target)
    {
        double targetLeft = Canvas.GetLeft(target);
        double targetTop = Canvas.GetTop(target);

        Rect targetRect = new(
            targetLeft,
            targetTop,
            target.ActualWidth,
            target.ActualHeight);

        FrameworkElement[] elements =
        {
            ClockContainer,
            TimerContainer,
            MemoContainer,
            TodoContainer
        };

        foreach (FrameworkElement element in elements)
        {
            if (element == target)
            {
                continue;
            }

            Rect otherRect = new(
                Canvas.GetLeft(element),
                Canvas.GetTop(element),
                element.ActualWidth,
                element.ActualHeight);

            if (targetRect.IntersectsWith(otherRect))
            {
                return true;
            }
        }

        return false;
    }

    private static double GetSafeLeft(FrameworkElement element)
    {
        double left = Canvas.GetLeft(element);
        return double.IsFinite(left) ? left : 0;
    }

    private static double GetSafeTop(FrameworkElement element)
    {
        double top = Canvas.GetTop(element);
        return double.IsFinite(top) ? top : 0;
    }

    private static double SafeNumber(double value, double fallback = 0)
    {
        return double.IsFinite(value) ? value : fallback;
    }
}