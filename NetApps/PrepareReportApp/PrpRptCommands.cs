using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace PrepareReport
{
    public class PrpRptCommands
    {
        public static RoutedCommand MoveReportFiles =
          new RoutedCommand("MoveReportFiles", typeof(MainWindow),
              new InputGestureCollection(new InputGesture[] {new KeyGesture(Key.M, ModifierKeys.Control) }));
        public static RoutedCommand ZipReportFiles =
          new RoutedCommand("ZipReportFiles", typeof(MainWindow),
              new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.Z, ModifierKeys.Control) }));
        public static RoutedCommand SummarizeReportFiles =
          new RoutedCommand("SummarizeReportFiles", typeof(MainWindow),
              new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.S, ModifierKeys.Control) }));
        public static RoutedCommand RegExpTester =
          new RoutedCommand("RegExpTester", typeof(MainWindow),
              new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.R, ModifierKeys.Control) }));
        public static RoutedCommand OpenReportsFolder =
          new RoutedCommand("OpenReportsFolder", typeof(MainWindow),
              new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F, ModifierKeys.Control) }));
    }
}
