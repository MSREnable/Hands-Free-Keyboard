namespace TokenTreeTest
{
    internal class TokenTreeFormatter
    {
        private static void Expand(IList<string> expansion, string prefix, TokenTreeBase parent)
        {
            var index = 0;
            while (index < parent.Children.Length)
            {
                var child = parent.Children[index];
                index++;

                string line;
                if (prefix == String.Empty)
                {
                    line = $"[{child.Text}]";
                }
                else if (index == 1)
                {
                    line = $"{prefix} [{child.Text}]";
                }
                else
                {
                    line = $"{prefix.Substring(0, prefix.Length - 1)} {child.Text}]";
                }

                while (child.Children.Length == 0 &&
                    index < parent.Children.Length &&
                    parent.Children[index].Text.StartsWith(child.Text))
                {
                    line += $" ]{parent.Children[index].Text.Substring(child.Text.Length)}]";
                    child = parent.Children[index];
                    index++;
                }

                if (child.Children.Length == 0)
                {
                    expansion.Add(line);
                }
                else
                {
                    Expand(expansion, line, child);
                }
            }
        }

        public static IEnumerable<string> Expand(TokenTreeRoot root)
        {
            var expansion = new List<string>();

            Expand(expansion, string.Empty, root);

            return expansion;
        }
    }
}
