﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Walkabout.Views
{
    /// <summary>
    /// This class represents the possible options for search operation.
    /// </summary>
    [Flags]
    public enum FindOptions
    {
        /// <summary>
        /// Perform case-insensitive non-word search.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Perform case-sensitive search.
        /// </summary>
        MatchCase = 0x00000001,
        /// <summary>
        /// Perform the search against whole word.
        /// </summary>
        MatchWholeWord = 0x00000002,
    }

    /// <summary>
    /// This class encapsulates the find operations for<see cref="FlowDocument"/>.
    /// </summary>
    public sealed class FindManager
    {
        private FlowDocument inputDocument;
        private TextPointer currentPosition;

        /// <summary>
        /// Initializes a new instance of the<see cref="FindReplaceManager"/> 
        /// class given the specified<see cref="FlowDocument"/> instance.
        /// </summary>
        /// <param name="inputDocument">the input document</param>
        public FindManager(FlowDocument inputDocument)
        {
            if (inputDocument == null)
            {
                throw new ArgumentNullException("documentToFind");
            }

            this.inputDocument = inputDocument;
            this.currentPosition = inputDocument.ContentStart;
        }

        /// <summary>
        /// Gets and sets the offset position for the<see cref="FindReplaceManager"/>
        /// </summary>
        public TextPointer CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.CompareTo(inputDocument.ContentStart) < 0 ||
                    value.CompareTo(inputDocument.ContentEnd) > 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                currentPosition = value;
            }
        }

        /// <summary>
        /// Find next match of the input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="findOptions">the search options</param>
        /// <returns>The<see cref="TextRange"/> instance representing the input string.</returns>
        /// <remarks>
        /// This method will advance the<see cref="CurrentPosition"/> to next context position.
        /// </remarks>
        public TextRange FindNext(String input, FindOptions findOptions = FindOptions.None)
        {
            TextRange textRange = GetTextRangeFromPosition(ref currentPosition, input, findOptions);
            return textRange;
        }

        /// <summary>
        /// Finds the corresponding<see cref="TextRange"/> instance 
        /// representing the input string given a specified text pointer position.
        /// </summary>
        /// <param name="position">the current text position</param>
        /// <param name="textToFind">input text</param>
        /// <param name="findOptions">the search option</param>
        /// <returns>
        /// An<see cref="TextRange"/> instance represeneting the matching
        /// string withing the text container.
        /// </returns>
        public TextRange GetTextRangeFromPosition(ref TextPointer position,
                                                  String input,
                                                  FindOptions findOptions)
        {
            Boolean matchCase = (findOptions & FindOptions.MatchCase) == FindOptions.MatchCase;
            Boolean matchWholeWord = (findOptions & FindOptions.MatchWholeWord)
                                                        == FindOptions.MatchWholeWord;

            TextRange textRange = null;

            while (position != null)
            {
                if (position.CompareTo(inputDocument.ContentEnd) == 0)
                {
                    break;
                }

                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    String textRun = position.GetTextInRun(LogicalDirection.Forward);
                    StringComparison stringComparison = matchCase ?
                        StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                    Int32 indexInRun = textRun.IndexOf(input, stringComparison);

                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        TextPointer nextPointer = position.GetPositionAtOffset(input.Length);
                        textRange = new TextRange(position, nextPointer);

                        if (matchWholeWord)
                        {
                            if (IsWholeWord(textRange)) // Test if the "textRange" represents a word.
                            {
                                // If a WholeWord match is found, directly terminate the loop.
                                position = position.GetPositionAtOffset(input.Length);
                                break;
                            }
                            else
                            {
                                // If a WholeWord match is not found, go to next recursion to find it.
                                position = position.GetPositionAtOffset(input.Length);
                                return GetTextRangeFromPosition(ref position, input, findOptions);
                            }
                        }
                        else
                        {
                            // If a none-WholeWord match is found, directly terminate the loop.
                            position = position.GetPositionAtOffset(input.Length);
                            break;
                        }
                    }
                    else
                    {
                        // If a match is not found, go over to the next context position after the "textRun".
                        position = position.GetPositionAtOffset(textRun.Length);
                    }
                }
                else
                {
                    //If the current position doesn't represent a text context position, go to the next context position.
                    // This can effectively ignore the formatting or embed element symbols.
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
            }

            if (textRange != null)
            {
                return new TextRange(textRange.Start, textRange.Start.GetPositionAtOffset(input.Length));
            }
            return null;
        }

        /// <summary>
        /// determines if the specified character is a valid word character.
        /// here only underscores, letters, and digits are considered to be valid.
        /// </summary>
        /// <param name="character">character specified</param>
        /// <returns>Boolean value didicates if the specified character is a valid word character</returns>
        private Boolean IsWordChar(Char character)
        {
            return Char.IsLetterOrDigit(character) || character == '_';
        }

        /// <summary>
        /// Tests if the string within the specified<see cref="TextRange"/>instance is a word.
        /// </summary>
        /// <param name="textRange"><see cref="TextRange"/>instance to test</param>
        /// <returns>test result</returns>
        private Boolean IsWholeWord(TextRange textRange)
        {
            Char[] chars = new Char[1];

            if (textRange.Start.CompareTo(inputDocument.ContentStart) == 0 || textRange.Start.IsAtLineStartPosition)
            {
                textRange.End.GetTextInRun(LogicalDirection.Forward, chars, 0, 1);
                return !IsWordChar(chars[0]);
            }
            else if (textRange.End.CompareTo(inputDocument.ContentEnd) == 0)
            {
                textRange.Start.GetTextInRun(LogicalDirection.Backward, chars, 0, 1);
                return !IsWordChar(chars[0]);
            }
            else
            {
                textRange.End.GetTextInRun(LogicalDirection.Forward, chars, 0, 1);
                if (!IsWordChar(chars[0]))
                {
                    textRange.Start.GetTextInRun(LogicalDirection.Backward, chars, 0, 1);
                    return !IsWordChar(chars[0]);
                }
            }

            return false;
        }
    }
}
