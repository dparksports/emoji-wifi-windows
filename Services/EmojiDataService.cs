using System;
using System.Collections.Generic;
using System.Linq;

namespace EmojiWifiWindows.Services
{
    public class EmojiDataService
    {
        private List<(string Name, string Emojis)> _combinations;
        private Dictionary<string, string> _singleEmojis;

        public EmojiDataService()
        {
            LoadData();
        }

        private void LoadData()
        {
            // Data from combos.csv
            _combinations = new List<(string, string)>
            {
                ("Tech Hub", "💻📶🌐"), ("Signal Strong", "📡⚡🔥"), ("Network Master", "🔗💾🎮"), ("Digital Space", "🌐💻📱"),
                ("WiFi Zone", "📶🔗💡"), ("Space Station", "🚀🛰️🌌"), ("Galaxy Network", "🌌⭐🌑"), ("Rocket WiFi", "🚀⚡💨"),
                ("Astronaut Zone", "👨‍🚀🛰️🌌"), ("Cosmic Signal", "⭐🌌📡"), ("Gaming Hub", "🎮🎵🎧"), ("Game Zone", "🎮⚔️🛡️"),
                ("Player One", "🎮👾🤖"), ("Gaming Station", "🎮🎸🎤"), ("Arcade WiFi", "🎮💾🔫"), ("Music Studio", "🎵🎧🎤"),
                ("Rock WiFi", "🎸🤘🎵"), ("Sound Wave", "🎵🌊🎧"), ("Music Zone", "🎤🎸🎵"), ("Audio Hub", "🎧🎵🎤"),
                ("Nature WiFi", "🌲🌻🌱"), ("Forest Signal", "🌲🏞️🌿"), ("Garden Network", "🌻🌱🌿"), ("Tree WiFi", "🌲🌳🌱"),
                ("Natural Zone", "🌿🌻🌱"), ("Food Network", "🍕🍔🍟"), ("Pizza WiFi", "🍕🍕🍕"), ("Burger Zone", "🍔🍟🥤"),
                ("Snack Hub", "🍟🍕🍰"), ("Foodie WiFi", "🍕🍔🍰"), ("Cool Zone", "😎🔥⚡"), ("Stylish WiFi", "😎💎✨"),
                ("Awesome Network", "😎👍🔥"), ("Epic WiFi", "🔥⚡💥"), ("Legendary Zone", "👑⚡🔥"), ("Dark Network", "🖤🌑👻"),
                ("Ghost WiFi", "👻💀🖤"), ("Mystery Zone", "🔮🌑👻"), ("Shadow Network", "🖤🌑👻"), ("Night WiFi", "🌙⭐👻"),
                ("Dark Vader", "🖤🤖⚔️"), ("Fun Zone", "😄🎉🎈"), ("Happy WiFi", "😊🌈✨"), ("Party Network", "🎉🎊🎈"),
                ("Joy Zone", "😄😊🎉"), ("Smile WiFi", "😊💖✨"), ("Cat Zone", "🐱😸🐾"), ("Dog WiFi", "🐶🐕🐾"),
                ("Panda Paradise", "🐼🎋🎍"), ("Animal Kingdom", "🐱🐶🐼"), ("Pet Network", "🐾🐱🐶"), ("Storm WiFi", "⛈️⚡🌧️"),
                ("Sunny Zone", "☀️🌞🌻"), ("Rainbow Network", "🌈☀️🌧️"), ("Weather Hub", "🌤️⛈️🌈"), ("Sky WiFi", "☁️🌤️🌈"),
                ("Love Zone", "💖💕💗"), ("Heart WiFi", "❤️💙💚"), ("Sweet Network", "💖🍰💕"), ("Romance Zone", "💕💖💗"),
                ("Love Hub", "❤️💕💖"), ("Power Zone", "⚡🔥💥"), ("Energy WiFi", "⚡🔋💡"), ("Lightning Fast", "⚡💨🚀"),
                ("Power Hub", "⚡🔥💥"), ("Energy Zone", "🔋⚡💡"), ("Simple WiFi", "✨💫⭐"), ("Clean Zone", "🤍✨💫"),
                ("Pure Network", "🤍💫✨"), ("Minimal WiFi", "✨🤍💫"), ("Clear Zone", "💫✨🤍")
            };

            // Data from single.csv
            _singleEmojis = new Dictionary<string, string>
            {
                {"📶", "Antenna Bars - Perfect for WiFi signal strength"}, {"📡", "Satellite Antenna - For space-age connectivity"},
                {"💻", "Laptop - Classic computer symbol"}, {"📱", "Mobile Phone - Modern smartphone icon"},
                {"🌐", "Globe - Worldwide internet connection"}, {"🔗", "Link - Network connection symbol"},
                {"💾", "Floppy Disk - Data storage and tech nostalgia"}, {"🎮", "Video Game - Gaming and entertainment"},
                {"🚀", "Rocket - Fast, powerful, and futuristic"}, {"🛰️", "Satellite - Space communication"},
                {"🌌", "Milky Way - Cosmic and mysterious"}, {"🌑", "New Moon - Dark and elegant"},
                {"⭐", "Star - Bright and shining"}, {"👨‍🚀", "Astronaut - Space explorer"},
                {"🤖", "Robot - AI and technology"}, {"👾", "Alien Monster - Gaming and sci-fi"},
                {"⚔️", "Crossed Swords - Battle and strength"}, {"🛡️", "Shield - Protection and security"},
                {"🔫", "Pistol - Action and power"}, {"💥", "Collision - Explosive energy"},
                {"🖤", "Black Heart - Dark and mysterious"}, {"❤️", "Red Heart - Love and passion"},
                {"💙", "Blue Heart - Calm and peaceful"}, {"💚", "Green Heart - Nature and growth"},
                {"💜", "Purple Heart - Royal and mysterious"}, {"🤍", "White Heart - Pure and clean"},
                {"🎵", "Musical Note - Music and rhythm"}, {"🎧", "Headphone - Audio and music"},
                {"🎤", "Microphone - Voice and performance"}, {"🎸", "Guitar - Rock music and instruments"},
                {"🍕", "Pizza - Food and fun"}, {"🍔", "Hamburger - Fast food and casual"},
                {"🍟", "French Fries - Snacks and comfort food"}, {"🍰", "Shortcake - Sweet treats and celebration"},
                {"🌲", "Evergreen Tree - Nature and forest"}, {"🏞️", "National Park - Scenic landscapes"},
                {"🌻", "Sunflower - Bright and cheerful"}, {"🐱", "Cat Face - Cute and playful"},
                {"🐶", "Dog Face - Loyal and friendly"}, {"🐼", "Panda Face - Adorable and rare"},
                {"💡", "Light Bulb - Ideas and innovation"}, {"🔑", "Key - Access and secrets"},
                {"🔒", "Locked - Security and privacy"}, {"⚡", "High Voltage - Power and energy"},
                {"🔥", "Fire - Hot and intense"}, {"❄️", "Snowflake - Cold and pure"},
                {"🌈", "Rainbow - Colorful and magical"}, {"😎", "Sunglasses - Cool and stylish"},
                {"🤓", "Nerd Face - Smart and geeky"}, {"😈", "Devil - Mischievous and playful"},
                {"👻", "Ghost - Spooky and mysterious"}, {"💀", "Skull - Dark and edgy"},
                {"👍", "Thumbs Up - Approval and positivity"}, {"👎", "Thumbs Down - Disapproval"},
                {"✌️", "Peace Sign - Peace and victory"}, {"🤘", "Rock On - Metal and rock music"},
                {"👊", "Fist - Power and strength"}, {"🧠", "Brain - Intelligence and thinking"},
                {"💭", "Thought Balloon - Ideas and thoughts"}, {"🌱", "Seedling - Growth and new beginnings"},
                {"🔬", "Microscope - Science and research"}, {"⚗️", "Alembic - Chemistry and experiments"}
            };
        }

        public (string Name, string Emojis) GetRandomCombination()
        {
            var random = new Random();
            return _combinations[random.Next(_combinations.Count)];
        }

        public string GetRandomSingleEmoji()
        {
            var random = new Random();
            return _singleEmojis.Keys.ElementAt(random.Next(_singleEmojis.Count));
        }

        public string GetRandomLengthEmoji(int min = 1, int max = 4)
        {
            var random = new Random();
            int length = random.Next(min, max + 1);
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += GetRandomSingleEmoji();
            }
            return result;
        }

        public List<(string Name, string Emojis)> GetAllCombinations()
        {
            return _combinations;
        }

        public string GetDescription(string emoji)
        {
            if (_singleEmojis.TryGetValue(emoji, out string desc))
            {
                return desc;
            }
            return "A unique emoji symbol";
        }
    }
}
