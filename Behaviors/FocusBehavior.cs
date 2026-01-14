using System.Windows;
using System.Windows.Controls;

namespace Base64Utils.Behaviors
{
    public static class FocusBehavior
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && (bool)e.NewValue && control.IsEnabled)
            {
                // Use Dispatcher to ensure the control is loaded and ready
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    control.Focus();
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }
    }
}
