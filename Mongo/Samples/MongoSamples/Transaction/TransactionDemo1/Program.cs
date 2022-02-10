﻿using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace TransactionDemo1
{
    /// <summary>
    ///  该例子来自于：https://www.mongodb.com/developer/how-to/transactions-c-dotnet/
    /// </summary>
    class Program
    {
        public class Product
        {
            [BsonId]
            public ObjectId Id { get; set; }
            [BsonElement("SKU")]
            public int SKU { get; set; }
            [BsonElement("Description")]
            public string Description { get; set; }
            [BsonElement("Price")]
            public Double Price { get; set; }
        }

        const string MongoDBConnectionString = "mongodb://admin:bestadmin@192.168.123.162:27018/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false";

        async static Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            await UpdateProductsAsync();
        }

        private static async Task<bool> UpdateProductsAsync()
        {
            // Create client connection to our MongoDB database
            var client = new MongoClient(MongoDBConnectionString);
            // Create the collection object that represents the "products" collection
            var database = client.GetDatabase("TestMongoDBStore");
            var products = database.GetCollection<Product>("products");
            // Clean up the collection if there is data in there
            await database.DropCollectionAsync("products");
            // collections can't be created inside a transaction so create it first
            await database.CreateCollectionAsync("products");

            // Create a session object that is used when leveraging transactions
            using (var session = await client.StartSessionAsync())
            {
                // Begin transaction
                session.StartTransaction();
                try
                {
                    // Create some sample data
                    var tv = new Product
                    {
                        Description = "Television",
                        SKU = 4001,
                        Price = 2000
                    };
                    var book = new Product
                    {
                        Description = "A funny book",
                        SKU = 43221,
                        Price = 19.99
                    };
                    var dogBowl = new Product
                    {
                        Description = "Bowl for Fido",
                        SKU = 123,
                        Price = 40.00
                    };

                    // Insert the sample data 
                    // Mongo DB C# Driver 如果想要某个操作是 transaction 的一部分，则必须要传入 session. see https://stackoverflow.com/q/59742053/6294524
                    await products.InsertOneAsync(session, tv);
                    await products.InsertOneAsync(session, book);
                    await products.InsertOneAsync(session, dogBowl);
                    var resultsBeforeUpdates = await products
                                    .Find<Product>(session, Builders<Product>.Filter.Empty)
                                    .ToListAsync();
                    Console.WriteLine("Original Prices:\n");
                    foreach (Product d in resultsBeforeUpdates)
                    {
                        Console.WriteLine(
                                    String.Format("Product Name: {0}\tPrice: {1:0.00}",
                                        d.Description, d.Price)
                        );
                    }
                    // Increase all the prices by 10% for all products
                    var update = new UpdateDefinitionBuilder<Product>()
                            .Mul<Double>(r => r.Price, 1.1);
                    await products.UpdateManyAsync(session,
                            Builders<Product>.Filter.Empty,
                            update); //,options);

                    // Made it here without error? Let's commit the transaction
                    await session.CommitTransactionAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error writing to MongoDB: " + e.Message);
                    await session.AbortTransactionAsync();                    
                }


                // Let's print the new results to the console
                Console.WriteLine("\n\nNew Prices (10% increase):\n");
                var resultsAfterCommit = await products
                        .Find<Product>(session, Builders<Product>.Filter.Empty)
                        .ToListAsync();
                foreach (Product d in resultsAfterCommit)
                {
                    Console.WriteLine(
                        String.Format("Product Name: {0}\tPrice: {1:0.00}",
                                                    d.Description, d.Price)
                    );
                }
                return true;
            }
        }
    }
}
