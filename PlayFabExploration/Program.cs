using PlayFab;
using PlayFab.ClientModels;
using PlayFab.ServerModels;
using System;
using System.Collections.Generic;

namespace PlayFabExploration
{
    class Program
    {
        //used to generate random scores
        private static Random _rand = new Random();
        //list of playfab ids - in a real scenario this would be stored along with the user info
        private static List<string> _playFabUsers = new List<string>();
        private static int _numUsers = 200;

        static void Main(string[] args)
        {
            //play fab information - generated in the playfab portal
            PlayFabSettings.TitleId = "<playfab title id>";
            PlayFabSettings.DeveloperSecretKey = "<play fab secret id, found in settings>";

            var done = false;
            while (!done)
            {
                PrintMenu();
                done = HandleSelection();
            }
        }
        private static bool HandleSelection()
        {
            var done = false;
            var input = Console.ReadKey();
            Console.WriteLine();

            switch (input.Key)
            {
                case ConsoleKey.D1:
                    var fabID = CreateUserFromCustomID("User_12345");
                    UpdateDisplayName(fabID, "A display name");
                    break;
                case ConsoleKey.D2:
                    var fabID2 = RegisterANewPlayfabUser("Some Name", "newuser1");
                    //ConnectFabUserToCustomID(fabID2, "User_12344");
                    break;
                case ConsoleKey.D3:
                    Create200Users();
                    AssignScoresToAllUsers();
                    break;
                case ConsoleKey.D4:
                    ShowLeaderboardAroundAGivenUser("FB7DE3690AED230F");
                    break;
                case ConsoleKey.D5:
                    EstablishFriendsRelationships();
                    break;
                case ConsoleKey.D6:
                    ShowFriendLeaderboardForAGivenUser("387EA56DA9109CA2");
                    break;
                default:
                    done = true;
                    break;
            }
            Console.ReadKey();
            return done;
        }

        //show friends leaderboard
        private static void ShowFriendLeaderboardForAGivenUser(string playFabId)
        {
            var request = new PlayFab.ServerModels.GetFriendLeaderboardRequest
            {
                PlayFabId = playFabId,
                StatisticName = "global",
                MaxResultsCount = 20,
                IncludeFacebookFriends = true
            };
            var result = PlayFabServerAPI.GetFriendLeaderboardAsync(request).GetAwaiter().GetResult();
            if (result.Error != null)
                Console.WriteLine(result.Error.ErrorMessage);
            Console.WriteLine($"Friend leaderboard for user: {playFabId}");
            Console.WriteLine("------------------");
            Console.WriteLine("Pos\tUser\t\t\tPlayer\tScore");
            foreach (var score in result.Result.Leaderboard)
            {
                if (score.PlayFabId.Length > 15)
                    Console.WriteLine($"{score.Position}\t{score.PlayFabId}\t{score.DisplayName}\t{score.StatValue}");
                else
                    Console.WriteLine($"{score.Position}\t{score.PlayFabId}\t\t{score.DisplayName}\t{score.StatValue}");
            }
            Console.WriteLine("------------------");
        }
        private static void EstablishFriendsRelationships()
        {
            Console.WriteLine("Establishing friends relationships");
            MakeFriends("387EA56DA9109CA2", "AE1E55E370CE1D7D");
            MakeFriends("387EA56DA9109CA2", "36CDE6759E03E751");
            MakeFriends("387EA56DA9109CA2", "D4D634581695759C");
        }
        // make friends between two users
        private static void MakeFriends(string playerFabID, string friendFabID)
        {
            var request = new PlayFab.ServerModels.AddFriendRequest
            {
                PlayFabId = playerFabID,
                FriendPlayFabId = friendFabID
            };
            var result = PlayFabServerAPI.AddFriendAsync(request);
            if (result.Result.Error != null)
                Console.WriteLine(result.Result.Error.ErrorMessage);
        }

