using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Security.Cryptography;

namespace SEMIFINAL

{
    
    public class Player
    {
        public string PlayerID { get; set; }
        public string Name { get; set; }
        public int Gold { get; set; }
        public int Score { get; set; }
    }

    internal class Program
    {
        
        private static FirebaseClient firebase = new FirebaseClient("https://project1-80569-default-rtdb.firebaseio.com/");
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("FireShap installed successfully!");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("SEMIFINAL.json")
            });

            Console.WriteLine("Firebase Admin SDK đã được khởi tạo thành công!");

            await GenerateTestPlayers(10);
            await DisplayAllPlayers();
            await UpdatePlayer("p01", 2000, 999);
            await DeletePlayer("p01");
            await Top5Gold();
            await Top5Core();
            await AddOnePlayer();
        }
    


        public static async Task GenerateTestPlayers(int num)
        {
            var random = new Random();

            for (int i = 1; i <= num; i++)
            {
                var player = new Player
                {
                    PlayerID = $"p{i:D2}",
                    Name = $"Player{i}",
                    Gold = random.Next(1000, 5000),
                    Score = random.Next(100, 3000)
                };

                await firebase.Child("Players").Child(player.PlayerID).PutAsync(player);
                Console.WriteLine($"Đã thêm: {player.Name}");
            }
        }
        public static async Task AddOnePlayer()
        {
            var player = new Player
            {
                PlayerID = "p11",
                Name = "VIP",
                Gold = 9999,
                Score = 9999
            };

            await firebase.Child("Players").Child(player.PlayerID).PutAsync(player);
            Console.WriteLine("Đã ép thêm 1 player vào danh sách Players.");
        }

        public static async Task DisplayAllPlayers()
        {
            var players = await firebase.Child("Players").OnceAsync<Player>();

            if (!players.Any())
            {               
                return;
            }

            Console.WriteLine("\nDanh sách người chơi:");
            foreach (var player in players)
            {
                var p = player.Object;
                Console.WriteLine($"{p.Name} | ID: {p.PlayerID} | Gold: {p.Gold} | Score: {p.Score}");
            }
        }

        public static async Task UpdatePlayer(string playerId, int? gold = null, int? score = null)
        {
            
                var player = await firebase.Child("Players").Child(playerId).OnceSingleAsync<Player>();

                if (player == null)
                {                  
                    return;
                }

                if (gold.HasValue) player.Gold = gold.Value;
                if (score.HasValue) player.Score = score.Value;

                await firebase.Child("Players").Child(playerId).PutAsync(player);
                Console.WriteLine($"Đã cập nhật: {player.Name}");            
            
        }

        public static async Task DeletePlayer(string playerId)
        {            
                await firebase.Child("Players").Child(playerId).DeleteAsync();
                Console.WriteLine($"Đã xoá người chơi có ID: {playerId}");
           
        }

        public static async Task Top5Gold()
        {
            var players = await firebase.Child("Players").OnceAsync<Player>();

            var top5 = players
                .OrderByDescending(p => p.Object.Gold)
                .Take(5)
                .Select(p => new
                {
                    Name = p.Object.Name,
                    Gold = p.Object.Gold
                })
                .ToList();

            Console.WriteLine("\nTOP 5 GOLD:");
            foreach (var p in top5)
            {
                Console.WriteLine($"{p.Name} - Gold: {p.Gold}");
            }

            for (int i = 0; i < top5.Count; i++)
            {
                await firebase.Child("TopGold").Child($"p{(i + 1):00}").PutAsync(top5[i]);
            }
        }

        public static async Task Top5Core()
        {
            
            var players = await firebase.Child("Players").OnceAsync<Player>();

            
            var top5 = players
                .OrderByDescending(p => p.Object.Score)
                .Take(5)
                .Select((p, index) => new
                {
                    PlayerID = p.Object.PlayerID,
                    Name = p.Object.Name,
                    Gold = p.Object.Gold,
                    Score = p.Object.Score,
                    Index = index + 1, 
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            
            Console.WriteLine("\nTOP 5 SCORE:");
            foreach (var p in top5)
            {
                Console.WriteLine($"{p.Index}: {p.Name} - Score: {p.Score}");
            }

            
            foreach (var p in top5)
            {
                await firebase.Child("TopScore").Child(p.PlayerID).PutAsync(p);
            }

        }

    }
}
