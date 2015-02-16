namespace Squid.Text
{
    public static class Tokens
    {
        /*public static List<TokenElement> Get(string q)
        {
            string regex = "(?i)^";

            foreach (String st in q.Split(' '))
            {
                if (st != " ")
                    regex += "(?=.*?" + st + ")"; // (st + "|");
            }

            regex += ".*$";
                        
            var tokens = from o in Graph.Stores.Cypher
                         .Match("(s:Store)")
                         .Where("s.Name =~ {regex}")
                         .WithParam("regex", regex)
                         .Return(s => s.As<StoreNode>())
                         .Results.ToList()
                         select new TokenElement{ id = "store:" + o.Name, name = o.Name, image = o.Logo };

            var res = tokens.ToList();
                        
            return res;
        }*/
    }

    public class TokenElement
    {
        public string id;
        public string name;
        public string image;
    }        
}
