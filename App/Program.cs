using System;
using GraphQL;
using GraphQL.Types;

namespace App
{
    public class Query 
    {
        [GraphQLMetadata("jedis")]
        public IEnumerable<Jedi> GetJedis() 
        {
            return StarWarsDB.GetJedis();
        }

        [GraphQLMetadata("hello")]
        public string GetHello() 
        {
            return "Hello Query class";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var schema = Schema.For(
                @"
                type Jedi {
                    name: String,
                    side: String
                }

                type Query {
                    hello: String,
                    jedis: [Jedi]
                }
                ",
                _ =>
                {
                    _.Types.Include<Query>();
                }
            );

            var root = new { Hello = "Hello World!" };
            var json = schema.Execute(_ =>
            {
                _.Query = "{ hello }";
                _.Root = root;
            });

            Console.WriteLine(json);
        }
    }
}
