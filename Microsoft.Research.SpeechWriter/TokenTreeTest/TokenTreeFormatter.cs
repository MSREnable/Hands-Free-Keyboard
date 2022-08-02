namespace TokenTreeTest
{
    internal class TokenTreeFormatter
    {
        private static void Expand<TPayload>(IList<IList<TPayload>> expansion, IList<TPayload> prefix, ITokenTreeParent<TPayload> parent)
            where TPayload : ITreeToken<TPayload>
        {
            var index = 0;
            while (index < parent.Children.Length)
            {
                var child = parent.Children[index];
                index++;

                var line = new List<TPayload>(prefix);
                if (index == 1 || line.Count == 0)
                {
                    line.Add(child.Payload);
                }
                else
                {
                    var tail = line[line.Count - 1];
                    var join = tail.Join(child.Payload);
                    if (join != null)
                    {
                        line[line.Count - 1] = join;
                    }
                    else
                    {
                        line.Add(child.Payload);
                    }
                }

                var isPrefix = true;
                while (isPrefix &&
                    child.Children.Length == 0 &&
                    index < parent.Children.Length)
                {
                    var suffix = parent.Children[index].Payload.Suffix(child.Payload);
                    if (suffix == null)
                    {
                        isPrefix = false;
                    }
                    else
                    {
                        line.Add(suffix);

                        child = parent.Children[index];
                        index++;
                    }
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

        public static IEnumerable<IList<TPayload>> Expand<TPayload>(ITokenTreeRoot<TPayload> root)
            where TPayload : ITreeToken<TPayload>
        {
            var expansion = new List<IList<TPayload>>();

            Expand(expansion, new List<TPayload>(), root);

            return expansion;
        }
    }
}
