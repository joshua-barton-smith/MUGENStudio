using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using MUGENStudio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MUGENStudio.Graphic
{
    public class MugenCompletionItem : ICompletionData
    {
        public MugenCompletionItem(string text, CompletionMode mode, string Desc)
        {
            this.Text = text;
            this.Mode = mode;
            this.Desc = Desc;
            this.Priority = 1;
        }

        public string Text { get; private set; }
        public CompletionMode Mode { get; }
        public string Desc { get; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return this.Desc; }
        }

        public double Priority { get; }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            // do different insertion based on the completion rule
            switch(this.Mode)
            {
                // section header, add a closing ] and move caret appropriately
                case CompletionMode.SECTION_HEADER:
                    textArea.Document.Replace(completionSegment, string.Format("{0}]", this.Text));
                    textArea.Caret.Column = textArea.Caret.Column + (this.Text.Length - completionSegment.Length);
                    break;
                // property LHS, add an = and move caret forwards
                // todo - can we show a type tooltip here?
                case CompletionMode.PROPERTY_LHS:
                    textArea.Document.Replace(completionSegment, string.Format("{0} = ", this.Text));
                    textArea.Caret.Column = textArea.Caret.Column + this.Text.Length + 3;
                    break;
                case CompletionMode.PROPERTY_RHS:
                case CompletionMode.PROPERTY_RHS_MULTIENUM:
                    textArea.Document.Replace(completionSegment, this.Text);
                    break;
            }
            
        }

        public enum CompletionMode
        {
            NONE,
            SECTION_HEADER,
            PROPERTY_LHS,
            PROPERTY_RHS,
            PROPERTY_RHS_MULTIENUM,
            ABORT
        }
    }
}
