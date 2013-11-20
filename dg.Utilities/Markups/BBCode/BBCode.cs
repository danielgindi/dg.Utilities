using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities.BBCode
{
    /// <summary>
    /// Written by danielgindi@gmail.com
    /// </summary>
    public class BBCodeParser
    {
        public BBCodeParser() { }
        public BBCodeParser(string[] KnownTags, ParseTagDelegate Delegate)
        {
            this.KnownTags = KnownTags;
            this.Delegate = Delegate;
        }
        public BBCodeParser(string[] KnownTags)
        {
            this.KnownTags = KnownTags;
        }

        #region Types
        public delegate string ParseTagDelegate(string tag, string content, Dictionary<string, string> arguments);
        private enum ParsingPhase
        {
            None = 0,
            FoundStartTagBeg = 1, // [
            FoundStartTagName = 2, // [tagName 
            FoundStartTagEnd = 3, // [...]
            FoundEndTagBeg = 4, // [/
        }
        #endregion

        #region Properties
        private List<string> _KnownTags = new List<string>();
        private ParseTagDelegate _Delegate = null;
        private string[] EmptyStringArray = new string[] { };

        /// <summary>
        /// List of known tags. Any tags not on this list will be ignored.
        /// </summary>
        public string[] KnownTags
        {
            get
            {
                if (_KnownTags == null) return EmptyStringArray;
                else return _KnownTags.ToArray();
            }
            set
            {
                if (value == null) value = new string[] { };
                List<string> tags = new List<string>(value.Length);
                string tag1;
                foreach (string tag in value)
                {
                    tag1 = tag.Trim().ToLowerInvariant();
                    if (tag1.Length == 0) continue;
                    if (tags.Contains(tag1)) continue;
                    tags.Add(tag1);
                }
                _KnownTags = tags;
            }
        }

        /// <summary>
        /// Sends the tag, its parameters, and its content
        /// Expects the replacement HTML for that tag
        /// </summary>
        public ParseTagDelegate Delegate
        {
            get
            {
                return _Delegate;
            }
            set
            {
                _Delegate = value;
            }
        }
        #endregion

        #region Methods
        public string Parse(string input)
        {
            int returnPosition;
            return ParseInner(input, 0, new List<string>(), out returnPosition);
        }

        private string ParseInner(string input, int position, List<string> currentTags, out int returnPosition)
        {
            int inputLength = input.Length;
            int lastKnownPosition = position; // For when parsing ends unexpectedly
            int retractPosition = position; // For when the tag is found unregistered
            string beginTag = "";
            string endTag = "";
            ParsingPhase phase = ParsingPhase.None;

            char c, nextC;
            bool hasNextC;

            Dictionary<string, string> args = new Dictionary<string, string>();
            string argName = null;
            string argValue = null;
            char argQuote = '-';

            StringBuilder output = new StringBuilder();
            string dataInsideTags = @"";

            while (position < inputLength)
            {
                c = input[position];
                hasNextC = ((position + 1) < inputLength);
                nextC = hasNextC ? input[position + 1] : ' ';

                if (phase == ParsingPhase.None || phase == ParsingPhase.FoundStartTagEnd)
                {
                    if (c == '[')
                    {
                        if (hasNextC && nextC == '[')
                        {
                            // Skip
                            position++;
                        }
                        else if (nextC == '/')
                        {
                            if (phase == ParsingPhase.FoundStartTagEnd)
                            {
                                endTag = "";
                                phase = ParsingPhase.FoundEndTagBeg;
                                retractPosition = position; // Save this position in case the tag is not registered
                                position++; // Increase current position by extra 1, for extra '/'
                            }
                            else
                            { // SHOULD NEVER REACH HERE. Found end tag only. Break the loop.
                                if (currentTags.Count > 0)
                                {
                                    break;
                                }
                                else
                                {
                                    output.Append(c);
                                }
                            }
                        }
                        else
                        {
                            beginTag = "";
                            phase = ParsingPhase.FoundStartTagBeg;
                            retractPosition = position; // Save this position in case the tag is not registered
                        }
                    }
                    else
                    {
                        if (phase == ParsingPhase.FoundStartTagEnd)
                        {
                            int retPos = 0;
                            currentTags.Add(beginTag);
                            dataInsideTags += ParseInner(input, position, currentTags, out retPos);
                            currentTags.RemoveAt(currentTags.Count - 1);
                            position = retPos - 1;
                        }
                        else
                        {
                            output.Append(c);
                        }
                    }
                }
                else if (phase == ParsingPhase.FoundStartTagBeg)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-' || c == '_')
                    {
                        beginTag += c;
                    }
                    else if (c == ']')
                    {
                        beginTag = beginTag.ToLowerInvariant();
                        if (!_KnownTags.Contains(beginTag))
                        { // This tag is unregistered, ignore it!
                            output.Append(input.Substring(retractPosition, (position - retractPosition + 1)));
                            phase = ParsingPhase.None;
                            lastKnownPosition = position + 1;
                            beginTag = @"";
                        }
                        else
                        {
                            args.Clear();
                            phase = ParsingPhase.FoundStartTagEnd;
                        }
                    }
                    else
                    {
                        beginTag = beginTag.ToLowerInvariant();
                        if (!_KnownTags.Contains(beginTag))
                        { // This tag is unregistered, ignore it!
                            output.Append(input.Substring(retractPosition, (position - retractPosition + 1)));
                            phase = ParsingPhase.None;
                            lastKnownPosition = position + 1;
                            beginTag = @"";
                        }
                        else
                        {
                            args.Clear();
                            if (c == '/' && nextC == ']')
                            { // Self closing tag! e.g. [tagName/]
                                phase = ParsingPhase.FoundEndTagBeg;
                                endTag = beginTag;
                            }
                            else
                            {
                                phase = ParsingPhase.FoundStartTagName;
                            }
                        }
                    }
                }
                else if (phase == ParsingPhase.FoundStartTagName)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-' || c == '_')
                    {
                        if (argName == null)
                        {// Start reading argument name... [tagName argName
                            argName = @"";
                            argName += c;
                            argQuote = ' ';
                        }
                        else if (argName != null && argValue == null && argQuote == '-')
                        {
                            // Wrap up the previous argument name... [tagName argName
                            args[argName] = @"";
                            argName = argValue = null;
                            argQuote = ' ';

                            // Start reading argument name... [tagName argName
                            argName = @"";
                            argName += c;
                        }
                        else if (argName != null && argValue == null && argQuote == ' ')
                        {// Keep reading argument name... [tagName argName
                            argName += c;
                        }
                        else if (argName != null && argValue == null && argQuote == '=')
                        {// Start reading value without quotes... [tagName argName=value
                            argQuote = ' ';
                            argValue = @"";
                            argValue += c;
                        }
                        else if (argName != null && argValue != null)
                        {// Keep reading value... [tagName argName=value or [tagName argName="value
                            argValue += c;
                        }
                    }
                    else if (c == '=')
                    {
                        if (argName != null && argValue == null)
                        {// Start looking for quotes or value... [tagName argName=
                            argQuote = '=';
                        }
                        else if (argName != null && argValue != null && argQuote != ' ')
                        {// Add to value... [tagName argName="value=
                            argValue += c;
                        }
                        else if (argName != null && argValue != null && argQuote == ' ')
                        {// Wrap it up... [tagName argName=value=
                            args[argName] = argValue;
                            argName = argValue = null;
                            argQuote = ' ';
                        }
                    }
                    else if (c == '\'' || c == '\"')
                    {
                        if (argName != null && argValue == null && argQuote == '=')
                        { // Start reading value... [tagName argName=" or [tagName argName='
                            argQuote = c;
                            argValue = "";
                        }
                        else if (argName != null && argValue != null && (argQuote == c || argQuote == ' '))
                        { // Wrap it up... [tagName argName="value" or [tagName argName=value"
                            args[argName] = argValue;
                            argName = argValue = null;
                            argQuote = ' ';
                        }
                    }
                    else
                    {
                        if (argName != null && argValue != null && argQuote != ' ')
                        {// Add to value... [tagName argName="value%$#%#
                            argValue += c;
                        }
                        else if (argName != null && argValue != null && argQuote == ' ')
                        { // Wrap it up... [tagName argName=value%$#%#
                            args[argName] = argValue;
                            argName = argValue = null;
                            argQuote = '-';

                            if (c == ']') phase = ParsingPhase.FoundStartTagEnd;
                            else if (c == '/' && nextC == ']')
                            { // Self closing tag! e.g. [tagName/]
                                phase = ParsingPhase.FoundEndTagBeg;
                                endTag = beginTag;
                            }
                        }
                        else
                        { // Next time if it finds alphanum character, it will start a NEW arg name.
                            if (argName != null) argQuote = '-';
                            if (c == ']')
                            {
                                if (argName != null)
                                {
                                    args[argName] = @"";
                                    argName = null;
                                }
                                phase = ParsingPhase.FoundStartTagEnd;
                            }
                            else if (c == '/' && nextC == ']')
                            { // Self closing tag! e.g. [tagName/]
                                if (argName != null)
                                {
                                    args[argName] = @"";
                                    argName = null;
                                }
                                phase = ParsingPhase.FoundEndTagBeg;
                                endTag = beginTag;
                            }
                        }
                    }
                }
                else if (phase == ParsingPhase.FoundEndTagBeg)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-' || c == '_')
                    {
                        endTag += c;
                    }
                    else if (c == ']')
                    {
                        endTag = endTag.ToLowerInvariant();

                        if (!_KnownTags.Contains(endTag))
                        { // This tag is unregistered, ignore it!
                            dataInsideTags += input.Substring(retractPosition, (position - retractPosition + 1));
                            phase = ParsingPhase.FoundStartTagEnd;
                            endTag = @"";
                        }
                        else
                        {
                            if (Delegate != null)
                            {
                                // Call delegate on current tag with the contents and arguments;
                                // Append replacement html of BBCode to the output stream
                                output.Append(Delegate(beginTag, dataInsideTags, args));
                                dataInsideTags = @"";
                            }
                            if (endTag != beginTag && currentTags.Contains(endTag))
                            {
                                // Start tag and End tag do not match;
                                // But the end tag is found earlier in the chain;
                                // So wrap up until reaching back to that tag.
                                returnPosition = position - endTag.Length - 1; // Get to the beginning position of this end tag
                                return output.ToString();
                            }
                            else
                            {
                                // Start tag and End tag MATCH, OR:
                                // Start tag and End tag do not match;
                                // And the end tag is NOT found earlier in the chain;
                                phase = ParsingPhase.None; // Back to first phase - look for the next Start tag.
                                lastKnownPosition = position + 1;

                                args.Clear();
                                dataInsideTags = @"";
                                beginTag = endTag = @"";
                            }
                        }
                    }
                    else
                    {
                        // ERROR. Ignore.
                    }
                }
                position++;
            }

            if (phase != ParsingPhase.None && lastKnownPosition < inputLength)
            {
                output.Append(input.Substring(lastKnownPosition));
            }

            returnPosition = position;
            return output.ToString();
        }
        #endregion
    }
}
