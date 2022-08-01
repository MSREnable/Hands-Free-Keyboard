namespace TokenTreeTest
{
    internal class TokenTreeFormatter
    {
        private static void Expand(IList<string> expansion, string prefix, ITokenTreeParent<string> parent)
        {
            var index = 0;
            while (index < parent.Children.Length)
            {
                var child = parent.Children[index];
                index++;

                string line;
                if (prefix == String.Empty)
                {
                    line = $"[{child.Payload}]";
                }
                else if (index == 1)
                {
                    line = $"{prefix} [{child.Payload}]";
                }
                else
                {
                    line = $"{prefix.Substring(0, prefix.Length - 1)} {child.Payload}]";
                }

                while (child.Children.Length == 0 &&
                    index < parent.Children.Length &&
                    parent.Children[index].Payload.StartsWith(child.Payload))
                {
                    line += $" ]{parent.Children[index].Payload.Substring(child.Payload.Length)}]";
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
