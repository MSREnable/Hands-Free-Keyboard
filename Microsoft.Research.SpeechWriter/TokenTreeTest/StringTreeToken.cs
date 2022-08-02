namespace TokenTreeTest
{
    public class StringTreeToken : ITreeToken<StringTreeToken>
    {
        private StringTreeToken(string text, bool isSuffix)
        {
            Text = text;
            IsSuffix = isSuffix;
        }

        public StringTreeToken(string text)
            : this(text, false)
        {
        }

        public string Text { get; }

        public bool IsSuffix { get; }

        StringTreeToken ITreeToken<StringTreeToken>.Join(StringTreeToken token)
        {
            var join = new StringTreeToken(Text + ' ' + token.Text);
            return join;
        }

        StringTreeToken? ITreeToken<StringTreeToken>.Suffix(StringTreeToken token)
        {
            StringTreeToken? value;

            if (Text.StartsWith(token.Text))
            {
                value = new StringTreeToken(Text.Substring(token.Text.Length), true);
            }
            else
            {
                value = null;
            }

            return value;
        }

        public override string ToString()
        {
            return (IsSuffix ? ']' : '[') + Text + ']';
        }
    }
}
