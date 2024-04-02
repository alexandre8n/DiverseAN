using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PrepareReport.Utl
{
    public class WndUtl
    {
        static public bool AskYN(string msg)
        {
            return MessageBox.Show(msg, "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public static StringBuilder GetVisualTreeInfo(Visual element)
        {
            StringBuilder sbListControls;
            if (element == null)
            {
                throw new ArgumentNullException(String.Format("Element {0} is null !", element.ToString()));
            }

            sbListControls = new StringBuilder();

            GetControlsList(element, 0, sbListControls);

            return sbListControls;
        }

        private static void GetControlsList(Visual control, int level, StringBuilder sbListControls)
        {
            const int indent = 4;
            int ChildNumber = VisualTreeHelper.GetChildrenCount(control);

            for (int i = 0; i <= ChildNumber - 1; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(control, i);

                sbListControls.Append(new string(' ', level * indent));
                sbListControls.Append(v.GetType());
                sbListControls.Append(Environment.NewLine);

                if (VisualTreeHelper.GetChildrenCount(v) > 0)
                {
                    GetControlsList(v, level + 1, sbListControls);
                }
            }
        }

        public static Visual GetNextSibling(Visual control)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(control);
            if(parentObject == null)
                return null;
            int iChilds = VisualTreeHelper.GetChildrenCount(parentObject);
            if(iChilds == 0)
                return null;
            for (int i = 0; i < iChilds; i++)
            {
                var child = VisualTreeHelper.GetChild(parentObject, i) as Visual;
                if(child == control)
                {
                    if(i+1 == iChilds)
                        return null;
                    child = VisualTreeHelper.GetChild(parentObject, i+1) as Visual;
                    return child;
                }
            }
            return null;
        }
        public static List<Visual> GetSiblingsStartingFrom(Visual control)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(control);
            if (parentObject == null)
                return null;
            int iChilds = VisualTreeHelper.GetChildrenCount(parentObject);
            if (iChilds == 0)
                return null;
            var resSiblingControls = new List<Visual>();
            for (int i = 0; i < iChilds; i++)
            {
                var child = VisualTreeHelper.GetChild(parentObject, i) as Visual;
                if (child == control)
                {
                    resSiblingControls.Add(child);
                    for (int j = i+1; j < iChilds; j++)
                    {
                        child = VisualTreeHelper.GetChild(parentObject, j) as Visual;
                        if(child != null)
                            resSiblingControls.Add(child);
                    }
                    break;
                }
            }
            return resSiblingControls;
        }

        public static string GetName(object obj)
        {
            // First see if it is a FrameworkElement
            var element = obj as FrameworkElement;
            if (element != null)
                return element.Name;
            // If not, try reflection to get the value of a Name property.
            try { return (string)obj.GetType().GetProperty("Name").GetValue(obj, null); }
            catch
            {
                // Last of all, try reflection to get the value of a Name field.
                try { return (string)obj.GetType().GetField("Name").GetValue(obj); }
                catch { return null; }
            }
        }

    }
}
