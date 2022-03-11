using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Helpful_Tools_for_Microsoft_AX
{
    /// <summary>
    /// Interaction logic for ToolManagerControl.
    /// </summary>
    public partial class ToolManagerControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolManagerControl"/> class.
        /// </summary>
        public ToolManagerControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void CallLabelTranslator_Click(object sender, RoutedEventArgs e)
        {
            Window callLabelTranslator = new Window();
            callLabelTranslator.Content = new LabelTranslatorControl();
            callLabelTranslator.Show();

        }

        private void CallLabelSearch_Click(object sender, RoutedEventArgs e)
        {
            Window callLabelSearch = new Window();
            callLabelSearch.Content = new LabelSearchControl();
            callLabelSearch.Show();
        }
    }
}