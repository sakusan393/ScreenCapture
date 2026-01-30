using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ScreenCapture
{
    public partial class DraggableText : UserControl
    {
        // XAMLの TextBox へのアクセス用プロパティ
        public System.Windows.Controls.TextBox TextBoxControl => TextBox;

        public DraggableText()
        {
            InitializeComponent();

            // デバッグ: 背景を強制的に透明に設定
            TextBox.Background = System.Windows.Media.Brushes.Transparent;
            TextBox.BorderBrush = System.Windows.Media.Brushes.Transparent;
            
            // 編集中はドラッグ無効（入力を優先）
            TextBox.GotFocus += (_, __) =>
            {
                DragThumb.IsHitTestVisible = false;
                TextBox.IsHitTestVisible = true;
            };

            // 編集が終わったらドラッグ有効（TextBoxを透過してThumbでドラッグ可能に）
            TextBox.LostFocus += (_, __) =>
            {
                DragThumb.IsHitTestVisible = true;
                TextBox.IsHitTestVisible = false;
            };

            DragThumb.DragDelta += (s, e) =>
            {
                var left = Canvas.GetLeft(this);
                var top = Canvas.GetTop(this);

                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                Canvas.SetLeft(this, left + e.HorizontalChange);
                Canvas.SetTop(this, top + e.VerticalChange);
            };

            // Thumbをダブルクリックしたら編集モードに入る
            DragThumb.MouseDoubleClick += (_, __) =>
            {
                TextBox.Focus();
                TextBox.SelectAll();
            };
        }

        // 外部から編集モードを終了できるメソッド
        public void EndEdit()
        {
            // TextBoxからフォーカスを外す（別の要素にフォーカスを移す）
            DragThumb.Focus();
        }

        public void SetStyle(double fontSize, Color color)
        {
            TextBox.FontSize = fontSize;
            TextBox.Foreground = new SolidColorBrush(color);
        }

        public double GetFontSize() => TextBox.FontSize;

        public Color GetColor()
            => TextBox.Foreground is SolidColorBrush b ? b.Color : Colors.Yellow;
    }
}