        private static void ShowLeaderboardAroundAGivenUser(string playFabId)
        {
            var request = new GetLeaderboardAroundUserRequest
            {
                StatisticName = "global",
                MaxResultsCount = 20,
                PlayFabId = playFabId
            };
            var result = PlayFabServerAPI.GetLeaderboardAroundUserAsync(request).GetAwaiter().GetResult();
            Console.WriteLine($"Leaderboard around user: {playFabId}");
            Console.WriteLine("------------------");
            Console.WriteLine("Pos\tUser\t\t\tPlayer\tScore");
            foreach (var score in result.Result.Leaderboard)
            {
                if (score.PlayFabId.Length > 15)
                    Console.WriteLine($"{score.Position}\t{score.PlayFabId}\t{score.DisplayName}\t{score.StatValue}");
                else
                    Console.WriteLine($"{score.Position}\t{score.PlayFabId}\t\t{score.DisplayName}\t{score.StatValue}");
            }
            Console.WriteLine("------------------");
        }
        private static void AssignScoresToAllUsers()
        {
            int score;
            foreach(string fabId in _playFabUsers)
            {
                score = _rand.Next(1000);
                AssignScore(fabId, score);
            }
        }
        //update the score for a given user on the "global" leaderboard
        private static void AssignScore(string fabId, int score)
        {
            var request = new PlayFab.ServerModels.UpdatePlayerStatisticsRequest
            {
                PlayFabId = fabId,
                Statistics = new List<PlayFab.ServerModels.StatisticUpdate>
                {
                    new PlayFab.ServerModels.StatisticUpdate
                    {
                        StatisticName = "global",
                        Value = score
                    }
                }
            };
            var result = PlayFabServerAPI.UpdatePlayerStatisticsAsync(request).GetAwaiter().GetResult();
            if (result.Error != null)
                Console.WriteLine(result.Error.ErrorMessage);
        }
        //create 200 users using the "register method"
        private static void Create200Users()
        {
            Console.WriteLine("Creating 200 users");

            //option 1
            for(int i = 0; i < _numUsers; i++)
            {
                var userName = $"player{i}";
                var displayName = $"person {i}";
                _playFabUsers.Add(RegisterANewPlayfabUser(displayName, userName));
            }

            //option 2
            //for (int i = 0; i < _numUsers; i++)
            //{
            //    _playFabUsers.Add(CreateUserFromCustomID($"custom{i}"));
            //}
        }
        //Link an existing playfab user to a custom id
        //You must be logged in to do this
        private static void ConnectFabUserToCustomID(string fabID2, string customID)
        {
            var request = new LinkCustomIDRequest
            {
                CustomId = customID
            };
            var result = PlayFabClientAPI.LinkCustomIDAsync(request).GetAwaiter().GetResult();
            if (result.Error != null)
                Console.WriteLine(result.Error.ErrorMessage);
        }
        // Create a play fab user with info
        private static string RegisterANewPlayfabUser(string displayName, string userName)
        {
            Console.WriteLine($"Registering a new playfab user: {userName}:{displayName}");
            var request = new RegisterPlayFabUserRequest
            {
                Username = userName,
                Password = "password",
                DisplayName = displayName,
                //Email = "someone@somewhere.com",
                RequireBothUsernameAndEmail = false
            };
            var result = PlayFabClientAPI.RegisterPlayFabUserAsync(request).GetAwaiter().GetResult();
            if (result.Error != null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
                return null;
            }
            else
            {
                return result.Result.PlayFabId;                
            }
        }
        //Update a users display name
        private static void UpdateDisplayName(string fabID, string displayName)
        {
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            };
            var result = PlayFabClientAPI.UpdateUserTitleDisplayNameAsync(request).GetAwaiter().GetResult();
        }
        // Create a user from a given custom id
        private static string CreateUserFromCustomID(string customID)
        {
            var request = new LoginWithCustomIDRequest
            {
                CustomId = customID,
                CreateAccount = true
            };
            var result = PlayFabClientAPI.LoginWithCustomIDAsync(request).GetAwaiter().GetResult();
            if (result.Error == null)
                return result.Result.PlayFabId;
            else
            {
                Console.WriteLine(result.Error.ErrorMessage);
                return null;
            }
        }
        private static void PrintMenu()
        {
            Console.WriteLine("1. Create User");
            Console.WriteLine("2. Register User");
            Console.WriteLine("3. Create 200 Users and add random scores");
            Console.WriteLine("4. Get Scores around a user");
            Console.WriteLine("5. Set up some player friendships");
            Console.WriteLine("6. Get friends leaderboard");
        }
    }
}
