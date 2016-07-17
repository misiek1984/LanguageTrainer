using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MK.UI.WPF.Behaviours
{
    public static class MouseBehaviour
    {
        #region MouseDown

        public static ICommand GetMouseDownCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseDownCommand);
        }

        public static void SetMouseDownCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseDownCommand, value);
        }

        public static readonly DependencyProperty MouseDownCommand =
            DependencyProperty.RegisterAttached("MouseDownCommand",
                                                typeof(ICommand), typeof(MouseBehaviour),
                                                new UIPropertyMetadata(null, OnMouseDownCommandChanged));

        private static void OnMouseDownCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;

            if(element == null)
                return;

            element.MouseDown -= element_MouseDown;

            var cmd = e.NewValue as ICommand;
            if (cmd != null)
                element.MouseDown += element_MouseDown;
        }

        private static void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            var cmd = GetMouseDownCommand(element);
            
            if(cmd.CanExecute(e))
                cmd.Execute(e);
        }

        #endregion

        #region MouseUp

        public static ICommand GetMouseUpCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseUpCommand);
        }

        public static void SetMouseUpCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseUpCommand, value);
        }

        public static readonly DependencyProperty MouseUpCommand =
            DependencyProperty.RegisterAttached("MouseUpCommand",
                                                typeof(ICommand), typeof(MouseBehaviour),
                                                new UIPropertyMetadata(null, OnMouseUpCommandChanged));

        private static void OnMouseUpCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            element.MouseUp -= element_MouseUp;     

            var cmd = e.NewValue as ICommand;
            if (cmd != null)
                element.MouseUp += element_MouseUp;
        }

        private static void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            var cmd = GetMouseUpCommand(element);

            if (cmd.CanExecute(e))
                cmd.Execute(e);
        }

        #endregion

        #region MouseMove

        public static ICommand GetMouseMoveCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseMoveCommand);
        }

        public static void SetMouseMoveCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseMoveCommand, value);
        }

        public static readonly DependencyProperty MouseMoveCommand =
            DependencyProperty.RegisterAttached("MouseMoveCommand",
                                                typeof(ICommand), typeof(MouseBehaviour),
                                                new UIPropertyMetadata(null, OnMouseMoveCommandChanged));

        private static void OnMouseMoveCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            element.MouseMove -= element_MouseMove;

            var cmd = e.NewValue as ICommand;
            if (cmd != null)
                element.MouseMove += element_MouseMove;
        }

        private static void element_MouseMove(object sender, MouseEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            var cmd = GetMouseMoveCommand(element);

            if (cmd.CanExecute(e))
                cmd.Execute(e);
        }

        #endregion

        #region MouseWheel

        public static ICommand GetMouseWheelCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseWheelCommand);
        }

        public static void SetMouseWheelCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseWheelCommand, value);
        }

        public static readonly DependencyProperty MouseWheelCommand =
            DependencyProperty.RegisterAttached("MouseWheelCommand",
                                                typeof(ICommand), typeof(MouseBehaviour),
                                                new UIPropertyMetadata(null, OnMouseWheelCommandChanged));

        private static void OnMouseWheelCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            element.MouseWheel -= element_MouseWheel;

            var cmd = e.NewValue as ICommand;
            if (cmd != null)
                element.MouseWheel += element_MouseWheel;
        }

        private static void element_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as UIElement;

            if (element == null)
                return;

            var cmd = GetMouseWheelCommand(element);

            if (cmd.CanExecute(e))
                cmd.Execute(e);
        }

        #endregion
    }
}
