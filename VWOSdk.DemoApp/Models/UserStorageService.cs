using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VWOSdk.DemoApp
{
    public class UserStorageService : IUserStorageService
    {
        private static string path = @"userStorageMaps.json";
        private ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, dynamic>>> _userStorageMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, dynamic>>>();

        public UserStorageService()
        {
            Load();
        }

        private void Load()
        {
            if (System.IO.File.Exists(path) == false)
                System.IO.File.Create(path);

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, dynamic>>>>(json);
                if (data != null)
                {
                    _userStorageMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, dynamic>>>(data);
                }
                else
                    _userStorageMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, Dictionary<string, dynamic>>>();
            }
            catch { }
        }

        public UserStorageMap Get(string userId, string CampaignKey)
        {
            Dictionary<string, dynamic> userDict = null;
            if (_userStorageMap.TryGetValue(CampaignKey, out ConcurrentDictionary<string, Dictionary<string, dynamic>> userMap))
                userMap.TryGetValue(userId, out userDict);

            if (userDict != null)
                return new UserStorageMap(userId, CampaignKey, userDict["VariationName"], userDict["GoalIdentifier"], userDict["MetaData"]);

            return null;
        }

        public void Set(UserStorageMap userStorageMap)
        {
            if (_userStorageMap.TryGetValue(userStorageMap.CampaignKey, out ConcurrentDictionary<string, Dictionary<string, dynamic>> userMap) == false)
            {
                userMap = new ConcurrentDictionary<string, Dictionary<string, dynamic>>();
                _userStorageMap[userStorageMap.CampaignKey] = userMap;
            }
            if (userMap.ContainsKey(userStorageMap.UserId) && userMap[userStorageMap.UserId] != null && userStorageMap.GoalIdentifier != null)
            {
                userMap[userStorageMap.UserId]["GoalIdentifier"] = userStorageMap.GoalIdentifier;
            }
            else
            {
                userMap[userStorageMap.UserId] = new Dictionary<string, dynamic>() {
                    { "VariationName", userStorageMap.VariationName },
                    { "GoalIdentifier", userStorageMap.GoalIdentifier },
                    { "MetaData",userStorageMap.MetaData }
                };
            }
            SaveAsync();
        }

        public async Task SaveAsync()
        {
            System.IO.File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(_userStorageMap, Newtonsoft.Json.Formatting.Indented));
        }
    }
}