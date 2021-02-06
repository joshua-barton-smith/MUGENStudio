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
    /// <summary>
    /// represents a single autocompletion item.
    /// </summary>
    public class MugenCompletionItem : ICompletionData
    {
        /// <summary>
        /// autocompletion item with its mode + description
        /// </summary>
        /// <param name="text">text to complete with</param>
        /// <param name="mode">mode for insertion</param>
        /// <param name="desc">description</param>
        public MugenCompletionItem(string text, CompletionMode mode, string desc)
        {
            this.Text = text;
            this.Mode = mode;
            this.Desc = desc;
            this.Priority = 1;
        }

        /// <summary>
        /// text to be inserted
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// mode for insertion
        /// </summary>
        public CompletionMode Mode { get; }
        /// <summary>
        /// tooltip description
        /// </summary>
        public string Desc { get; }

        
        /// <summary>
        /// content to display in window
        /// </summary>
        public object Content
        {
            get { return this.Text; }
        }

        /// <summary>
        /// tooltip description
        /// </summary>
        public object Description
        {
            get { return this.Desc; }
        }

        /// <summary>
        /// priority of this item
        /// </summary>
        public double Priority { get; }

        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        /// <summary>
        /// inserts autocompleted item
        /// </summary>
        /// <param name="textArea"></param>
        /// <param name="completionSegment"></param>
        /// <param name="insertionRequestEventArgs"></param>
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

        /// <summary>
        /// mode for insertion, changes method slightly
        /// </summary>
        public enum CompletionMode
        {
            /// <summary>
            /// none, insert normally
            /// </summary>
            NONE,
            /// <summary>
            /// insert an additional closing square brace
            /// </summary>
            SECTION_HEADER,
            /// <summary>
            /// insert an additional equals
            /// </summary>
            PROPERTY_LHS,
            /// <summary>
            /// basically behaves normally
            /// </summary>
            PROPERTY_RHS,
            /// <summary>
            /// indicator that multiple enum values can be filled
            /// </summary>
            PROPERTY_RHS_MULTIENUM,
            /// <summary>
            /// used in parsing to indicate comment found
            /// </summary>
            ABORT
        }
    }
}
